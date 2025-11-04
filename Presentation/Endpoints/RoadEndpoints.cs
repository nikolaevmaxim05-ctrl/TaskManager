using Microsoft.AspNetCore.Authorization;

namespace TaskManager.Presentation.Endpoints;

public static class RoadEndpoints
{
    public static void MapRoadEndpoints(this IEndpointRouteBuilder app)
    {
        app.Map("/login", () => Results.Redirect("/LogInPage.html"));
        app.Map("/signin", () => Results.Redirect("/SighInPage.html"));
        app.Map("/", async (HttpContext context) =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync("wwwroot/index.html");
        });
        app.Map("/work", [Authorize] () => Results.Redirect("/WorkPage.html"));
        app.Map("/tasks/{id:guid}", [Authorize] async (Guid id, HttpContext context) =>
        {
            await context.Response.SendFileAsync("wwwroot/TaskPage.html");
        });
            
        app.Map("/tasks/edit/{id:guid}", [Authorize] async (Guid id, HttpContext context) =>
        {
            await context.Response.SendFileAsync("wwwroot/TaskEditPage.html");
        });
        app.Map("/profile/{id:guid}", [Authorize] async (HttpContext context) =>
        {
            await context.Response.SendFileAsync("wwwroot/Profile.html");
        });
        app.Map("/api/friendList", [Authorize] async (HttpContext context) =>
        {
            await context.Response.SendFileAsync("wwwroot/Friends.html");
        });
        app.Map("/api/add-friend", [Authorize] async (HttpContext context) =>
        {
            await context.Response.SendFileAsync("wwwroot/AddFriends.html");
        });
        app.Map("/chat/{id:guid}", [Authorize] async (HttpContext context) =>
        {
            await context.Response.SendFileAsync("wwwroot/ChatPage.html");
        });
    }
}