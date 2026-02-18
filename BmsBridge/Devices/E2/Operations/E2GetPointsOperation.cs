using System.Text.Json.Nodes;

public sealed class E2GetPointsOperation : E2BaseDeviceOperation
{
    public override string Name => "E2.GetMultiExpandedStatus";

    public string ControllerName { get; }
    public string CellName { get; }
    protected override JsonArray? Parameters => CreateParameters();

    public IReadOnlyList<(int Index, string PointName)> PointsToQuery { get; }

    public E2GetPointsOperation(
        Uri endpoint,
        string controllerName,
        string cellName,
        IReadOnlyList<(int Index, string PointName)> pointsToQuery,
        ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        ControllerName = controllerName;
        CellName = cellName;
        PointsToQuery = pointsToQuery;
    }

    private JsonArray CreateParameters()
    {
        var paramArray = new JsonArray();

        foreach (var (index, _) in PointsToQuery)
        {
            var key = $"{ControllerName}:{CellName}:{index}";
            paramArray.Add(key);
        }

        _logger.LogDebug($"Number of properties in request: {paramArray.Count}");

        return new JsonArray { paramArray };
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        return json?["result"]?["data"] as JsonArray;
    }

    public override async Task<DeviceOperationResult<JsonNode?>> ExecuteAsync(
        IDeviceHttpExecutor executor,
        CancellationToken ct)
    {
        // If no parameters, skip the request entirely
        if (PointsToQuery.Count == 0)
        {
            _logger.LogDebug("Skipping E2.GetMultiExpandedStatus because there are no points to query.");
            return DeviceOperationResult<JsonNode?>.FromSuccess(new JsonArray());
        }

        return await base.ExecuteAsync(executor, ct);
    }
}
