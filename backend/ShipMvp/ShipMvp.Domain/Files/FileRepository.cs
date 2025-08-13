using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShipMvp.Core.Persistence;
using ShipMvp.Core.Attributes;

namespace ShipMvp.Domain.Files;

/// <summary>
/// Entity Framework implementation of IFileRepository
/// </summary>
[UnitOfWork]
public class FileRepository : IFileRepository
{
    private readonly IDbContext _context;
    private readonly DbSet<File> _dbSet;

    public FileRepository(IDbContext context)
    {
        _context = context;
        _dbSet = context.Set<File>();
    }

    public async Task<File?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.Id == id && !f.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<File>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.UserId == userId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<File>> GetByContainerAsync(string containerName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.ContainerName == containerName && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<File> InsertAsync(File file, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(file, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return file;
    }

    public async Task<File> UpdateAsync(File file, CancellationToken cancellationToken = default)
    {
        file.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(file);
        await _context.SaveChangesAsync(cancellationToken);
        return file;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await GetByIdAsync(id, cancellationToken);
        if (file != null)
        {
            file.IsDeleted = true;
            file.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<(IEnumerable<File> Files, int TotalCount)> GetPaginatedAsync(
        int skip,
        int take,
        Guid? userId = null,
        string? containerName = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(f => !f.IsDeleted);

        if (userId.HasValue)
        {
            query = query.Where(f => f.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(containerName))
        {
            query = query.Where(f => f.ContainerName == containerName);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var files = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (files, totalCount);
    }
} 