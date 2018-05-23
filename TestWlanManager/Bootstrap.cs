using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWlanManager.PopWindows;
using TestWlanManager.ViewModel;
using TestWlanManager.WlanApi;

namespace TestWlanManager
{
    public class Bootstrap
    {
        public Bootstrap()
        {
            Register(EngineContext.ContainerManager);
        }

        private void Register(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindow>().InstancePerLifetimeScope();
            builder.RegisterType<MainViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<PacpWindow>().InstancePerLifetimeScope();
            builder.RegisterType<PacpViewModel>().InstancePerLifetimeScope();
            
            EngineContext.Container= builder.Build();
    }

    }
}
