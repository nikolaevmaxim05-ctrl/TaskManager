using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Application.Validators;
using TaskManager.Application.Validators.MessageValidators;
using TaskManager.Infrastructure.DB;
using TaskManager.Infrastructure.DB.Repository;
using TaskManager.Infrastructure.Logging;
using TaskManager.Presentation.Endpoints;

namespace TaskManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.AccessDeniedPath = "/";

                    options.Cookie.Name = "TaskManagerAuth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;

                    options.ExpireTimeSpan = TimeSpan.FromHours(12);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization();
     

            builder.Services.AddDbContext<UserContext>(option =>
            {
                option.UseSqlite(connectionString);
            });
            
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<INoteRepository, NoteRepository>();
            builder.Services.AddScoped<IUserConfirmationRepository, UserConfirmationRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            
            
            builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IUserConfirmationService, UserConfirmationService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IFriendServise, FriendService>();
            builder.Services.AddScoped<INoteService, NoteService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            
            

            builder.Services.AddValidatorsFromAssemblyContaining<NoteCreatedDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<NoteUpdateDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UserUpdateDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UserStatsUpdateDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UserConfirmationCreateDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<MessageCreateDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<MessageUpdateDtoValidator>();
                   
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });
            
            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAntiforgery();
            
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
                RequestPath = ""
            });
            
            app.MapCsrfEndpoints();
            app.MapAuthEndpoints();
            app.MapRoadEndpoints();
            app.MapNoteEndpoints();
            app.MapUserEndpoints();
            app.MapFriendEndpoints();
            app.MapNotificationEndpoints();
            app.MapChatEndpoints();
            
            
            
            
            app.Run();
        }
    }
}
