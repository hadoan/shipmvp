namespace ShipMvp.Core.Persistence;

/// <summary>
/// Exception thrown when persistence operations fail
/// </summary>
public sealed class PersistenceException : Exception
{
    public PersistenceException(string message) : base(message) { }
    public PersistenceException(string message, Exception innerException) : base(message, innerException) { }
}
