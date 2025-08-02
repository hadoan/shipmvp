//#if NET9_0
//using Microsoft.EntityFrameworkCore;

//namespace ShipMvp.Core.Persistence.Ef;

///// <summary>
///// Entity Framework implementation of IRepository
///// </summary>
//public class EfRepository<TEntity, TKey>(DbContext db)
//    : IRepository<TEntity, TKey>
//    where TEntity : class, IEntity<TKey>
//{
//    private readonly DbSet<TEntity> _set = db.Set<TEntity>();

//    public Task<TEntity?> GetAsync(TKey id, CancellationToken ct = default)
//        => _set.FindAsync(new object?[] { id }, ct).AsTask();

//    public Task AddAsync(TEntity entity, CancellationToken ct = default)
//        => _set.AddAsync(entity, ct).AsTask();

//    public Task RemoveAsync(TEntity entity, CancellationToken _ = default)
//    {
//        _set.Remove(entity);
//        return Task.CompletedTask;
//    }
//}
//#endif
