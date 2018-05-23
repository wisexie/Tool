using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzTree.Wlan.UWP
{
    public class EngineContext
    {
        private static ContainerBuilder _ContainerManager;

        private static object _LockObject = new object();

        public static IContainer Container;

        public static ContainerBuilder ContainerManager
        {
            get
            {
                if (_ContainerManager == null)
                {
                    lock (_LockObject)
                    {
                        if (_ContainerManager == null)
                        {
                            _ContainerManager = new ContainerBuilder();
                        }

                    }
                }
                return _ContainerManager;
            }
        }
    }
}
