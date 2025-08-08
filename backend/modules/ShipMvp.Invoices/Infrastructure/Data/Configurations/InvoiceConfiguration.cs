using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipMvp.Invoices.Domain.Entities;

namespace ShipMvp.Invoices.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever(); // We generate GUIDs in domain

        builder.Property(x => x.CustomerName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Configure Money value object
        builder.OwnsOne(x => x.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        // Configure InvoiceItems collection
        builder.OwnsMany(x => x.Items, item =>
        {
            item.WithOwner().HasForeignKey("InvoiceId");
            item.Property<Guid>("Id").ValueGeneratedNever();
            item.HasKey("Id", "InvoiceId");

            item.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            item.OwnsOne(x => x.Amount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("ItemAmount")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("ItemCurrency")
                    .IsRequired()
                    .HasMaxLength(3);
            });
        });

        // Indexes
        builder.HasIndex(x => x.CustomerName)
            .HasDatabaseName("IX_Invoices_CustomerName");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_Invoices_Status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_Invoices_CreatedAt");
    }
}
