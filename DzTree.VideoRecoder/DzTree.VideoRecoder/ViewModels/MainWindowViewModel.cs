using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autofac;
using System.Reflection;
using Caliburn.Micro;
using DzTree.VideoRecoder.Core;

namespace DzTree.VideoRecoder
{
    public class MainWindowViewModel : Screen, IMainWindowViewModel, IHaveDisplayName
    {
        public MainWindowViewModel()
        {
            this.DisplayName = "小树视频处理";
        }
    

        public void ShowContent(object item)
        {
            if (item != null)
            {
                object tag = (item as TreeViewItem).Tag;
                StrTabItemHeader = (item as TreeViewItem).Header.ToString();
                if (tag != null)
                {
                    using (var scope = AutoFacModel.Container.BeginLifetimeScope())
                    {
                        object viewModel = "";
                        string[] arrTag = tag.ToString().Split(',');
                        Type type = Assembly.Load(arrTag[1]).GetType(arrTag[0]);
                        if (arrTag.Length > 2)
                        {
                             viewModel = scope.ResolveNamed(arrTag[2], type);
                        }
                        else
                        {
                            viewModel = scope.Resolve(type);
                        }

                        contentControl = viewModel;
                    }
                }
            }
        }

        private object _contentControl;
        public object contentControl{
            get
            {
                return _contentControl;
            }
            set
            {
                _contentControl = value;
                NotifyOfPropertyChange(() => contentControl);
            }
        }

        private string _StrTabItemHeader;

        public string StrTabItemHeader
        {
            get
            {
                return _StrTabItemHeader;
            }
            set
            {
                _StrTabItemHeader = value;
                NotifyOfPropertyChange(() => StrTabItemHeader);
            }
        }
    }
}
