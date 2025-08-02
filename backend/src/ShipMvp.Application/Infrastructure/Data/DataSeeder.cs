using Microsoft.EntityFrameworkCore;
using ShipMvp.Core.Security;
using ShipMvp.Domain.Identity;
using ShipMvp.Domain.Subscriptions;

namespace ShipMvp.Application.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Create password hasher for seeding users with passwords
        var passwordHasher = new PasswordHasher();

        // Seed Subscription Plans first (only if they don't exist)
        if (!await context.SubscriptionPlans.AnyAsync())
        {
            var plans = new[]
            {
                SubscriptionPlan.CreateFreePlan(),
                SubscriptionPlan.CreateProPlan("price_1234567890abcdefghijklmnop"),
                SubscriptionPlan.CreateEnterprisePlan("price_9876543210zyxwvutsrqponmlk")
            };

            await context.SubscriptionPlans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
        }

        // Seed Users (only if they don't exist)
        if (!await context.Users.AnyAsync())
        {
        var users = new[]
        {
            new User(
                Guid.NewGuid(),
                "admin",
                "Admin",
                "User",
                "admin@shipmvp.com",
                passwordHasher.HashPassword("Admin123!"),
                PhoneNumber.CreateOrDefault("+1234567890"),
                true
            ).AddRole("Admin").ConfirmEmail(),
            
            new User(
                Guid.NewGuid(),
                "testuser@gmail.com",
                "Test",
                "User",
                "testuser@gmail.com",
                passwordHasher.HashPassword("Test123!"),
                PhoneNumber.CreateOrDefault("+0987654321"),
                true
            ).ConfirmEmail(),
            
            new User(
                Guid.NewGuid(),
                "demo",
                "Demo",
                "Account",
                "demo@shipmvp.com",
                passwordHasher.HashPassword("Demo123!"),
                null,
                true
            ).ConfirmEmail().AddRole("User")
        };

        await context.Users.AddRangeAsync(users);

        // Seed User Subscriptions (all start with free plan)
        var subscriptions = users.Select(user => UserSubscription.CreateFreeSubscription(user.Id)).ToArray();
        await context.UserSubscriptions.AddRangeAsync(subscriptions);

        // Seed Usage Tracking
        var usageTracking = users.Select(user => SubscriptionUsage.Create(user.Id)).ToArray();
        await context.SubscriptionUsages.AddRangeAsync(usageTracking);

        // Save all seeded data
        await context.SaveChangesAsync();
        }

        // TODO: Fix invoice item seeding - SQLite composite key issue
        // Invoice seeding is temporarily disabled due to InvoiceItem owned entity configuration issues
        // The problem is with the composite key (InvoiceId, Id) for owned entities in SQLite
        /*
        // Seed Sample Invoices
        var invoices = new[]
        {
            new Invoice(
                Guid.NewGuid(),
                "Acme Corporation",
                new[]
                {
                    InvoiceItem.Create("Web Development Services", 2500.00m),
                    InvoiceItem.Create("Database Setup", 750.00m)
                }
            ),
            
            new Invoice(
                Guid.NewGuid(),
                "TechStart LLC",
                new[]
                {
                    InvoiceItem.Create("Mobile App Development", 5000.00m),
                    InvoiceItem.Create("UI/UX Design", 1500.00m),
                    InvoiceItem.Create("Testing & QA", 800.00m)
                }
            ).MarkAsPaid(),
            
            new Invoice(
                Guid.NewGuid(),
                "Digital Solutions Inc",
                new[]
                {
                    InvoiceItem.Create("Consulting Services", 1200.00m),
                    InvoiceItem.Create("Project Management", 600.00m)
                }
            ),
            
            new Invoice(
                Guid.NewGuid(),
                "StartupXYZ",
                new[]
                {
                    InvoiceItem.Create("API Development", 3000.00m),
                    InvoiceItem.Create("Documentation", 400.00m)
                }
            )
        };

        await context.Invoices.AddRangeAsync(invoices);
        await context.SaveChangesAsync();
        */
    }
}
