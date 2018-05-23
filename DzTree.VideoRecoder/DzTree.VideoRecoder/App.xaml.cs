using DzTree.VideoRecoder.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace DzTree.VideoRecoder
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            AppBootstrappe boot = new AppBootstrappe();
            Type type = Assembly.Load("DzTree.VideoRecoder").GetType("DzTree.VideoRecoder.IMainWindowViewModel");
            boot.OnStartup(type);
        }
    }
}
