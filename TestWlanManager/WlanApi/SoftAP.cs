using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TestWlanManager.Helper;

namespace TestWlanManager.WlanApi 
{
    public class SoftAP
    {
        #region class variables

        private IntPtr handle = new IntPtr();
        private UInt32 version = new UInt32();

        //for easy state check:
        private bool handleConnected = false;
        private bool serverRunning = false;

        //for handling error codes with exceptions:
        private NativeWLanAPI._WLAN_HOSTED_NETWORK_REASON lastError = new NativeWLanAPI._WLAN_HOSTED_NETWORK_REASON();
        private int lastErrorCode = 0;

        #endregion

        /// <summary>
        /// Starts the Software Access Point
        /// </summary>
        /// <returns>True if the server has been started</returns>
        public bool Start()
        {

            NativeWLanAPI.WlanHostedNetworkStartUsing(handle, out lastError, IntPtr.Zero);
            checkStatusAndThrow("启动Wi-Fi失败");

            NativeWLanAPI.WlanHostedNetworkForceStart(handle, out lastError, IntPtr.Zero);
            checkStatusAndThrow("启动Wi-Fi失败");


            WlanShare.ShareStart();

            return (serverRunning = true);
        }

        /// <summary>
        /// Stops the Software Access Point
        /// </summary>
        /// <returns>Returns True if the AP has been stopped</returns>
        public bool Stop()
        {
            if (serverRunning)
            {
                lastErrorCode = (int)NativeWLanAPI.WlanHostedNetworkForceStop(handle, out lastError, IntPtr.Zero);

                checkStatusAndThrow("Could not stop SoftAP");

                serverRunning = false;
            }

            return true;
        }

        /// <summary>
        /// Query a status of Software Access Point
        /// </summary>
        /// <returns>A construct with status of the AP</returns>
        public HostedNetworkStatus getStatus()
        {
            var addressList= GetListArp();

            IntPtr ppStatus = IntPtr.Zero;
            lastErrorCode = (int)NativeWLanAPI.WlanHostedNetworkQueryStatus(handle, out ppStatus, IntPtr.Zero);

            checkStatusAndThrow("Could not retrive SoftAP status", false);

            NativeWLanAPI._WLAN_HOSTED_NETWORK_STATUS qstat = (NativeWLanAPI._WLAN_HOSTED_NETWORK_STATUS)Marshal.PtrToStructure(ppStatus, typeof(NativeWLanAPI._WLAN_HOSTED_NETWORK_STATUS));

            bool isActive = qstat.HostedNetworkState == NativeWLanAPI._WLAN_HOSTED_NETWORK_STATE.wlan_hosted_network_active ? true : false;
            int peersConnected = (int)qstat.dwNumberOfPeers;
            Guid guid = qstat.IPDeviceID;
            string networkMac = BitConverter.ToString(qstat.wlanHostedNetworkBSSID.address);//网卡地址
            LinkedList <HostedNetworkPeer> peers = new LinkedList<HostedNetworkPeer>();

            IntPtr offset = Marshal.OffsetOf(typeof(NativeWLanAPI._WLAN_HOSTED_NETWORK_STATUS), "PeerList");
            for (int i = 0; i < peersConnected; i++)
            {
                IntPtr ppeer = new IntPtr(ppStatus.ToInt64() + offset.ToInt64());
                NativeWLanAPI._WLAN_HOSTED_NETWORK_PEER_STATE peer = (NativeWLanAPI._WLAN_HOSTED_NETWORK_PEER_STATE)Marshal.PtrToStructure(ppeer, typeof(NativeWLanAPI._WLAN_HOSTED_NETWORK_PEER_STATE));

                string mac = BitConverter.ToString(peer.PeerMacAddress.address);
                bool authenticated = ((int)peer.PeerAuthState == 1 ? true : false);
                string ipAddress = addressList.LastOrDefault(x => string.Equals(x.MacAddress,mac,StringComparison.CurrentCultureIgnoreCase))?.IpAddress;

                HostedNetworkPeer hpeer = new HostedNetworkPeer(mac, authenticated, DnsHelper.GetHostName(ipAddress),ipAddress);

                peers.AddLast(hpeer);

                offset += Marshal.SizeOf(peer);
            }

            NativeWLanAPI.WlanFreeMemory(ppStatus);

            return new HostedNetworkStatus(isActive, guid, peersConnected, peers,networkMac);
        }

