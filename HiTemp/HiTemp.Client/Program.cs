using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;

var eventHubNamespace = "hitemp.servicebus.windows.net";
var eventHubName = "hitemphub";
var credential = new DefaultAzureCredential();

await using var client = new EventHubProducerClient(eventHubNamespace, eventHubName, credential);

using var batch = await client.CreateBatchAsync();

for (int i = 1; i < 4; i++)
{
    var bytes = Encoding.UTF8.GetBytes($"Event {i}");
    var data = new EventData(bytes);

    // TODO Check error if batch size is too big. Now just ignore it.
    batch.TryAdd(data);
}

await client.SendAsync(batch);

Console.WriteLine("Published 3 events");
