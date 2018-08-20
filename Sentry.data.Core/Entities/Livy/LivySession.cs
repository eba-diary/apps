using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities
{
    public class LivySessionList
    {
        public int from { get; set; }
        public int total { get; set; }
        public List<LivySession> sessions { get; set; }
    }

    public class LivySession
    {
        public int id { get; set; }
        public string appId { get; set; }
        public string owner { get; set; }
        public string proxyUser { get; set; }
        public string kind { get; set; }

        //public string log { get; set; }
        public string state { get; set; }

        //public string appInfo { get; set; }

        public string livyURL { get; set; }
    }

    public class LivyCreation
    {
        public virtual int ID { get; set; }
        public virtual int LivySession_ID { get; set; }

        [JsonProperty("name")]
        public virtual string Session_NME { get; set; }

        [JsonProperty("proxyUser")]
        public virtual string Associate_ID { get; set; }
        public virtual Boolean Active_IND { get; set; }
        public virtual DateTime Start_DTM { get; set; }
        public virtual DateTime End_DTM { get; set; }
        public virtual Boolean ForDSC_IND { get; set; }

        [JsonProperty("kind")]
        public virtual string Kind { get; set; }

        [JsonProperty("jars")]
        public virtual List<String> Jars { get; set; }

        [JsonProperty("pyFiles")]
        public virtual List<string> PyFiles { get; set; }

        [JsonProperty("driverMemory")]
        public virtual string DriverMemory { get; set; }

        [JsonProperty("driverCores")]
        public virtual int DriverCores { get; set; }

        [JsonProperty("executorMemory")]
        public virtual string ExecutorMemory { get; set; }

        [JsonProperty("executorCores")]
        public virtual int ExecutorCores { get; set; }

        [JsonProperty("numExecutors")]
        public virtual int NumExecutors { get; set; }

        [JsonProperty("queue")]
        public virtual string Queue { get; set; }

        [JsonProperty("heartbeatTimeoutInSecond")]
        public virtual int HeartbeatTimeoutInSecond { get; set; }
    }
}
