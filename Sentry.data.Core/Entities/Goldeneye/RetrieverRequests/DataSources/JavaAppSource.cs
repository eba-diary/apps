using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class JavaAppSource : DataSource
    {
        private string _options;

        public JavaAppSource()
        {
            SourceAuthType = new AnonymousAuthentication();
            IsUserPassRequired = false;
            PortNumber = 0;
            KeyCode = Guid.NewGuid().ToString().Substring(0, 13);
            Created = DateTime.Now;
            Modified = DateTime.Now;
        }

        public override string SourceType
        {
            get
            {
                return GlobalConstants.DataSourceDiscriminator.JAVA_APP_SOURCE;
            }
        }

        public override Uri CalcRelativeUri(RetrieverJob Job)
        {
            return null;
        }

        public override string GetDropPrefix(RetrieverJob Job)
        {
            throw new NotImplementedException();
        }

        public virtual SourceOptions Options
        {
            get
            {
                if (String.IsNullOrEmpty(_options))
                {
                    return null;
                }
                else
                {
                    SourceOptions a = JsonConvert.DeserializeObject<SourceOptions>(_options);
                    return a;
                }
            }
            set
            {
                _options = JsonConvert.SerializeObject(value);
            }
        }
    }
}
