namespace CLI.Library;

public static class Helper
{
    public static bool StringCompare(string s1, string s2)
        => s1.Equals(s2, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Parse command‑line arguments
    /// First non‑-- argument stored in _func (optional); remaining positional arguments into _other
    /// --key value → key:[value]; if no value given, defaults to "1"
    /// </summary>
    /// <param name="args">Main entry point args</param>
    /// <param name="SupportParam">Whitelist of allowed -- parameters, null to skip validation</param>
    /// <param name="RequiredParam">Set of mandatory -- parameters, null to skip validation</param>
    /// <returns>_func = sub‑command; _other = remaining positional arguments; other keys for -- parameters</returns>
    /// <exception cref="ArgumentException">Unsupported parameter / missing mandatory -- parameter</exception>
    public static Dictionary<string, string[]> ReadCmdParam(
        string[] args,
        string[]? SupportParam,
        IEnumerable<string>? RequiredParam)
    {
        Dictionary<string, string[]> res = new(StringComparer.OrdinalIgnoreCase);
        List<string> others = [];
        bool firstPosConsumed = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg.StartsWith("--"))
            {
                string paramName = arg[2..];
                string val = "1";

                if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                {
                    val = args[i + 1];
                    i++;
                }

                if (SupportParam != null && !SupportParam.Contains(paramName, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Unsupported command‑line parameter: --{paramName}");
                }

                if (res.TryGetValue(paramName, out string[]? value))
                {
                    res[paramName] = [.. value, val];
                }
                else
                {
                    res[paramName] = [val];
                }
            }
            else
            {
                if (!firstPosConsumed)
                {
                    res["_func"] = [arg];
                    firstPosConsumed = true;
                }
                else
                {
                    others.Add(arg);
                }
            }
        }

        if (others.Count > 0)
        {
            res["_other"] = [.. others];
        }

        // Validate mandatory -- parameters
        if (RequiredParam != null)
        {
            foreach (var req in RequiredParam)
            {
                if (!res.ContainsKey(req))
                {
                    throw new ArgumentException($"Missing required command‑line parameter: --{req}");
                }
            }
        }

        return res;
    }
}
