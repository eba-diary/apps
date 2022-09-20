using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Livy
{

    /*{"id":61,
     * "code":"tmp_976e163bafaf490aa1d6d87547f4349e = spark.read.format('csv').option('inferSchema', 'true').load('s3a://sentry-dataset-management-np-nr/data-dev/government/quarterly_census_of_employment_and_wages_2.0/304/2018/5/17/1997.annual.singlefile.csv')",
     * "state":"available",
     * "output":
     * {
     *      "status":"ok",
     *      "execution_count":61,
     *      "data":
     *      {
     *          "text/plain":""
     *      }
     * },
     * "progress":1.0}
    */

    public class LivyReply
    {
        public int id { get; set; }
        public string appId { get; set; }
        public string code { get; set; }
        public string state { get; set; }
        public LivyOutput output { get; set; }
        public float progress { get; set; }
        public string livyURL { get; set; }
        public Dictionary<string,string> appInfo { get; set; }
        public string[] log { get; set; }
        public bool IsActive()
        {
            if (state == "dead" || state == "error" || state == "success")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }

    public class LivyOutput
    {
        public string status { get; set; }
        public int execution_count { get; set; }
        public LivyData data { get; set; }
    }

    public class LivyData
    {
        [JsonProperty("text/plain")]
        public string text { get; set; }
    }
}
