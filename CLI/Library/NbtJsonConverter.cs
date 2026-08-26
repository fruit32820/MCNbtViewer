using SharpNBT;
using System.Text.Json;

namespace CLI.Library
{
    public static class NbtJsonConverter
    {
        public static JsonDocument NbtToJson(Tag nbtRoot)
        {
            var writerOptions = new JsonWriterOptions { Indented = true };
            using var stream = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(stream, writerOptions);

            WriteNbtAnnotated(jsonWriter, nbtRoot);

            jsonWriter.Flush();
            stream.Position = 0;
            return JsonDocument.Parse(stream);
        }

        public static CompoundTag JsonToNbt(JsonDocument jsonDoc)
        {
            var rootEl = jsonDoc.RootElement;
            var rootTag = ReadAnnotatedElement(rootEl, "");
            if (rootTag is not CompoundTag rootCompound)
                throw new InvalidDataException("Annotated‑JSON root node must be a compound");
            return rootCompound;
        }

        #region NBT → Annotated JSON (System.Text.Json Utf8JsonWriter)
        private static void WriteNbtAnnotated(Utf8JsonWriter jw, Tag tag)
        {
            jw.WriteStartObject();
            jw.WriteString("$type", tag.Type.ToString().ToLowerInvariant());

            switch (tag)
            {
                case CompoundTag compound:
                    {
                        jw.WritePropertyName("$value");
                        jw.WriteStartObject();
                        foreach (var child in compound)
                        {
                            jw.WritePropertyName(child.Name ?? string.Empty);
                            WriteNbtAnnotated(jw, child);
                        }
                        jw.WriteEndObject();
                        break;
                    }

                case ListTag list:
                    {
                        jw.WriteString("$listType", list.ChildType.ToString().ToLowerInvariant());
                        jw.WritePropertyName("$value");
                        jw.WriteStartArray();
                        foreach (var item in list)
                        {
                            WriteNbtAnnotated(jw, item);
                        }
                        jw.WriteEndArray();
                        break;
                    }

                case ByteTag t:
                    jw.WriteNumber("$value", t.Value);
                    break;
                case ShortTag t:
                    jw.WriteNumber("$value", t.Value);
                    break;
                case IntTag t:
                    jw.WriteNumber("$value", t.Value);
                    break;
                case LongTag t:
                    jw.WriteNumber("$value", t.Value);
                    break;
                case FloatTag t:
                    jw.WriteNumber("$value", t.Value);
                    break;
                case DoubleTag t:
                    jw.WriteNumber("$value", t.Value);
                    break;
                case StringTag t:
                    jw.WriteString("$value", t.Value);
                    break;

                case ByteArrayTag tByteArr:
                    {
                        jw.WritePropertyName("$value");
                        jw.WriteStartArray();
                        foreach (var b in tByteArr.ToArray())
                            jw.WriteNumberValue(b);
                        jw.WriteEndArray();
                        break;
                    }
                case IntArrayTag tIntArr:
                    {
                        jw.WritePropertyName("$value");
                        jw.WriteStartArray();
                        foreach (var v in tIntArr.ToArray())
                            jw.WriteNumberValue(v);
                        jw.WriteEndArray();
                        break;
                    }
                case LongArrayTag tLongArr:
                    {
                        jw.WritePropertyName("$value");
                        jw.WriteStartArray();
                        foreach (var v in tLongArr.ToArray())
                            jw.WriteNumberValue(v);
                        jw.WriteEndArray();
                        break;
                    }

                default:
                    jw.WriteNull("$value");
                    break;
            }
            jw.WriteEndObject();
        }
        #endregion

        #region Annotated JSON → NBT
        private static Tag ReadAnnotatedElement(JsonElement el, string tagName)
        {
            if (el.ValueKind != JsonValueKind.Object)
                throw new InvalidDataException($"Tag [{tagName}] must be an object with $type property");

            if (!el.TryGetProperty("$type", out var typeProp))
                throw new InvalidDataException($"Tag [{tagName}] is missing required $type field");

            var typeStr = typeProp.GetString()!;
            TagType tagType = Enum.Parse<TagType>(typeStr, true);

            switch (tagType)
            {
                case TagType.Byte:
                    return new ByteTag(tagName, el.GetProperty("$value").GetSByte());
                case TagType.Short:
                    return new ShortTag(tagName, el.GetProperty("$value").GetInt16());
                case TagType.Int:
                    return new IntTag(tagName, el.GetProperty("$value").GetInt32());
                case TagType.Long:
                    return new LongTag(tagName, el.GetProperty("$value").GetInt64());
                case TagType.Float:
                    return new FloatTag(tagName, el.GetProperty("$value").GetSingle());
                case TagType.Double:
                    return new DoubleTag(tagName, el.GetProperty("$value").GetDouble());
                case TagType.String:
                    return new StringTag(tagName, el.GetProperty("$value").GetString()!);

                case TagType.ByteArray:
                    {
                        var arrEl = el.GetProperty("$value");
                        var list = arrEl.EnumerateArray().Select(x => x.GetByte()).ToArray();
                        return new ByteArrayTag(tagName, list);
                    }
                case TagType.IntArray:
                    {
                        var arrEl = el.GetProperty("$value");
                        var list = arrEl.EnumerateArray().Select(x => x.GetInt32()).ToArray();
                        return new IntArrayTag(tagName, list);
                    }
                case TagType.LongArray:
                    {
                        var arrEl = el.GetProperty("$value");
                        var list = arrEl.EnumerateArray().Select(x => x.GetInt64()).ToArray();
                        return new LongArrayTag(tagName, list);
                    }

                case TagType.List:
                    {
                        var listTypeStr = el.GetProperty("$listType").GetString()!;
                        TagType innerType = Enum.Parse<TagType>(listTypeStr, true);
                        var listTag = new ListTag(tagName, innerType);
                        var arrEl = el.GetProperty("$value");
                        foreach (var itemEl in arrEl.EnumerateArray())
                        {
                            listTag.Add(ReadAnnotatedElement(itemEl, ""));
                        }
                        return listTag;
                    }

                case TagType.Compound:
                    {
                        var cmp = new CompoundTag(tagName);
                        var valueObj = el.GetProperty("$value");
                        foreach (var prop in valueObj.EnumerateObject())
                        {
                            cmp.Add(ReadAnnotatedElement(prop.Value, prop.Name));
                        }
                        return cmp;
                    }

                default:
                    throw new NotSupportedException($"Unsupported tag type: {tagType}");
            }
        }
        #endregion

        public static string NbtToJsonString(Tag nbtRoot)
        {
            using var doc = NbtToJson(nbtRoot);
            return doc.RootElement.GetRawText();
        }

        public static CompoundTag JsonStringToNbt(string jsonText)
        {
            using var doc = JsonDocument.Parse(jsonText);
            return JsonToNbt(doc);
        }

        /// <summary>
        /// Write NBT directly to stream (prefer FileStream, no intermediate string, UTF‑8 no‑BOM)
        /// </summary>
        /// <param name="nbtRoot">Root NBT tag</param>
        /// <param name="stream">Output stream, e.g. FileStream</param>
        public static void NbtToStream(Tag nbtRoot, Stream stream)
        {
            var writerOptions = new JsonWriterOptions
            {
                Indented = true
            };
            using var jsonWriter = new Utf8JsonWriter(stream, writerOptions);
            WriteNbtAnnotated(jsonWriter, nbtRoot);
            jsonWriter.Flush();
        }
    }
}
