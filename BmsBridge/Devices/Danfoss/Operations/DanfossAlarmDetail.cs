using System.Text.Json.Nodes;
using System.Xml.Linq;

public sealed class DanfossAlarmDetailOperation : DanfossBaseDeviceOperation
{
    public override string Name => "alarm_detail";

    public DanfossAlarmDetailOperation(Uri endpoint, string reference, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        var attributes = new List<XAttribute>();

        attributes.Add(new XAttribute("current", reference));
        attributes.Add(new XAttribute("only", "any"));
        attributes.Add(new XAttribute("expanded", "2"));
        attributes.Add(new XAttribute("date_format", "2"));
        attributes.Add(new XAttribute("time_format", "1"));

        _extraAttributes = attributes;
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
