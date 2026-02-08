using System.Text.Json.Nodes;
using System.Text.Json;

public abstract class E3BaseDeviceOperation : BaseDeviceOperation
{
    protected virtual JsonObject? Parameters => null;

    protected E3BaseDeviceOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory) { }

    protected override IReadOnlyDictionary<string, string> DefaultHeaders =>
        new Dictionary<string, string>
        {
            ["Connection"] = "close",
            ["Content-Type"] = "application/json"
        };


    protected override HttpRequestMessage BuildRequest()
    {
        // Build the payload using anonymous objects, not JsonNode
        object payload;

        if (Parameters is JsonObject obj)
        {
            // Convert JsonObject → Dictionary<string, object?>
            var dict = obj.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value is JsonValue v ? v.GetValue<object>() : kvp.Value
            );

            payload = new
            {
                jsonrpc = "2.0",
                method = Name,
                id = "0",
                @params = dict
            };
        }
        else
        {
            payload = new
            {
                jsonrpc = "2.0",
                method = Name,
                id = "0"
            };
        }

        // Serialize to JSON exactly like Python
        var json = JsonSerializer.Serialize(payload);

        _logger.LogCritical(json);

        // Encode into ?m=... exactly like your working operations
        var formDict = new Dictionary<string, string>
        {
            ["m"] = json
        };

        var formUrlEncoded = new FormUrlEncodedContent(formDict);
        var query = formUrlEncoded.ReadAsStringAsync().Result;
        var newUrl = $"{Endpoint}?{query}";

        return new HttpRequestMessage(HttpMethod.Post, newUrl)
        {
            Content = new StringContent("")
        };
    }

    // protected override HttpRequestMessage BuildRequest()
    // {
    //     // Start with the base payload
    //     var root = new Dictionary<string, object?>
    //     {
    //         ["jsonrpc"] = "2.0",
    //         ["method"] = Name,
    //         ["id"] = "0"
    //     };
    //
    //     // If we have parameters, flatten them into the root object
    //     if (Parameters is JsonObject obj)
    //     {
    //         foreach (var kvp in obj)
    //         {
    //             if (kvp.Value is JsonValue v)
    //                 root[kvp.Key] = v.GetValue<object>();
    //             else
    //                 root[kvp.Key] = kvp.Value; // unlikely but safe
    //         }
    //     }
    //
    //     // Serialize to JSON exactly like Python
    //     var json = JsonSerializer.Serialize(root);
    //
    //     _logger.LogCritical(json);
    //
    //     // Wrap in ?m=... exactly like your working operations
    //     var formDict = new Dictionary<string, string>
    //     {
    //         ["m"] = json
    //     };
    //
    //     var formUrlEncoded = new FormUrlEncodedContent(formDict);
    //     var query = formUrlEncoded.ReadAsStringAsync().Result;
    //     var newUrl = $"{Endpoint}?{query}";
    //
    //     return new HttpRequestMessage(HttpMethod.Post, newUrl)
    //     {
    //         Content = new StringContent("")
    //     };
    // }
    protected override JsonNode? Translate(HttpResponseMessage response)
        => JsonNode.Parse(response.Content.ReadAsStringAsync().Result);
}
