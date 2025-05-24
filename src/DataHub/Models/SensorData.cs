namespace Microsoft.AspNetCore.Builder;

public class SensorData
{
    public string SensorId { get; set; }
    public string SensorType { get; set; }
    public DateTime Timestamp { get; set; }
    public string Location { get; set; }
    public Dictionary<string, object> Values { get; set; }
}