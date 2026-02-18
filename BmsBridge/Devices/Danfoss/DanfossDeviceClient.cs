using System.Text.Json.Nodes;

public sealed class DanfossDeviceClient : BaseDeviceClient
{
    private bool _initialized;

    public override BmsType DeviceType => BmsType.Danfoss;

    // Oneshot objects
    private JsonObject? _unitsData;
    private JsonObject? _parmVersions;
    private JsonObject? _storeSchedule;
    private List<JsonObject> _sensors = new();
    private List<JsonObject> _inputs = new();
    private List<JsonObject> _relays = new();
    private List<JsonObject> _var_outs = new();
    private List<JsonObject> _lighting = new();

    // Polling data objects
    private JsonArray _polledData = new();
    private List<JsonObject> _hvac = new();
    private List<JsonObject> _hvacs = new();
    private List<JsonObject> _devices = new();
    private List<JsonObject> _suctionGroups = new();
    private List<JsonObject> _lightingZones = new();
    private List<JsonObject> _hvacUnits = new();
    private List<JsonObject> _hvacService = new();
    private List<JsonObject> _circuits = new();
    private List<JsonObject> _condensers = new();
    private JsonObject? _meters;
    private List<JsonObject> _alarms = new();
    private List<JsonObject> _sensor = new();
    private List<JsonObject> _input = new();
    private List<JsonObject> _relay = new();
    private List<JsonObject> _var_out = new();
    private List<JsonObject> _lists = new();

    public DanfossDeviceClient(
        Uri endpoint,
        IDeviceHttpExecutor pipelineExecutor,
        INormalizerService normalizer,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice
    ) : base(endpoint: endpoint,
            pipelineExecutor: pipelineExecutor,
            normalizer: normalizer,
            loggerFactory: loggerFactory,
            iotDevice: iotDevice
        )
    { }

    public override async Task InitializeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation($"Initializing Danfoss device client at {_endpoint}");

        _initialized = true;

        // Only poll once per restart:
        try
        {
            _unitsData = await ReadUnitsAsync(ct);
            _parmVersions = await ReadParmVersionsAsync(ct);
            _storeSchedule = await ReadStoreScheduleAsync(ct);
            _sensors = await ReadSensorsAsync(ct);
            _inputs = await ReadInputsAsync(ct);
            _relays = await ReadRelaysAsync(ct);
            _var_outs = await ReadVarOutsAsync(ct);

            _polledData = new();
        }
        catch
        {
            _logger.LogError($"Failed to initialize device at {DeviceIp}.");
            _initialized = false;
        }

