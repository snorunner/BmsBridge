using System.Xml.Linq;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using System.Xml;

public abstract class DanfossBaseDeviceOperation : BaseDeviceOperation
{
    protected IEnumerable<XAttribute>? _extraAttributes = null;

    protected Dictionary<string, string> _requiredParams;

    protected DanfossBaseDeviceOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        _requiredParams = new Dictionary<string, string>
        {
            ["lang"] = "e",
            ["units"] = "U"
        };
    }

    protected override IReadOnlyDictionary<string, string> DefaultHeaders =>
        new Dictionary<string, string>
        {
            ["Connection"] = "close",
        };

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
        string xmlString = element.ToString(SaveOptions.DisableFormatting);

        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(xmlString, System.Text.Encoding.UTF8, "application/xml")
        };

        ApplyHeaders(request);

        return request;
    }

    protected JsonArray EnforceData(JsonNode? node)
    {
        return node switch
        {
            JsonArray arr => arr,
            JsonObject obj => new JsonArray { obj.DeepClone() },
            _ => new JsonArray()
        };
    }

    protected override JsonNode? Translate(HttpResponseMessage response)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(response.Content.ReadAsStringAsync().Result);
        string jsonText = JsonConvert.SerializeXmlNode(xmlDoc);
        return JsonNode.Parse(jsonText);
    }
}
