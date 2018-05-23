using DzTree.Wlan.UWP.ViewModel.WifiModel;
using DzTree.Wlan.UWP.WlanApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.NetworkOperators;

namespace DzTree.Wlan.UWP.Services
{
    internal class SoftAp : ISoftAp
    {
        private WiFiDirectAdvertisementPublisher _publisher;
        private NetworkOperatorTetheringManager _tetheringManager;
        WiFiDirectConnectionListener _listener;

        public SoftAp()
        {
            _setting = new WlanSetting();
            Init();
        }

        private WlanSetting _setting;

        public WlanSetting GetSetting()
        {
            return _setting;
        }

        private void Init()
        {
            var connectionProfiles = NetworkInformation.GetConnectionProfiles();
            ConnectionProfile ethernetConnectionProfile = null;
            foreach (var item in NetworkInformation.GetHostNames())
            {
                string name = item.DisplayName;
                string name2 = item.CanonicalName;

                if (connectionProfiles.Where(x => x.NetworkAdapter?.NetworkAdapterId == item.IPInformation?.NetworkAdapter?.NetworkAdapterId).FirstOrDefault() != null)
                {
                    ethernetConnectionProfile = connectionProfiles.Where(x => x.NetworkAdapter?.NetworkAdapterId == item.IPInformation?.NetworkAdapter?.NetworkAdapterId).First();
                }
            }
            _tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(ethernetConnectionProfile);
            var config = _tetheringManager.GetCurrentAccessPointConfiguration();
            _setting.Name = config.Ssid;
            _setting.Password = config.Passphrase;
        }

        public async void  Start(WlanSetting setting)
        {
            try
            {
               
                //StartPublisher(setting);
                //StartListener();
                await StartNetworkOperatorTetheringManager(setting);
            }
            catch (Exception ex)
            {

            }
          
        }

        public async void Stop()
        {
            try
            {
                if (_tetheringManager != null)
                {
                    await _tetheringManager.StopTetheringAsync();
                }
              
                //_publisher.Stop();
                //_publisher.StatusChanged -= OnStatusChanged;
            }
            catch (Exception ex)
            {

          
            }
           
           
        }

        public List<WlanStatusInfo> GetConnectData()
        {
            return  _tetheringManager.GetTetheringClients().Select(item => new WlanStatusInfo()
            {
                Authenticated = true,
                HostName = item.HostNames.FirstOrDefault(x => x.Type == HostNameType.DomainName)?.RawName,
                IpAddress = item.HostNames.FirstOrDefault(x => x.Type == HostNameType.Ipv4)?.RawName,
                MAC = item.MacAddress
            }).ToList();
        }

        private void OnStatusChanged(WiFiDirectAdvertisementPublisher sender, WiFiDirectAdvertisementPublisherStatusChangedEventArgs args)
        {

        }


        private void StartPublisher(WlanSetting setting)
        {
            _publisher = new WiFiDirectAdvertisementPublisher();
            _publisher.Advertisement.ListenStateDiscoverability = WiFiDirectAdvertisementListenStateDiscoverability.Normal;
            _publisher.Advertisement.IsAutonomousGroupOwnerEnabled = true;
            _publisher.StatusChanged += OnStatusChanged;
            _publisher.Advertisement.LegacySettings.IsEnabled = true;

            var creds = new Windows.Security.Credentials.PasswordCredential();
            _publisher.Advertisement.LegacySettings.Ssid = setting.Name;
            creds.Password = setting.Password;
            _publisher.Advertisement.LegacySettings.Passphrase = creds;

            _publisher.Start();
        }


        private async Task<bool>  StartNetworkOperatorTetheringManager(WlanSetting setting)
        {
           
            var Config = _tetheringManager.GetCurrentAccessPointConfiguration();
            _setting = setting;
            Config.Ssid = setting.Name;
            Config.Passphrase = setting.Password;
       
            await _tetheringManager.ConfigureAccessPointAsync(Config);
            var result = await _tetheringManager.StartTetheringAsync();
            if (result.Status == TetheringOperationStatus.Success)
            {
               
                return true;
            }
             
            else
                return false;

        }

        private void StartListener()
        {
            _listener = new WiFiDirectConnectionListener();
            try
            {
                _listener.ConnectionRequested += OnConnectionRequested;
               
            }
            catch (Exception ex)
            {
              
                return;
            }
        }

        private void OnConnectionRequested(WiFiDirectConnectionListener sender, WiFiDirectConnectionRequestedEventArgs args)
        {
           
            //WiFiDirectConnectionRequest connectionRequest = args.GetConnectionRequest();
            //string deviceName = connectionRequest.DeviceInformation.Name;
            //string result = string.Empty;
            //GetData(connectionRequest);
        }

        private async void GetData(WiFiDirectConnectionRequest connectionRequest)
        {
            try
            {
                var wfdDevice = await WiFiDirectDevice.FromIdAsync(connectionRequest.DeviceInformation.Id);
                var endPoint = wfdDevice.GetConnectionEndpointPairs();
                string result = string.Empty;
                foreach (var item in endPoint)
                {
                    result = item.LocalHostName + "\r\n";
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}
