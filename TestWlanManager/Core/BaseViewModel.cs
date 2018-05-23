using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestWlanManager
{
    public class BaseViewModel : ViewModelBase
    {
        private RelayCommand _loadedCommand;
        private RelayCommand _unloadedCommand;

        public BaseViewModel()
        {
            InitCommand();
        }

        public ICommand LoadedCommand
        {
            get { return _loadedCommand ?? (_loadedCommand = new RelayCommand(Loaded, () => true)); }
        }

        public ICommand UnloadedCommand
        {
            get { return _unloadedCommand ?? (_unloadedCommand = new RelayCommand(Unloaded, () => true)); }
        }

        protected virtual void InitCommand()
        {

        }

        protected virtual void Loaded()
        {
            if (!isLoad)
            {
                RegisterMessage();
                isLoad = true;
            }

        }

        protected virtual void Unloaded()
        {
            Dispose();
        }

        /// <summary>
        /// 默认消息
        /// </summary>
        public virtual void MessageLoad() { Loaded(); }


        public void Dispose()
        {
            UnRegisterMessage();
        }

        #region "Message"

        public virtual void RegisterMessage()
        {

        }

        public virtual void UnRegisterMessage()
        {

        }

        #endregion



        private bool _isBusy;

        /// <summary>
        /// 是否繁忙
        /// </summary>
        public virtual bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged(() => IsBusy);
                }
            }
        }

        public bool isLoad = false;

        public bool IsSuccess = false;
    }
}
