using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWlanManager.WlanApi
{
    /// <summary>
    /// A structure holding MAC address and an authentication status of a peer connected to the hosted network
    /// </summary>
    public struct HostedNetworkPeer
    {
        private string _MAC;
        private bool _Authenticated;
        private string _HostName;
        private string _IpAddress;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mac">MAC address of the peer</param>
        /// <param name="authenticated">Peer's authentication status</param>
        public HostedNetworkPeer(string mac, bool authenticated,string hostName,string ipAddress)
        {
            _MAC = mac;
            _Authenticated = authenticated;
            _HostName = hostName;
            _IpAddress = ipAddress;
        }

        /// <summary>
        /// Read only, returns MAC address of the peer
        /// </summary>
        public string MAC { get { return _MAC; } }

        /// <summary>
        /// Read only, returns authentication status of the peer
        /// </summary>
        public bool Authenticated { get { return _Authenticated; } }

        public string HostName { get { return _HostName; } }

        public string IpAddress { get { return _IpAddress; } }
    }
}
