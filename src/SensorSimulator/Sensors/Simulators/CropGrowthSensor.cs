namespace DefaultNamespace.Simulators;

public class CropGrowthSensor
{
    private readonly Random _random = new Random();
    private readonly string _sensorId;
    private readonly string _location;

    public CropGrowthSensor(string sensorId, string location)
    {
        _sensorId = sensorId;
        _location = location;
    }

    public SensorData GenerateData()
    {
        return new SensorData
        {
            SensorId = _sensorId,
            SensorType = "CropGrowth",
            Timestamp = DateTime.UtcNow,
            Location = _location,
            Values = new Dictionary<string, object>
            {
                ["plant_height_cm"] = Math.Round(20 + _random.NextDouble() * 150, 2),
                ["leaf_area_index"] = Math.Round(_random.NextDouble() * 6, 2),
                ["chlorophyll_content"] = Math.Round(25 + _random.NextDouble() * 50, 2),
                ["growth_stage"] = new[] { "Germination", "Vegetative", "Flowering", "Fruiting" }[_random.Next(4)]
            }
        };
    }
}