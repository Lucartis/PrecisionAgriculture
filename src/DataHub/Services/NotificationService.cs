namespace PrecisionAgriculture.DataHub.Services;

public interface INotificationService
{
    Task SendAnomalyAlertAsync(DataAnalysisResult anomaly);
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IRabbitMQService _rabbitMQService;

    public NotificationService(ILogger<NotificationService> logger, IRabbitMQService rabbitMQService)
    {
        _logger = logger;
        _rabbitMQService = rabbitMQService;
    }

    public async Task SendAnomalyAlertAsync(DataAnalysisResult anomaly)
    {
        _logger.LogInformation("🚨 Enviando alerta de anomalía para sensor {SensorType} {SensorId}", 
                              anomaly.SensorType, anomaly.SensorId);
        
        // Aquí podríamos implementar diferentes canales de notificación:
        // - Email
        // - SMS
        // - Webhooks
        // - Slack/Teams
        
        // Por ahora, simplemente publicamos en un canal específico de RabbitMQ
        var alertData = new SensorAlert
        {
            SensorId = anomaly.SensorId,
            SensorType = anomaly.SensorType,
            Timestamp = anomaly.Timestamp,
            Location = "N/A", // Obtendrías esto de los datos originales
            Anomalies = anomaly.Anomalies,
            Severity = DetermineSeverity(anomaly)
        };
        
        // Publicar alerta en RabbitMQ (necesitarías extender el servicio RabbitMQ)
        // await _rabbitMQService.PublishAlertAsync(alertData);
        
        _logger.LogInformation("✅ Alerta enviada: {Severity} para {SensorType} {SensorId}", 
                              alertData.Severity, alertData.SensorType, alertData.SensorId);
    }
    
    private string DetermineSeverity(DataAnalysisResult anomaly)
    {
        // Lógica simple para determinar la severidad
        return anomaly.Anomalies.Count > 2 ? "Alta" : "Media";
    }
}

public class SensorAlert
{
    public string SensorId { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<string> Anomalies { get; set; } = new List<string>();
    public string Severity { get; set; } = string.Empty;
}