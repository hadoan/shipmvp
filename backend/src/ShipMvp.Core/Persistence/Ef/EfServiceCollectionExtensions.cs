#if NET9_0
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ShipMvp.Core.Persistence.Ef;

/// <summary>
/// Extension methods for registering EF persistence services
/// </summary>
public static class EfServiceCollectionExtensions
{
    public static IServiceCollection AddEfPersistence<TDb>(this IServiceCollection services)
        where TDb : DbContext, IDbContext
    {
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        //services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<TDb>());
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<TDb>());
        return services;
    }
}
#endif