        /// <summary>
        /// Queries the settings of Software Access Point
        /// </summary>
        /// <returns>Current settings of the AP</returns>
        public HostedNetworkSettings getSettings()
        {
            UInt32 pdwDataSize = new UInt32();
            IntPtr ppvData = IntPtr.Zero;
            NativeWLanAPI._WLAN_OPCODE_VALUE_TYPE pWlanOpcodeValueType = new NativeWLanAPI._WLAN_OPCODE_VALUE_TYPE();
            NativeWLanAPI._WLAN_HOSTED_NETWORK_OPCODE opcode = NativeWLanAPI._WLAN_HOSTED_NETWORK_OPCODE._WLAN_HOSTED_NETWORK_OPCODE_connection_settings;

            lastErrorCode = (int)NativeWLanAPI.WlanHostedNetworkQueryProperty(handle, opcode, out pdwDataSize, out ppvData, out pWlanOpcodeValueType, IntPtr.Zero);

            checkStatusAndThrow("Could not get network settings at this time");

            NativeWLanAPI._WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS qsettings = (NativeWLanAPI._WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS)Marshal.PtrToStructure(ppvData, typeof(NativeWLanAPI._WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS));

            int maxPeers = (int)qsettings.dwMaxNumberOfPeers;
            string ssid = qsettings.hostedNetworkSSID.ucSSID;
            string password = getPassword();

            NativeWLanAPI.WlanFreeMemory(ppvData);

            return new HostedNetworkSettings(ssid, password, maxPeers);
        }

        /// <summary>
        /// Sets settings of AP to the ones given in the @settings
        /// </summary>
        /// <param name="settings">Construct with settings for AP</param>
        public void setSettings(HostedNetworkSettings settings)
        {
            setNetworkSettings(settings.SSID, settings.MaxPeers);
            setPassword(settings.Password);
        }


        public List<ArpIpAddress> GetListArp()
        {
            List<NativeWLanAPI.MIB_IPNETROW> ArpList = new List<NativeWLanAPI.MIB_IPNETROW>();
            List<ArpIpAddress> addresses = new List<ArpIpAddress>();
            int size = 0;
            NativeWLanAPI.GetIpNetTable(IntPtr.Zero, ref size, true);
            IntPtr ipList =Marshal.AllocHGlobal(size);
            if ((int)NativeWLanAPI.GetIpNetTable(ipList, ref size, true) == 0)
            {
                var num = Marshal.ReadInt32(ipList);
                var ptr = IntPtr.Add(ipList, 4);
                for (int i = 0; i < num; i++)
                {
                    ArpList.Add((NativeWLanAPI.MIB_IPNETROW)Marshal.PtrToStructure(ptr, typeof(NativeWLanAPI.MIB_IPNETROW)));
                    ptr = IntPtr.Add(ptr, Marshal.SizeOf(typeof(NativeWLanAPI.MIB_IPNETROW)));
                }
                Marshal.FreeHGlobal(ipList);
            }
            foreach (var item in ArpList)
            {
                addresses.Add(new ArpIpAddress() {IpAddress= ArpHelper.ConvertIpToStr(item.Addr), MacAddress= ArpHelper.CovnertMacToStr(item.PhysAddr) });
            }
            return addresses;
        }

        /// <summary>
        /// Enables the Software AP. Sometimes when Start() won't work, Enable() and then Start() might work.
        /// </summary>
        public void Enable()
        {
            NativeWLanAPI._WLAN_HOSTED_NETWORK_OPCODE opcode = NativeWLanAPI._WLAN_HOSTED_NETWORK_OPCODE._WLAN_HOSTED_NETWORK_OPCODE_enable;

            int size = Marshal.SizeOf(new Int32());

            IntPtr setting = Marshal.AllocHGlobal(size);

            Marshal.WriteInt32(setting, 1);

            lastErrorCode = (int)NativeWLanAPI.WlanHostedNetworkSetProperty(handle, opcode, (uint)size, setting, out lastError, IntPtr.Zero);

            Marshal.FreeHGlobal(setting);

            checkStatusAndThrow("Could not enable Hosted Access Point! ");
        }

