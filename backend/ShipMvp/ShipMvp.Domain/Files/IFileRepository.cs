using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShipMvp.Domain.Files;

/// <summary>
/// Repository interface for File entities
/// </summary>
public interface IFileRepository
{
    /// <summary>
    /// Gets a file by its ID
    /// </summary>
    Task<File?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files by user ID
    /// </summary>
    Task<IEnumerable<File>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files by container name
    /// </summary>
    Task<IEnumerable<File>> GetByContainerAsync(string containerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new file
    /// </summary>
    Task<File> InsertAsync(File file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing file
    /// </summary>
    Task<File> UpdateAsync(File file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file (soft delete)
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated files
    /// </summary>
    Task<(IEnumerable<File> Files, int TotalCount)> GetPaginatedAsync(
        int skip,
        int take,
        Guid? userId = null,
        string? containerName = null,
        CancellationToken cancellationToken = default);
}
