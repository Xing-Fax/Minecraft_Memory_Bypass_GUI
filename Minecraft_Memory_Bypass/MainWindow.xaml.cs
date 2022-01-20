using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

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
        /// <returns>返回进程PID</returns>
        [DllImport("Memory_Bypass.dll", CallingConvention = CallingConvention.Winapi)]
        public extern static int GetProcId(string Name);
        /// <summary>
        /// 通过进程名称加进程ID得到进程基地址
        /// </summary>
        /// <param name="nID">进程PID</param>
        /// <param name="Name">进程名称</param>
        /// <returns>返回基地址(十进制)</returns>
        [DllImport("Memory_Bypass.dll", CallingConvention = CallingConvention.Winapi)]
        public extern static long GetModuleBaseAddress(long nID, string Name);
        /// <summary>
        /// 通过进程ID得到窗口句柄
        /// </summary>
        /// <param name="nID">进程PID</param>
        /// <returns>返回句柄(十进制)</returns>
        [DllImport("Memory_Bypass.dll", CallingConvention = CallingConvention.Winapi)]
        public extern static long GetProcessHandle(int nID);
        /// <summary>
        /// 写入内存
        /// </summary>
        /// <param name="Handle">窗口句柄</param>
        /// <param name="Address">内存地址</param>
        /// <param name="Buffer">写入内容</param>
        /// <param name="nSize">缓冲区大小</param>
        /// <returns>是否写入成功</returns>
        [DllImport("Memory_Bypass.dll", CallingConvention = CallingConvention.Winapi)]
        public extern static bool WriteMemory(long Handle, long Address, byte[] Buffer,int nSize);

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

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/xingchuanzhen/Minecraft_Bypass_the_program");
        }

        private void 最小化_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            BeginStoryboard((Storyboard)FindResource("设置关闭"));
        }

        private void Assignment(string str)
        {
            Dispatcher.Invoke(new Action(delegate//同步线程
            {
                日志.Text += "\n[" + DateTime.Now.ToLongTimeString().ToString() + "]: " + str;
                框.ScrollToVerticalOffset(框.ExtentHeight);
            }));
        }

        private void 关闭_Click(object sender, RoutedEventArgs e)
        {
            名称.Text = Properties.Settings.Default.Program_Name;
            地址.Text = Properties.Settings.Default.Offset_Address;
            内容.Text = Properties.Settings.Default.Write_Content;
            提示.Visibility = Visibility.Collapsed;
            BeginStoryboard((Storyboard)FindResource("设置开启"));
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        private void Update_Data()
        {
            //得到程序名称
            Program_Name = Properties.Settings.Default.Program_Name;
            //得到偏移量
            string hexString = Properties.Settings.Default.Offset_Address.Replace("0x", "");
            //将偏移量转换为十进制
            Offset_Address = Int64.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
            //得到写入内容
            hexString = Properties.Settings.Default.Write_Content.Replace("0x", "");
            //将写入内容转换为十进制
            Write_Content = int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }

        public MainWindow()
        {
            InitializeComponent();
            //隐藏设置界面
            设置.Visibility = Visibility.Collapsed;
            //打印日志
            日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: " + "程序启动";
            //获取计算机UWP程序安装列表
            string Inf = Operate.RunCmd(@"CD C:\Program Files\WindowsApps & C: & DIR");
            //判断是否安装了MinecraftUWP
            if (Inf.Contains("Microsoft.MinecraftUWP"))
            {
                //得到游戏版本
                Assignment("游戏版本：" + Operate.Substring(Inf, "Microsoft.MinecraftUWP_", "_"));
            }
            else
            {
                //输出错误信息
                Assignment("请先安装 MinecraftUWP!");
                启动.IsEnabled = false;
            }
        }

        void Start_in_the_background(object sender, DoWorkEventArgs e)
        {
            try
            {
                //刷新用户输入数据
                Update_Data();
                Assignment("启动进程：MinecraftUWP");
                Process.Start("minecraft:");                                            //启动MinecraftUWP
                int PID = GetProcId(Program_Name);                                      //得到进程PID
                Assignment("进程PID ：" + PID);
                long Address = GetModuleBaseAddress(PID, Program_Name);                 //得到进程基地址(十进制)
                Assignment("进程基址：0x" + Address.ToString("X"));
                long Handle = GetProcessHandle(PID);                                    //得到窗口句柄
                Assignment("窗口句柄：0x" + Handle.ToString("X"));
                byte[] Buffer = new byte[] { (byte)Write_Content };                     //设置缓冲区
                Assignment("偏移地址：0x" + Offset_Address.ToString("X"));
                Assignment("写入内存：0x" + Write_Content.ToString("X"));
                bool Result = WriteMemory(Handle, Address + Offset_Address, Buffer,1);  //将数据写入内存
                Assignment("执行结果：" + Result.ToString().Replace("True", "写入成功!").Replace("False", "写入失败!"));
            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() =>
                        {
                            Assignment("发生错误：未知错误");
                            MessageBox.Show(ex.ToString(), "错误！");
                        }));
                }).Start();
            }
        }

        private void 启动_Click(object sender, RoutedEventArgs e)
        {
            //监测库文件是否存在
            if(File.Exists("Memory_Bypass.dll"))
            {
                //后台执行
                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    bw.DoWork += new DoWorkEventHandler(Start_in_the_background);
                    bw.RunWorkerAsync();
                }
            }
            else
            {
                Assignment("发生错误：Memory_Bypass 丢失");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //判断输入是否为空
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
    }
}