        public SoftAP() 
        {
            if (!(openHandle(2) && initSettings()))
            {
                throw new ApplicationException("Could not open handle.");
            }

        }

        ~SoftAP()
        {
            NativeWLanAPI.WlanCloseHandle(handle, IntPtr.Zero);
        }

        private bool openHandle(int serverVersion)
        {
            lastErrorCode = (int)NativeWLanAPI.WlanOpenHandle((uint)serverVersion, IntPtr.Zero, out version, out handle);

            checkStatusAndThrow("Couldn't retrive handle to SoftAP");

            return (handleConnected = true);
        }

        private bool initSettings()
        {
            lastErrorCode = (int)NativeWLanAPI.WlanHostedNetworkInitSettings(handle, out lastError, IntPtr.Zero);
            checkStatusAndThrow("Didnt initialize settings correctly", true);

            return true;
        }

        private string getPassword()
        {
            string pass = string.Empty;
            bool persistent;
            bool passphrase;
            uint length;
            IntPtr ppass = IntPtr.Zero;

            int error = (int)NativeWLanAPI.WlanHostedNetworkQuerySecondaryKey(handle, out length, out ppass, out passphrase, out persistent, out lastError, IntPtr.Zero);

            if (error != 0)
            {
                throw new ApplicationException("Retriving password failed!");
            }
            else
            {
                if (length != 0 && ppass != IntPtr.Zero)
                {
                    pass = Marshal.PtrToStringAnsi(ppass, (int)length);
                    NativeWLanAPI.WlanFreeMemory(ppass);
                }
            }
            return pass;
        }

        private void setPassword(string pass)
        {
            if (pass.Length < 8 || pass.Length > 63)
            {
                throw new ArgumentException("密码必须在8到63个字符之间");
            }
            NativeWLanAPI.WlanHostedNetworkSetSecondaryKey(handle, (uint)(pass.Length + 1), pass, true, true, out lastError, IntPtr.Zero);
            checkStatusAndThrow("设置密码失败");
        }

        private void setNetworkSettings(string ssid, int maxPeers)
        {
            NativeWLanAPI._DOT11_SSID ossid = new NativeWLanAPI._DOT11_SSID();
            NativeWLanAPI._WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS settings = new NativeWLanAPI._WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS();

            if (ssid.Length > 32)
            {
                throw new ArgumentException("Wi-Fi名称必须小于32个字符");
            }
            else
            {
                ossid.uSSIDLength = (uint)ssid.Length;
                ossid.ucSSID = ssid;
                settings.hostedNetworkSSID = ossid;
            }

            if (maxPeers == 0)
            {
                throw new ArgumentException("Max number of peers must be greater than 0");
            }
            settings.dwMaxNumberOfPeers = (uint)maxPeers;

            int size = Marshal.SizeOf(settings);
            IntPtr pSettings = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(settings, pSettings, true);
            NativeWLanAPI._WLAN_HOSTED_NETWORK_OPCODE opcode = NativeWLanAPI._WLAN_HOSTED_NETWORK_OPCODE._WLAN_HOSTED_NETWORK_OPCODE_connection_settings;

            NativeWLanAPI.WlanHostedNetworkSetProperty(handle, opcode, (uint)size, pSettings, out lastError, IntPtr.Zero);
            checkStatusAndThrow("设置Wi-Fi名称失败");

            Marshal.FreeHGlobal(pSettings);
        }

        private void checkStatusAndThrow(string message, bool reasonIncluded = true)
        {
            int tmp_lastErrorCode;
            NativeWLanAPI._WLAN_HOSTED_NETWORK_REASON tmp_lastError;

            if (reasonIncluded)
            {
                if (lastError != NativeWLanAPI._WLAN_HOSTED_NETWORK_REASON._WLAN_HOSTED_NETWORK_REASON_success || lastErrorCode != 0)
                {
                    tmp_lastErrorCode = lastErrorCode;
                    lastErrorCode = 0;

                    tmp_lastError = lastError;
                    lastError = NativeWLanAPI._WLAN_HOSTED_NETWORK_REASON._WLAN_HOSTED_NETWORK_REASON_success;

                    throw new ApplicationException(message + "! (" + tmp_lastErrorCode + ") WLAN_HOSTED_NETWORK_REASON: " + tmp_lastError.ToString());
                }
            }
            else
            {
                if (lastErrorCode != 0)
                {
                    tmp_lastErrorCode = lastErrorCode;
                    lastErrorCode = 0;

                    lastError = NativeWLanAPI._WLAN_HOSTED_NETWORK_REASON._WLAN_HOSTED_NETWORK_REASON_success;

                    throw new ApplicationException(message + "! (" + tmp_lastErrorCode + ")");
                }
            }
        }