        _polledData.Add(_unitsData);
        _polledData.Add(_parmVersions);
        _polledData.Add(_storeSchedule);
        _sensors.ForEach(_polledData.Add);
        _inputs.ForEach(_polledData.Add);
        _relays.ForEach(_polledData.Add);
        _var_outs.ForEach(_polledData.Add);
    }

    public override async Task PollAsync(CancellationToken ct = default)
    {
        await EnsureInitialized();

        _lighting = await ReadLightingAsync(ct);
        _hvac = await ReadHvacAsync(ct);
        _hvacs = await ReadHvacsAsync(ct);
        _hvacUnits = await ReadHvacUnitsAsync(ct);
        _hvacService = await ReadHvacServiceAsync(ct);
        _devices = await ReadDevicesAsync(ct);
        _suctionGroups = await ReadSuctionGroupsAsync(ct);
        _condensers = await ReadCondensersAsync(ct);
        _circuits = await ReadCircuitsAsync(ct);
        _lightingZones = await ReadLightingZonesAsync(ct);
        _meters = await ReadMetersAsync(ct);
        _alarms = await ReadAlarmsAsync(ct);
        _sensor = await ReadSensorAsync(ct);
        _input = await ReadInputAsync(ct);
        _var_out = await ReadVarOutAsync(ct);
        _relay = await ReadRelayAsync(ct);

        // EXPERIMENTAL
        _lists = await ReadListAsync(ct);

        _hvac.ForEach(_polledData.Add);
        _hvacs.ForEach(_polledData.Add);
        _hvacUnits.ForEach(_polledData.Add);
        _hvacService.ForEach(_polledData.Add);
        _devices.ForEach(_polledData.Add);
        _suctionGroups.ForEach(_polledData.Add);
        _condensers.ForEach(_polledData.Add);
        _circuits.ForEach(_polledData.Add);
        _lighting.ForEach(_polledData.Add);
        _lightingZones.ForEach(_polledData.Add);
        _polledData.Add(_meters);
        _alarms.ForEach(_polledData.Add);
        _sensor.ForEach(_polledData.Add);
        _input.ForEach(_polledData.Add);
        _var_out.ForEach(_polledData.Add);
        _relay.ForEach(_polledData.Add);
        _lists.ForEach(_polledData.Add);

        var diff = _dataWarehouse.ProcessIncoming(_polledData);
        await _iotDevice.SendMessageAsync(diff, ct);

        _polledData = new JsonArray();
        ClearPollingDataObjects();
    }

    // ------------------------------------------------------------
    // Initialization helpers
    // ------------------------------------------------------------

    private async Task TestPrintAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Testing E2 get cell list operation");
        var op = new DanfossReadUnitsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success)
        {
            _logger.LogError($"Operation failed: {result.ErrorType}, {result.ErrorMessage}");
            _initialized = false;
            return;
        }

        _logger.LogInformation("Raw JSON result:\n{Json}", result.Data?.ToJsonString());
    }

    private async Task<JsonObject> ReadUnitsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadUnitsOperation(_endpoint, _loggerFactory);
        return await ControllerLevelParse(op, ct);
    }

    private async Task<JsonObject> ReadParmVersionsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadParmVersionsOperation(_endpoint, _loggerFactory);
        return await ControllerLevelParse(op, ct);
    }

    private async Task<JsonObject> ReadStoreScheduleAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadStoreScheduleOperation(_endpoint, _loggerFactory);
        return await ControllerLevelParse(op, ct);
    }

    private async Task<List<JsonObject>> ReadSensorsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadSensorsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return InjectedNodetypeParse(result, "2");
    }

    private async Task<List<JsonObject>> ReadSensorAsync(CancellationToken ct = default)
    {
        var addresses = new List<(string node, string mod, string point)>();

        foreach (var entry in _sensors)
        {
            var node = entry["data"]?["node"]?.GetValue<string>();
            var mod = entry["data"]?["mod"]?.GetValue<string>();
            var point = entry["data"]?["point"]?.GetValue<string>();

            if (node is null || mod is null || point is null)
                continue;

            var address = (node, mod, point);
            addresses.Add(address);
        }

        var op = new DanfossReadSensorOperation(_endpoint, addresses, _loggerFactory);
        var response = await op.ExecuteAsync(_pipelineExecutor, ct);
        return DynamicAddressParseNoNT(response, "2");
    }

    private async Task<List<JsonObject>> ReadInputsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadInputsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return InjectedNodetypeParse(result, "0");
    }

    private async Task<List<JsonObject>> ReadInputAsync(CancellationToken ct = default)
    {
        var addresses = new List<(string node, string mod, string point)>();

        foreach (var entry in _inputs)
        {
            var node = entry["data"]?["node"]?.GetValue<string>();
            var mod = entry["data"]?["mod"]?.GetValue<string>();
            var point = entry["data"]?["point"]?.GetValue<string>();

            if (node is null || mod is null || point is null)
                continue;

            var address = (node, mod, point);
            addresses.Add(address);
        }

        var op = new DanfossReadInputOperation(_endpoint, addresses, _loggerFactory);
        var response = await op.ExecuteAsync(_pipelineExecutor, ct);
        return DynamicAddressParseNoNT(response, "0");
    }

    private async Task<List<JsonObject>> ReadRelaysAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadRelaysOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return InjectedNodetypeParse(result, "1");
    }

    private async Task<List<JsonObject>> ReadRelayAsync(CancellationToken ct = default)
    {
        var addresses = new List<(string node, string mod, string point)>();

        foreach (var entry in _sensors)
        {
            var node = entry["data"]?["node"]?.GetValue<string>();
            var mod = entry["data"]?["mod"]?.GetValue<string>();
            var point = entry["data"]?["point"]?.GetValue<string>();

            if (node is null || mod is null || point is null)
                continue;

            var address = (node, mod, point);
            addresses.Add(address);
        }

        var op = new DanfossReadRelayOperation(_endpoint, addresses, _loggerFactory);
        var response = await op.ExecuteAsync(_pipelineExecutor, ct);
        return DynamicAddressParseNoNT(response, "1");
    }

    private async Task<List<JsonObject>> ReadVarOutsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadVarOutsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return InjectedNodetypeParse(result, "3");
    }

    private async Task<List<JsonObject>> ReadVarOutAsync(CancellationToken ct = default)
    {
        var addresses = new List<(string node, string mod, string point)>();

        foreach (var entry in _sensors)
        {
            var node = entry["data"]?["node"]?.GetValue<string>();
            var mod = entry["data"]?["mod"]?.GetValue<string>();
            var point = entry["data"]?["point"]?.GetValue<string>();

            if (node is null || mod is null || point is null)
                continue;

            var address = (node, mod, point);
            addresses.Add(address);
        }

        var op = new DanfossReadVarOutOperation(_endpoint, addresses, _loggerFactory);
        var response = await op.ExecuteAsync(_pipelineExecutor, ct);
        return DynamicAddressParseNoNT(response, "3");
    }

    // ------------------------------------------------------------
    // Polling helpers
    // ------------------------------------------------------------

    private async Task<List<JsonObject>> ReadHvacAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadHvacOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return DynamicAddressParse(result);
    }

    private async Task<List<JsonObject>> ReadListAsync(CancellationToken ct = default)
    {
        var outList = new List<JsonObject>();

        foreach (var hvacEntry in _hvac)
        {
            var node = hvacEntry["data"]?["@node"]?.GetValue<string>() ?? "0";
            var nodeType = hvacEntry["data"]?["@nodetype"]?.GetValue<string>() ?? "0";

            var op = new DanfossReadListOperation(
                endpoint: _endpoint,
                nodeType: nodeType,
                tableAddress: "20021", // We have to guess this number. TODO: fix later
                node: node,
                combo: hvacEntry["data"]?["@combo"]?.GetValue<string>() ?? "0",
                index: hvacEntry["data"]?["@index"]?.GetValue<string>() ?? "0",
                bpIndex: hvacEntry["data"]?["@bpidx"]?.GetValue<string>() ?? "0",
                argument1: hvacEntry["data"]?["@arg1"]?.GetValue<string>() ?? "0",
                useParent: "0",
                configType: "0",
                sType: hvacEntry["data"]?["stype"]?.GetValue<string>() ?? "0",
                subGroup: "0",
                page: "0",
                oldConfigType: "0",
                isConfigure: "0",
                group: "0",
                loggerFactory: _loggerFactory
            );

            var result = await ControllerLevelParse(op, ct, $"list:{nodeType}:{node}:20021");

            outList.Add(result);
        }

        return outList;
    }

    private async Task<JsonObject> ReadMetersAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadMetersOperation(_endpoint, _loggerFactory);
        return await ControllerLevelParse(op, ct, "meters");
    }

    private async Task<List<JsonObject>> ReadAlarmsAsync(CancellationToken ct = default)
    {
        var returnList = new List<JsonObject>();

        var summaryOp = new DanfossAlarmSummaryOperation(_endpoint, _loggerFactory);
        var summaryResult = await summaryOp.ExecuteAsync(_pipelineExecutor, ct);

        if (!summaryResult.Success)
            return returnList;

        var summaryObj = summaryResult.Data![0]?.AsObject();

        if (summaryObj is null)
            return returnList;

        // Normalize "ref" into a JsonArray
        var refNode = summaryObj["active"]?["ref"];
        var activeAlarmRefs = NormalizeToArray(refNode);

        foreach (var reference in activeAlarmRefs)
        {
            var stringRef = reference?.GetValue<string>();

            if (stringRef is null)
                continue;

            var detailOp = new DanfossAlarmDetailOperation(_endpoint, stringRef, _loggerFactory);

            var result = await ControllerLevelParse(
                detailOp,
                ct,
                $"alarm:ref{reference}"
            );

            returnList.Add(result);
        }

        return returnList;
    }

    private async Task<List<JsonObject>> ReadHvacsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadHvacsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return DynamicAddressParse(result);
    }

    private async Task<List<JsonObject>> ReadLightingZonesAsync(CancellationToken ct = default)
    {
        var outList = new List<JsonObject>();

        foreach (var lightingEntry in _lighting)
        {
            var zoneIndex = lightingEntry["data"]?["index"]?.GetValue<string>();

            if (zoneIndex is null)
                continue;

            var op = new DanfossReadLightingZoneOperation(_endpoint, zoneIndex, _loggerFactory);
            var result = await ControllerLevelParse(op, ct, $"lighting_zone:i{zoneIndex}");
            outList.Add(result);
        }

        return outList;
    }

    private async Task<List<JsonObject>> ReadSuctionGroupsAsync(CancellationToken ct = default)
    {
        var outList = new List<JsonObject>();

        var seen = new HashSet<(string rackId, string suctionId)>();

        foreach (var device in _devices)
        {
            var suctionId = device["data"]?["@suction_id"]?.GetValue<string>();
            var rackId = device["data"]?["@rack_id"]?.GetValue<string>();

            if (suctionId is null || rackId is null)
                continue;

            var key = (rackId, suctionId);
            if (!seen.Add(key))
                continue;

            var op = new DanfossReadSuctionGroupOperation(_endpoint, rackId, suctionId, _loggerFactory);
            var result = await ControllerLevelParse(op, ct, $"suction_group:rid{rackId}:sid{suctionId}");
            outList.Add(result);
        }

        return outList;
    }

    private async Task<List<JsonObject>> ReadCondensersAsync(CancellationToken ct = default)
    {
        var outList = new List<JsonObject>();

        var seen = new HashSet<string>();

        foreach (var suct in _suctionGroups)
        {
            var rackId = suct["data"]?["@rack_id"]?.GetValue<string>();

            if (rackId is null)
                continue;

            if (!seen.Add(rackId))
                continue;

            var op = new DanfossReadCondenserOperation(_endpoint, rackId, _loggerFactory);
            var result = await ControllerLevelParse(op, ct, $"condenser:rid{rackId}");
            outList.Add(result);
        }

        return outList;
    }

    private async Task<List<JsonObject>> ReadCircuitsAsync(CancellationToken ct = default)
    {
        var outList = new List<JsonObject>();

        var seen = new HashSet<(string rackId, string suctionId, string stringCircuit)>();

        foreach (var suct in _suctionGroups)
        {
            var data = suct["data"]?.AsObject();
            if (data is null)
                continue;

            var suctionId = data["@suction_id"]?.GetValue<string>();
            var rackId = data["@rack_id"]?.GetValue<string>();
            var maxCircuitsStr = data["num_circuits"]?.GetValue<string>();

            if (string.IsNullOrWhiteSpace(suctionId) ||
                string.IsNullOrWhiteSpace(rackId) ||
                string.IsNullOrWhiteSpace(maxCircuitsStr))
            {
                continue;
            }

            if (!int.TryParse(maxCircuitsStr, out var totalCircuits))
                continue;

            for (int i = 1; i <= totalCircuits; i++)
            {
                var stringCircuit = i.ToString();

                var key = (rackId, suctionId, stringCircuit);
                if (!seen.Add(key))
                    continue;

                var op = new DanfossReadCircuitOperation(_endpoint, rackId, suctionId, stringCircuit, _loggerFactory);
                var result = await ControllerLevelParse(op, ct, $"circuit:rid{rackId}:sid{suctionId}:cid{stringCircuit}");

                outList.Add(result);
            }
        }

        return outList;
    }

    private async Task<List<JsonObject>> ReadHvacUnitsAsync(CancellationToken ct = default)
    {
        var outList = new List<JsonObject>();

        foreach (var hvacEntry in _hvacs)
        {
            var ahindex = hvacEntry["data"]?["@ahindex"]?.GetValue<string>();

            if (ahindex is null)
                continue;

            var op = new DanfossReadHvacUnitOperation(_endpoint, ahindex, _loggerFactory);
            var result = await op.ExecuteAsync(_pipelineExecutor, ct);
            var data = DynamicAddressParse(result);
            data.ForEach(outList.Add);
        }

        return outList;
    }

    private async Task<List<JsonObject>> ReadHvacServiceAsync(CancellationToken ct = default)
    {
        var outList = new List<JsonObject>();

        foreach (var hvacEntry in _hvacs)
        {
            var ahindex = hvacEntry["data"]?["@ahindex"]?.GetValue<string>();

            if (ahindex is null)
                continue;

            var op = new DanfossReadHvacServiceOperation(_endpoint, ahindex, _loggerFactory);
            var data = await ControllerLevelParse(op, ct, $"ahi{ahindex}");
            outList.Add(data);
        }

        return outList;
    }

    private async Task<List<JsonObject>> ReadLightingAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadLightingOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return DynamicAddressParse(result);
    }

    private async Task<List<JsonObject>> ReadDevicesAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadDevicesOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return DynamicAddressParse(result);
    }

    // ------------------------------------------------------------
    // Utility
    // ------------------------------------------------------------

    private async Task EnsureInitialized()
    {
        if (!_initialized)
            await InitializeAsync();
    }

    private void ClearPollingDataObjects()
    {
        _hvac = new();
        _hvacs = new();
        _hvacUnits = new();
        _devices = new();
        _suctionGroups = new();
        _lighting = new();
        _lightingZones = new();
        _circuits = new();
        _condensers = new();
        _meters = null;
        _alarms = new();
        _sensor = new();
        _input = new();
        _relay = new();
        _var_out = new();
        _lists = new();
    }

    private List<JsonObject> DynamicAddressParse(DeviceOperationResult<JsonNode?> result)
    {
        var resultArray = result?.Data?.AsArray();

        if (!result!.Success || resultArray is null)
        {

            _logger.LogDebug("Either the request failed or the result array is null.");
            return new List<JsonObject>();
        }

        var returnList = new List<JsonObject>();


        foreach (var entry in resultArray)
        {
            if (entry is null)
            {
                _logger.LogDebug("An entry in the result array was null.");
                continue;
            }

            var entryObj = entry.AsObject();

            if (!entryObj.TryGetPropertyValue("@nodetype", out var nodeType) ||
                !entryObj.TryGetPropertyValue("@node", out var node) ||
                !entryObj.TryGetPropertyValue("@mod", out var mod) ||
                !entryObj.TryGetPropertyValue("@point", out var point))
            {
                _logger.LogDebug("An entry was missing one of: nodetype, node, mod, or point!");
                continue;
            }

            var normalizedEntry = _normalizer.Normalize(
                DeviceIp,
                DeviceType.ToString(),
                $"nt{nodeType}:n{node}:m{mod}:p{point}",
                entryObj
            );

            returnList.Add(normalizedEntry);
        }

        return returnList;
    }

    private List<JsonObject> DynamicAddressParseNoNT(DeviceOperationResult<JsonNode?> result, string nodeType)
    {
        var resultArray = result?.Data?.AsArray();

        if (!result!.Success || resultArray is null)
        {

            _logger.LogDebug("Either the request failed or the result array is null.");
            return new List<JsonObject>();
        }

        var returnList = new List<JsonObject>();


        foreach (var entry in resultArray)
        {
            if (entry is null)
            {
                _logger.LogDebug("An entry in the result array was null.");
                continue;
            }

            var entryObj = entry.AsObject();

            if (!entryObj.TryGetPropertyValue("@node", out var node) ||
                !entryObj.TryGetPropertyValue("@mod", out var mod) ||
                !entryObj.TryGetPropertyValue("@point", out var point))
            {
                _logger.LogDebug("An entry was missing one of: nodetype, node, mod, or point!");
                continue;
            }

            var normalizedEntry = _normalizer.Normalize(
                DeviceIp,
                DeviceType.ToString(),
                $"nt{nodeType}:n{node}:m{mod}:p{point}",
                entryObj
            );

            returnList.Add(normalizedEntry);
        }

        return returnList;
    }

    private List<JsonObject> InjectedNodetypeParse(DeviceOperationResult<JsonNode?> result, string nodeType)
    {
        var resultArray = result?.Data?.AsArray();

        if (!result!.Success || resultArray is null)
        {
            _logger.LogDebug("Either the request failed or the result array is null.");
            return new List<JsonObject>();
        }

        var returnList = new List<JsonObject>();


        foreach (var entry in resultArray)
        {
            if (entry is null)
            {
                _logger.LogDebug("An entry in the result array was null.");
                continue;
            }

            var entryObj = entry.AsObject();

            if (!entryObj.TryGetPropertyValue("node", out var node) ||
                !entryObj.TryGetPropertyValue("mod", out var mod) ||
                !entryObj.TryGetPropertyValue("point", out var point))
            {
                _logger.LogDebug("An entry is missing one of: node, mod, point!");
                continue;
            }

            var normalizedEntry = _normalizer.Normalize(
                DeviceIp,
                DeviceType.ToString(),
                $"nt{nodeType}:n{node}:m{mod}:p{point}",
                entryObj
            );

            returnList.Add(normalizedEntry);
        }

        return returnList;
    }

    private async Task<JsonObject> ControllerLevelParse(DanfossBaseDeviceOperation op, CancellationToken ct, string dataAddress = "ControllerInfo")
    {
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success)
            return new JsonObject();

        var entry = result.Data?[0]?.AsObject();

        return _normalizer.Normalize(
            DeviceIp,
            DeviceType.ToString(),
            dataAddress,
            entry
        );
    }

    private static JsonArray NormalizeToArray(JsonNode? node)
    {
        return node switch
        {
            JsonArray arr => arr,
            JsonValue val => new JsonArray(val),
            JsonObject obj => new JsonArray(obj),
            null => new JsonArray(),
            _ => new JsonArray()
        };
    }
}
