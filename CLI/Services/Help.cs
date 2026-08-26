using CLI.Library;

namespace CLI.Services
{
    public class Help()
    {
        /// <summary>显示帮助信息，FuncName 留空显示全局帮助</summary>
        /// <param name="FuncName">需要显示帮助的方法</param>
        public static void Show(string FuncName="")
        {
            string id=FuncName;
            if (FuncName == "")
                id="_general";
            if (HelpInfo.Info.TryGetValue(id, out var text))
            {
                Console.WriteLine(text);
            }
            else
            {
                Console.WriteLine($"错误：未知命令 {id}");
            }
        }
    }
}