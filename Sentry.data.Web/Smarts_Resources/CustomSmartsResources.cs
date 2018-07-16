using System;
using System.Collections.Generic;
using Sentry.Smarts.Resource;
using Hangfire;

namespace Sentry.data.Web.SmartsResources
{
    public class CustomSmartsResources : CustomResource
    {
        public override void RequestResourceStatus()
        {
            List<KeyValuePair<string, DateTime>> heartbeatList = new List<KeyValuePair<string, DateTime>>();
            DateTime HeartbeathThreshold = DateTime.Now.ToUniversalTime().AddMinutes(-5);

            int oldHeartbeatCount = 0;

            try
            {
                Logger.Info("Establishing Hangfire Monitor");

                foreach (Hangfire.Storage.Monitoring.ServerDto server in JobStorage.Current.GetMonitoringApi().Servers())
                {
                    DateTime hbeat;
                    hbeat = server.Heartbeat ?? DateTime.MinValue;

                    heartbeatList.Add(new KeyValuePair<string, DateTime>(server.Name, hbeat));

                    //If heartbeat is older than 5 min, increment oldHeartbeatCounter
                    if ((HeartbeathThreshold - hbeat).TotalSeconds > 0)
                    {
                        oldHeartbeatCount++;
                    }
                    Logger.Info($"Found Hangfire Server - {server.Name} | ServerStarted:{server.StartedAt} | Heartbeat:{server.Heartbeat} | HeartbeatThreshold:{HeartbeathThreshold}" );
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed establishing Hangfire Monitor", ex);
            }
                        

            if (heartbeatList.Count == 0 || oldHeartbeatCount == heartbeatList.Count)
            {
                //either no servers detected, or all servers have heartbeats older than 5 minutes
                SetStatus(ResourceStatus.Down);
            }
            else if (oldHeartbeatCount > 0 && heartbeatList.Count > oldHeartbeatCount)
            {
                //Some of the hangfire servers have heartbeats within the last 5 minutes
                SetStatus(ResourceStatus.Degraded);
            }
            else if (oldHeartbeatCount == 0 && heartbeatList.Count > 0)
            {
                SetStatus(ResourceStatus.Up);
            }
            else
            {
                SetStatus(ResourceStatus.Unknown);
            }
        }
    }
}