using Business.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Business.Api.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception exception)
        {
            if (exception is not NotFoundException and not ValidationException and not ConflictException and not UnauthorizedAccessException and not ArgumentException)
                logger.LogError(exception, "Unhandled request failure for {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = exception switch { NotFoundException => 404, ValidationException => 400, ConflictException => 409, UnauthorizedAccessException => 401, ArgumentException => 400, _ => 500 };
            if (exception is ValidationException validation)
            {
                var errors = validation.Errors.GroupBy(x => string.IsNullOrWhiteSpace(x.PropertyName) ? "request" : char.ToLowerInvariant(x.PropertyName[0]) + x.PropertyName[1..])
                    .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray());
                await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed", Extensions = { ["correlationId"] = context.TraceIdentifier } }); return;
            }
            if (exception is ConflictException { Field: not null } conflict)
            {
                await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(new Dictionary<string, string[]> { [conflict.Field] = [conflict.Message] }) { Status = 409, Title = "Conflict", Extensions = { ["correlationId"] = context.TraceIdentifier } }); return;
            }
            await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = context.Response.StatusCode, Title = exception switch { NotFoundException => "Not found", ConflictException => "Conflict", _ => "Request failed" }, Detail = exception is NotFoundException or ConflictException or UnauthorizedAccessException or ArgumentException ? exception.Message : "An unexpected error occurred.", Extensions = { ["correlationId"] = context.TraceIdentifier } });
        }
    }
}
