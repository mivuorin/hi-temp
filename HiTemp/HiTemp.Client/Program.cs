using Avro;
using Avro.IO;
using Avro.Reflect;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using HiTemp.Schema;

// TODO Multiple device id feature.
var deviceId = new Guid("42B2D950-331C-4F97-B05D-C730E58308BF");

var writer = new ReflectWriter<Measurement>(Resources.Schema());

var random = new Random();

var eventHubNamespace = "hitemp.servicebus.windows.net";
var eventHubName = "hitemphub";
var credential = new DefaultAzureCredential();

await using var client = new EventHubProducerClient(eventHubNamespace, eventHubName, credential);

using var batch = await client.CreateBatchAsync();

// TODO Send messages until stopped.
for (int i = 1; i < 4; i++)
{
    var value = 0 + (random.NextDouble() * 100);

    var message = new Measurement
    {
        DeviceId = deviceId,
        Value = value,
        TimestampMs = DateTime.UtcNow,
    };

    using var memoryStream = new MemoryStream();
    var encoder = new BinaryEncoder(memoryStream);
    writer.Write(message, encoder);

    var bytes = memoryStream.GetBuffer();
    var data = new EventData(bytes);

    // TODO Check error if batch size is too big. Now just ignore it.
    batch.TryAdd(data);
}

await client.SendAsync(batch);

Console.WriteLine("Published 3 events");
