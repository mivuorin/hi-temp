using Avro;
using Avro.Reflect;
using HiTemp.Functions;
using HiTemp.Schema;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost? host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton((_) =>
        {
            Schema? schema = Resources.Schema();
            return new ReflectReader<Measurement>(schema, schema);
        });

        services.AddSingleton<IAdxClient, AdxClient>();
    })
    .Build();

host.Run();