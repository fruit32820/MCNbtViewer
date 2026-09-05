using SharpNBT;
using CLI.Library;

namespace McNbtViewerCLI.Services
{
    internal class Read
    {
        public static void Run(string inFile, string outFile, McEdition edition)
        {
            if (!Path.Exists(inFile))
            {
                Console.WriteLine("错误 (来自 read) :");
                Console.WriteLine("路径: " + inFile + " 不存在! ");
                return;
            }
            if (File.Exists(outFile))
            {
                Console.Write($"文件 {outFile} 已存在，是否覆盖？(Y/N): ");
                var key = Console.ReadKey(true);
                Console.WriteLine();
                if (char.ToUpper(key.KeyChar) != 'Y')
                {
                    Console.WriteLine("已取消");
                    return;
                }
            }

            string? outDir = Path.GetDirectoryName(outFile);
            if (!string.IsNullOrEmpty(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            using var fs = new FileStream(outFile, FileMode.Create, FileAccess.Write);
            CompoundTag nbtData = (CompoundTag)NbtIo.ReadNbtFile(inFile, edition);
            NbtJsonConverter.NbtToStream(nbtData, fs);
        }
    }
}
