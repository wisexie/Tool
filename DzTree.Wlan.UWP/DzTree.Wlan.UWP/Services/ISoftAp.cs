using DzTree.Wlan.UWP.ViewModel.WifiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;

namespace DzTree.Wlan.UWP.Services
{
    internal interface ISoftAp
    {
        void Start(WlanSetting setting);

        void Stop();

        List<WlanStatusInfo> GetConnectData();
        WlanSetting GetSetting();

    }
}
