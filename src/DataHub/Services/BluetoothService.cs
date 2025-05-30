using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Text.Json;
using PrecisionAgriculture.DataHub.Models;

namespace PrecisionAgriculture.DataHub.Services;

public class BluetoothService : IHostedService, IDisposable
{
    private readonly ILogger<BluetoothService> _logger;
    private readonly ISensorDataAnalyzer _sensorDataAnalyzer;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly INotificationService _notificationService;
    private BluetoothListener _listener;
    private bool _running = false;
    private Task _listenerTask;
    private readonly Guid _serviceId = new Guid("00001101-0000-1000-8000-00805F9B34FB"); // UUID estándar para servicios RFCOMM

    public BluetoothService(
        ILogger<BluetoothService> logger,
        ISensorDataAnalyzer sensorDataAnalyzer,
        IRabbitMQService rabbitMQService,
        INotificationService notificationService)
    {
        _logger = logger;
        _sensorDataAnalyzer = sensorDataAnalyzer;
        _rabbitMQService = rabbitMQService;
        _notificationService = notificationService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔵 Iniciando servicio Bluetooth");

        try
        {
            // Configurar y arrancar el servidor Bluetooth
            _listener = new BluetoothListener(_serviceId);
            _listener.ServiceName = "PrecisionAgricultureDataHub";
            _listener.Start();

            _running = true;
            _listenerTask = Task.Run(ListenForClientsAsync);

            _logger.LogInformation("✅ Servidor Bluetooth iniciado - Esperando conexiones");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al iniciar el servidor Bluetooth");
        }

        return Task.CompletedTask;
    }

    private async Task ListenForClientsAsync()
    {
        while (_running)
        {
            try
            {
                // Aceptar nueva conexión
                var client = await Task.Run(() => _listener.AcceptBluetoothClient());
                _logger.LogInformation("📱 Nueva conexión Bluetooth desde: {Address}", client.RemoteMachineName);

                // Iniciar un nuevo hilo para manejar esta conexión
                _ = Task.Run(() => HandleClientAsync(client));
            }
            catch (Exception ex) when (!_running)
            {
                // Excepción normal al detener
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en el servidor Bluetooth al aceptar conexión");
            }
        }
    }

    // En BluetoothService.cs, modifica el método HandleClientAsync:

private async Task HandleClientAsync(BluetoothClient client)
{
    try
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream))
        using (var writer = new StreamWriter(stream) { AutoFlush = true })
        {
            while (_running && client.Connected)
            {
                // Leer mensaje JSON
                var jsonData = await reader.ReadLineAsync();
                if (jsonData == null) break;

                _logger.LogInformation("📥 Datos recibidos por Bluetooth");

                // Determinar si los datos están en formato nuevo o antiguo
                SensorData sensorData;
                try 
                {
                    // Intenta deserializar como formato nuevo
                    var payload = JsonSerializer.Deserialize<SensorDataPayload>(jsonData);
                    if (payload != null)
                    {
                        // Convertir al modelo interno
                        sensorData = BluetoothDataConverter.FromBluetoothFormat(payload);
                    }
                    else
                    {
                        // Intenta formato antiguo
                        sensorData = JsonSerializer.Deserialize<SensorData>(jsonData);
                    }
                }
                catch
                {
                    // Si falla, intenta el formato antiguo
                    sensorData = JsonSerializer.Deserialize<SensorData>(jsonData);
                }

                if (sensorData != null)
                {
                    // Analizar datos para detectar anomalías
                    var analysisResult = await _sensorDataAnalyzer.AnalyzeDataAsync(sensorData);

                    // Enviar datos a RabbitMQ
                    await _rabbitMQService.PublishSensorDataAsync(sensorData);

                    // Convertir al formato nuevo para enviar respuesta
                    var bluetoothPayload = BluetoothDataConverter.ToBluetoothFormat(sensorData);
                    
                    // Añadir información de anomalías
                    var response = new 
                    {
                        sensors = bluetoothPayload.Sensors,
                        timestamp = bluetoothPayload.Timestamp,
                        anomalyDetected = analysisResult.IsAnomaly,
                        anomalies = analysisResult.IsAnomaly ? analysisResult.Anomalies : new List<string>()
                    };

                    await writer.WriteLineAsync(JsonSerializer.Serialize(response));

                    // Si hay anomalías, enviar notificación
                    if (analysisResult.IsAnomaly)
                    {
                        await _notificationService.SendAnomalyAlertAsync(analysisResult);
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Error al procesar datos del cliente Bluetooth");
    }
}

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Deteniendo servicio Bluetooth");
        _running = false;
        _listener?.Stop();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _listener?.Stop();
    }
}