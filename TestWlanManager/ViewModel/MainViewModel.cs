using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using TestWlanManager.Helper;
using TestWlanManager.ViewModel.WifiModel;
using TestWlanManager.WlanApi;
using TestWlanManager.WlanPcap;
using TestWlanManager.PopWindows;
using Autofac;

namespace TestWlanManager.ViewModel
{

    public partial class MainViewModel : BaseViewModel
    {
        private SoftAP _ap;

        KeyboardHook _hook;

        /// <summary>
        /// 是否启动
        /// </summary>
        bool _isStart;

        DispatcherTimer _timer;

        /// <summary>
        /// 第一次启动
        /// </summary>
        bool _isFirst = true;

        private WlanSetting _Setting = new WlanSetting();

        public WlanSetting Setting
        {
            get
            {
              return  _Setting;
            }

            set
            {

                _Setting = value;
                this.RaisePropertyChanged(() => Setting);

            }
        }


        private ObservableCollection<WlanStatusInfo> _StatusInfo = new ObservableCollection<WlanStatusInfo>();

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

        private string _StatusInfoSelectItem;

        public string StatusInfoSelectItem
        {
            get
            {
                return _StatusInfoSelectItem;
            }

            set
            {

                _StatusInfoSelectItem = value;
                this.RaisePropertyChanged(() => StatusInfoSelectItem);

            }
        }


        private string _StartAndStopContent="启动";

        public string StartAndStopContent
        {
            get
            {
                return _StartAndStopContent;
            }

            set
            {

                _StartAndStopContent = value;
                this.RaisePropertyChanged(() => StartAndStopContent);

            }
        }

        #region Command

        public ICommand StartCommand { get; set; }

        public ICommand PopDetailCommand { get; set; }

        #endregion

        #region 属性



        #endregion


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            StatusInfo = new ObservableCollection<WlanStatusInfo>();
        }

        protected override void InitCommand()
        {
            StartCommand = new RelayCommand(StartAction);
            PopDetailCommand = new RelayCommand(PopDetailAction);
            base.InitCommand();
        }

        /// <summary>
        /// 加载事件
        /// </summary>
        protected override void Loaded()
        {
            base.Loaded();
            try
            {
                _ap = new SoftAP();
                _timer = new DispatcherTimer();
                _timer.Interval = new TimeSpan(0, 0, 4);
                _timer.Tick += _timer_Tick;
                _timer.Start();

                var result = _ap.getSettings();
                Setting.Name = result.SSID;
                Setting.Password = result.Password.Replace("\0", "");

                _hook = new KeyboardHook();
                _hook.KeyDownEvent += Hook_KeyDownEvent;
                _hook.Start();
            }
            catch (Exception ex)
            {
                LogHelper.ShowErrorMessage(ex);
            }
        }

        protected override void Unloaded()
        {
            Stop();
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartAction()
        {
            try
            {
                if (!_isStart)
                {
                    if (SetWifiData())
                    {
                        _ap.Start();
                        StartChange(true);
                    }
                    else
                    {
                        StartChange(false);
                    }
                }
                else
                {
                    Stop();
                }

            }
            catch (Exception ex)
            {
                StartChange(false);
                LogHelper.ShowErrorMessage(ex);
            }

        }

        private void PopDetailAction()
        {
            if (this.StatusInfoSelectItem != null)
            {
                PacpWindow window = EngineContext.Container.Resolve<PacpWindow>();
                window.DataContext= EngineContext.Container.Resolve<PacpViewModel>();
                window.Owner = App.Current.MainWindow;
                window.ShowInTaskbar = false;
                window.ShowDialog();
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            StatusChange();
        }


        private bool SetWifiData()
        {
            bool result = false;
            if (!string.IsNullOrWhiteSpace(Setting.Name) && !string.IsNullOrWhiteSpace(Setting.Password))
            {
                result = true;
                HostedNetworkSettings hs = new HostedNetworkSettings(Setting.Name, Setting.Password, 100);
                _ap.setSettings(hs);
                _ap.Enable();
            }

            return result;
        }

        /// <summary>
        /// 隐藏功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hook_KeyDownEvent(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyValue == (int)System.Windows.Forms.Keys.NumPad7 && (int)System.Windows.Forms.Control.ModifierKeys == ((int)System.Windows.Forms.Keys.Alt | (int)System.Windows.Forms.Control.ModifierKeys))
            {
                App.Current.MainWindow.ShowInTaskbar = true;
                App.Current.MainWindow.Show();
            }
        }

        private void Stop()
        {
            try
            {
                _ap.Stop();
                WlanShare.DisShareStart();
                DevicePacp.Instance.StopPacp();
            }
            catch
            {
               
            }
            finally
            {
                _isFirst = true;
                StartChange(false);
            }
        }

        private void StatusChange()
        {
            StatusInfo.Clear();
            if (_isStart)
            {
                try
                {
                    _timer.Stop();
                    var connectResult = _ap.getStatus();
                    StartChange(connectResult.IsActive);
                 
                    foreach (var item in connectResult.Peers)
                    {
                        StatusInfo.Add(new WlanStatusInfo() { Authenticated = item.Authenticated, HostName = item.HostName, IpAddress = item.IpAddress, MAC = item.MAC });
                    }
                    

                    if (_isFirst && !String.IsNullOrEmpty(connectResult.MAC))
                    {
                        //DevicePacp.Instance.StartPacp(connectResult.MAC);
                        _isFirst = false;
                    }

                    _timer.Start();
                }
                catch (Exception ex)
                {
                   // LogHelper.ShowErrorMessage(ex);
                    _timer.Start();
                }
            }
        }

        /// <summary>
        /// 设置是否启动
        /// </summary>
        /// <param name="isStart"></param>
        private void StartChange(bool isStart)
        {
            _isStart = isStart;
            if (_isStart)
            {
                StartAndStopContent = "停止";
            }
            else
            {
                StatusInfo.Clear();
                StartAndStopContent = "启动";
            }
        }
    }
}