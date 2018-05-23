using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.VideoRecoder.Domain.Dev
{
    public class VideoAndAudioDev
    {
        public VideoAndAudioDev()
        {
            Videos = new List<VideoDev>();
            Audios = new List<AudioDev>();
        }
        public List<VideoDev> Videos { get; set; }

        public List<AudioDev> Audios { get; set; }
    }
}
