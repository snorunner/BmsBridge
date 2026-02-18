using System.Text.Json.Nodes;
using System.Xml.Linq;

public sealed class DanfossReadCondenserOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_condenser";

    public DanfossReadCondenserOperation(
        Uri endpoint,
        string rackId,
        ILoggerFactory loggerFactory
    )
        : base(endpoint, loggerFactory)
    {
        var attributes = new List<XAttribute>();

        attributes.Add(new XAttribute("rack_id", rackId));

        _extraAttributes = attributes;
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
