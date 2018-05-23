using Autofac;
using DzTree.Wlan.UWP.Services;
using DzTree.Wlan.UWP.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.Wlan.UWP
{
    internal class Bootstrap
    {
        public Bootstrap()
        {
            Register(EngineContext.ContainerManager);
        }

        private void Register(ContainerBuilder builder)
        {
            builder.RegisterType<SoftAp>().As<ISoftAp>().SingleInstance();
            builder.RegisterType<MainViewModel>(); 
            EngineContext.Container = builder.Build();
        }

    }
}
