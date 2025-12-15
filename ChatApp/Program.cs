using ChatApp.API.Hubs;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Services;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Repositories;
using ChatApp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

try
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        options.LogTo(message => Log.Information("SQL: {Sql}", message), LogLevel.Information);
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            policy =>
            {
                policy.WithOrigins(
                            "http://localhost:5173",
                            "http://172.19.46.111:5173"
                        )
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
    });


    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<IPasswordHasher<ChatApp.Domain.Entities.User>, PasswordHasher<ChatApp.Domain.Entities.User>>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddSingleton<IUserConnectionService, UserConnectionService>();

    builder.Services.AddScoped<IMessageService, MessageService>();
    builder.Services.AddScoped<IConversationService, ConversationService>();
    builder.Services.AddScoped<IMessageRepository, MessageRepository>();
    builder.Services.AddScoped<IConversationRepository, ConversationRepository>();

    builder.Services.AddSignalR();

    builder.Host.UseSerilog();
    builder.Services.AddControllers();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();


    app.UseMiddleware<RequestLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();


    app.UseCors("AllowFrontend");

    app.UseAuthentication();
    app.UseAuthorization();


    app.MapHub<ChatHub>("/hub/chat");

    app.MapControllers();

    Log.Information("Application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}