using DzTree.Wlan.UWP.Core;
using DzTree.Wlan.UWP.Services;
using DzTree.Wlan.UWP.ViewModel.WifiModel;
using DzTree.Wlan.UWP.WlanApi;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace DzTree.Wlan.UWP.ViewModel
{
    internal partial class MainViewModel : BaseViewModel
    {
        public ICommand StartCommand { get; set; }

        private ISoftAp _softAp;
        private WlanSetting _Setting = new WlanSetting();
        private bool _isStart = false;
        private string _startAndStopContent = "启动";
        DispatcherTimer _timer;
        private ObservableCollection<WlanStatusInfo> _StatusInfo = new ObservableCollection<WlanStatusInfo>();



        public MainViewModel(ISoftAp softAp)
        {
            _softAp = softAp;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 4);
            _timer.Tick += _timer_Tick; ;
            _timer.Start();
            Setting = softAp.GetSetting();
        }



        public ObservableCollection<WlanStatusInfo> StatusInfo
        {
            get
            {
                return _StatusInfo;
            }
            set
            {
                _StatusInfo = value;
                this.RaisePropertyChanged(() => StatusInfo);
            }
        }

        private void _timer_Tick(object sender, object e)
        {
            _timer.Stop();
            if (_isStart)
            {

              
                try
                {
                    StatusInfo = new ObservableCollection<WlanStatusInfo>(_softAp.GetConnectData());
                }
                catch (Exception ex)
                {


                }
                finally
                {
                    _timer.Start();
                }
     
            }
        }

        public WlanSetting Setting
        {
            get{return _Setting;}
            set{ _Setting = value; this.RaisePropertyChanged(() => Setting);}
        }


        public string StartAndStopContent
        {
            get{return _startAndStopContent;}
            set{_startAndStopContent = value;this.RaisePropertyChanged(() => StartAndStopContent);}
        }

        protected override void InitCommand()
        {
            StartCommand = new RelayCommand(StartAction);
        }

        private void StartAction()
        {
            if (!_isStart)
            {
                _softAp.Start(_Setting);
                _isStart = true;
                StartAndStopContent = "暂停";
            }
            else
            {
                _softAp.Stop();
                _isStart = false;
                StartAndStopContent = "启动";
            }

        }
    }
}
