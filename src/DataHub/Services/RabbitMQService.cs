using System.Text;
using System.Text.Json;
using RabbitMQ.Client;


namespace PrecisionAgriculture.DataHub.Services;

    // Configuración de RabbitMQ
    public class RabbitMQConfig
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string ExchangeName { get; set; } = "agriculture.events";
        public string QueueName { get; set; } = "sensor.data";
        public string RoutingKey { get; set; } = "sensor.data.received";
    }

    // Servicio de RabbitMQ
    public interface IRabbitMQService
    {
        Task PublishSensorDataAsync(SensorData sensorData);
        void Dispose();
    }

    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly RabbitMQConfig _config;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(RabbitMQConfig config, ILogger<RabbitMQService> logger)
        {
            _config = config;
            _logger = logger;

            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _config.HostName,
                    Port = _config.Port,
                    UserName = _config.UserName,
                    Password = _config.Password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar exchange y queue
                _channel.ExchangeDeclare(_config.ExchangeName, ExchangeType.Topic, durable: true);
                _channel.QueueDeclare(_config.QueueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(_config.QueueName, _config.ExchangeName, _config.RoutingKey);

                _logger.LogInformation("✓ RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "✗ Failed to establish RabbitMQ connection");
                throw;
            }
        }

        public async Task PublishSensorDataAsync(SensorData sensorData)
        {
            try
            {
                var message = JsonSerializer.Serialize(sensorData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.Type = sensorData.SensorType;
                properties.Headers = new Dictionary<string, object>
                {
                    ["sensor_id"] = sensorData.SensorId,
                    ["location"] = sensorData.Location,
                    ["timestamp"] = sensorData.Timestamp.ToString("O")
                };

                _channel.BasicPublish(
                    exchange: _config.ExchangeName,
                    routingKey: _config.RoutingKey,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation(
                    "📡 Published sensor data: {SensorType} from {SensorId} at {Location}",
                    sensorData.SensorType, sensorData.SensorId, sensorData.Location
                );

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "✗ Failed to publish sensor data from {SensorId}", 
                    sensorData.SensorId
                );
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }