using System.Text.Json;
using System.Text.Json.Serialization;
using Bangboo;
using Bangboo.Data;
using Bangboo.Modules;
using Bangboo.Modules.Services;
using Bangboo.Server.Services;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Scalar.AspNetCore;

var discordServiceModuleType = typeof(DiscordServiceModule);
var discordTypes = typeof(Program).Assembly
    .GetTypes()
    .Where(type =>
        type is { IsClass: true, IsAbstract: false } &&
        discordServiceModuleType.IsAssignableFrom(type));

var serverServiceModuleType = typeof(ServerServicesModule);
var serverTypes = typeof(Program).Assembly
    .GetTypes()
    .Where(type =>
        type is { IsClass: true, IsAbstract: false } &&
        serverServiceModuleType.IsAssignableFrom(type));

var middlewareServiceModuleType = typeof(MiddlewareModule);
var middlewareTypes = typeof(Program).Assembly
    .GetTypes()
    .Where(type =>
        type is { IsClass: true, IsAbstract: false } &&
        type != middlewareServiceModuleType &&
        middlewareServiceModuleType.IsAssignableFrom(type) &&
        (
            type.GetMethod("Invoke") is not null ||
            type.GetMethod("InvokeAsync") is not null
        ));

// Web API
var builder = WebApplication.CreateBuilder(args);

// Dicord bot
var discordBuilder = Host.CreateDefaultBuilder()
    .UseDiscordGateway(options =>
    {
        options.Intents = GatewayIntents.All;
    })
    .ConfigureServices(services =>
    {
        // Dotenv
        services.AddOptions<Env>()
            .Bind(builder.Configuration.GetSection("Env"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Constants
        services.AddSingleton<Constants>(_ => new Constants());
        services.AddSingleton<Emojis>(_ => new Emojis());

        // Database
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DbConnection")
            );
        });

        services.AddScoped<DatabaseService>();
        //services.AddScoped<AutomodService>();

        // Discord Services
        foreach (var moduleType in discordTypes)
        {
            services.AddSingleton(moduleType);
        }

        services.AddGatewayHandlers(typeof(Program).Assembly);
        services.AddApplicationCommands();
        services.AddComponentInteractions<ModalInteraction, ModalInteractionContext>();
    });

var bot = discordBuilder.Build();
bot.AddModules(typeof(Program).Assembly);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDashboard", policy =>
    {

        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        /*
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        */
    });
});

builder.Services.AddOptions<Env>()
    .Bind(builder.Configuration.GetSection("Env"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DbConnection")
    );
});
builder.Services.AddSingleton(_ => bot.Services.GetRequiredService<RestClient>());
builder.Services.AddSingleton(_ => bot.Services.GetRequiredService<GatewayClient>());
builder.Services.AddScoped<DatabaseService>();
foreach (var moduleType in serverTypes)
{
    builder.Services.AddScoped(moduleType);
}

var app = builder.Build();

app.UseDeveloperExceptionPage();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
    });

    app.UseReDoc(options =>
    {
        options.SpecUrl("/openapi/v1.json");
    });

    app.MapScalarApiReference();
}

app.UseCors("AllowDashboard");

foreach (var moduleType in middlewareTypes)
{
    app.UseMiddleware(moduleType);
}

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (context.Database.CanConnect())
    {
        Console.WriteLine("PostgreSQL connected!!");
    }
    else
    {
        Console.WriteLine("PostgreSQL connection failed!");
    }
}

await Task.WhenAll(app.RunAsync(), bot.RunAsync());
