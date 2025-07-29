using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PowerOrchestrator.API.Middleware;
using PowerOrchestrator.API.Modules;
using PowerOrchestrator.Infrastructure.Data;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure Autofac as the service provider factory
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Configure services that need to be registered with the framework
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<PowerOrchestratorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure FluentValidation
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

// Configure Autofac container
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Register AutoMapper using Autofac integration with assembly scanning
    // This scans for all AutoMapper Profile classes in the specified assemblies
    containerBuilder.RegisterAutoMapper(
        typeof(Program).Assembly // API assembly contains our mapping profiles
    );
    
    // Register our custom modules
    containerBuilder.RegisterModule<CoreModule>();
});

var app = builder.Build();

// Validate AutoMapper configuration at startup
try
{
    var mapper = app.Services.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
    Log.Information("AutoMapper configuration validated successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "AutoMapper configuration validation failed");
    throw;
}

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

// Add global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

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
        await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(response));
    }
});

// Simple health check endpoints
app.MapGet("/health/ready", () => Results.Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow }));
app.MapGet("/health/live", () => Results.Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow }));

try
{
    Log.Information("Starting PowerOrchestrator API with Autofac DI container");
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

// Make Program class available for testing
public partial class Program { }
