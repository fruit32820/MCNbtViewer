using CLI.Library;
using CLI.Services;
using static CLI.Library.Helper;

try
{
    // SupportParam: target, edition；解析器层不设置全局 RequiredParam
    var param = ReadCmdParam(args, ["target", "edition"], null);

    // Get sub‑command _func
    if (!param.TryGetValue("_func", out var funcArr))
    {
        Help.Show();
        return;
    }
    string cmd = funcArr[0];

    // help sub‑command
    if (StringCompare(cmd, "help"))
    {
        if (param.TryGetValue("_other", out var helpOther) && helpOther.Length >= 1)
        {
            Help.Show(helpOther[0]);
        }
        else
        {
            Help.Show();
        }
        return;
    }

    // read sub‑command
    if (StringCompare(cmd, "read"))
    {
        McEdition edition = McEdition.Java;
        if (param.TryGetValue("edition", out var edArr))
        {
            if (StringCompare(edArr[0], "Bedrock"))
                edition = McEdition.Bedrock;
        }

        if (!param.TryGetValue("target", out var targetArr))
        {
            Console.Error.WriteLine("Error: --target parameter is required");
            return;
        }
        string target = targetArr[0];

        if (!param.TryGetValue("_other", out var posArr) || posArr.Length < 1)
        {
            Console.Error.WriteLine("Error: Missing input file positional argument");
            return;
        }
        string inFile = posArr[0];

        Read.Run(inFile, target, edition);
    }

    // write sub‑command
    if (StringCompare(cmd, "write"))
    {
        if (!param.TryGetValue("_other", out var posArr) || posArr.Length < 1)
        {
            Console.Error.WriteLine("Error: Input JSON file path is required");
            return;
        }
        string inJson = posArr[0];

        if (!param.TryGetValue("target", out var targetArr))
        {
            Console.Error.WriteLine("Error: --target output file must be specified");
            return;
        }

        McEdition edition = McEdition.Java;
        if (param.TryGetValue("edition", out var edArr))
        {
            if (StringCompare(edArr[0], "Bedrock"))
                edition = McEdition.Bedrock;
        }
        foreach (string outDat in targetArr)
            Write.Run(inJson, outDat, edition);
    }
}
catch (ArgumentException ex)
{
    // 捕获 ReadCmdParam 抛出：不支持参数 / 缺失必选--参数
    Console.Error.WriteLine(ex.Message);
    Environment.Exit(1);
}
