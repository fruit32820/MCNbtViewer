using System.Collections.Generic;

namespace McNbtViewerCLI.Library
{
    public class HelpInfo
    {
        public static readonly Dictionary<string, string> Info = new(StringComparer.OrdinalIgnoreCase)
        {
            ["_general"] =
                "Help\n\n" +
                "Usage:\n" +
                "  cli <command> <input> --target <output> [options]\n\n" +
                "Available commands:\n" +
                "  read\n" +
                "    Read NBT file and export to JSON\n" +
                "    Options:\n" +
                "      --edition <java|bedrock>    Default: java\n" +
                "  write\n" +
                "    Read JSON file and export to NBT\n" +
                "    Options:\n" +
                "      --edition <java|bedrock>    Default: java\n" +
                "  help\n" +
                "    Show this help message\n\n" +
                "Run cli help <command> for detailed command-specific help",

            ["read"] =
                "Help: read\n" +
                "Read NBT file and export to JSON\n\n" +
                "Usage:\n" +
                "  cli read <input-nbt> --target <output-json> [--edition <type>]\n\n" +
                "Arguments:\n" +
                "  <input-nbt>      Path to source NBT file\n" +
                "  --target          Path for output JSON file\n" +
                "  [--edition]       java / bedrock, default java\n\n" +
                "Examples:\n" +
                "  cli read test.nbt --target out.json\n" +
                "  cli read level.dat --target level.json --edition Bedrock",

            ["write"] =
                "Help: write\n" +
                "Read JSON file and export to NBT\n\n" +
                "Usage:\n" +
                "  cli write <input-json> --target <output-nbt> [--edition <type>]\n\n" +
                "Arguments:\n" +
                "  <input-json>     Path to source JSON file\n" +
                "  --target          Path for output NBT file\n" +
                "  [--edition]       java / bedrock, default java\n\n" +
                "Examples:\n" +
                "  cli write out.json --target test.nbt\n" +
                "  cli write level.json --target level.dat --edition Bedrock"
        };
    }
}
