namespace HiTemp.Schema
{
    public class Measurement
    {
        public Guid DeviceId { get; set; } = Guid.Empty;
        public double Value { get; set; }
        public DateTime TimestampMs { get; set; }
    }
}
