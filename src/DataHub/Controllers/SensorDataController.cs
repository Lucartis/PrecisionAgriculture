using Microsoft.AspNetCore.Mvc;
using PrecisionAgriculture.DataHub.Services;

namespace PrecisionAgriculture.DataHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorDataController : ControllerBase
{
    private readonly IRabbitMQService _rabbitMQService;
    private readonly ILogger<SensorDataController> _logger;

    public SensorDataController(IRabbitMQService rabbitMQService, ILogger<SensorDataController> logger)
    {
        _rabbitMQService = rabbitMQService;
        _logger = logger;
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

            // Enviar a RabbitMQ
            await _rabbitMQService.PublishSensorDataAsync(sensorData);

            return Ok(new { 
                message = "Sensor data received and queued successfully",
                sensorId = sensorData.SensorId,
                timestamp = DateTime.UtcNow
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
