using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.Wlan.UWP.WlanApi
{
    public class IpConfig
    {
        public const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
        public const int ERROR_BUFFER_OVERFLOW = 111;
        public const int MAX_ADAPTER_NAME_LENGTH = 256;
        public const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        public const int MIB_IF_TYPE_OTHER = 1;
        public const int MIB_IF_TYPE_ETHERNET = 6;
        public const int MIB_IF_TYPE_TOKENRING = 9;
        public const int MIB_IF_TYPE_FDDI = 15;
        public const int MIB_IF_TYPE_PPP = 23;
        public const int MIB_IF_TYPE_LOOPBACK = 24;
        public const int MIB_IF_TYPE_SLIP = 28;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct IP_ADAPTER_INFO
    {
        public IntPtr Next;
        public Int32 ComboIndex;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = IpConfig.MAX_ADAPTER_NAME_LENGTH + 4)]
        public string AdapterName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = IpConfig.MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
        public string AdapterDescription;
        public UInt32 AddressLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpConfig.MAX_ADAPTER_ADDRESS_LENGTH)]
        public byte[] Address;
        public Int32 Index;
        public UInt32 Type;
        public UInt32 DhcpEnabled;
        public IntPtr CurrentIpAddress;
        public IP_ADDR_STRING IpAddressList;
        public IP_ADDR_STRING GatewayList;
        public IP_ADDR_STRING DhcpServer;
        public bool HaveWins;
        public IP_ADDR_STRING PrimaryWinsServer;
        public IP_ADDR_STRING SecondaryWinsServer;
        public Int32 LeaseObtained;
        public Int32 LeaseExpires;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct IP_ADDR_STRING
    {
        public IntPtr Next;
        public IP_ADDRESS_STRING IpAddress;
        public IP_ADDRESS_STRING IpMask;
        public Int32 Context;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct IP_ADDRESS_STRING
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Address;
    }

    public class AdapterInfo
    {
        public string Type { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string MAC { get; set; }
    }

    public class IpApi
    {
        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);


        public static List<AdapterInfo> GetAdapters()
        {
            var adapters = new List<AdapterInfo>();

            long structSize = Marshal.SizeOf<IP_ADAPTER_INFO>();
            IntPtr pArray = Marshal.AllocHGlobal(new IntPtr(structSize));

            int ret = GetAdaptersInfo(pArray, ref structSize);

            if (ret == IpConfig.ERROR_BUFFER_OVERFLOW) // ERROR_BUFFER_OVERFLOW == 111
            {
                // Buffer was too small, reallocate the correct size for the buffer.
                pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));

                ret = GetAdaptersInfo(pArray, ref structSize);
            }

            if (ret == 0)
            {
                // Call Succeeded
                IntPtr pEntry = pArray;

                do
                {
                    var adapter = new AdapterInfo();

                    // Retrieve the adapter info from the memory address
                    var entry = (IP_ADAPTER_INFO)Marshal.PtrToStructure<IP_ADAPTER_INFO>(pEntry);

                    // Adapter Type
                    switch (entry.Type)
                    {
                        case IpConfig.MIB_IF_TYPE_ETHERNET:
                            adapter.Type = "Ethernet";
                            break;
                        case IpConfig.MIB_IF_TYPE_TOKENRING:
                            adapter.Type = "Token Ring";
                            break;
                        case IpConfig.MIB_IF_TYPE_FDDI:
                            adapter.Type = "FDDI";
                            break;
                        case IpConfig.MIB_IF_TYPE_PPP:
                            adapter.Type = "PPP";
                            break;
                        case IpConfig.MIB_IF_TYPE_LOOPBACK:
                            adapter.Type = "Loopback";
                            break;
                        case IpConfig.MIB_IF_TYPE_SLIP:
                            adapter.Type = "Slip";
                            break;
                        default:
                            adapter.Type = "Other/Unknown";
                            break;
                    } // switch

                    adapter.Name = entry.AdapterName;
                    adapter.Description = entry.AdapterDescription;

                    // MAC Address (data is in a byte[])
                    adapter.MAC = string.Join("-", Enumerable.Range(0, (int)entry.AddressLength).Select(s => string.Format("{0:X2}", entry.Address[s])));

                    // Get next adapter (if any)

                    adapters.Add(adapter);

                    pEntry = entry.Next;
                }
                while (pEntry != IntPtr.Zero);

                Marshal.FreeHGlobal(pArray);
            }
            else
            {
                Marshal.FreeHGlobal(pArray);
                throw new InvalidOperationException("GetAdaptersInfo failed: " + ret);
            }

            return adapters;
        }


        public static string CovnertMacToStr(byte[] m)
        {
            string result = string.Empty;
            if (m != null && m.Length > 4)
            {
                result = string.Format("{0:x2}-{1:x2}-{2:x2}-{3:x2}-{4:x2}-{5:x2}", m[0], m[1], m[2], m[3], m[4], m[5]);
            }
            return result;
        }

        public static string ConvertIpToStr(int addr)
        {
            var b = BitConverter.GetBytes(addr);
            return string.Format("{0}.{1}.{2}.{3}", b[0], b[1], b[2], b[3]);
        }
        public static string ConvertMacStrToStr(string m)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(m) && m.Length > 11)
            {
                result = string.Format("{0:x2}-{1:x2}-{2:x2}-{3:x2}-{4:x2}-{5:x2}", m[0].ToString() + m[1].ToString(), m[2].ToString() + m[3].ToString(), m[4].ToString() + m[5].ToString(), m[6].ToString() + m[7].ToString(), m[8].ToString() + m[9].ToString(), m[10].ToString() + m[11].ToString());
            }
            return result;
        }
    }
}
