// Modelos/BluetoothModels.cs
namespace PrecisionAgriculture.DataHub.Models
{
    public class SensorReading
    {
        public string Type { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public string Location { get; set; }
        public long Timestamp { get; set; }
    }

    public class SensorDataPayload
    {
        public List<SensorReading> Sensors { get; set; } = new();
        public long Timestamp { get; set; }
    }
}