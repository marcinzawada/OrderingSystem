using Microsoft.EntityFrameworkCore;
using OrderConsumer.Service.Database;
using OrderConsumer.Service.DTOs;
using OrderConsumer.Service.Services;
using Serilog;

namespace OrderConsumer.Service;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            Log.Information("Starting web application");
            Log.Information("This application receives orders from RabbitMQ, calculates the value of the order and writes it to the PostgreSQL database");

            builder.Host.UseSerilog((hostContext, services, configuration) => {
                configuration.ReadFrom.Configuration(hostContext.Configuration);
            });

            builder.Services.AddControllers();

            builder.Services.Configure<RabbitMqConfiguration>(
                rabbitMqConfig => builder.Configuration.GetSection(nameof(RabbitMqConfiguration)).Bind(rabbitMqConfig));

            builder.Services.AddDbContext<DataContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
            builder.Services.AddSingleton<ITimeProvider, TimeProvider>();
            builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
            
            builder.Services.AddHostedService<RabbitMqMessageConsumer>();

            var app = builder.Build();

            app.UseSerilogRequestLogging();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                db.Database.Migrate();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

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

    }
    
}