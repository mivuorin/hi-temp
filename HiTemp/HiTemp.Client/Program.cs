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

var source = new CancellationTokenSource();
CancellationToken token = source.Token;

Console.WriteLine("Publishing events...");
Task sendingTask = Task.Run(async () =>
{
    var eventCount = 0;
    while (!token.IsCancellationRequested)
    {
        using EventDataBatch batch = await client.CreateBatchAsync();

        var value = 0 + random.NextDouble() * 100;

        var message = new Measurement
        {
            DeviceId = deviceId,
            Value = value,
            TimestampMs = DateTime.UtcNow
        };

        using var memoryStream = new MemoryStream();
        var encoder = new BinaryEncoder(memoryStream);
        writer.Write(message, encoder);

        var bytes = memoryStream.GetBuffer();
        var data = new EventData(bytes)
        {
            ContentType = "application/avro"
        };

        if (!batch.TryAdd(data))
        {
            throw new InvalidOperationException(
                $"Event Hub Batch size {batch.SizeInBytes} exceeded it's maximum size {batch.MaximumSizeInBytes}.");
        }

        await client.SendAsync(batch);

        Console.WriteLine("Published event {0}", ++eventCount);
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}, source.Token);

Console.WriteLine("Press any key to stop publishing...");
Console.ReadKey();

source.Cancel();