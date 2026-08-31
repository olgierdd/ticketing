namespace Ticketing.Api.Infrastructure.Exceptions;

public sealed class BusinessRuleViolationException(string message) : Exception(message);

