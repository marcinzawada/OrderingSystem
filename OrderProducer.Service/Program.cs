using OrderProducer.Service.Configs;
using OrderProducer.Service.Services;
using Serilog;
using System.Text;

namespace OrderProducer.Service;

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