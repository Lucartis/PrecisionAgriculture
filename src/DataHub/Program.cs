using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PrecisionAgriculture.DataHub.Services;

namespace PrecisionAgriculture.DataHub
{ 
    // Programa principal
    public class Program
    {
        public static DateTime StartTime { get; private set; }

        public static void Main(string[] args)
        {
            StartTime = DateTime.UtcNow;
            
            var builder = WebApplication.CreateBuilder(args);

            // Configuración
            builder.Services.Configure<RabbitMQConfig>(
                builder.Configuration.GetSection("RabbitMQ"));

            // Servicios
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Añadir el servicio Bluetooth
            builder.Services.AddHostedService<BluetoothService>();
            builder.Services.AddSingleton<ISensorDataAnalyzer, SensorDataAnalyzer>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();


            // RabbitMQ Service
            builder.Services.AddSingleton<IRabbitMQService>(provider =>
            {
                var config = provider.GetRequiredService<IOptions<RabbitMQConfig>>().Value;
                var logger = provider.GetRequiredService<ILogger<RabbitMQService>>();
                return new RabbitMQService(config, logger);
            });
            

            // CORS para desarrollo
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Pipeline de middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();
            app.UseAuthorization();
            app.MapControllers();

            // Mensaje de inicio
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("🌱 Agriculture Data Hub starting...");
            logger.LogInformation("📡 Listening for sensor data on /api/sensordata");
            logger.LogInformation("🐰 RabbitMQ integration enabled");

            app.Run();
        }
    }
}