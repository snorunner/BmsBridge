using System.Text.Json.Nodes;
using System.Xml.Linq;

public sealed class DanfossReadLightingZoneOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_lighting_zone";

    public DanfossReadLightingZoneOperation(Uri endpoint, string index, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        var attributes = new List<XAttribute>();

        attributes.Add(new XAttribute("index", index));

        _extraAttributes = attributes;
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
