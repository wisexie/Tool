using PacketDotNet;
using PacketDotNet.Ieee80211;
using SharpPcap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWlanManager.Helper;
using System.Windows.Threading;

namespace TestWlanManager.WlanPcap
{
    public class DevicePacp
    {

        public static DevicePacp Instance
        {
            get {

                if (_Instance == null)
                {
                    lock(objectlocks)
                    {
                        if (_Instance == null)
                        {
                            _Instance = new DevicePacp();
                        }
                    }
                }
                return _Instance;
            }
        }

        private static DevicePacp _Instance;

        private string _Mac = string.Empty;
        private ICaptureDevice _Device;
        private ConcurrentQueue<Packet> _Packets=new ConcurrentQueue<Packet>();
        private static object objectlocks = new object();

        private List<DevicePacket> _DeviceSource = new List<DevicePacket>();

        private DispatcherTimer _Timer;

        /// <summary>
        /// 通讯包数据源
        /// </summary>
        public List<DevicePacket> DeviceSource
        {
            get { return _DeviceSource; }
        }



        public DevicePacp()
        {
            _Timer = new DispatcherTimer();
            _Timer.Tick += _Timer_Tick;
            _Timer.Interval = new TimeSpan(0, 0, 4);
            _Timer.Start();
            
        }

        private void _Timer_Tick(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                _Timer.Stop();
                HandelPacketQueue();
                _Timer.Start();
            });
        }

        /// <summary>
        /// 开始抓包
        /// </summary>
        /// <param name="mac"></param>
        public void StartPacp(string mac)
        {
            _Mac = mac;
            foreach (var item in CaptureDeviceList.Instance)
            {
                if (string.Equals(ArpHelper.ConvertMacStrToStr((item as dynamic).Interface.MacAddress.ToString()),_Mac,StringComparison.CurrentCultureIgnoreCase))
                {
                    _Device = item;
                    break; 
                }
            }
            if (_Device != null)
            {
                _Device.OnPacketArrival -= _Device_OnPacketArrival;

                _Device.OnPacketArrival += _Device_OnPacketArrival;
                 int readTimeoutMilliseconds = 1000;
                _Device.Open(DeviceMode.Normal, readTimeoutMilliseconds);
                _Device.StartCapture();
            }
        }

        /// <summary>
        /// 停止抓包
        /// </summary>
        public void StopPacp()
        {
            if (_Device != null)
            {
                _Device.StopCapture();
                _Device.Close();
                _Device.OnPacketArrival -= _Device_OnPacketArrival;
                _Packets = new ConcurrentQueue<Packet>();
                _Device = null;
            }

            _DeviceSource.Clear();


        }

        /// <summary>
        /// 抓包事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            Task.Factory.StartNew(() => {
                Packet packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                if (packet.IsAddPacket()&&_Packets.Count<20)
                {
                    _Packets.Enqueue(packet);
                }
              
            });
        }

        private void HandelPacketQueue()
        {
            try
            {
                Packet packet;
                if (_DeviceSource.Count > 20)
                    return;

                bool result = _Packets.TryDequeue(out packet);
                while (result)
                {
                    if (packet != null)
                    {
                        var source = packet.ConvertPacketToDevice();
                        this._DeviceSource.Add(source);
                    }
                    if (_DeviceSource.Count > 20)
                        return;
                }
            }
            catch (Exception)
            {
            }
        }

    }
}
