using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using TestWlanManager.WlanPcap;

namespace TestWlanManager.PopWindows
{
    /// <summary>
    /// PacpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PacpWindow : Window
    {
        DispatcherTimer _timer;

        public PacpWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 4);
            _timer.Tick += _timer_Tick; ;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            this.Packets.Items.Clear();
            var datas=  DevicePacp.Instance.DeviceSource.ToList();
            foreach (var item in datas)
            {
                this.Packets.Items.Add(item);
            }
           
        }
    }
}
