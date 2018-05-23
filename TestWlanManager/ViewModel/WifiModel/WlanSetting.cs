using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWlanManager.ViewModel.WifiModel
{
    public class WlanSetting: ObservableObject
    {
        private string _Name;

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


        private string _Password;

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
