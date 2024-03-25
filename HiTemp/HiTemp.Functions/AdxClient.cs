using System.Text.Json;
using HiTemp.Schema;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Ingest;

namespace HiTemp.Functions;

public class AdxClient : IAdxClient
{
    public void Ingest(IEnumerable<Measurement> measurements)
    {
        var connectionString =
            "https://hitempadx.northeurope.kusto.windows.net; Fed=true; Accept=true;";
        KustoConnectionStringBuilder connectionBuilder = new KustoConnectionStringBuilder(connectionString)
            //.WithAadSystemManagedIdentity() // TODO Figure correct kusto credentials.
            .WithAadAzCliAuthentication();

        IKustoIngestClient client = KustoIngestFactory.CreateStreamingIngestClient(connectionBuilder);
        var ingestionProperties = new KustoIngestionProperties
        {
            DatabaseName = "hitemp",
            TableName = "Measurement",
            Format = DataSourceFormat.json
        };

        using MemoryStream stream = JsonStream(measurements);

        var options = new StreamSourceOptions
        {
            CompressionType = DataSourceCompressionType.None,
            LeaveOpen = false,
            Size = stream.Length
        };

        client.IngestFromStream(stream, ingestionProperties, options);
    }

    private static MemoryStream JsonStream(IEnumerable<Measurement> measurements)
    {
        // TODO Use buffers for writing.
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);

        foreach (Measurement measurement in measurements)
        {
            var line = JsonSerializer.Serialize(measurement);
            writer.WriteLine(line);
        }

        writer.Flush();
        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }
}