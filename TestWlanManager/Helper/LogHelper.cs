using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestWlanManager.Helper
{
    public class LogHelper
    {
        public static void ShowErrorMessage(Exception ex)
        {
            string result = string.Empty;
            if (ex != null)
            {
                result += "错误：" + ex.Message;
                if (ex.InnerException != null)
                {
                    result += "\r\n";
                    result += "详情：" + ex.InnerException.Message;
                }

            }

            MessageBox.Show(result);
        }
    }
}
