using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TaskManager;
using TaskManager.Infrastructure.DB;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var projectDir = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "../../../../TaskManager")
        );

        builder.UseContentRoot(projectDir);

        builder.ConfigureServices(services =>
        {
            // удаляем настоящую БД
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<UserContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // InMemory вместо SQLite
            services.AddDbContext<UserContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // создаём БД
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserContext>();
            db.Database.EnsureCreated();
        });
    }
}