using System.Text.Json;
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

        return new JsonArray { paramArray };
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        return json?["data"]?["result"] as JsonArray;
    }
}
