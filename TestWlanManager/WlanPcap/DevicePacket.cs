using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWlanManager.WlanPcap
{
    public class DevicePacket
    {

        /// <summary>
        /// 协议
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public Int64 DataLen { get; set; }

        /// <summary>
        /// 源地址
        /// </summary>
        public string SourceAddress { get; set; }

        /// <summary>
        /// 源地址端口
        /// </summary>
        public string SourcePort { get; set; }

        /// <summary>
        /// 目标地址
        /// </summary>
        public string DestinationAddress { get; set; }

        /// <summary>
        /// 目标端口
        /// </summary>
        public string DestinationPort { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }
    }
}
