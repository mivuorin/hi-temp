using HiTemp.Schema;

namespace HiTemp.Functions;

public interface IAdxClient
{
    void Ingest(IEnumerable<Measurement> measurements);
}