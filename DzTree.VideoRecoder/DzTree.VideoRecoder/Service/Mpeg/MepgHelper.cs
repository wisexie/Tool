using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DzTree.VideoRecoder.Domain.Dev;
using DzTree.VideoRecoder.Helper;

namespace DzTree.VideoRecoder.Service.Mpeg
{
    /// <summary>
    /// 调用ffmepg的exe文件 后续希望换成 dll
    /// </summary>
    public class MepgHelper
    {
        /// <summary>
        /// 调用ffmpeg.exe
        /// </summary>
        public static void ExecVideoCommand(string arg, DataReceivedEventHandler outRec, EventHandler sendComplete)
        {
            //实例化一个进程类
            using (Process cmd = new Process())
            {
                cmd.StartInfo.FileName = FfmpegPath;
                cmd.StartInfo.Arguments = arg;
                cmd.StartInfo.UseShellExecute = false; //此处必须为false否则引发异常
                cmd.StartInfo.RedirectStandardError = true;
                cmd.StartInfo.RedirectStandardInput = true; //标准输入
                //cmd.StartInfo.RedirectStandardOutput = true; //标准输出

                //不显示命令行窗口界面
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.ErrorDataReceived += new DataReceivedEventHandler(outRec);
     
                cmd.Start();
                cmd.BeginErrorReadLine();
                cmd.WaitForExit();
                cmd.StandardInput.Close();
                cmd.Close();
                sendComplete.Invoke(null, null);
            }
        }

        /// <summary>
        /// 发送消息 
        /// </summary>
        /// <param name="message"></param>
        public static void Stop()
        {
            var processes = Process.GetProcessesByName("ffmpeg");
            if (processes != null)
            {
                foreach (var p in processes)
                {

                    WinApiHelper.AttachConsole(p.Id);
                   // WinApiHelper.SetConsoleCtrlHandler(IntPtr.Zero, true);
                    WinApiHelper.GenerateConsoleCtrlEvent(0, p.Id);
                    WinApiHelper.FreeConsole();
                    break;
                }
            }
        }

        public static DevState GetDevId(string msg,DevState state,ref VideoAndAudioDev dev,ref string devName)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                if (msg.Contains(VideoDevConst))
                    state = DevState.VideoState;
                else if (msg.Contains(AudioStateConst))
                    state = DevState.AudioState;
                else if (msg.Contains(DevIdConst))
                {
                
                    string devId = msg.Substring(msg.IndexOf(DevIdConst),msg.Length- msg.IndexOf(DevIdConst)).Replace(DevIdConst,"").Replace("\"", "").Trim();
                    switch (state)
                    {
                        case DevState.VideoState:
                            dev.Videos.Add(new VideoDev() { DevId= devId,DevName= EncoderHelper.ASCIToUtf8(devName) });
                            break;
                        case DevState.AudioState:
                            dev.Audios.Add(new AudioDev() { DevId = devId, DevName = EncoderHelper.ASCIToUtf8(devName) });
                            break;
                    }
                }
                devName = msg;
            }
            return state;
        }

        private static readonly string FfmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");

        private const string VideoDevConst = "DirectShow video devices";
        private const string AudioStateConst = "DirectShow audio devices";
        private const string DevIdConst = "Alternative name";

    }
}
