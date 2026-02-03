using System.Xml.Linq;
using System.Text;

public abstract class DanfossBaseDeviceOperation : BaseDeviceOperation
{
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

    protected HttpRequestMessage BuildRequest(string action, IEnumerable<XAttribute>? extraAttributes = null)
    {
        var attributes = new List<XAttribute>();

        attributes.AddRange(
            _requiredParams.Select(kv => new XAttribute(kv.Key, kv.Value))
        );

        attributes.Add(new XAttribute("action", action));

        if (extraAttributes != null)
            attributes.AddRange(extraAttributes);

        var element = new XElement("cmd", attributes);
        string xmlString = element.ToString(SaveOptions.DisableFormatting);

        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(xmlString, Encoding.UTF8, "application/xml")
        };

        ApplyHeaders(request);

        return request;
    }

    protected override HttpRequestMessage BuildRequest()
    {
        return BuildRequest(Name);
    }
}
