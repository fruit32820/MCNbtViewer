using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GUI;

public static class CliHelper
{
    // 改成你CLI的完整路径
    private const string CliExePath = @"C:\Users\123\source\repos\MCNbtViewer\CLI\bin\Publish\win-x64\CLI.exe";

    /// <summary>read：nbt文件 → json字符串；失败返回null</summary>
    public static async Task<string?> ReadNbtToJson(string nbtFilePath, string edition)
    {
        string tempJson = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = CliExePath,
                Arguments = $"read \"{nbtFilePath}\" --target \"{tempJson}\" --edition {edition}",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Debug.Print($"参数：read \"{nbtFilePath}\" --target \"{tempJson}\" --edition {edition}\n");
            using var proc = Process.Start(psi);
            string stderr = await proc!.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
                return null;

            return await File.ReadAllTextAsync(tempJson);
        }
        finally
        {
            if (File.Exists(tempJson)) File.Delete(tempJson);
        }
    }

    /// <summary>write：json字符串 → 输出nbt文件到targetPath；成功true，失败false</summary>
    public static async Task<bool> WriteJsonToNbt(string targetNbtPath, string jsonContent, string edition)
    {
        string tempJson = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        try
        {
            await File.WriteAllTextAsync(tempJson, jsonContent);

            var psi = new ProcessStartInfo
            {
                FileName = CliExePath,
                Arguments = $"write \"{tempJson}\" --target \"{targetNbtPath}\" --edition {edition}",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            string stderr = await proc!.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            return proc.ExitCode == 0;
        }
        finally
        {
            if (File.Exists(tempJson)) File.Delete(tempJson);
        }
    }
}
