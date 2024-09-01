using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderConsumer.Service.Database;
using OrderConsumer.Service.Domain;
using OrderConsumer.Service.DTOs;
using OrderConsumer.Service.Services;
using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Text;

namespace OrderConsumer.Service;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting web application");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((hostContext, services, configuration) => {
                configuration.ReadFrom.Configuration(hostContext.Configuration);
            });

            // Add services to the container.

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

            // Configure the HTTP request pipeline.

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