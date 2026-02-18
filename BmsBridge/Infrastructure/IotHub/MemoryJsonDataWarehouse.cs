using System.Text.Json.Nodes;

public class MemoryJsonDataWarehouse : IJsonDataWarehouse
{
    private JsonArray? _baseline;
    private IEnumerable<string> _preservedFields;
    private readonly List<string> _ignores = new() { "ip", "device_key" };

    public MemoryJsonDataWarehouse(IEnumerable<string> preservedFields)
    {
        _preservedFields = preservedFields;
    }

    public JsonNode ProcessIncoming(JsonArray incoming)
    {
        if (_baseline is null)
        {
            _baseline = incoming.DeepClone().AsArray();
            return incoming;
        }

        var diff = JsonDiffer.Diff(_baseline, incoming, _ignores);

        _baseline = incoming.DeepClone().AsArray();

        return diff;
    }

    public JsonNode? GetJsonData()
        => _baseline;
}