        private static class NativeWLanAPI
        {
            //********************************************************************* NATIVE FUNCTIONS
            #region native windows functions

            [DllImport("wlanapi.dll", EntryPoint = "WFDOpenHandle")]
            public static extern int WfdOpenHandle(
//[In] uint dwClientVersion,
            [In] uint dwClientVersion,
            [Out] out uint pdwNegotiatedVersion,
            [Out] out IntPtr phClientHandle);

            /*************************************************
             *  DWORD WINAPI WlanOpenHandle(
             *      _In_        DWORD dwClientVersion,
             *      _Reserved_  PVOID pReserved,
             *      _Out_       PDWORD pdwNegotiatedVersion,
             *      _Out_       PHANDLE phClientHandle
             *  );                                              
            *************************************************/
[DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanOpenHandle(
                [In] UInt32 dwClientVersion,
                [In, Out] IntPtr pReserved,
                [Out] out UInt32 pdwNegotiatedVersion,
                [Out] out IntPtr phClientHandle
                );

            /*************************************************
             *  DWORD WINAPI WlanCloseHandle(
             *      _In_        HANDLE hClientHandle,
             *      _Reserved_  PVOID pReserved
             *  );
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanCloseHandle(
                [In] IntPtr hClientHandle,
                [In, Out] IntPtr pReserved
                );

            /*************************************************
             *  DWORD WINAPI WlanHostedNetworkSetSecondaryKey(
             *      _In_        HANDLE hClientHandle,
             *      _In_        DWORD dwKeyLength,
             *      _In_        PUCHAR pucKeyData,
             *      _In_        BOOL bIsPassPhrase,
             *      _In_        BOOL bPersistent,
             *      _Out_opt_   P_WLAN_HOSTED_NETWORK_REASON pFailReason,
             *      _Reserved_  PVOID pvReserved
             *  );
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanHostedNetworkSetSecondaryKey(
                [In] IntPtr hClientHandle,
                [In] UInt32 dwKeyLength,
                [In] string pucKeyData,
                [In] Boolean bIsPassPhrase,
                [In] Boolean bPersistent,
                [Out] out _WLAN_HOSTED_NETWORK_REASON pFailReason,
                [In, Out] IntPtr pvReserved
                );

            /*************************************************
             *  DWORD WINAPI WlanHostedNetworkQuerySecondaryKey(
             *    _In_        HANDLE hClientHandle,
             *    _Out_       DWORD pdwKeyLength,
             *    _Out_       PUCHAR *ppucKeyData,
             *    _Out_       PBOOL pbIsPassPhrase,
             *    _Out_       PBOOL pbPersistent,
             *    _Out_opt_   PWLAN_HOSTED_NETWORK_REASON pFailReason,
             *    _Reserved_  PVOID pvReserved
             *  ); 
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanHostedNetworkQuerySecondaryKey(
                [In] IntPtr hClientHandle,
                [Out] out UInt32 pdwKeyLength,
                [Out] out IntPtr ppucKeyData,
                [Out] out bool pbIsPassPhrase,
                [Out] out bool pbPersistent,
                [Out] out _WLAN_HOSTED_NETWORK_REASON pFailReason,
                [In, Out] IntPtr pvReserved
                );

            /*************************************************
             *  DWORD WINAPI WlanHostedNetworkInitSettings(
             *    _In_        HANDLE hClientHandle,
             *    _Out_opt_   PWLAN_HOSTED_NETWORK_REASON pFailReason,
             *    _Reserved_  PVOID pvReserved
             *  ); 
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanHostedNetworkInitSettings(
                [In] IntPtr hClientHandle,
                [Out] out _WLAN_HOSTED_NETWORK_REASON pFailReason,
                [In, Out] IntPtr pvReserved
                );

            /*************************************************
             *  DWORD WINAPI WlanHostedNetworkSetProperty(
             *      _In_        HANDLE hClientHandle,
             *      _In_        _WLAN_HOSTED_NETWORK_OPCODE OpCode,
             *      _In_        DWORD dwDataSize,
             *      _In_        PVOID pvData,
             *      _Out_opt_   P_WLAN_HOSTED_NETWORK_REASON pFailReason,
             *      _Reserved_  PVOID pvReserved
             *  );
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanHostedNetworkSetProperty(
                [In] IntPtr hClientHandle,
                [In] _WLAN_HOSTED_NETWORK_OPCODE OpCode,
                [In] UInt32 dwDataSize,
                [In] IntPtr pvData,
                [Out] out _WLAN_HOSTED_NETWORK_REASON pFailReason,
                [In, Out] IntPtr pvReserved
                );

            /*************************************************
             *  DWORD WINAPI WlanHostedNetworkQueryProperty(
             *    _In_        HANDLE hClientHandle,
             *    _In_        WLAN_HOSTED_NETWORK_OPCODE OpCode,
             *    _Out_       PDWORD pdwDataSize,
             *    _Out_       PVOID *ppvData,
             *    _Out_       PWLAN_OPCODE_VALUE_TYPE *pWlanOpcodeValueType,
             *    _Reserved_  PVOID pvReserved
             *  );
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanHostedNetworkQueryProperty(
                [In] IntPtr hClientHandle,
                [In] _WLAN_HOSTED_NETWORK_OPCODE OpCode,
                [Out] out UInt32 pdwDataSize,
                [Out] out IntPtr ppvData,
                [Out] out _WLAN_OPCODE_VALUE_TYPE pWlanOpcodeValueType,
                [In, Out] IntPtr pvReserved
                );

            /*************************************************
             *  DWORD WINAPI WlanHostedNetworkForceStart(
             *      _In_        HANDLE hClientHandle,
             *      _Out_opt_   P_WLAN_HOSTED_NETWORK_REASON pFailReason,
             *      _Reserved_  PVOID pvReserved
             *  );
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanHostedNetworkForceStart(
                [In] IntPtr hClientHandle,
                [Out] out _WLAN_HOSTED_NETWORK_REASON pFailReason,
                [In, Out] IntPtr pvReserved
                );

            /*************************************************
             *  DWORD WINAPI WlanHostedNetworkStartUsing(
             *    _In_        HANDLE hClientHandle,
             *    _Out_opt_   PWLAN_HOSTED_NETWORK_REASON pFailReason,
             *    _Reserved_  PVOID pvReserved
             *  ); 
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanHostedNetworkStartUsing(
                [In] IntPtr hClientHandle,
                [Out] out _WLAN_HOSTED_NETWORK_REASON pFailReason,
                [In, Out] IntPtr pvReserved
                );

            /*************************************************
             *  DWORD WINAPI WlanHostedNetworkForceStop(
             *      _In_        HANDLE hClientHandle,
             *      _Out_opt_   P_WLAN_HOSTED_NETWORK_REASON pFailReason,
             *      _Reserved_  PVOID pvReserved
             *  );
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanHostedNetworkForceStop(
                [In] IntPtr hClientHandle,
                [Out] out _WLAN_HOSTED_NETWORK_REASON pFailReason,
                [In, Out] IntPtr pvReserved
                );

            /*************************************************
             *  DWORD WINAPI WlanHostedNetworkStopUsing(
             *      _In_        HANDLE hClientHandle,
             *      _Out_opt_   P_WLAN_HOSTED_NETWORK_REASON pFailReason,
             *      _Reserved_  PVOID pvReserved
             *  );
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanHostedNetworkStopUsing(
                [In] IntPtr hClientHandle,
                [Out] out _WLAN_HOSTED_NETWORK_REASON pFailReason,
                [In, Out] IntPtr pvReserved
                );

            /*************************************************
             *  DWORD WINAPI WlanHostedNetworkQueryStatus(
             *    _In_        HANDLE hClientHandle,
             *    _Out_       PWLAN_HOSTED_NETWORK_STATUS *ppWlanHostedNetworkStatus,
             *    _Reserved_  PVOID pvReserved
             *  );
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern UInt32 WlanHostedNetworkQueryStatus(
                [In] IntPtr hClientHandle,
                [Out] out IntPtr ppWlanHostedNetworkStatus,
                [In, Out] IntPtr pvReserved
                );

            /*************************************************
             *  VOID WINAPI WlanFreeMemory(
             *      _In_  PVOID pMemory
             *  );
            *************************************************/
            [DllImport("Wlanapi.dll", SetLastError = true)]
            public static extern void WlanFreeMemory(
                [In] IntPtr pMemory
                );

            #endregion

            //********************************************************************* NATIVE STRUCTS
            #region native windows structs

            /*************************************************
             *  typedef struct _DOT11_SSID {
             *      ULONG uSSIDLength;
             *      UCHAR ucSSID[DOT11_SSID_MAX_LENGTH];
             *  }
            *************************************************/
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct _DOT11_SSID
            {
                public UInt32 uSSIDLength;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string ucSSID;
            }

            /*************************************************
             *  typedef struct _WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS {
             *      DOT11_SSID hostedNetworkSSID;
             *      DWORD      dwMaxNumberOfPeers;
             *  } 
            *************************************************/
            [StructLayout(LayoutKind.Sequential)]
            public struct _WLAN_HOSTED_NETWORK_CONNECTION_SETTINGS
            {
                public _DOT11_SSID hostedNetworkSSID;
                public UInt32 dwMaxNumberOfPeers;
            }

            /*************************************************
             *  typedef struct _WLAN_HOSTED_NETWORK_STATUS {
             *    WLAN_HOSTED_NETWORK_STATE      HostedNetworkState;
             *    GUID                           IPDeviceID;
             *    DOT11_MAC_ADDRESS              wlanHostedNetworkBSSID;
             *    DOT11_PHY_TYPE                 dot11PhyType;
             *    ULONG                          ulChannelFrequency;
             *    DWORD                          dwNumberOfPeers;
             *    WLAN_HOSTED_NETWORK_PEER_STATE PeerList[1];
             *  }
            *************************************************/
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct _WLAN_HOSTED_NETWORK_STATUS
            {
                public _WLAN_HOSTED_NETWORK_STATE HostedNetworkState;
                public Guid IPDeviceID;
                public _DOT11_MAC_ADDRESS wlanHostedNetworkBSSID;
                public _DOT11_PHY_TYPE dot11PhyType;
                public UInt32 ulChannelFrequency;
                public UInt32 dwNumberOfPeers;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
                public _WLAN_HOSTED_NETWORK_PEER_STATE[] PeerList;
            }

            /*************************************************
             *  typedef UCHAR DOT11_MAC_ADDRESS[6];
            *************************************************/
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct _DOT11_MAC_ADDRESS
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
                public byte[] address;
                /*
                public byte one;
                public byte two;
                public byte three;
                public byte four;
                public byte five;
                public byte six;
                 * */
            }

