using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.Wlan.UWP.ViewModel.WifiModel
{
    internal class WlanSetting : ObservableObject
    {
        private string _Name="Wise1234";

        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {

                _Name = value;
                this.RaisePropertyChanged(() => Name);

            }
        }


        private string _Password="12345678";

        public string Password
        {
            get
            {
                return _Password;
            }

            set
            {

                _Password = value;
                this.RaisePropertyChanged(() => Password);

            }
        }
    }
}
