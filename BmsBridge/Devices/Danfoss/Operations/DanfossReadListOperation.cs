using System.Text.Json.Nodes;
using System.Xml.Linq;

public sealed class DanfossReadListOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_list";

    public DanfossReadListOperation(
            Uri endpoint,
            string tableAddress,
            string nodeType,
            string node,
            string combo,
            string index,
            string bpIndex,
            string argument1,
            string configType,
            string useParent,
            string isConfigure,
            string sType,
            string group,
            string subGroup,
            string page,
            string oldConfigType,
            ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        var attributes = new List<XAttribute>();

        attributes.Add(new XAttribute("nodetype", nodeType));
        attributes.Add(new XAttribute("tableaddress", tableAddress));
        attributes.Add(new XAttribute("node", node));
        attributes.Add(new XAttribute("combo", combo));
        attributes.Add(new XAttribute("index", index));
        attributes.Add(new XAttribute("bpidx", bpIndex));
        attributes.Add(new XAttribute("arg1", argument1));
        attributes.Add(new XAttribute("useparent", useParent));
        attributes.Add(new XAttribute("configuretype", configType));
        attributes.Add(new XAttribute("isconfigure", isConfigure));
        attributes.Add(new XAttribute("stype", sType));
        attributes.Add(new XAttribute("subgroup", subGroup));
        attributes.Add(new XAttribute("page", page));
        attributes.Add(new XAttribute("old_cfgtype", oldConfigType));

        _extraAttributes = attributes;
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
