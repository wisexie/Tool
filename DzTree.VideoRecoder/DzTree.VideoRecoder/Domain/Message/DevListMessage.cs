using DzTree.VideoRecoder.Domain.Dev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.VideoRecoder.Domain.Message
{
    public class DevListMessage
    {
        public string Msg { get; set; }

        public VideoAndAudioDev Data { get; set; }


    }
}
