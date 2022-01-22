using System.Windows;

namespace Minecraft_Memory_Bypass
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static string[] Com_Line_Args;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Com_Line_Args = e.Args;
        }
    }
}
