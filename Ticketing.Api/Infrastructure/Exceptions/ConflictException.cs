namespace Ticketing.Api.Infrastructure.Exceptions;

public sealed class ConflictException(string message) : Exception(message);

