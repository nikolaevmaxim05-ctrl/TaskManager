using System.Net;
using System.Text.Json;

namespace TaskManager.Infrastructure.Logging;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Неизвестная необработанная ошибка: {Message}", e.Message);
            await HandleExeptionAsync(context, e);
        }
    }

    private static Task HandleExeptionAsync(HttpContext context, Exception ex)
    {
        var problem = new
        {
            status = (int)HttpStatusCode.InternalServerError,
            title = "Внутренняя ошибка сервера",
            detail = ex.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = problem.status;
        var json = JsonSerializer.Serialize(problem);

        return  context.Response.WriteAsync(json);
    }
}