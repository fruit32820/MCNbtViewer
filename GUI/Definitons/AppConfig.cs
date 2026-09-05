using System.IO;
using System.Text.Json;

namespace McNbtViewerGUI;

public class AppConfig
{
    public string SelectedLanguage { get; set; } = "en‑US";
    public string CliExecutablePath { get; set; } = "MCNbtCli.exe";
    public string DefaultNbtEdition { get; set; } = "java";

    private static string GetConfigFilePath()
    {
        var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        var dataDir = Path.Combine(exeDir, "data");
        Directory.CreateDirectory(dataDir);
        return Path.Combine(dataDir, "config.json");
    }

    public static AppConfig LoadFromFile()
    {
        var path = GetConfigFilePath();

        if (!File.Exists(path))
        {
            // 无配置文件：生成默认文件
            var defaultCfg = new AppConfig();
            defaultCfg.SaveToFile();
            return defaultCfg;
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            // 文件损坏，返回默认配置（不覆盖原有坏文件）
            return new AppConfig();
        }
    }

    public void SaveToFile()
    {
        var path = GetConfigFilePath();
        var jsonOption = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(this, jsonOption);
        File.WriteAllText(path, json);
    }
}
