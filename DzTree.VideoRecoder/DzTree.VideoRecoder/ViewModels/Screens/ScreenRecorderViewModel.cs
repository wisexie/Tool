using Caliburn.Micro;
using DzTree.VideoRecoder.Core;
using DzTree.VideoRecoder.Domain.Dev;
using DzTree.VideoRecoder.Domain.Message;
using DzTree.VideoRecoder.Service.Mpeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;


namespace DzTree.VideoRecoder.ViewModels.Screens
{
    public class ScreenRecorderViewModel : Screen, IScreenRecorderViewModel, IHaveDisplayName, IHandle<DevListMessage>
    {
        IMpegService _MpegService;
        IEventAggregator _evens;
        IWindowManager _windowMange;

        public ScreenRecorderViewModel(IMpegService MpegService, IEventAggregator events,IWindowManager windowManager)
        {
            _MpegService = MpegService;
            _evens = events;
            _evens.Subscribe(this);
            _windowMange = windowManager;
        }
        VideoAndAudioDev _Devs = new VideoAndAudioDev();
        public VideoAndAudioDev Devs
        {
            get { return _Devs; }
            set
            {
                _Devs = value;
                NotifyOfPropertyChange(() => Devs);
            }
        }

        AudioDev _AudioSelectItem = new AudioDev();
        public AudioDev AudioSelectItem
        {
            get { return _AudioSelectItem; }
            set
            {
                _AudioSelectItem = value;
                NotifyOfPropertyChange(() => AudioSelectItem);
            }
        }

        public void GetVideo()
        {
            _MpegService.GetVideoAndAudioDev();
        }

        /// <summary>
        /// 开始录制
        /// </summary>
        public void ScreenRecordVideo()
        {
            var myWindows = AutoFacModel.Container.Resolve<IFullScreenViewModel>();
            _windowMange.ShowDialog(myWindows);
            //ScreenData data = new ScreenData();
            //data.RectSelectArea= myWindows.RectSelectArea;
            //data.FileName = "d:\\"+DateTime.Now.ToString("yyyyMMddHHmmss")+ ".mp4";
            //Task.Run(() => _MpegService.RecordScreen(AudioSelectItem, data));

        }

        public void VideoStop()
        {
            Task.Run(() => _MpegService.Stop());
           
        }

        public void Handle(DevListMessage message)
        {
            Devs = message.Data;
        }
    }
}
