using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Sentry.data.Core
{
    public class ApplicationConfiguration
    {
        public virtual int Id { get; set; }
        public virtual string Application { get; set; }
        public virtual string Options { get; set; }
        public virtual JObject OptionsObject
        {
            get
            {
                if (String.IsNullOrEmpty(Options))
                {
                    return null;
                }
                else
                {
                    return JObject.Parse(Options);                    
                }
            }
        }
        public virtual DateTime Created { get; set; }
        public virtual DateTime Modified { get; set; }
    }
}
