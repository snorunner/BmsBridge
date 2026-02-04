using System.Text.Json.Nodes;

public sealed class DanfossDevicesInfo
{
    public string Nodetype { get; init; }
    public string Node { get; init; }
    public string Mod { get; init; }
    public string Point { get; init; }
    public JsonObject Data { get; }

    public string DeviceKey => $"{Nodetype}:{Node}:{Mod}:{Point}";

    public DanfossDevicesInfo(JsonObject data)
    {
        Data = data ?? new JsonObject();

        Nodetype = data?["@nodetype"]?.GetValue<string>() ?? "-256";
        Node = data?["@node"]?.GetValue<string>() ?? "-256";
        Mod = data?["@mod"]?.GetValue<string>() ?? "-256";
        Point = data?["@point"]?.GetValue<string>() ?? "-256";
    }
}
