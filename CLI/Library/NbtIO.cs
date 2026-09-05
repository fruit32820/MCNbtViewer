using SharpNBT;
using System.Buffers.Binary;

namespace McNbtViewerCLI.Library
{
    public enum McEdition
    {
        Java,
        Bedrock
    }

    public class NbtIo
    {
        public static Tag ReadNbtFile(string filePath, McEdition edition)
        {
            if (edition == McEdition.Java)
            {
                return NbtFile.Read(filePath, FormatOptions.Java);
            }

            using var fs = File.OpenRead(filePath);
            fs.Position = 8; // Skip Bedrock 8bytes header
            using var tr = new TagReader(fs, FormatOptions.LittleEndian);
            return tr.ReadTag();
        }

        public static void WriteNbtFile(string filePath, CompoundTag rootTag, McEdition edition)
        {
            if (edition == McEdition.Java)
            {
                NbtFile.Write(filePath, rootTag, FormatOptions.Java);
                return;
            }

            using var fs = File.Create(filePath);
            WriteBedrockToStream(fs, rootTag);
        }

        private static void WriteBedrockToStream(Stream stream, CompoundTag rootTag)
        {
            using var ms = new MemoryStream();
            // Add missing 3bytes
            ms.WriteByte(0x0A);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            using var tw = new TagWriter(ms, FormatOptions.LittleEndian);
            // rootTag is root Compound
            tw.WriteTag(rootTag);

            // payload is full NBT ontology
            var payload = ms.GetBuffer().AsSpan(0, (int)ms.Length);

            Span<byte> header = stackalloc byte[8];
            BinaryPrimitives.WriteInt32LittleEndian(header[..4], 10);
            BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(4, 4), (uint)payload.Length);

            stream.Write(header);
            stream.Write(payload);
        }
    }
}
