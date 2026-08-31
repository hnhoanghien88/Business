using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations.Restaurant;

public sealed class RestaurantAreaConfiguration : IEntityTypeConfiguration<RestaurantArea>
{
    public void Configure(EntityTypeBuilder<RestaurantArea> builder)
    {
        builder.ToTable("restaurant_areas");
        builder.HasKey(area => area.Id);
        builder.Property(area => area.Id).ValueGeneratedOnAdd();
        builder.Property(area => area.Code).HasMaxLength(50).IsRequired();
        builder.Property(area => area.Name).HasMaxLength(150).IsRequired();
        builder.Property(area => area.Description).HasMaxLength(500);
        builder.Property(area => area.DisplayOrder).HasDefaultValue(0);
        builder.Property(area => area.IsActive).HasDefaultValue(true);
        builder.Property(area => area.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(area => area.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(area => area.Code)
            .IsUnique()
            .HasDatabaseName("uk_restaurant_areas_code");
        builder.HasIndex(area => new { area.IsActive, area.DisplayOrder })
            .HasDatabaseName("ix_restaurant_areas_active_order");
    }
}

public sealed class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
{
    public void Configure(EntityTypeBuilder<RestaurantTable> builder)
    {
        builder.ToTable(
            "restaurant_tables",
            table =>
            {
                table.HasCheckConstraint("chk_restaurant_tables_capacity", "Capacity > 0");
                table.HasCheckConstraint(
                    "chk_restaurant_tables_status",
                    "Status IN ('Available','Occupied','Reserved','Cleaning','Disabled')");
            });
        builder.HasKey(table => table.Id);
        builder.Property(table => table.Id).ValueGeneratedOnAdd();
        builder.Property(table => table.Code).HasMaxLength(50).IsRequired();
        builder.Property(table => table.Name).HasMaxLength(150).IsRequired();
        builder.Property(table => table.Status).HasMaxLength(30).HasDefaultValue("Available");
        builder.Property(table => table.IsActive).HasDefaultValue(true);
        builder.Property(table => table.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(table => table.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(table => table.Code)
            .IsUnique()
            .HasDatabaseName("uk_restaurant_tables_code");
        builder.HasIndex(table => new { table.AreaId, table.Status, table.IsActive })
            .HasDatabaseName("ix_restaurant_tables_area_status");
        builder.HasOne(table => table.Area)
            .WithMany(area => area.Tables)
            .HasForeignKey(table => table.AreaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_restaurant_tables_area");
    }
}

public sealed class TableSessionConfiguration : IEntityTypeConfiguration<TableSession>
{
    public void Configure(EntityTypeBuilder<TableSession> builder)
    {
        builder.ToTable(
            "restaurant_table_sessions",
            table =>
            {
                table.HasCheckConstraint("chk_table_sessions_guest_count", "GuestCount > 0");
                table.HasCheckConstraint(
                    "chk_table_sessions_status",
                    "Status IN ('Open','Closed','Cancelled')");
            });
        builder.HasKey(session => session.Id);
        builder.Property(session => session.Id).ValueGeneratedOnAdd();
        builder.Property(session => session.GuestCount).HasDefaultValue(1);
        builder.Property(session => session.Status).HasMaxLength(30).HasDefaultValue("Open");
        builder.Property(session => session.OpenedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(session => session.Note).HasMaxLength(500);
        builder.Property(session => session.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(session => session.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(session => new { session.TableId, session.Status })
            .HasDatabaseName("ix_table_sessions_table_status");
        builder.HasIndex(session => session.OpenedDate)
            .HasDatabaseName("ix_table_sessions_opened_date");
        builder.HasOne(session => session.Table)
            .WithMany(table => table.Sessions)
            .HasForeignKey(session => session.TableId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_table_sessions_table");
    }
}
