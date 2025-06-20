﻿using Microsoft.AspNetCore.Mvc;
using PrecisionAgriculture.DataHub.Services;

namespace PrecisionAgriculture.DataHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorDataController : ControllerBase
{
    private readonly IRabbitMQService _rabbitMQService;
    private readonly ILogger<SensorDataController> _logger;
    private readonly ISensorDataAnalyzer _sensorDataAnalyzer;
    private readonly INotificationService _notificationService;
    private readonly IDatabaseService _databaseService;

    public SensorDataController(
        IRabbitMQService rabbitMQService, 
        ILogger<SensorDataController> logger,
        ISensorDataAnalyzer sensorDataAnalyzer,
        INotificationService notificationService,
        IDatabaseService databaseService)
    {
        _rabbitMQService = rabbitMQService;
        _logger = logger;
        _sensorDataAnalyzer = sensorDataAnalyzer;
        _notificationService = notificationService;
        _databaseService = databaseService;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveSensorData([FromBody] SensorData sensorData)
    {
        try
        {
            if (sensorData == null)
            {
                return BadRequest("Sensor data cannot be null");
            }

            if (string.IsNullOrEmpty(sensorData.SensorId) || string.IsNullOrEmpty(sensorData.SensorType))
            {
                return BadRequest("SensorId and SensorType are required");
            }

            _logger.LogInformation(
                "📥 Received sensor data: {SensorType} from {SensorId} at {Location}",
                sensorData.SensorType, sensorData.SensorId, sensorData.Location
            );

            // 1. Analizar datos de sensores (Preprocesamiento)
            var analysisResult = await _sensorDataAnalyzer.AnalyzeDataAsync(sensorData);

            // 2. Guardar en la base de datos PostgreSQL (después del preprocesamiento)
            await _databaseService.SaveSensorDataAsync(sensorData, analysisResult);
            _logger.LogInformation("💾 Datos guardados en PostgreSQL para sensor {SensorId}", sensorData.SensorId);

            // 3. Si hay anomalías, enviar notificación
            if (analysisResult.IsAnomaly)
            {
                await _notificationService.SendAnomalyAlertAsync(analysisResult);
                _logger.LogWarning("⚠️ Anomalía detectada en sensor {SensorId}, notificación enviada", sensorData.SensorId);
            }

            // 4. Enviar a RabbitMQ (datos originales)
            await _rabbitMQService.PublishSensorDataAsync(sensorData);

            return Ok(new {
                message = "Sensor data received, analyzed, saved to database and queued successfully",
                sensorId = sensorData.SensorId,
                timestamp = DateTime.UtcNow,
                anomaliesDetected = analysisResult.IsAnomaly
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sensor data from {SensorId}", sensorData?.SensorId);
            return StatusCode(500, "Internal server error while processing sensor data");
        }
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            service = "Agriculture Data Hub"
        });
    }

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        return Ok(new { 
            status = "Data Hub is running",
            timestamp = DateTime.UtcNow,
            uptime = DateTime.UtcNow.Subtract(Program.StartTime)
        });
    }
}
