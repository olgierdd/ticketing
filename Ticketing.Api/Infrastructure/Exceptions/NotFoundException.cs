namespace Ticketing.Api.Infrastructure.Exceptions;

public sealed class NotFoundException(string message) : Exception(message);

