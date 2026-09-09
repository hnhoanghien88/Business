using Business.Application.Restaurant.Layouts.Dtos;
using Business.Domain.Entities.Restaurant;

namespace Business.Application.Restaurant.Layouts;

public static class LayoutRules
{
    public static string Code(string value) => value.Trim().ToUpperInvariant();
    public static string Text(string value) => value.Trim();
    public static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    public static AreaDto Area(RestaurantArea value, int tableCount = 0, int openSessionCount = 0) =>
        new(value.Id, value.Code, value.Name, value.Description, value.DisplayOrder, value.IsActive, tableCount, openSessionCount, value.UpdatedDate);
    public static TableDto Table(RestaurantTable value, bool hasOpenSession = false) =>
        new(value.Id, value.AreaId, value.Area.Code, value.Area.Name, value.Area.IsActive, value.Code,
            value.Name, value.Capacity, value.Status, value.IsActive, hasOpenSession,
            !hasOpenSession, !hasOpenSession && value.Status == "Available" && value.IsActive, value.UpdatedDate);
}
