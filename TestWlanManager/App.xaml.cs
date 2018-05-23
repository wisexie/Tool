using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TestWlanManager
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Bootstrap boot = new Bootstrap();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception)
            {

              
            }
           
        }

    }
}
