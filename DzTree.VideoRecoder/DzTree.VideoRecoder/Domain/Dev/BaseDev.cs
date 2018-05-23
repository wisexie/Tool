using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.VideoRecoder.Domain.Dev
{
    public class BaseDev
    {
        /// <summary>
        /// 设备名 
        /// </summary>
        public string DevName { get; set; }

        /// <summary>
        /// 设备标识 
        /// </summary>
        public string DevId { get; set; }
    }

    /// <summary>
    /// 视频设备
    /// </summary>
    public class VideoDev : BaseDev
    { }

    /// <summary>
    /// 音频设备
    /// </summary>
    public class AudioDev : BaseDev
    { }
}
