using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft_Memory_Bypass
{
    class Operate
    {
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="path">写入路径</param>
        /// <returns></returns>
        public static string HttpDownloadFile(string url, string path)
        {
            // 设置参数
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();

            //创建本地文件写入流
            Stream stream = new FileStream(path, FileMode.Create);

            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }
            stream.Close();
            responseStream.Close();
            return path;
        }

        /// <summary>
        /// 执行cmd命令，返回执行结果
        /// </summary>
        /// <param name="command">命令内容</param>
        /// <returns>返回执行结果字符串</returns>
        public static string RunCmd(string command)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";           //确定程序名
            p.StartInfo.Arguments = "/c " + command;    //确定程式命令行
            p.StartInfo.UseShellExecute = false;        //Shell的使用
            p.StartInfo.RedirectStandardInput = true;   //重定向输入
            p.StartInfo.RedirectStandardOutput = true;  //重定向输出
            p.StartInfo.RedirectStandardError = true;   //重定向输出错误
            p.StartInfo.CreateNoWindow = true;          //设置置不显示示窗口
            p.Start();
            return p.StandardOutput.ReadToEnd();        //输出出流取得命令行结果果
        }

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="sourse">目标</param>
        /// <param name="startstr">从这里</param>
        /// <param name="endstr">到这里</param>
        /// <returns>返回不包含"从这里","到这里"的字符串</returns>
        public static string Substring(string sourse, string startstr, string endstr)
        {
            string result = string.Empty;
            int startindex, endindex;
            try
            {
                startindex = sourse.IndexOf(startstr);
                if (startindex == -1)
                    return result;
                string tmpstr = sourse.Substring(startindex + startstr.Length);
                endindex = tmpstr.IndexOf(endstr);
                if (endindex == -1)
                    return result;
                result = tmpstr.Remove(endindex);
            }
            catch { }
            return result;
        }
    }
}
