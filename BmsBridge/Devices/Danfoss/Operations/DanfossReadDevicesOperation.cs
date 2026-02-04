using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

public sealed class DanfossReadDevicesOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_devices";
    public IEnumerable<DanfossDevicesInfo>? Devices { get; private set; }

    public DanfossReadDevicesOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory) { }

    protected override async Task ParseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var xmlResponse = await response.Content.ReadAsStringAsync(ct);
        var xDoc = XDocument.Parse(xmlResponse);

        // Convert XML → JSON (Newtonsoft)
        string jsonText = JsonConvert.SerializeXNode(xDoc, Formatting.Indented, omitRootObject: false);
        JObject json = JObject.Parse(jsonText);

        // Debug print
        Console.WriteLine(json.ToString(Formatting.Indented));

        var respElement = json["resp"];
        ExportObject = respElement;

        // device may be an array OR a single object
        JToken? deviceToken = respElement?["device"];

        var outDevices = new List<DanfossDevicesInfo>();

        if (deviceToken is JArray arr)
        {
            foreach (var dev in arr)
                outDevices.Add(ConvertToDeviceInfo(dev));
        }
        else if (deviceToken is JObject single)
        {
            outDevices.Add(ConvertToDeviceInfo(single));
        }

        Devices = outDevices;
    }

    private DanfossDevicesInfo ConvertToDeviceInfo(JToken token)
    {
        // Convert Newtonsoft JObject → System.Text.Json.JsonObject
        var jsonObj = JsonNode.Parse(token.ToString())!.AsObject();
        return new DanfossDevicesInfo(jsonObj);
    }
}
