namespace DefaultNamespace.Simulators;

public class NutrientSensor
{
    private readonly Random _random = new Random();
    private readonly string _sensorId;
    private readonly string _location;

    public NutrientSensor(string sensorId, string location)
    {
        _sensorId = sensorId;
        _location = location;
    }

    public SensorData GenerateData()
    {
        return new SensorData
        {
            SensorId = _sensorId,
            SensorType = "Nutrient",
            Timestamp = DateTime.UtcNow,
            Location = _location,
            Values = new Dictionary<string, object>
            {
                ["nitrogen_ppm"] = Math.Round(_random.NextDouble() * 200, 2),
                ["phosphorus_ppm"] = Math.Round(_random.NextDouble() * 100, 2),
                ["potassium_ppm"] = Math.Round(_random.NextDouble() * 300, 2),
                ["conductivity"] = Math.Round(_random.NextDouble() * 2000, 2)
            }
        };
    }
}