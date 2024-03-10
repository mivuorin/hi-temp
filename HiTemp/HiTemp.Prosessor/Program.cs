using Avro.IO;
using Avro.Reflect;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using HiTemp.Schema;

var uri = new Uri("https://hitempprosessor.blob.core.windows.net/state");
var credentials = new DefaultAzureCredential();

var storage = new BlobContainerClient(uri, credentials);

var eventHubNamespace = "hitemp.servicebus.windows.net";
var eventHubName = "hitemphub";

var prosessor = new EventProcessorClient(storage, EventHubConsumerClient.DefaultConsumerGroupName, eventHubNamespace, eventHubName, credentials);

var schema = Resources.Schema();
var reader = new ReflectReader<Measurement>(schema, schema);

prosessor.ProcessEventAsync += ProcessEventAsync;
prosessor.ProcessErrorAsync += ProcessErrorAsync;

Console.WriteLine("Starting processing...");
await prosessor.StartProcessingAsync();

Console.WriteLine("Press any key to stop processing...");
Console.ReadKey();

await prosessor.StopProcessingAsync();

Task ProcessEventAsync(Azure.Messaging.EventHubs.Processor.ProcessEventArgs arg)
{
    using var stream = arg.Data.EventBody.ToStream();

    var decoder = new BinaryDecoder(stream);
    var measurement = reader.Read(decoder);

    var message = arg.Data.EventBody.ToString();
    Console.WriteLine("Received event: {0}", measurement.DeviceId);
    Console.WriteLine("\t{0} : {1:O} - {2}", measurement.DeviceId, measurement.TimestampMs, measurement.Value);

    return Task.CompletedTask;
}

Task ProcessErrorAsync(Azure.Messaging.EventHubs.Processor.ProcessErrorEventArgs arg)
{
    Console.WriteLine($"\tPartition '{arg.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
    Console.WriteLine(arg.Exception.Message);

    return Task.CompletedTask;
};
