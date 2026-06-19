using System.Text.Json;
using KnowledgeAi.Application.Common.Exceptions;

namespace KnowledgeAi.Api.ExceptionHandling;

public static class ExceptionHandlingExtensions
{
    public static void UseApiExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
        {
            var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

            var (statusCode, message) = exception switch
            {
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, exception.Message),
                ForbiddenAccessException => (StatusCodes.Status403Forbidden, exception.Message),
                FluentValidation.ValidationException validationException =>
                    (StatusCodes.Status400BadRequest, string.Join(' ', validationException.Errors.Select(e => e.ErrorMessage))),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
        }));
    }
}
