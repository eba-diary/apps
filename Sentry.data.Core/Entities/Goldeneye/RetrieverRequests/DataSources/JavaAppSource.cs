using Newtonsoft.Json;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;

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

        public override Uri CalcRelativeUri(RetrieverJob Job, NamedEnvironmentType datasetEnvironmentType, string CLA4260_QuartermasterNamedEnvironmentTypeFilter)
        {
            return null;
        }

        public override string GetDropPrefix(RetrieverJob Job)
        {
            throw new NotImplementedException();
        }

        public override void Validate(RetrieverJob job, ValidationResults validationResults)
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
