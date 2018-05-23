using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestWlanManager.Helper
{
    public class DnsHelper
    {
        public static string GetHostName(string ipAddress)
        {
            string result = string.Empty;
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                try
                {
                    var host = Dns.GetHostEntry(ipAddress);
                    result = host.HostName.Split('.').FirstOrDefault();
                }
                catch (Exception)
                {
                    result = "---";
                }
  

            }
            return result;
        }
    }
}
