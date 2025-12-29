using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Api.Entities;

namespace Ordering.Api.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();
        
        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);
        
        builder.Property(o => o.ShippingAddress)
            .HasMaxLength(500);
        
        builder.Property(o => o.ShippingCity)
            .HasMaxLength(100);
        
        builder.Property(o => o.ShippingCountry)
            .HasMaxLength(100);
        
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(o => o.CreatedDate)
            .IsRequired();
        
        builder.Property(o => o.UpdatedDate)
            .IsRequired(false);
    }
}
