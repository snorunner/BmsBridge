using System.Text.Json.Nodes;

public class DanfossHvacDevicesInfo : DanfossDevicesInfo
{
    public string AirHandlerIndex { get; init; }
    public override string DeviceKey => $"nodetype{Nodetype}:node{Node}:mod{Mod}:point{Point}:ahindex{AirHandlerIndex}";

    public DanfossHvacDevicesInfo(JsonObject data) : base(data)
    {
        AirHandlerIndex = data?["@ahindex"]?.GetValue<string>() ?? "-256";
    }
}
