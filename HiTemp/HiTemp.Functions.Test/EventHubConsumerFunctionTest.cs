using Avro.IO;
using Avro.Reflect;
using Azure.Messaging.EventHubs;
using FluentAssertions;
using FluentAssertions.ArgumentMatchers.Moq;
using HiTemp.Schema;
using Microsoft.Extensions.Logging;
using Moq;

namespace HiTemp.Functions.Test;

public class EventHubConsumerFunctionTest
{
    private readonly EventHubConsumerFunction consumerFunction;
    private readonly ReflectWriter<Measurement> writer;
    private readonly Mock<IAdxClient> adxClient;

    public EventHubConsumerFunctionTest()
    {
        // Use 1ms delta for DateTime comparison
        AssertionOptions.AssertEquivalencyUsing(options =>
            options
                .Using<DateTime>(ctx =>
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(1)))
                .WhenTypeIs<DateTime>());

        var logger = new Mock<ILogger<EventHubConsumerFunction>>();

        Avro.Schema schema = Resources.Schema();
        var classCache = new ClassCache();
        var reader = new ReflectReader<Measurement>(schema, schema, classCache);
        writer = new ReflectWriter<Measurement>(schema, classCache);

        adxClient = new Mock<IAdxClient>();

        consumerFunction = new EventHubConsumerFunction(adxClient.Object, reader, logger.Object);
    }

    [Fact]
    public void Reads_avro_event_format_and_ingest_message_to_adx()
    {
        var first = new Measurement
        {
            DeviceId = Guid.NewGuid(),
            TimestampMs = DateTime.UtcNow,
            Value = 1
        };

        var second = new Measurement
        {
            DeviceId = Guid.NewGuid(),
            TimestampMs = DateTime.UtcNow,
            Value = 2
        };

        var events = new[]
        {
            new EventData(ToBytes(first)),
            new EventData(ToBytes(second))
        };

        consumerFunction.Run(events);

        var expected = new[] { first, second };
        adxClient.Verify(c => c.Ingest(Its.EquivalentTo(expected)));
    }

    private byte[] ToBytes(Measurement first)
    {
        using var stream = new MemoryStream();
        var encoder = new BinaryEncoder(stream);
        writer.Write(first, encoder);

        return stream.GetBuffer();
    }
}