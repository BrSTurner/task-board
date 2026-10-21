namespace TaskBoard.Domain.Exceptions;

/// <summary>Thrown when an operation would violate an entity invariant.</summary>
public sealed class DomainException(string message) : Exception(message);
