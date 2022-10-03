using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Livy
{
    public class LivyBatch
    {
        public int Id { get; set; }
        public string Appid { get; set; }
        public Dictionary<string, string> AppInfo { get; set; }
        public string[] Log { get; set; }
        public string State { get; set; }
    }
}
