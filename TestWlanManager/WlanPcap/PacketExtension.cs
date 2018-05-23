using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWlanManager.Helper;

namespace TestWlanManager.WlanPcap
{
    public static class PacketExtension
    {
        public static T GetPacketType<T>(this Packet packet) where T : Packet
        {
            return packet.Extract(typeof(T)) as T;
        }


        public static bool IsAddPacket(this Packet packet)
        {
            bool result = false;
            TcpPacket ipPacket = packet.GetPacketType<TcpPacket>();
            if (ipPacket != null)
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 截获包基本信息
        /// </summary>
        /// <returns></returns>
        public static DevicePacket ConvertPacketToDevice(this Packet packet)
        {
            DevicePacket devPacket = new DevicePacket();

            devPacket.Protocol = "未知";
            devPacket.DataLen = packet.Bytes.Length;

            EthernetPacket ethernetPacket = packet.GetPacketType<EthernetPacket>() as EthernetPacket;
            if (ethernetPacket != null)
            {
                devPacket = packet.GetEthernetPacket(ethernetPacket, devPacket);
                return devPacket;

            }

            #region "暂时不处理协议"

            //PPPPacket ppp = packet.GetPacketType<PPPPacket>();
            //if (ppp != null)
            //{
            //    devPacket.Protocol = "PPP";
            //    devPacket.SourceAddress = "---";
            //    devPacket.DestinationAddress = "---";
            //    devPacket.Desc = $"协议类型:{ppp.Protocol.ToString()}";

            //    return devPacket;
            //}

            #endregion


            return devPacket;
        }

        public static DevicePacket GetEthernetPacket(this Packet packet, EthernetPacket ethernetPacket, DevicePacket devPacket)
        {

            //如果是IP类型
            IpPacket ipPacket = packet.GetPacketType<IpPacket>();
            if (ipPacket != null)
            {
                if (ipPacket.Version == IpVersion.IPv4)
                {
                    devPacket.Protocol = "IPv4";
                }
                else
                {
                    devPacket.Protocol = "IPv6";
                }
                devPacket.SourceAddress = ipPacket.SourceAddress.ToString();
                devPacket.DestinationAddress = ipPacket.DestinationAddress.ToString();
                devPacket.Desc = $"[下层协议:{ipPacket.NextHeader.ToString()}[版本:{ipPacket.Version.ToString()}]";


                TcpPacket tcpPacket = packet.GetPacketType<TcpPacket>();

                if (tcpPacket != null)
                {
                    devPacket.Protocol = "TCP";
                    devPacket.SourcePort = tcpPacket.SourcePort.ToString();
                    devPacket.DestinationPort = tcpPacket.DestinationPort.ToString();
                    return devPacket;
                }

                UdpPacket udpPacket = packet.GetPacketType<UdpPacket>();
                if (udpPacket != null)
                {
                    devPacket.Protocol = "UDP";
                    devPacket.SourcePort = udpPacket.SourcePort.ToString();
                    devPacket.DestinationPort = udpPacket.DestinationPort.ToString();
                    return devPacket;
                }

                #region "暂时不处理协议"

                //ICMPv4Packet icmpv4Packet = packet.GetPacketType<ICMPv4Packet>();
                //if (icmpv4Packet != null)
                //{
                //    devPacket.Protocol = "ICMPv4";
                //    devPacket.Desc = $"校验:{icmpv4Packet.Checksum.ToString()},类型:{icmpv4Packet.TypeCode.ToString()},序列号:{icmpv4Packet.Sequence.ToString()}";

                //    return devPacket;
                //}

                //ICMPv6Packet icmpv6Packet = packet.GetPacketType<ICMPv6Packet>();
                //if (icmpv6Packet != null)
                //{
                //    devPacket.Protocol = "ICMPv6";
                //    devPacket.Desc = $"Code:{icmpv6Packet.Code.ToString()},Type:{icmpv6Packet.Type.ToString()}";

                //    return devPacket;
                //}

                //IGMPv2Packet imgpv2Packet = packet.GetPacketType<IGMPv2Packet>();
                //if (imgpv2Packet != null)
                //{
                //    devPacket.Protocol = "IGMPv2";
                //    devPacket.Desc = $"只适用于IGMPv2,组地址:{imgpv2Packet.GroupAddress.ToString()},类型:{imgpv2Packet.Type.ToString()}";

                //    return devPacket;
                //}

                #endregion

                return devPacket;
            }


            #region "暂时不处理协议"

            //ARPPacket arpPacket = packet.GetPacketType<ARPPacket>();
            //if (arpPacket != null)
            //{
            //    devPacket.Protocol = "ARP";
            //    devPacket.SourceAddress = ArpHelper.CovnertMacToStr(arpPacket.SenderHardwareAddress.GetAddressBytes());
            //    devPacket.DestinationAddress = ArpHelper.CovnertMacToStr(arpPacket.TargetHardwareAddress.GetAddressBytes());
            //    devPacket.Desc = $"Arp操作方式:{arpPacket.Operation.ToString()},发送者:{arpPacket.SenderProtocolAddress.ToString()},目标:{arpPacket.TargetProtocolAddress.ToString()}";
            //    return devPacket;
            //}

            //WakeOnLanPacket wakeOnLanPacket = packet.GetPacketType<WakeOnLanPacket>();
            //if (wakeOnLanPacket != null)
            //{
            //    devPacket.Protocol = "Wake On Lan";
            //    devPacket.DestinationAddress = ArpHelper.CovnertMacToStr(wakeOnLanPacket.DestinationMAC.GetAddressBytes());
            //    devPacket.Desc = $"唤醒网络地址:{wakeOnLanPacket.DestinationMAC.ToString()},有效性:{wakeOnLanPacket.IsValid().ToString()}";
            //    return devPacket;
            //}

            //PPPoEPacket pPPoEPacket = packet.GetPacketType<PPPoEPacket>();
            //if (pPPoEPacket != null)
            //{
            //    devPacket.Protocol = "PPPoE";
            //    devPacket.Desc = $"{pPPoEPacket.Type.ToString()},{pPPoEPacket.Version.ToString()}";
            //    return devPacket;
            //}

            #endregion

            return devPacket;
        }
    }
}
