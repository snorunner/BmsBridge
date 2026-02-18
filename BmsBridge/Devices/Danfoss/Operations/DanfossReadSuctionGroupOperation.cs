using System.Text.Json.Nodes;
using System.Xml.Linq;

public sealed class DanfossReadSuctionGroupOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_suction_group";

    public DanfossReadSuctionGroupOperation(Uri endpoint, string rackId, string suctionId, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        var attributes = new List<XAttribute>();

        attributes.Add(new XAttribute("rack_id", rackId));
        attributes.Add(new XAttribute("suction_id", suctionId));

        _extraAttributes = attributes;
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
