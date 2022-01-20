using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Minecraft_Memory_Bypass
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 通过进程名称得到进程ID
        /// </summary>
        /// <param name="Name">进程名称</param>
        /// <returns>进程PID</returns>
        [DllImport("Memory_Bypass.dll", CallingConvention = CallingConvention.Winapi)]
        public extern static int GetProcId(string Name);
        /// <summary>
        /// 通过进程名称加进程ID得到进程基地址
        /// </summary>
        /// <param name="nID">进程PID</param>
        /// <param name="Name">进程名称</param>
        /// <returns></returns>
        [DllImport("Memory_Bypass.dll", CallingConvention = CallingConvention.Winapi)]
        public extern static long GetModuleBaseAddress(long nID, string Name);
        /// <summary>
        /// 通过进程ID得到窗口句柄
        /// </summary>
        /// <param name="nID">进程PID</param>
        /// <returns></returns>
        [DllImport("Memory_Bypass.dll", CallingConvention = CallingConvention.Winapi)]
        public extern static long GetProcessHandle(int nID);
        /// <summary>
        /// 写入内存
        /// </summary>
        /// <param name="Handle">窗口句柄</param>
        /// <param name="Address">内存地址</param>
        /// <param name="Buffer">写入内容</param>
        /// <returns>是否写入成功</returns>
        [DllImport("Memory_Bypass.dll", CallingConvention = CallingConvention.Winapi)]
        public extern static bool WriteMemory(long Handle, long Address, byte[] Buffer);

        /// <summary>
        /// 进程名称
        /// </summary>
        private static string Program_Name;
        /// <summary>
        /// 偏移地址
        /// </summary>
        private static long Offset_Address;
        /// <summary>
        /// 写入内容
        /// </summary>
        private static int Write_Content;

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }

        private void 关闭_Click(object sender, RoutedEventArgs e)
        {
            名称.Text = Properties.Settings.Default.Program_Name;
            地址.Text = Properties.Settings.Default.Offset_Address;
            内容.Text = Properties.Settings.Default.Write_Content;
            提示.Visibility = Visibility.Collapsed;
            BeginStoryboard((Storyboard)FindResource("设置开启"));
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/xingchuanzhen/Minecraft_Bypass_the_program");
        }

        private void 最小化_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Assignment(string str)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                日志.Text += "\n[" + DateTime.Now.ToLongTimeString().ToString() + "]: " + str;
                框.ScrollToVerticalOffset(框.ExtentHeight);
            }));
        }

        private void Update_Data()
        {
            Program_Name = Properties.Settings.Default.Program_Name;

            string hexString = Properties.Settings.Default.Offset_Address.Replace("0x", "");

            Offset_Address = Int64.Parse(hexString, System.Globalization.NumberStyles.HexNumber);

            hexString = Properties.Settings.Default.Write_Content.Replace("0x", "");

            Write_Content = int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }

        public MainWindow()
        {
            InitializeComponent();
            设置.Visibility = Visibility.Collapsed;
            日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: " + "程序启动";
            string Inf = Operate.RunCmd(@"CD C:\Program Files\WindowsApps & C: & DIR");
            if (Inf.Contains("Microsoft.MinecraftUWP"))
            {
                Assignment("游戏版本：" + Operate.Substring(Inf, "Microsoft.MinecraftUWP_", "_"));
            }
            else
            {
                Assignment("请先安装 MinecraftUWP!");
                启动.IsEnabled = false;
            }
        }

        void Start_in_the_background(object sender, DoWorkEventArgs e)
        {
            Update_Data();
            Assignment("启动Minecraft");
            Process.Start("minecraft:");
            int PID = GetProcId(Program_Name);
            Assignment("进程PID ：" + PID);
            long Address = GetModuleBaseAddress(PID, Program_Name);
            Assignment("进程基址：0x" + Address.ToString("X16"));
            long Handle = GetProcessHandle(PID);
            Assignment("窗口句柄：0x" + Handle.ToString("X16"));
            byte[] Buffer = new byte[] { (byte)Write_Content };
            Assignment("偏移地址：0x" + Offset_Address.ToString("X16"));
            Assignment("写入内存：0x" + Write_Content.ToString("X16"));
            bool Result = WriteMemory(Handle, Address + Offset_Address, Buffer);
            Assignment("执行结果：" + Result.ToString().Replace("True", "写入成功!").Replace("False", "写入失败!"));
        }

        private void 启动_Click(object sender, RoutedEventArgs e)
        {
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.DoWork += new DoWorkEventHandler(Start_in_the_background);
                bw.RunWorkerAsync();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if(名称.Text == "" || 地址.Text == "" || 内容.Text == "")
            {
               提示.Visibility = Visibility.Visible;
            }
            else
            {
                BeginStoryboard((Storyboard)FindResource("设置关闭"));
                Properties.Settings.Default.Program_Name = 名称.Text;
                Properties.Settings.Default.Offset_Address = 地址.Text;
                Properties.Settings.Default.Write_Content = 内容.Text;
                Properties.Settings.Default.Save();
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            BeginStoryboard((Storyboard)FindResource("设置关闭"));
        }
    }
}
