using System.Text.Json.Nodes;
using System.Xml.Linq;

public sealed class DanfossReadHvacServiceOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_hvac_service";

    public DanfossReadHvacServiceOperation(Uri endpoint, string ahindex, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        var attributes = new List<XAttribute>();

        attributes.Add(new XAttribute("ahindex", ahindex));

        _extraAttributes = attributes;
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
