using OrderProducer.Service.Configs;
using OrderProducer.Service.Services;
using Serilog;

namespace OrderProducer.Service;

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
            Log.Information("This application creates order creation messages and sends them to the RabbitMQ queue");

            builder.Host.UseSerilog((hostContext, services, configuration) =>
            {
                configuration.ReadFrom.Configuration(hostContext.Configuration);
            });

            ConfigureDependencyInjection(builder);

            var app = builder.Build();

            app.UseSerilogRequestLogging();

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

    private static void ConfigureDependencyInjection(WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<RabbitMqMessageProducer>();

        builder.Services.AddControllers();

        builder.Services.Configure<RabbitMqConfiguration>(
            rabbitMqConfig => builder.Configuration.GetSection(nameof(RabbitMqConfiguration)).Bind(rabbitMqConfig));

        builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
    }
}