using Caliburn.Micro;
using DzTree.VideoRecoder.Domain.Dev;
using DzTree.VideoRecoder.Domain.Message;
using DzTree.VideoRecoder.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.VideoRecoder.Service.Mpeg
{
    public class MpegService:IMpegService
    {
        IEventAggregator _events;
        public MpegService(IEventAggregator events)
        {
            _events = events;
            _events.Subscribe(this);
        }
        /// <summary>
        /// 获取屏幕上所有音频和视频设备
        /// </summary>
        public string GetVideoAndAudioDev()
        {
            string result = "";
            DevState state = DevState.None;
            VideoAndAudioDev devs = new VideoAndAudioDev();
            string devName=string.Empty;
            MepgHelper.ExecVideoCommand(VideoAndAudioList,
                (sender,e)=>
                    {
                        state= MepgHelper.GetDevId(e.Data, state, ref devs,ref devName);
                    },
                (sender,e)=>
                    {
                        _events.Publish(new DevListMessage { Data = devs }, (t) => { Task.Factory.StartNew(t); });
                    });
            return result;
        }

        /// <summary>
        /// 录制屏幕 
        /// </summary>
        /// <returns></returns>
        public bool RecordScreen(AudioDev dev, ScreenData data)
        {
            string area = (data.RectSelectArea.Width % 2 == 0 ? data.RectSelectArea.Width.ToString() : (data.RectSelectArea.Width + 1.0).ToString())
                + "x"
                + (data.RectSelectArea.Height % 2 == 0 ? data.RectSelectArea.Height.ToString() : (data.RectSelectArea.Height + 1.0).ToString());
            string command = string.Format(RecordScreenConst,
                data.RectSelectArea.X,
                data.RectSelectArea.Y,
                area, 
                data.FileName, dev.DevId);

            MepgHelper.ExecVideoCommand(command,
                (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        //Debug.Write(e.Data.ToString()+"\r\n");
                       
                    }
                },
                (sender, e) =>
                {
                  
                });
            return true;
        }


        /// <summary>
        /// 停止当前操作 
        /// </summary>
        /// <returns></returns>
        public void Stop()
        {
            MepgHelper.Stop();
          //  ConvertToMp4();
        }

        private void ConvertToMp4()
        {
            string command = string.Format(VideoToMp4, "d:\\test.avi", "d:\\test.mp4");
            MepgHelper.ExecVideoCommand(command,
                (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        //Debug.Write(e.Data.ToString() + "\r\n");

                    }
                },
                (sender, e) =>
                {

                });
        }

        /// <summary>
        /// 获取视频列表
        /// </summary>
        const string VideoAndAudioList = "ffmpeg -list_devices true -f dshow -i dummy";

        /// <summary>
        /// 录制桌面 
        /// </summary>
        const string RecordScreenDesktopConst = "-f gdigrab -i desktop -f dshow  -i audio=\"{0}\" {1}";
       
        /// <summary>
        /// 录制指定位置 
        /// </summary>
        const string RecordScreenConst = " -y -rtbufsize 100M -f dshow -i audio=\"{4}\"  -f gdigrab -framerate 25 -r 25  -offset_x {0} -offset_y {1} -video_size {2} -i desktop -pix_fmt yuv420p -async -1 -c:v libx264 {3}  ";
        //const string RecordScreenConst = " -f dshow -i audio=\"virtual-audio-capturer\" -f gdigrab -framerate 30 -r 30  -offset_x {0} -offset_y {1} -video_size {2}  -i desktop -pix_fmt yuv420p -c:v libx264 {3}  ";

        const string VideoToMp4 = "-i {0} -c:v libx264 -crf 19 -preset slow -c:a aac -b:a 192k -ac 2 {1}";


    }
}
