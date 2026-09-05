using CLI.Library;
using SharpNBT;
using System.Text.Json;

namespace McNbtViewerCLI.Services
{
    internal class Write
    {
        /// <summary>
        /// JSON文件写入NBT存档
        /// </summary>
        /// <param name="inJsonFile">输入json路径</param>
        /// <param name="outDatFile">输出level.dat路径</param>
        /// <param name="edition">Java / Bedrock</param>
        public static void Run(string inJsonFile, string outDatFile, McEdition edition)
        {
            if (!Path.Exists(inJsonFile))
            {
                Console.WriteLine("错误 (来自 write) :");
                Console.WriteLine("路径: " + inJsonFile + " 不存在! ");
                return;
            }
            if (File.Exists(outDatFile))
            {
                Console.Write($"文件 {outDatFile} 已存在，是否覆盖？(Y/N): ");
                var key = Console.ReadKey(true);
                Console.WriteLine();
                if (char.ToUpper(key.KeyChar) != 'Y')
                {
                    Console.WriteLine("已取消");
                    return;
                }
            }

            string? outDir = Path.GetDirectoryName(outDatFile);
            if (!string.IsNullOrEmpty(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            // 读取JSON → 转为CompoundTag
            string jsonText = File.ReadAllText(inJsonFile);
            using var doc = JsonDocument.Parse(jsonText);
            CompoundTag rootTag = NbtJsonConverter.JsonToNbt(doc);

            // 交给IO层完成：头部、大小端、压缩、Data节点包装全部下沉到NbtIo.WriteNbtFile
            NbtIo.WriteNbtFile(outDatFile, rootTag, edition);

            Console.WriteLine($"写入完成: {outDatFile}");
        }
    }
}