            /*************************************************
             *  typedef struct _WLAN_HOSTED_NETWORK_PEER_STATE {
             *    _DOT11_MAC_ADDRESS                   PeerMacAddress;
             *    _WLAN_HOSTED_NETWORK_PEER_AUTH_STATE PeerAuthState;
             *  }
            *************************************************/
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct _WLAN_HOSTED_NETWORK_PEER_STATE
            {
                public _DOT11_MAC_ADDRESS PeerMacAddress;
                public _WLAN_HOSTED_NETWORK_PEER_AUTH_STATE PeerAuthState;
            }

            #endregion

            //********************************************************************* NATIVE ENUMS
            #region native windows enums

            /*************************************************
             *  typedef enum _WLAN_HOSTED_NETWORK_OPCODE { 
             *    _WLAN_HOSTED_NETWORK_OPCODE_connection_settings,
             *    _WLAN_HOSTED_NETWORK_OPCODE_security_settings,
             *    _WLAN_HOSTED_NETWORK_OPCODE_station_profile,
             *    _WLAN_HOSTED_NETWORK_OPCODE_enable
             *  }
            *************************************************/
            public enum _WLAN_HOSTED_NETWORK_OPCODE
            {
                _WLAN_HOSTED_NETWORK_OPCODE_connection_settings,
                _WLAN_HOSTED_NETWORK_OPCODE_security_settings,
                _WLAN_HOSTED_NETWORK_OPCODE_station_profile,
                _WLAN_HOSTED_NETWORK_OPCODE_enable
            }

