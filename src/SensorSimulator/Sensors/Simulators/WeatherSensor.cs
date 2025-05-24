namespace DefaultNamespace.Simulators;

public class WeatherSensor
{
    private readonly Random _random = new Random();
    private readonly string _sensorId;
    private readonly string _location;

    public WeatherSensor(string sensorId, string location)
    {
        _sensorId = sensorId;
        _location = location;
    }

    public SensorData GenerateData()
    {
        return new SensorData
        {
            SensorId = _sensorId,
            SensorType = "Weather",
            Timestamp = DateTime.UtcNow,
            Location = _location,
            Values = new Dictionary<string, object>
            {
                ["temperature"] = Math.Round(18 + _random.NextDouble() * 20, 2),
                ["humidity"] = Math.Round(_random.NextDouble() * 100, 2),
                ["wind_speed"] = Math.Round(_random.NextDouble() * 30, 2),
                ["atmospheric_pressure"] = Math.Round(1000 + _random.NextDouble() * 50, 2),
                ["uv_index"] = _random.Next(1, 12)
            }
        };
    }
}