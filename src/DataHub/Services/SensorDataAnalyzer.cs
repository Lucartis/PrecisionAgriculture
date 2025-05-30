namespace PrecisionAgriculture.DataHub.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISensorDataAnalyzer
{
    Task<DataAnalysisResult> AnalyzeDataAsync(SensorData sensorData);
}

public class SensorDataAnalyzer : ISensorDataAnalyzer
{
    private readonly ILogger<SensorDataAnalyzer> _logger;
    private readonly Dictionary<string, SensorThresholds> _sensorThresholds;

    public SensorDataAnalyzer(ILogger<SensorDataAnalyzer> logger)
    {
        _logger = logger;
        // Configuración de umbrales por tipo de sensor
        _sensorThresholds = new Dictionary<string, SensorThresholds>
        {
            ["temperature"] = new SensorThresholds { Min = -10, Max = 50 },
            ["humidity"] = new SensorThresholds { Min = 0, Max = 100 },
            ["soil_moisture"] = new SensorThresholds { Min = 0, Max = 100 },
            ["light"] = new SensorThresholds { Min = 0, Max = 100000 }
            // Añadir más tipos de sensores según sea necesario
        };
    }

    public Task<DataAnalysisResult> AnalyzeDataAsync(SensorData sensorData)
    {
        var result = new DataAnalysisResult
        {
            SensorId = sensorData.SensorId,
            SensorType = sensorData.SensorType,
            Timestamp = sensorData.Timestamp,
            IsAnomaly = false,
            Anomalies = new List<string>()
        };

        // Verificar si tenemos umbrales para este tipo de sensor
        if (_sensorThresholds.TryGetValue(sensorData.SensorType.ToLower(), out var thresholds))
        {
            // Procesar cada medición
            foreach (var measurement in sensorData.Values)
            {
                // Intentar convertir el valor a double para poder compararlo
                if (TryConvertToDouble(measurement.Value, out double numericValue))
                {
                    if (numericValue < thresholds.Min || numericValue > thresholds.Max)
                    {
                        result.IsAnomaly = true;
                        string anomalyMessage = $"Valor {measurement.Key}: {numericValue} está fuera del rango normal ({thresholds.Min}-{thresholds.Max})";
                        result.Anomalies.Add(anomalyMessage);
                        _logger.LogWarning("⚠️ Anomalía detectada: {SensorType} {SensorId} - {Anomaly}",
                                          sensorData.SensorType, sensorData.SensorId, anomalyMessage);
                    }
                }
                else
                {
                    _logger.LogWarning("No se pudo convertir el valor {Value} a un número para análisis", measurement.Value);
                }
            }
        }
        else
        {
            _logger.LogInformation("No hay umbrales configurados para el tipo de sensor {SensorType}", sensorData.SensorType);
        }

        return Task.FromResult(result);
    }

    private bool TryConvertToDouble(object value, out double result)
    {
        result = 0;
        
        if (value == null)
            return false;
            
        // Intenta convertir directamente si es posible
        if (value is double doubleValue)
        {
            result = doubleValue;
            return true;
        }
        
        // Intenta hacer una conversión desde otros tipos numéricos comunes
        if (value is int intValue)
        {
            result = intValue;
            return true;
        }
        
        if (value is float floatValue)
        {
            result = floatValue;
            return true;
        }
        
        // Intenta convertir desde string u otros tipos
        return double.TryParse(value.ToString(), out result);
    }
}

public class SensorThresholds
{
    public double Min { get; set; }
    public double Max { get; set; }
}

public class DataAnalysisResult
{
    public string SensorId { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsAnomaly { get; set; }
    public List<string> Anomalies { get; set; } = new List<string>();
}