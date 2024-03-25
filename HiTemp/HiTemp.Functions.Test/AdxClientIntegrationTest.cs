using HiTemp.Schema;

namespace HiTemp.Functions.Test;

public class AdxClientIntegrationTest
{
    private static readonly Guid IntegrationTestDeviceId = new("0C4E85B1-9BB8-4E97-8C46-5B56A8B8728A");

    [Fact]
    public async Task Ingest_measurement_to_adx()
    {
        IAdxClient client = new AdxClient();

        var random = new Random();
        var value = random.NextDouble() * 100;

        IEnumerable<Measurement> measurements = new[]
        {
            new Measurement()
            {
                TimestampMs = DateTime.UtcNow,
                DeviceId = IntegrationTestDeviceId,
                Value = 1
            },
            new Measurement()
            {
                TimestampMs = DateTime.UtcNow,
                DeviceId = IntegrationTestDeviceId,
                Value = 12.3D // TODO If value is integer, then it' stored correctly into ADX.
            }
        };

        client.Ingest(measurements);
    }
}