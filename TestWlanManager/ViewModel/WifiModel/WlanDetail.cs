using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWlanManager.ViewModel.WifiModel
{
    public class WlanStatusInfo : ObservableObject
    {

        private string _MAC;

        public string MAC
        {
            get
            {
                return _MAC;
            }

            set
            {

                _MAC = value;
                this.RaisePropertyChanged(() => MAC);

            }
        }


        private bool _Authenticated;

        public bool Authenticated
        {
            get
            {
                return _Authenticated;
            }

            set
            {

                _Authenticated = value;
                this.RaisePropertyChanged(() => Authenticated);

            }
        }


        private string _HostName;

        public string HostName
        {
            get
            {
                return _HostName;
            }

            set
            {

                _HostName = value;
                this.RaisePropertyChanged(() => HostName);

            }
        }


        private string _IpAddress;

        public string IpAddress
        {
            get
            {
                return _IpAddress;
            }

            set
            {

                _IpAddress = value;
                this.RaisePropertyChanged(() => IpAddress);

            }
        }
    }

}
