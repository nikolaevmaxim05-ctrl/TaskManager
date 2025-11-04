using Microsoft.AspNetCore.Antiforgery;

namespace TaskManager.Presentation.Endpoints;

public static class CsrfEndpoints
{
    public static void MapCsrfEndpoints(this IEndpointRouteBuilder app)
    {
        // ✅ Маршрут для выдачи CSRF токена
        app.MapGet("/csrf-token", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Json(new { token = tokens.RequestToken });
        });


    }
}