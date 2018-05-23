using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWlanManager.Helper
{
    public class ArpHelper
    {
        public static string CovnertMacToStr(byte[] m)
        {
            string result = string.Empty;
            if (m!=null&&m.Length > 4)
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