            /*************************************************
             *  typedef enum _WLAN_HOSTED_NETWORK_REASON { 
             *    _WLAN_HOSTED_NETWORK_REASON_success = 0,
             *    _WLAN_HOSTED_NETWORK_REASON_unspecified,
             *    _WLAN_HOSTED_NETWORK_REASON_bad_parameters,
             *    _WLAN_HOSTED_NETWORK_REASON_service_shutting_down,
             *    _WLAN_HOSTED_NETWORK_REASON_insufficient_resources,
             *    _WLAN_HOSTED_NETWORK_REASON_elevation_required,
             *    _WLAN_HOSTED_NETWORK_REASON_read_only,
             *    _WLAN_HOSTED_NETWORK_REASON_persistence_failed,
             *    _WLAN_HOSTED_NETWORK_REASON_crypt_error,
             *    _WLAN_HOSTED_NETWORK_REASON_impersonation,
             *    _WLAN_HOSTED_NETWORK_REASON_stop_before_start,
             *    _WLAN_HOSTED_NETWORK_REASON_interface_available,
             *    _WLAN_HOSTED_NETWORK_REASON_interface_unavailable,
             *    _WLAN_HOSTED_NETWORK_REASON_miniport_stopped,
             *    _WLAN_HOSTED_NETWORK_REASON_miniport_started,
             *    _WLAN_HOSTED_NETWORK_REASON_incompatible_connection_started,
             *    _WLAN_HOSTED_NETWORK_REASON_incompatible_connection_stopped,
             *    _WLAN_HOSTED_NETWORK_REASON_user_action,
             *    _WLAN_HOSTED_NETWORK_REASON_client_abort,
             *    _WLAN_HOSTED_NETWORK_REASON_ap_start_failed,
             *    _WLAN_HOSTED_NETWORK_REASON_peer_arrived,
             *    _WLAN_HOSTED_NETWORK_REASON_peer_departed,
             *    _WLAN_HOSTED_NETWORK_REASON_peer_timeout,
             *    _WLAN_HOSTED_NETWORK_REASON_gp_denied,
             *    _WLAN_HOSTED_NETWORK_REASON_service_unavailable,
             *    _WLAN_HOSTED_NETWORK_REASON_device_change,
             *    _WLAN_HOSTED_NETWORK_REASON_properties_change,
             *    _WLAN_HOSTED_NETWORK_REASON_virtual_station_blocking_use,
             *    _WLAN_HOSTED_NETWORK_REASON_service_available_on_virtual_station
             *  }
            *************************************************/
            public enum _WLAN_HOSTED_NETWORK_REASON
            {
                _WLAN_HOSTED_NETWORK_REASON_success = 0,
                _WLAN_HOSTED_NETWORK_REASON_unspecified,
                _WLAN_HOSTED_NETWORK_REASON_bad_parameters,
                _WLAN_HOSTED_NETWORK_REASON_service_shutting_down,
                _WLAN_HOSTED_NETWORK_REASON_insufficient_resources,
                _WLAN_HOSTED_NETWORK_REASON_elevation_required,
                _WLAN_HOSTED_NETWORK_REASON_read_only,
                _WLAN_HOSTED_NETWORK_REASON_persistence_failed,
                _WLAN_HOSTED_NETWORK_REASON_crypt_error,
                _WLAN_HOSTED_NETWORK_REASON_impersonation,
                _WLAN_HOSTED_NETWORK_REASON_stop_before_start,
                _WLAN_HOSTED_NETWORK_REASON_interface_available,
                _WLAN_HOSTED_NETWORK_REASON_interface_unavailable,
                _WLAN_HOSTED_NETWORK_REASON_miniport_stopped,
                _WLAN_HOSTED_NETWORK_REASON_miniport_started,
                _WLAN_HOSTED_NETWORK_REASON_incompatible_connection_started,
                _WLAN_HOSTED_NETWORK_REASON_incompatible_connection_stopped,
                _WLAN_HOSTED_NETWORK_REASON_user_action,
                _WLAN_HOSTED_NETWORK_REASON_client_abort,
                _WLAN_HOSTED_NETWORK_REASON_ap_start_failed,
                _WLAN_HOSTED_NETWORK_REASON_peer_arrived,
                _WLAN_HOSTED_NETWORK_REASON_peer_departed,
                _WLAN_HOSTED_NETWORK_REASON_peer_timeout,
                _WLAN_HOSTED_NETWORK_REASON_gp_denied,
                _WLAN_HOSTED_NETWORK_REASON_service_unavailable,
                _WLAN_HOSTED_NETWORK_REASON_device_change,
                _WLAN_HOSTED_NETWORK_REASON_properties_change,
                _WLAN_HOSTED_NETWORK_REASON_virtual_station_blocking_use,
                _WLAN_HOSTED_NETWORK_REASON_service_available_on_virtual_station
            }

