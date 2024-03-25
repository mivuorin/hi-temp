using Avro.IO;
using Avro.Reflect;
using Azure.Messaging.EventHubs;
using HiTemp.Schema;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HiTemp.Functions;

public class EventHubConsumerFunction(
    IAdxClient adxClient,
    ReflectReader<Measurement> reader,
    ILogger<EventHubConsumerFunction> logger)
{
    [Function(nameof(EventHubConsumerFunction))]
    public void Run([EventHubTrigger("hitemphub", Connection = "EventHubConnection")] EventData[] events)
    {
        var measurements = new List<Measurement>(events.Length);

        foreach (EventData eventData in events)
        {
            using var stream = eventData.EventBody.ToStream();
            var decoder = new BinaryDecoder(stream);

            Measurement measurement = reader.Read(decoder);

            logger.LogInformation("Event Content-Type: {contentType}", eventData.ContentType);
            logger.LogInformation("Received event: {0} : {1:O} - {2}", measurement.DeviceId, measurement.TimestampMs,
                measurement.Value);

            measurements.Add(measurement);
        }

        adxClient.Ingest(measurements);
    }
}