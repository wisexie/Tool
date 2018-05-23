using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.VideoRecoder.Helper
{
    public class EncoderHelper
    {
        public static string ASCIToUtf8(string msg)
        {
            Byte[] b=  Encoding.Default.GetBytes(msg);
            Encoding utf8 =  Encoding.GetEncoding("utf-8");
            return utf8.GetString(b);
        }
           
    }
}