            /*************************************************
             *  typedef enum _WLAN_HOSTED_NETWORK_STATE { 
             *    wlan_hosted_network_unavailable,
             *    wlan_hosted_network_idle,
             *    wlan_hosted_network_active
             *  }
            *************************************************/
            public enum _WLAN_HOSTED_NETWORK_STATE
            {
                wlan_hosted_network_unavailable,
                wlan_hosted_network_idle,
                wlan_hosted_network_active
            }

            /*************************************************
             *  typedef enum _DOT11_PHY_TYPE { 
             *    dot11_phy_type_unknown     = 0,
             *    dot11_phy_type_any         = 0,
             *    dot11_phy_type_fhss        = 1,
             *    dot11_phy_type_dsss        = 2,
             *    dot11_phy_type_irbaseband  = 3,
             *    dot11_phy_type_ofdm        = 4,
             *    dot11_phy_type_hrdsss      = 5,
             *    dot11_phy_type_erp         = 6,
             *    dot11_phy_type_ht          = 7,
             *    dot11_phy_type_IHV_start   = 0x80000000,
             *    dot11_phy_type_IHV_end     = 0xffffffff
             *  }
            *************************************************/
            public enum _DOT11_PHY_TYPE
            {
                dot11_phy_type_unknown = 0,
                dot11_phy_type_any = 0,
                dot11_phy_type_fhss,
                dot11_phy_type_dsss,
                dot11_phy_type_irbaseband,
                dot11_phy_type_ofdm,
                dot11_phy_type_hrdsss,
                dot11_phy_type_erp,
                dot11_phy_type_ht,
                dot11_phy_type_IHV_start,
                dot11_phy_type_IHV_end
            }

