using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ShipMvp.Core.Persistence;
using ShipMvp.Domain.Identity;
using ShipMvp.Domain.Integrations;
using ShipMvp.Domain.Shared;
using ShipMvp.Domain.Subscriptions;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipMvp.Application.Infrastructure.Integrations.Data;

namespace ShipMvp.Application.Infrastructure.Data;

public class AppDbContext : DbContext, IDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Identity
    public DbSet<User> Users { get; set; }

    // Integrations
    public DbSet<Integration> Integrations { get; set; }
    public DbSet<IntegrationCredential> IntegrationCredentials { get; set; }


    // Files
    public DbSet<Domain.Files.File> Files { get; set; }

    // Subscriptions
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<SubscriptionUsage> SubscriptionUsages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Explicitly ignore PhoneNumber as an entity type
        modelBuilder.Ignore<PhoneNumber>();
        modelBuilder.Ignore<Money>();
        modelBuilder.Ignore<PlanFeatures>();

        // Value Converter for PhoneNumber value object
        var phoneNumberConverter = new ValueConverter<PhoneNumber?, string?>(
            v => v == null ? null : v.Value,
            v => PhoneNumber.CreateOrDefault(v));

        // Configure entities
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Email)
                  .IsRequired()
                  .HasMaxLength(256);
            entity.Property(x => x.PhoneNumber)
                  .HasMaxLength(20)
                  .HasConversion(phoneNumberConverter);
            entity.Property(x => x.Username).IsRequired().HasMaxLength(256);
            entity.Property(x => x.PasswordHash).HasMaxLength(512);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.CreatedAt);
        });


        modelBuilder.Entity<Domain.Files.File>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.FileName).IsRequired().HasMaxLength(255);
            entity.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(x => x.FileSize).IsRequired();
            entity.HasIndex(x => x.CreatedAt);


        });

        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.PlanId).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.StripeSubscriptionId).HasMaxLength(255);
            entity.Property(x => x.StripeCustomerId).HasMaxLength(255);
            entity.Property(x => x.CurrentPeriodStart).IsRequired();
            entity.Property(x => x.CurrentPeriodEnd).IsRequired();
            entity.Property(x => x.CancelledAt);
            entity.Property(x => x.TrialEnd);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(255);
            entity.Property(x => x.Description).IsRequired();
            entity.Property(x => x.Interval).IsRequired().HasMaxLength(20);
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.StripeProductId).HasMaxLength(255);
            entity.Property(x => x.StripePriceId).HasMaxLength(255);

            // Configure Money value object
            entity.OwnsOne(x => x.Price, money =>
            {
                money.Property(m => m.Amount).HasColumnName("PriceAmount");
                money.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
            });

            // Configure PlanFeatures value object
            entity.OwnsOne(x => x.Features, features =>
            {
                features.Property(f => f.MaxInvoices).HasColumnName("MaxInvoices");
                features.Property(f => f.MaxUsers).HasColumnName("MaxUsers");
                features.Property(f => f.SupportLevel).HasColumnName("SupportLevel");
                features.Property(f => f.CustomBranding).HasColumnName("CustomBranding");
                features.Property(f => f.ApiAccess).HasColumnName("ApiAccess");
            });

            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<SubscriptionUsage>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.InvoiceCount).IsRequired();
            entity.Property(x => x.UserCount).IsRequired();
            entity.Property(x => x.LastUpdated).IsRequired();
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.CreatedAt);
        });

        // Apply Integration entity configurations
        modelBuilder.ConfigureIntegrationEntities();

        // Dynamically apply configurations from any loaded assembly
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

    }
}
