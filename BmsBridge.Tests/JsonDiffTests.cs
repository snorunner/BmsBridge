using System.Text.Json;
using System.Text.Json.Nodes;

public class JsonDiffTests
{
    [Fact]
    public void Diff_ReturnsOnlyChangedFields()
    {
        // Baseline JSON
        var baselineJson = """
        {
            "name": "Alice",
            "age": 30,
            "settings": {
                "theme": "light",
                "notifications": true
            }
        }
        """;

        // Incoming JSON with changes
        var incomingJson = """
        {
            "name": "Alice",
            "age": 31,
            "settings": {
                "theme": "dark",
                "notifications": true
            }
        }
        """;

        var baseline = JsonNode.Parse(baselineJson);
        var incoming = JsonNode.Parse(incomingJson);

        List<string> ignores = new() { "name" };

        // Act
        var diff = JsonDiffer.Diff(baseline, incoming, ignores);

        // Expected diff
        var expectedDiffJson = """
        {
            "name": "Alice",
            "age": 31,
            "settings": {
                "theme": "dark"
            }
        }
        """;

        var expected = JsonNode.Parse(expectedDiffJson);

        // Assert
        Assert.Equal(
            expected.ToJsonString(),
            diff.ToJsonString()
        );
    }

    [Fact]
    public void Diff_ReturnsEmptyObject_WhenNoChanges()
    {
        var json = """
        {
            "a": 1,
            "b": 2
        }
        """;

        var baseline = JsonNode.Parse(json);
        var incoming = JsonNode.Parse(json);

        var diff = JsonDiffer.Diff(baseline, incoming);

        Assert.Equal("{}", diff.ToJsonString());
    }

    [Fact]
    public void Diff_DetectsAddedFields()
    {
        var baselineJson = """
        {
            "a": 1
        }
        """;

        var incomingJson = """
        {
            "a": 1,
            "b": 2
        }
        """;

        var baseline = JsonNode.Parse(baselineJson);
        var incoming = JsonNode.Parse(incomingJson);

        var diff = JsonDiffer.Diff(baseline, incoming);

        var expected = JsonNode.Parse("""{ "b": 2 }""");

        Assert.Equal(expected.ToJsonString(), diff.ToJsonString());
    }
}
