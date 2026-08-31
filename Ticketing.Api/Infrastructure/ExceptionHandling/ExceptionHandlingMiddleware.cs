using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Api.Infrastructure.Exceptions;

namespace Ticketing.Api.Infrastructure.ExceptionHandling;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

            var (status, title) = exception switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
                NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                BusinessRuleViolationException => (StatusCodes.Status400BadRequest, "Business rule violation"),
                DbUpdateException => (StatusCodes.Status500InternalServerError, "Database error"),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected server error")
            };

            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            }

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}

