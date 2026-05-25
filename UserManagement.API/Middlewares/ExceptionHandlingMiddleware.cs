using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Domain.Exceptions;

namespace UserManagement.API.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleAsync(context, ex); }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            ValidationException => (422, "Validation error"),
            NotFoundException => (404, "Resource not found"),
            ConflictException => (409, "Conflict"),
            UnauthorizedException => (401, "Unauthorized"),
            DomainException => (400, "Domain rule violation"),
            _ => (500, "Internal server error")
        };

        if (status == 500) _logger.LogError(ex, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = ex.Message,
        };

        // Inclui erros de campo detalhados para ValidationException
        if (ex is ValidationException ve)
        {
            problem.Extensions["errors"] = ve.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}