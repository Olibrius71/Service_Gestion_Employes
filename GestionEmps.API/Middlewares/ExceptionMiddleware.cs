using System.Text.Json;
using GestionEmps.API.Models;
using GestionEmps.Core.Exceptions;

namespace GestionEmps.API.Middlewares;

/// <summary>
/// Middleware for handling unhandled exceptions globally in the HTTP request pipeline.
/// Captures exceptions, logs them, and returns a standardized error response.
/// </summary>
public class ExceptionMiddleware
{
    /// <summary>
    /// Delegate for invoking the next middleware component in the pipeline.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// Logger for capturing exception details and middleware-level events.
    /// </summary>
    private readonly ILogger<ExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware, capturing and handling any unhandled exceptions.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Handles an exception and writes a standardized error response to the HTTP response stream.
    /// Determines the type of exception, logs it, and assigns the appropriate status code and payload.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="exception">The captured exception.</param>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        _logger.LogError(exception,
            "Exception non gérée capturée. TraceId: {TraceId}",
            traceId);

        var errorResponse = exception switch
        {
            ValidationException validationException =>
                ErrorResponse.CreateValidation(validationException.Errors, traceId),

            SgeException sgeException =>
                ErrorResponse.Create(
                    sgeException.Message,
                    sgeException.ErrorCode,
                    sgeException.StatusCode,
                    traceId),

            ArgumentNullException =>
                ErrorResponse.Create(
                    "Un paramètre requis est manquant.",
                    "ARGUMENT_NULL",
                    400,
                    traceId),

            ArgumentException =>
                ErrorResponse.Create(
                    "Un paramètre fourni est invalide.",
                    "INVALID_ARGUMENT",
                    400,
                    traceId),

            UnauthorizedAccessException =>
                ErrorResponse.Create(
                    "Accès non autorisé.",
                    "UNAUTHORIZED",
                    401,
                    traceId),

            NotImplementedException =>
                ErrorResponse.Create(
                    "Fonctionnalité non implémentée.",
                    "NOT_IMPLEMENTED",
                    501,
                    traceId),

            TimeoutException =>
                ErrorResponse.Create(
                    "L'opération a expiré.",
                    "TIMEOUT",
                    408,
                    traceId),

            _ =>
                ErrorResponse.Create(
                    "Une erreur interne du serveur est survenue.",
                    "INTERNAL_SERVER_ERROR",
                    500,
                    traceId)
        };

        context.Response.StatusCode = errorResponse.StatusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}
