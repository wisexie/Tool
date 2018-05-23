using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWlanManager.WlanApi
{
    /// <summary>
    /// A structure holding status of the hosted network (GUID, active/inactive, peers connected)
    /// </summary>
    public struct HostedNetworkStatus
    {
        private bool _IsActive;
        private int _PeersConnected;
        private Guid _GUID;
        private LinkedList<HostedNetworkPeer> _Peers;
        private string _MAC;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isActive">Network active(true)/inactive(false)</param>
        /// <param name="guid">GUID of the Software Access Point</param>
        /// <param name="peersConnected">Number of peers connected</param>
        /// <param name="peers">List of all connected peers</param>
        public HostedNetworkStatus(bool isActive, Guid guid, int peersConnected, LinkedList<HostedNetworkPeer> peers,string mac)
        {
            _IsActive = isActive;
            _GUID = guid;
            _PeersConnected = peersConnected;
            _Peers = peers;
            _MAC = mac;
        }

        /// <summary>
        /// Read only, returns boolean indicating if network is up or not
        /// </summary>
        public bool IsActive { get { return _IsActive; } }

        /// <summary>
        /// Read only, returns a number of connected peers
        /// </summary>
        public int PeersConnected { get { return _PeersConnected; } }

        /// <summary>
        /// Read only, returns GUID of the Software Access Point
        /// </summary>
        public Guid GUID { get { return _GUID; } }

        /// <summary>
        /// Read only, returns a list of all connected peers
        /// </summary>
        public LinkedList<HostedNetworkPeer> Peers { get { return _Peers; } }

        public string MAC { get { return _MAC; } }
    }
}
