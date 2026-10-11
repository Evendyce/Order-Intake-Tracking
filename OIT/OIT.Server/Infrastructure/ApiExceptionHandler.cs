using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OIT.Server.Domain;

namespace OIT.Server.Infrastructure;

/// <summary>Maps expected business/database conflicts to recoverable API errors without exposing SQL internals.</summary>
public class ApiExceptionHandler(IProblemDetailsService problems, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, detail) = exception switch
        {
            BusinessException business => (business.Status, business.Message),
            DbUpdateConcurrencyException => (409, "This record changed. Reload it before saving."),
            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } }
                => (409, "This reference or email already exists. Reload and try again."),
            DbUpdateException { InnerException: SqlException { Number: 547 } }
                => (409, "This client is now referenced by an order. Reload before removing it."),
            _ => (500, "An unexpected error occurred. Please try again.")
        };
        // Unexpected failures retain diagnostic detail in server logs, not in the public response.
        if (status == 500) logger.LogError(exception, "Unhandled API error for {Path}", context.Request.Path);
        context.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails { Status = status, Title = "Request could not be completed", Detail = detail }
        });
    }
}
