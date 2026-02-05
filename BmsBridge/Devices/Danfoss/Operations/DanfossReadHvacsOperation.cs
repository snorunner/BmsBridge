using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

public sealed class DanfossReadHvacsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_hvacs";
    public IEnumerable<DanfossHvacDevicesInfo>? Devices;

    public DanfossReadHvacsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override async Task ParseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var xmlResponse = await response.Content.ReadAsStringAsync(ct);
        var xDoc = XDocument.Parse(xmlResponse);

        // Convert XML → JSON (Newtonsoft)
        string jsonText = JsonConvert.SerializeXNode(xDoc, Formatting.Indented, omitRootObject: false);
        JObject json = JObject.Parse(jsonText);

        var respElement = json["resp"];
        var hvacsElement = respElement?["hvacs"];
        JToken? hvacElement = hvacsElement?["hvac"];

        // Console.WriteLine(hvacElement.ToString(Formatting.Indented));
        ExportObject = hvacsElement;

        var outDevices = new List<DanfossHvacDevicesInfo>();

        if (hvacElement is JArray arr)
        {
            foreach (var ah in arr)
                outDevices.Add(ConvertToDeviceInfo(ah));
        }
        else if (hvacElement is JObject single)
        {
            outDevices.Add(ConvertToDeviceInfo(single));
        }
        Devices = outDevices;
    }

    private DanfossHvacDevicesInfo ConvertToDeviceInfo(JToken token)
    {
        // Convert Newtonsoft JObject → System.Text.Json.JsonObject
        var jsonObj = JsonNode.Parse(token.ToString())!.AsObject();
        return new DanfossHvacDevicesInfo(jsonObj);
    }
}
