using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NETCONLib;
using System.Windows;
using Autofac;

namespace TestWlanManager.WlanApi
{
    public class WlanShare
    {
        /// <summary>
        /// 设置网络共享
        /// </summary>
        public static void ShareStart()
        {
            var icsManager = EngineContext.Container.Resolve<IcsManager>();
            try
            {
                Guid publicConnectionGuid = (from c in icsManager.Connections
                                              where !c.props.DeviceName.ToLowerInvariant().Contains("virtual") && c.props.Status == tagNETCON_STATUS.NCS_CONNECTED 
                                             select c.Guid).FirstOrDefault();

                Guid privateConnectionGuid = (from c in icsManager.Connections
                                         where c.props.DeviceName.ToLowerInvariant().Contains("microsoft virtual wifi miniport adapter") // Windows 7
                                         || c.props.DeviceName.ToLowerInvariant().Contains("microsoft hosted network virtual adapter") // Windows 8
                                         select c.Guid).FirstOrDefault();
                icsManager.EnableIcs(publicConnectionGuid, privateConnectionGuid);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// 停止网络共享
        /// </summary>
        public static void DisShareStart()
        {
            try
            {
                //var manager = new NetSharingManager();
                //var connections = manager.EnumEveryConnection;
                //if (connections != null)
                //{
                //    foreach (INetConnection c in connections)
                //    {
                //        try
                //        {
                //            var props = manager.NetConnectionProps[c];
                //            var sharingCfg = manager.INetSharingConfigurationForINetConnection[c];

                //            if (props.Status == tagNETCON_STATUS.NCS_CONNECTED && !props.DeviceName.ToLower().Contains("virtual"))
                //            {
                //                sharingCfg.DisableSharing();
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //        }
                //   }
                // }
                var icsManager = EngineContext.Container.Resolve<IcsManager>();
                icsManager.DisableIcsOnAll();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
