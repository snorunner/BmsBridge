using System.Text.Json.Nodes;
using System.Xml.Linq;

public sealed class DanfossReadRelayOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_relay";
    private readonly IEnumerable<(string node, string mod, string point)> _addresses;

    public DanfossReadRelayOperation(Uri endpoint, IEnumerable<(string node, string mod, string point)> addresses, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        var attributes = new List<XAttribute>();

        attributes.Add(new XAttribute("valid_only", "1"));

        _extraAttributes = attributes;
        _addresses = addresses;
    }

    protected override HttpRequestMessage BuildRequest()
    {
        var attributes = new List<XAttribute>();
        attributes.AddRange(
            _requiredParams.Select(kv => new XAttribute(kv.Key, kv.Value))
        );

        attributes.Add(new XAttribute("action", Name));

        if (_extraAttributes is not null)
            attributes.AddRange(_extraAttributes);

        var element = new XElement("cmd", attributes);

        foreach (var (node, mod, point) in _addresses)
        {
            element.Add(
                new XElement("relay",
                    new XAttribute("node", node),
                    new XAttribute("mod", mod),
                    new XAttribute("point", point)
                )
            );
        }

        string xmlString = element.ToString(SaveOptions.DisableFormatting);

        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(xmlString, System.Text.Encoding.UTF8, "application/xml")
        };

        ApplyHeaders(request);

        return request;
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"]?["relay"];

        return EnforceData(node);
    }
}
