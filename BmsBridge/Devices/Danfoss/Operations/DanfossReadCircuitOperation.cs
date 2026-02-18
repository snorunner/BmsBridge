using System.Text.Json.Nodes;
using System.Xml.Linq;

public sealed class DanfossReadCircuitOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_circuit";

    public DanfossReadCircuitOperation(
        Uri endpoint,
        string rackId,
        string suctionId,
        string circuitId,
        ILoggerFactory loggerFactory
    )
        : base(endpoint, loggerFactory)
    {
        var attributes = new List<XAttribute>();

        attributes.Add(new XAttribute("rack_id", rackId));
        attributes.Add(new XAttribute("suction_id", suctionId));
        attributes.Add(new XAttribute("circuit_id", circuitId));

        _extraAttributes = attributes;
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
