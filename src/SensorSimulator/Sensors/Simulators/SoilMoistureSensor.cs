namespace DefaultNamespace.Simulators;

public class SoilMoistureSensor
{
    private readonly Random _random = new Random();
    private readonly string _sensorId;
    private readonly string _location;

    public SoilMoistureSensor(string sensorId, string location)
    {
        _sensorId = sensorId;
        _location = location;
    }

    public SensorData GenerateData()
    {
        return new SensorData
        {
            SensorId = _sensorId,
            SensorType = "SoilMoisture",
            Timestamp = DateTime.UtcNow,
            Location = _location,
            Values = new Dictionary<string, object>
            {
                ["moisture_percentage"] = Math.Round(_random.NextDouble() * 100, 2),
                ["soil_temperature"] = Math.Round(15 + _random.NextDouble() * 25, 2),
                ["ph_level"] = Math.Round(6.0 + _random.NextDouble() * 2, 2)
            }
        };
    }
}