using DzTree.VideoRecoder.Domain.Dev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.VideoRecoder.Service.Mpeg
{
    public interface IMpegService
    {
        /// <summary>
        /// 获取屏幕上所有音频和视频设备
        /// </summary>
        string GetVideoAndAudioDev();

        /// <summary>
        /// 录制屏幕 
        /// </summary>
        /// <returns></returns>
        bool RecordScreen(AudioDev dev, ScreenData data);

        /// <summary>
        /// 停止当前操作 
        /// </summary>
        /// <returns></returns>
        void Stop();
    }
}