            /*************************************************
             *  typedef enum _WLAN_HOSTED_NETWORK_PEER_AUTH_STATE { 
             *    wlan_hosted_network_peer_state_invalid,
             *    wlan_hosted_network_peer_state_authenticated
             *  }
            *************************************************/
            public enum _WLAN_HOSTED_NETWORK_PEER_AUTH_STATE
            {
                wlan_hosted_network_peer_state_invalid,
                wlan_hosted_network_peer_state_authenticated
            }

            /*************************************************
             *  typedef enum _WLAN_OPCODE_VALUE_TYPE { 
             *    wlan_opcode_value_type_query_only           = 0,
             *    wlan_opcode_value_type_set_by_group_policy  = 1,
             *    wlan_opcode_value_type_set_by_user          = 2,
             *    wlan_opcode_value_type_invalid              = 3
             *  }
            *************************************************/
            public enum _WLAN_OPCODE_VALUE_TYPE
            {
                wlan_opcode_value_type_query_only,
                wlan_opcode_value_type_set_by_group_policy,
                wlan_opcode_value_type_set_by_user,
                wlan_opcode_value_type_invalid
            }

            #endregion

            #region "iphlapi"

            [DllImport("iphlpapi.dll", SetLastError = true)]
            public static extern UInt32 GetIpNetTable(
                 IntPtr pTcpTable,
                 ref int pdwSize,
                 bool bOrder
                );

            #endregion

            #region "structs"

            public struct MIB_IPNETROW
            {
                public int Index;
                public int PhysAddrLen;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public byte[] PhysAddr;
                public int Addr;
                public int Type;
            }

            #endregion

        }
    }
}
