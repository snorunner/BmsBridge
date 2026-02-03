using System.Text.Json.Nodes;

public sealed class E2GetControllerListOperation : E2BaseDeviceOperation
{
    public override string Name => "E2.GetControllerList";

    public IReadOnlyList<E2ControllerInfo>? Controllers { get; private set; }
    public E2ControllerInfo? PrimaryController => Controllers?.FirstOrDefault();

    public E2GetControllerListOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    public sealed class E2ControllerInfo
    {
        public string Name { get; init; } = "";
        public int Type { get; init; }
        public string Model { get; init; } = "";
        public string Revision { get; init; } = "";
        public int Subnet { get; init; }
        public int Node { get; init; }
    }

    protected override async Task ParseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var txt = await response.Content.ReadAsStringAsync(ct);

        var json = JsonNode.Parse(txt);

        var resultArray = json?["result"] as JsonArray;

        if (resultArray == null)
        {
            Controllers = Array.Empty<E2ControllerInfo>();
            return;
        }

        var list = new List<E2ControllerInfo>();

        foreach (var item in resultArray)
        {
            if (item is not JsonObject obj)
                continue;

            var info = new E2ControllerInfo
            {
                Name = obj["name"]?.ToString() ?? "",
                Type = obj["type"]?.GetValue<int>() ?? 0,
                Model = obj["model"]?.ToString() ?? "",
                Revision = obj["revision"]?.ToString() ?? "",
                Subnet = obj["subnet"]?.GetValue<int>() ?? 0,
                Node = obj["node"]?.GetValue<int>() ?? 0
            };

            list.Add(info);
        }

        Controllers = list;

        ExportObject = PrimaryController;
    }
}
