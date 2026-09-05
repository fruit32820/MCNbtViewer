namespace McNbtViewerGUI.Converters;

class OtherConverters
{
    public static string VersionConvert(string? ver,string lang="zh-CN")
    {
        string res = ver switch
        {
            "Java版" => "java",
            "基岩版" => "bedrock",
            _ => "java",
        };
        return res;
    }
}
