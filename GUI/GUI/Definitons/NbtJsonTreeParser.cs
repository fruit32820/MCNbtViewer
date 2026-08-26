using GUI.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GUI;

public static class NbtJsonTreeParser
{
    /// <summary>从JSON字符串构建树节点</summary>
    public static JsonTreeItem Parse(string jsonText, string rootName = "root")
    {
        using var doc = JsonDocument.Parse(jsonText);
        return ParseElement(doc.RootElement, rootName);
    }

    private static JsonTreeItem ParseElement(in JsonElement elem, string nodeName)
    {
        var item = new JsonTreeItem { Name = nodeName };

        if (elem.ValueKind != JsonValueKind.Object)
            return item;

        // 读取 $type
        if (elem.TryGetProperty("$type", out var typeEl))
        {
            item.Type = typeEl.GetString() ?? string.Empty;
        }
        // 读取 $listType (仅list)
        if (elem.TryGetProperty("$listType", out var listTypeEl))
        {
            item.ListType = listTypeEl.GetString();
        }

        if (!elem.TryGetProperty("$value", out var valueEl))
            return item;

        switch (item.Type)
        {
            case "compound":
                {
                    // compound $value 是对象：遍历所有子属性作为子节点
                    if (valueEl.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in valueEl.EnumerateObject())
                        {
                            var child = ParseElement(prop.Value, prop.Name);
                            item.Children.Add(child);
                        }
                    }
                    break;
                }
            case "list":
                {
                    // list $value 是数组
                    if (valueEl.ValueKind == JsonValueKind.Array)
                    {
                        // 设置后缀
                        item.NodeSuffix = $": {valueEl.GetArrayLength()} 条目";

                        foreach (var arrItem in valueEl.EnumerateArray())
                        {
                            var child = ParseElement(arrItem, "");
                            item.Children.Add(child);
                        }
                    }
                    break;
                }
            case "bytearray":
            case "intarray":
            case "longarray":
                {
                    // 数组标签，把json数组转为object[]存到RawValue
                    if (valueEl.ValueKind == JsonValueKind.Array)
                    {
                        var list = new List<object>();
                        foreach (var arrItem in valueEl.EnumerateArray())
                        {
                            list.Add(GetJsonElementValue(arrItem));
                        }
                        item.RawValue = list.ToArray();
                        item.NodeSuffix = $": {list.Count} 元素";
                    }
                    break;
                }
            default:
                {
                    // byte / short / int / long / float / double / string 基础类型
                    item.RawValue = GetJsonElementValue(valueEl);
                    item.NodeSuffix = item.RawValue?.ToString()!;
                    break;
                }
        }
        return item;
    }

    /// <summary>把JsonElement取出对应的CLR基础对象</summary>
    private static object? GetJsonElementValue(in JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number when el.TryGetByte(out var b) => b,
            JsonValueKind.Number when el.TryGetInt16(out var s) => s,
            JsonValueKind.Number when el.TryGetInt32(out var i) => i,
            JsonValueKind.Number when el.TryGetInt64(out var l) => l,
            JsonValueKind.Number when el.TryGetSingle(out var f) => f,
            JsonValueKind.Number when el.TryGetDouble(out var d) => d,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => el.GetRawText()
        };
    }

    #region 【可选】把JsonTreeItem再写回NBT‑JSON字符串（编辑后导出）
    public static string WriteBack(JsonTreeItem rootItem)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true });
        WriteNode(writer, rootItem);
        writer.Flush();
        return System.Text.Encoding.UTF8.GetString(ms.ToArray());
    }

    private static void WriteNode(Utf8JsonWriter writer, JsonTreeItem item)
    {
        writer.WriteStartObject();
        writer.WriteString("$type", item.Type);
        if (!string.IsNullOrEmpty(item.ListType))
        {
            writer.WriteString("$listType", item.ListType);
        }
        writer.WritePropertyName("$value");

        if (item.Type == "compound")
        {
            writer.WriteStartObject();
            foreach (var child in item.Children)
            {
                writer.WritePropertyName(child.Name);
                WriteNode(writer, child);
            }
            writer.WriteEndObject();
        }
        else if (item.Type == "list")
        {
            writer.WriteStartArray();
            foreach (var child in item.Children)
            {
                WriteNode(writer, child);
            }
            writer.WriteEndArray();
        }
        else if (item.Type is "bytearray" or "intarray" or "longarray")
        {
            writer.WriteStartArray();
            if (item.RawValue is object[] arr)
            {
                foreach (var v in arr)
                {
                    WriteJsonValue(writer, v);
                }
            }
            writer.WriteEndArray();
        }
        else
        {
            WriteJsonValue(writer, item.RawValue);
        }
        writer.WriteEndObject();
    }

    private static void WriteJsonValue(Utf8JsonWriter writer, object? value)
    {
        if (value == null) { writer.WriteNullValue(); return; }
        switch (value)
        {
            case byte b: writer.WriteNumberValue(b); break;
            case short s: writer.WriteNumberValue(s); break;
            case int i: writer.WriteNumberValue(i); break;
            case long l: writer.WriteNumberValue(l); break;
            case float f: writer.WriteNumberValue(f); break;
            case double d: writer.WriteNumberValue(d); break;
            case string str: writer.WriteStringValue(str); break;
            default: writer.WriteStringValue(value.ToString()); break;
        }
    }
    #endregion
}
