using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Infrastructure;
using PowerOrchestrator.Infrastructure.Data;
using PowerOrchestrator.Infrastructure.Repositories;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<PowerOrchestratorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Configure FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

// Configure MediatR
builder.Services.AddMediatR(typeof(Program).Assembly);

// Configure Redis (optional, will not fail if Redis is unavailable)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    try
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });
    }
    catch (Exception ex)
    {
        Log.Warning("Failed to configure Redis: {Message}", ex.Message);
    }
}

// Register repositories and unit of work
builder.Services.AddScoped<IScriptRepository, ScriptRepository>();
builder.Services.AddScoped<IExecutionRepository, ExecutionRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IHealthCheckRepository, HealthCheckRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        timeout: TimeSpan.FromSeconds(30))
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

// Add Redis health check if configured
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddHealthChecks()
        .AddRedis(redisConnectionString, name: "redis", timeout: TimeSpan.FromSeconds(10));
}

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "PowerOrchestrator API", 
        Version = "v1",
        Description = "PowerOrchestrator Phase 1 API for managing PowerShell scripts and executions"
    });

    // Include XML documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS (for development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PowerOrchestrator API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
    app.UseCors("Development");
}

app.UseHttpsRedirection();
app.UseRouting();

// Add logging middleware
app.UseSerilogRequestLogging();

// Map controllers
app.MapControllers();

// Map health check endpoints
app.MapHealthChecks("/health", new()
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration.TotalMilliseconds,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Duration = entry.Value.Duration.TotalMilliseconds,
                Description = entry.Value.Description,
                Exception = entry.Value.Exception?.Message
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Simple health check endpoints
app.MapGet("/health/ready", () => Results.Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow }));
app.MapGet("/health/live", () => Results.Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow }));

try
{
    Log.Information("Starting PowerOrchestrator API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
