using System.Reflection;

namespace HiTemp.Schema
{
    public static class Resources
    {
        public static string JsonSchema()
        {
            var resourceName = "HiTemp.Schema.schema.json";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static Avro.Schema Schema()
        {
            return Avro.Schema.Parse(JsonSchema());
        }
    }
}
