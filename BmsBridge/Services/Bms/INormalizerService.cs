using System.Text.Json.Nodes;

public interface INormalizerService
{
    JsonObject Normalize(
        string deviceIp,
        string deviceType,
        string dataAddress,
        JsonObject? rawData);
}
