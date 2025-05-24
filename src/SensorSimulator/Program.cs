using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DefaultNamespace;
using DefaultNamespace.Simulators;


namespace PrecisionAgriculture.SensorSimulator
{
    
    // Cliente HTTP para enviar datos al Hub
    public class SensorDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _hubUrl;

        public SensorDataClient(string hubUrl)
        {
            _httpClient = new HttpClient();
            _hubUrl = hubUrl;
        }

        public async Task SendDataAsync(SensorData data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_hubUrl}/api/sensordata", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✓ Data sent successfully from {data.SensorType} sensor {data.SensorId}");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to send data from {data.SensorType} sensor {data.SensorId}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error sending data from {data.SensorType} sensor {data.SensorId}: {ex.Message}");
            }
        }
    }

    // Simulador principal
    public class SensorSimulator
    {
        private readonly List<object> _sensors;
        private readonly SensorDataClient _client;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public SensorSimulator(string hubUrl)
        {
            _client = new SensorDataClient(hubUrl);
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Inicializar sensores distribuidos en diferentes ubicaciones
            _sensors = new List<object>
            {
                // Sensores de humedad del suelo
                new SoilMoistureSensor("SOIL_001", "Field_A_Zone_1"),
                new SoilMoistureSensor("SOIL_002", "Field_A_Zone_2"),
                new SoilMoistureSensor("SOIL_003", "Field_B_Zone_1"),
                
                // Sensores climáticos
                new WeatherSensor("WEATHER_001", "Field_A_Station"),
                new WeatherSensor("WEATHER_002", "Field_B_Station"),
                
                // Sensores de nutrientes
                new NutrientSensor("NUTRIENT_001", "Field_A_Zone_1"),
                new NutrientSensor("NUTRIENT_002", "Field_B_Zone_1"),
                
                // Sensores de crecimiento
                new CropGrowthSensor("GROWTH_001", "Field_A_Crop_Corn"),
                new CropGrowthSensor("GROWTH_002", "Field_B_Crop_Wheat")
            };
        }

        public async Task StartSimulationAsync()
        {
            Console.WriteLine("🌱 Starting Precision Agriculture Sensor Simulation...");
            Console.WriteLine($"📡 Configured {_sensors.Count} sensors");
            Console.WriteLine("Press Ctrl+C to stop simulation\n");

            var tasks = _sensors.Select(sensor => SimulateSensorAsync(sensor, _cancellationTokenSource.Token)).ToArray();
            
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n🛑 Simulation stopped");
            }
        }

        private async Task SimulateSensorAsync(object sensor, CancellationToken cancellationToken)
        {
            var random = new Random();
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    SensorData data = sensor switch
                    {
                        SoilMoistureSensor s => s.GenerateData(),
                        WeatherSensor s => s.GenerateData(),
                        NutrientSensor s => s.GenerateData(),
                        CropGrowthSensor s => s.GenerateData(),
                        _ => null
                    };

                    if (data != null)
                    {
                        await _client.SendDataAsync(data);
                    }

                    // Intervalo aleatorio entre 5-15 segundos para simular lecturas reales
                    var delay = TimeSpan.FromSeconds(5 + random.NextDouble() * 10);
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in sensor simulation: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }

    // Programa principal
    class Program
    {
        static async Task Main(string[] args)
        {
            // URL del Hub (configurable)
            var hubUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
            
            var simulator = new SensorSimulator(hubUrl);
            
            // Manejar Ctrl+C para parar la simulación
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                simulator.Stop();
            };

            await simulator.StartSimulationAsync();
        }
    }
}