using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.VideoRecoder.Domain.Dev
{
    public enum DevState
    {
        /// <summary>
        /// 无状态
        /// </summary>
        None,
        /// <summary>
        /// 视频状态
        /// </summary>
        VideoState,

        /// <summary>
        /// 音频状态
        /// </summary>
        AudioState
    }
}
