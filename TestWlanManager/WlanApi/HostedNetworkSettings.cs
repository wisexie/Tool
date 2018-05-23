using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWlanManager.WlanApi
{
    /// <summary>
    /// A structure used to manage a Software Access Point settings, such as SSID, Password and max number of peers.
    /// </summary>
    public struct HostedNetworkSettings
    {
        private int _MaxPeers;
        private string _SSID;
        private string _Password;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ssid">SSID of the Hosted Network</param>
        /// <param name="password">Password required to connect to the network</param>
        /// <param name="maxPeers">Maximum peers allowed to connect to the network</param>
        public HostedNetworkSettings(string ssid, string password, int maxPeers)
        {
            _MaxPeers = maxPeers;
            _SSID = ssid;
            _Password = password;
        }

        /// <summary>
        /// Read only, gets the maximum number of peers allowed to connect to the network
        /// </summary>
        public int MaxPeers { get { return _MaxPeers; } }

        /// <summary>
        /// Read only, gets the SSID of the hosted network
        /// </summary>
        public string SSID { get { return _SSID; } }

        /// <summary>
        /// Read only, gets the password required to connect to the hosted network
        /// </summary>
        public string Password { get { return _Password; } }
    }
}
