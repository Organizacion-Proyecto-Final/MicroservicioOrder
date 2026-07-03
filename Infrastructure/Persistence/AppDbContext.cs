using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderNote> OrderNotes => Set<OrderNote>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<FacturaDetail> FacturaDetails => Set<FacturaDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Table>(e =>
        {
            e.ToTable("Tables");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnType("uniqueidentifier").ValueGeneratedNever();
            e.Property(t => t.Number).HasMaxLength(20).IsRequired();
            e.Property(t => t.SeatCount).IsRequired();
            e.Property(t => t.Location).HasMaxLength(80).IsRequired();
            e.Property(t => t.IsEnabled).IsRequired();
            e.Property(t => t.Version).IsRowVersion();
            e.HasIndex(t => t.Number).IsUnique();
        });

        modelBuilder.Entity<Status>(e =>
        {
            e.ToTable("Statuses");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).ValueGeneratedNever();
            e.Property(s => s.Name).HasMaxLength(50).IsRequired();
            e.Property(s => s.Type).HasMaxLength(30).IsRequired();
            e.HasIndex(s => new { s.Name, s.Type }).IsUnique();
            e.HasData(
                new Status(OrderStatusIds.Open, "Open", StatusTypes.Order),
                new Status(OrderStatusIds.InProgress, "InProgress", StatusTypes.Order),
                new Status(OrderStatusIds.ReadyToClose, "ReadyToClose", StatusTypes.Order),
                new Status(OrderStatusIds.Closed, "Closed", StatusTypes.Order),
                new Status(OrderStatusIds.Cancelled, "Cancelled", StatusTypes.Order),
                new Status(OrderItemStatusIds.Pending, "Pending", StatusTypes.OrderItem),
                new Status(OrderItemStatusIds.SentToKitchen, "SentToKitchen", StatusTypes.OrderItem),
                new Status(OrderItemStatusIds.Ready, "Ready", StatusTypes.OrderItem),
                new Status(OrderItemStatusIds.Delivered, "Delivered", StatusTypes.OrderItem),
                new Status(OrderItemStatusIds.Cancelled, "Cancelled", StatusTypes.OrderItem));
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).HasColumnType("uniqueidentifier").ValueGeneratedNever();
            e.Property(o => o.TableId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(o => o.WaiterId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(o => o.StatusId).IsRequired();
            e.Property(o => o.Total).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(o => o.CreatedAt).IsRequired();
            e.Property(o => o.UpdatedAt).IsRequired();
            e.Property(o => o.ClosedAt);
            e.Property(o => o.Version).IsRowVersion();

            e.HasOne(o => o.Table)
             .WithMany()
             .HasForeignKey(o => o.TableId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.Status)
             .WithMany()
             .HasForeignKey(o => o.StatusId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(o => o.Items)
             .WithOne(i => i.Order)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(o => o.Notes)
             .WithOne(n => n.Order)
             .HasForeignKey(n => n.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(o => o.StatusHistory)
             .WithOne(h => h.Order)
             .HasForeignKey(h => h.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(o => o.StatusId);
            e.HasIndex(o => o.TableId);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasColumnType("uniqueidentifier").ValueGeneratedNever();
            e.Property(i => i.OrderId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(i => i.ProductId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(i => i.ProductType).HasMaxLength(50).IsRequired();
            e.Property(i => i.ProductNameSnapshot).HasMaxLength(200).IsRequired();
            e.Property(i => i.UnitPriceSnapshot).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(i => i.DurationMinutesSnapshot).IsRequired();
            e.Property(i => i.Quantity).IsRequired();
            e.Property(i => i.StatusId).IsRequired();
            e.Property(i => i.Notes).HasMaxLength(500);
            e.Property(i => i.SentToKitchenAt);
            e.Property(i => i.ReadyAt);
            e.Property(i => i.CreatedAt).IsRequired();
            e.Property(i => i.UpdatedAt).IsRequired();

            e.HasOne(i => i.Status)
             .WithMany()
             .HasForeignKey(i => i.StatusId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(i => i.OrderId);
            e.HasIndex(i => i.StatusId);
        });

        modelBuilder.Entity<OrderNote>(e =>
        {
            e.ToTable("OrderNotes");
            e.HasKey(n => n.Id);
            e.Property(n => n.Id).HasColumnType("uniqueidentifier").ValueGeneratedNever();
            e.Property(n => n.OrderId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(n => n.CreatedByUserId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(n => n.Note).HasMaxLength(1000).IsRequired();
            e.Property(n => n.CreatedAt).IsRequired();
            e.HasIndex(n => n.OrderId);
        });

        modelBuilder.Entity<OrderStatusHistory>(e =>
        {
            e.ToTable("OrderStatusHistories");
            e.HasKey(h => h.Id);
            e.Property(h => h.Id).HasColumnType("uniqueidentifier").ValueGeneratedNever();
            e.Property(h => h.OrderId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(h => h.PreviousStatusId);
            e.Property(h => h.NewStatusId).IsRequired();
            e.Property(h => h.ChangedByUserId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(h => h.ChangedAt).IsRequired();

            e.HasOne(h => h.PreviousStatus)
             .WithMany()
             .HasForeignKey(h => h.PreviousStatusId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(h => h.NewStatus)
             .WithMany()
             .HasForeignKey(h => h.NewStatusId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(h => h.OrderId);
        });

        modelBuilder.Entity<Factura>(e =>
        {
            e.ToTable("Facturas");
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedOnAdd();
            e.Property(f => f.TableName).IsRequired();
            e.Property(f => f.Date).IsRequired();
            e.Property(f => f.IsPaid).IsRequired();
            e.Property(f => f.Total).HasColumnType("decimal(18,2)").IsRequired();

            e.HasIndex(f => f.Date);
            e.HasIndex(f => f.IsPaid);
            e.HasIndex(f => f.TableName);

            e.HasMany(f => f.Details)
             .WithOne(d => d.Factura)
             .HasForeignKey(d => d.FacturaId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FacturaDetail>(e =>
        {
            e.ToTable("FacturaDetails");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).ValueGeneratedOnAdd();
            e.Property(d => d.Quantity).IsRequired();
            e.Property(d => d.ProductName).HasMaxLength(200).IsRequired();
            e.Property(d => d.Price).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(d => d.FacturaId).IsRequired();

            e.HasIndex(d => d.FacturaId);
        });

        ConfigureUtcDateTimes(modelBuilder);
    }

    private static void ConfigureUtcDateTimes(ModelBuilder modelBuilder)
    {
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => NormalizeToUtc(v),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? NormalizeToUtc(v.Value) : null,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(dateTimeConverter);
                if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableDateTimeConverter);
            }
    }

    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
