using System;
using System.ComponentModel;
using System.Diagnostics;
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
            Process.Start("https://github.com/xingchuanzhen/Minecraft_Memory_Bypass_GUI");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            BeginStoryboard((Storyboard)FindResource("设置关闭"));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            最小化_Click(null, null);
        }

        void Exit(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(400);
            Environment.Exit(0);
        }

        private void 最小化_Click(object sender, RoutedEventArgs e)
        {
            if(!(主窗体.Visibility == Visibility.Collapsed))
                BeginStoryboard((Storyboard)FindResource("程序关闭"));
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.DoWork += new DoWorkEventHandler(Exit);
                bw.RunWorkerAsync();
            }
        }

        private void 关闭_Click(object sender, RoutedEventArgs e)
        {
            名称.Text = Properties.Settings.Default.Program_Name;
            地址.Text = Properties.Settings.Default.Offset_Address;
            内容.Text = Properties.Settings.Default.Write_Content;
            退出.IsChecked = Properties.Settings.Default.Quit;
            提示.Visibility = Visibility.Collapsed;
            BeginStoryboard((Storyboard)FindResource("设置开启"));
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="str">输出内容</param>
        private void Assignment(string str)
        {
            Dispatcher.Invoke(new Action(delegate//同步线程
            {
                日志.Text += "\n[" + DateTime.Now.ToLongTimeString().ToString() + "]: " + str;
                框.ScrollToVerticalOffset(框.ExtentHeight);
            }));
        }

        /// <summary>
        /// 刷新设置数据
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

        /// <summary>
        /// 不显示图形化界面
        /// </summary>
        private static bool Interface_NO = false;

        /// <summary>
        /// 立即执行,执行成功后自动退出
        /// </summary>
        private static bool Implement_YES = false;

        /// <summary>
        /// 初始化命令行参数
        /// </summary>
        /// <param name="Args">参数内容</param>
        private void Initialization(string[] Args)
        {
            for (int i = 0; i < Args.Length; i++)
            {
                if (Args[i] == "Interface_NO")
                    Interface_NO = true;

                if (Args[i] == "Implement_YES")
                    Implement_YES = true;
            }

            //不显示窗体(Interface_NO)不能单独使用
            if (Interface_NO && !Implement_YES)
            {
                MessageBox.Show("Interface_NO 参数不能单独使用！");
                Environment.Exit(0);
            }
        }

        public MainWindow()
        {
            //初始化命令行参数
            Initialization(App.Com_Line_Args);

            InitializeComponent();

            //隐藏显示界面
            if (Interface_NO)
                BeginStoryboard((Storyboard)FindResource("隐藏显示"));

            //隐藏设置界面
            设置.Visibility = Visibility.Collapsed;
            //打印日志
            日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: " + "程序版本：1.0.0.0";
            //获取计算机UWP程序安装列表
            string Inf = Operate.RunCmd(@"CD C:\Program Files\WindowsApps & C: & DIR");
            //判断是否安装了MinecraftUWP
            if (Inf.Contains("Microsoft.MinecraftUWP"))
            {
                //得到游戏版本
                Assignment("游戏版本：" + Operate.Substring(Inf, "Microsoft.MinecraftUWP_", "_"));
                if (Implement_YES)
                    启动_Click(null,null);
            }
            else
            {
                //输出错误信息
                Assignment("请先安装 MinecraftUWP!");
                启动.IsEnabled = false;
                if (Interface_NO)
                    MessageBox.Show("请先安装 MinecraftUWP!");
            }
        }

        void Start_in_the_background(object sender, DoWorkEventArgs e)
        {
            try
            {
                Update_Data();                                                          //刷新用户输入数据
                Assignment("启动进程：MinecraftUWP");
                Process.Start("minecraft:");                                            //启动MinecraftUWP
                int PID = GetProcId(Program_Name);                                      //得到进程PID
                Assignment("进程标识：" + PID);
                long Address = GetModuleBaseAddress(PID, Program_Name);                 //得到进程基地址(十进制)
                Assignment("进程基址：0x" + Address.ToString("X"));
                long Handle = GetProcessHandle(PID);                                    //得到窗口句柄
                Assignment("窗口句柄：0x" + Handle.ToString("X"));
                byte[] Buffer = new byte[] { (byte)Write_Content };                     //设置缓冲区
                Assignment("偏移地址：0x" + Offset_Address.ToString("X"));
                Assignment("写入内存：0x" + Write_Content.ToString("X"));
                bool Result = WriteMemory(Handle, Address + Offset_Address, Buffer, 1);  //将数据写入内存
                Assignment("执行结果：" + Result.ToString().Replace("True", "写入成功!").Replace("False", "写入失败!"));
                Thread.Sleep(1000);
                //判断是否执行成功，成功后自动关闭程序(提前用户设置后)
                if(Result && (Properties.Settings.Default.Quit || Implement_YES))
                    Dispatcher.Invoke(new Action(delegate { 最小化_Click(null, null); }));
            }
            catch (Exception ex)
            {
                new Thread(() =>
                {   //打印异常错误
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() =>
                        {
                            Assignment("发生错误：未知错误");
                            MessageBox.Show(ex.ToString(), "错误!");
                        }));
                }).Start();
            }
        }

        private void 启动_Click(object sender, RoutedEventArgs e)
        {
            //后台执行
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.DoWork += new DoWorkEventHandler(Start_in_the_background);
                bw.RunWorkerAsync();
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
                //写入设置
                BeginStoryboard((Storyboard)FindResource("设置关闭"));
                Properties.Settings.Default.Program_Name = 名称.Text;
                Properties.Settings.Default.Offset_Address = 地址.Text;
                Properties.Settings.Default.Write_Content = 内容.Text;
                Properties.Settings.Default.Quit = (bool)退出.IsChecked;
                Properties.Settings.Default.Save();
            }
        }
    }
}
