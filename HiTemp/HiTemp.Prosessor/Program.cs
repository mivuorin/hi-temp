using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using System.Text;

var uri = new Uri("https://hitempprosessor.blob.core.windows.net/state");
var credentials = new DefaultAzureCredential();

var storage = new BlobContainerClient(uri, credentials);

var eventHubNamespace = "hitemp.servicebus.windows.net";
var eventHubName = "hitemphub";

var prosessor = new EventProcessorClient(storage, EventHubConsumerClient.DefaultConsumerGroupName, eventHubNamespace, eventHubName, credentials);

prosessor.ProcessEventAsync += ProcessEventAsync;
prosessor.ProcessErrorAsync += ProcessErrorAsync;

Console.WriteLine("Starting processing...");
await prosessor.StartProcessingAsync();

Console.WriteLine("Press any key to stop processing...");
Console.ReadKey();

await prosessor.StopProcessingAsync();

Task ProcessEventAsync(Azure.Messaging.EventHubs.Processor.ProcessEventArgs arg)
{
    var message = arg.Data.EventBody.ToString();
    Console.WriteLine("\tReceived event: {0}", message);

    return Task.CompletedTask;
}

Task ProcessErrorAsync(Azure.Messaging.EventHubs.Processor.ProcessErrorEventArgs arg)
{
    Console.WriteLine($"\tPartition '{arg.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
    Console.WriteLine(arg.Exception.Message);

    return Task.CompletedTask;
};
