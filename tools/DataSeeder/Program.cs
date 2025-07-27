using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerOrchestrator.Infrastructure.Data;

// Build configuration
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Add DbContext
        services.AddDbContext<PowerOrchestratorDbContext>(options =>
            options.UseNpgsql("Host=localhost;Port=5432;Database=powerorchestrator_dev;Username=powerorch;Password=PowerOrch2025!"));
        
        // Add the seeder
        services.AddScoped<DataSeeder>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

try
{
    Console.WriteLine("Starting database seeding...");
    
    using var scope = host.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    
    await seeder.SeedAsync();
    
    Console.WriteLine("Database seeding completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during seeding: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Environment.Exit(1);
}
