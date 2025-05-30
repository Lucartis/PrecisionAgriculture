// Servicios/BluetoothDataConverter.cs

using PrecisionAgriculture.DataHub.Models;

namespace PrecisionAgriculture.DataHub.Services
{
    public static class BluetoothDataConverter
    {
        // Convierte SensorData a formato Bluetooth
        public static SensorDataPayload ToBluetoothFormat(SensorData sensorData)
        {
            var payload = new SensorDataPayload
            {
                Timestamp = new DateTimeOffset(sensorData.Timestamp).ToUnixTimeMilliseconds()
            };

            if (sensorData.Values != null)
            {
                foreach (var item in sensorData.Values)
                {
                    // Intenta extraer unidad del nombre de la clave o usa vacío
                    string unit = ExtractUnit(item.Key);
                    string sensorType = CleanSensorType(item.Key);

                    payload.Sensors.Add(new SensorReading
                    {
                        Type = sensorType,
                        Value = Convert.ToDouble(item.Value),
                        Unit = unit,
                        Location = sensorData.Location,
                        Timestamp = new DateTimeOffset(sensorData.Timestamp).ToUnixTimeMilliseconds()
                    });
                }
            }

            return payload;
        }

        // Convierte desde el formato Bluetooth a SensorData
        public static SensorData FromBluetoothFormat(SensorDataPayload payload)
        {
            var sensorData = new SensorData
            {
                SensorId = "mobile_sensor", // O un ID predeterminado
                SensorType = "multi_sensor",
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(payload.Timestamp).DateTime,
                Location = payload.Sensors?.FirstOrDefault()?.Location ?? string.Empty,
                Values = new Dictionary<string, object>()
            };

            if (payload.Sensors != null)
            {
                foreach (var sensor in payload.Sensors)
                {
                    string keyName = string.IsNullOrEmpty(sensor.Unit) 
                        ? sensor.Type 
                        : $"{sensor.Type}_{sensor.Unit}";
                    
                    sensorData.Values[keyName] = sensor.Value;
                }
            }

            return sensorData;
        }

        private static string ExtractUnit(string key)
        {
            // Ejemplos: temperatura_C → C, humedad_percent → percent
            if (key.Contains("_"))
            {
                var parts = key.Split('_');
                if (parts.Length > 1)
                    return parts[1];
            }
            
            // Mapeo común de unidades
            if (key.Contains("temp", StringComparison.OrdinalIgnoreCase)) return "°C";
            if (key.Contains("hum", StringComparison.OrdinalIgnoreCase)) return "%";
            if (key.Contains("ph", StringComparison.OrdinalIgnoreCase)) return "";
            
            return "";
        }

        private static string CleanSensorType(string key)
        {
            // Extrae el nombre del sensor sin la unidad
            if (key.Contains("_"))
            {
                return key.Split('_')[0];
            }
            return key;
        }
    }
}