// using System.Text.Json;
using System.Text.Json.Nodes;

public static class JsonDiffer
{
    public static JsonNode Diff(
        JsonNode baseline,
        JsonNode incoming,
        IEnumerable<string>? alwaysIncludeFields = null)
    {
        var includeSet = alwaysIncludeFields != null
            ? new HashSet<string>(alwaysIncludeFields)
            : new HashSet<string>();

        return DiffNodes(baseline, incoming, includeSet) ?? new JsonObject();
    }

    private static JsonNode? DiffNodes(
        JsonNode? baseline,
        JsonNode? incoming,
        HashSet<string> includeSet)
    {
        if (baseline == null || incoming == null || baseline.GetType() != incoming.GetType())
            return incoming?.DeepClone();

        if (baseline is JsonValue && incoming is JsonValue)
        {
            if (!JsonValueEquals(baseline.AsValue(), incoming.AsValue()))
                return incoming.DeepClone();

            return null;
        }

        if (baseline is JsonObject baseObj && incoming is JsonObject incObj)
        {
            var diff = new JsonObject();

            foreach (var kvp in incObj)
            {
                bool forceInclude = includeSet.Contains(kvp.Key);

                baseObj.TryGetPropertyValue(kvp.Key, out var baseChild);
                var childDiff = DiffNodes(baseChild, kvp.Value, includeSet);

                if (forceInclude)
                {
                    diff[kvp.Key] = kvp.Value!.DeepClone();
                }
                else if (childDiff != null)
                {
                    diff[kvp.Key] = childDiff;
                }
            }

            return diff.Count > 0 ? diff : null;
        }

        if (baseline is JsonArray baseArr && incoming is JsonArray incArr)
        {
            var diffArr = new JsonArray();

            for (int i = 0; i < incArr.Count; i++)
            {
                JsonNode? baseChild = i < baseArr.Count ? baseArr[i] : null;
                var childDiff = DiffNodes(baseChild, incArr[i], includeSet);

                if (childDiff is JsonObject objDiff)
                {
                    if (!objDiff.ContainsKey("data"))
                        continue;

                    if (objDiff["data"] is JsonObject dataObj && dataObj.Count == 0)
                        continue;

                    diffArr.Add(childDiff);
                }
                // diffArr.Add(childDiff);
            }
            return diffArr;
        }

        return incoming.DeepClone();
    }

    private static bool JsonValueEquals(JsonValue a, JsonValue b)
    {
        return a.ToJsonString() == b.ToJsonString();
    }
}
