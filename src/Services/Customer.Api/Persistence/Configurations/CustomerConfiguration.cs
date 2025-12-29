using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customer.Api.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Entities.Customer>
{
    public void Configure(EntityTypeBuilder<Entities.Customer> builder)
    {
        builder.ToTable("Customers");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.HasIndex(c => c.Email)
            .IsUnique();
        
        builder.Property(c => c.Phone)
            .HasMaxLength(20);
        
        builder.Property(c => c.Address)
            .HasMaxLength(500);
        
        builder.Property(c => c.City)
            .HasMaxLength(100);
        
        builder.Property(c => c.Country)
            .HasMaxLength(100);
        
        builder.Property(c => c.CreatedDate)
            .IsRequired();
        
        builder.Property(c => c.UpdatedDate)
            .IsRequired(false);
    }
}
