using System.Data;
using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Ordering.Dtos;
using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence;

public sealed class MySqlOrderingRepository(BusinessDbContext dbContext) : IOrderingRepository
{
    public async Task<OrderPreviewDto> PreviewAsync(
        ulong sessionId, IReadOnlyList<OrderLineInput> lines, string? promotionCode,
        CancellationToken cancellationToken)
    {
        var sessionValid = await dbContext.TableSessions.AnyAsync(
            value => value.Id == sessionId && value.Status == "Open"
                && value.Table.Status == "Occupied", cancellationToken);
        if (!sessionValid) throw new ConflictException("The table session is not open.", "sessionId");

        var ids = lines.Select(line => line.FoodVariantId).Distinct().ToList();
        var variants = await dbContext.FoodVariants
            .Include(value => value.Food).ThenInclude(value => value.Category)
            .Where(value => ids.Contains(value.Id)).ToListAsync(cancellationToken);
        foreach (var line in lines)
        {
            var variant = variants.SingleOrDefault(value => value.Id == line.FoodVariantId);
            if (variant is null || !variant.IsActive || !variant.Food.IsActive
                || !variant.Food.Category.IsActive || !variant.IsAvailable)
                throw new ConflictException($"Variant '{line.FoodVariantId}' is not available.", "items");
        }

        var subtotal = lines.Sum(line =>
            variants.Single(value => value.Id == line.FoodVariantId).CurrentPrice * line.Quantity);
        var (discount, normalizedCode, message) = await EvaluatePromotionAsync(
            variants, subtotal, promotionCode, cancellationToken);
        return new OrderPreviewDto(subtotal, discount, subtotal - discount, normalizedCode, message);
    }

    public async Task<OrderConfirmationDto> ConfirmAsync(
        ulong sessionId, Guid requestId, decimal observedTotal, IReadOnlyList<OrderLineInput> lines,
        string? promotionCode, ulong? actorId, CancellationToken cancellationToken)
    {
        var replay = await dbContext.Orders.AsNoTracking()
            .Include(value => value.KitchenOrders)
            .SingleOrDefaultAsync(value => value.ClientRequestId == requestId, cancellationToken);
        if (replay is not null)
            return new OrderConfirmationDto(
                replay.Id, replay.OrderNo, replay.KitchenOrders.Single().KitchenNo,
                replay.TotalAmount, true);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var preview = await PreviewAsync(sessionId, lines, promotionCode, cancellationToken);
        if (preview.TotalAmount != observedTotal)
            throw new ConflictException(
                $"Total changed to {preview.TotalAmount:0.00}; confirm the current total and retry.",
                "observedTotal");

        var variantIds = lines.Select(line => line.FoodVariantId).Distinct().ToList();
        var variants = await dbContext.FoodVariants.Include(value => value.Food)
            .Where(value => variantIds.Contains(value.Id)).ToDictionaryAsync(value => value.Id, cancellationToken);
        var now = DateTime.UtcNow;
        var suffix = requestId.ToString("N")[..12].ToUpperInvariant();
        var order = new Order
        {
            ClientRequestId = requestId, OrderNo = $"ORD-{suffix}", TableSessionId = sessionId,
            Status = "Pending", SubtotalAmount = preview.SubtotalAmount,
            DiscountAmount = preview.DiscountAmount, TotalAmount = preview.TotalAmount,
            OrderedDate = now, CreatedBy = actorId, CreatedDate = now, UpdatedDate = now
        };
        foreach (var line in lines)
        {
            var variant = variants[line.FoodVariantId];
            order.Items.Add(new OrderItem
            {
                Order = order, FoodId = variant.FoodId, Food = variant.Food,
                FoodVariantId = variant.Id, FoodVariant = variant, FoodCode = variant.Food.Code,
                FoodName = variant.Food.Name, VariantName = variant.Name, Quantity = line.Quantity,
                UnitPrice = variant.CurrentPrice, TotalAmount = variant.CurrentPrice * line.Quantity,
                Note = string.IsNullOrWhiteSpace(line.Note) ? null : line.Note.Trim(),
                CreatedDate = now, UpdatedDate = now
            });
        }
        if (preview.PromotionCode is not null)
        {
            var promotion = await dbContext.PromotionCodes.SingleAsync(
                value => value.Code == preview.PromotionCode, cancellationToken);
            order.Promotions.Add(new OrderPromotion
            {
                Order = order, PromotionId = promotion.Id, Promotion = promotion,
                PromotionCode = promotion.Code, PromotionName = promotion.Name,
                DiscountAmount = preview.DiscountAmount, CreatedDate = now
            });
            promotion.UsageCount++;
        }
        var kitchen = new KitchenOrder
        {
            Order = order, KitchenNo = $"KIT-{suffix}", SentToKitchenDate = now,
            CreatedDate = now, UpdatedDate = now
        };
        foreach (var item in order.Items)
            kitchen.Items.Add(new KitchenOrderItem
            {
                KitchenOrder = kitchen, OrderItem = item, Quantity = item.Quantity,
                Note = item.Note, CreatedDate = now, UpdatedDate = now
            });
        order.KitchenOrders.Add(kitchen);
        dbContext.Orders.Add(order);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            var existing = await dbContext.Orders.AsNoTracking().Include(value => value.KitchenOrders)
                .SingleOrDefaultAsync(value => value.ClientRequestId == requestId, cancellationToken);
            if (existing is null) throw;
            return new OrderConfirmationDto(
                existing.Id, existing.OrderNo, existing.KitchenOrders.Single().KitchenNo,
                existing.TotalAmount, true);
        }
        return new OrderConfirmationDto(order.Id, order.OrderNo, kitchen.KitchenNo, order.TotalAmount, false);
    }

    private async Task<(decimal Discount, string? Code, string? Message)> EvaluatePromotionAsync(
        IReadOnlyList<FoodVariant> variants, decimal subtotal, string? code,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code)) return (0m, null, null);
        var normalized = code.Trim().ToUpperInvariant();
        var promotion = await dbContext.PromotionCodes
            .Include(value => value.PromotionFoods).Include(value => value.PromotionCategories)
            .SingleOrDefaultAsync(value => value.Code == normalized, cancellationToken)
            ?? throw new ConflictException("Promotion code was not found.", "promotionCode");
        var now = DateTime.UtcNow;
        if (!promotion.IsActive || now < promotion.StartDate || now > promotion.EndDate
            || (promotion.UsageLimit.HasValue && promotion.UsageCount >= promotion.UsageLimit)
            || (promotion.MinOrderAmount.HasValue && subtotal < promotion.MinOrderAmount))
            throw new ConflictException("Promotion is inactive, expired, exhausted, or below minimum.", "promotionCode");
        var scoped = promotion.PromotionFoods.Count == 0 && promotion.PromotionCategories.Count == 0
            || variants.Any(variant => promotion.PromotionFoods.Any(item => item.FoodId == variant.FoodId)
                || promotion.PromotionCategories.Any(item => item.CategoryId == variant.Food.CategoryId));
        if (!scoped) throw new ConflictException("Promotion does not apply to these items.", "promotionCode");
        var discount = promotion.DiscountType == "Percentage"
            ? subtotal * promotion.DiscountValue / 100m : promotion.DiscountValue;
        discount = Math.Min(discount, promotion.MaxDiscountAmount ?? discount);
        discount = Math.Min(discount, subtotal);
        return (decimal.Round(discount, 2), normalized, promotion.Name);
    }
}
