using Sentry.Configuration;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sentry.data.Core
{
    public class DfsSource : DataSource
    {
        public DfsSource()
        {
            //https://github.com/nhibernate/nhibernate-core/blob/466ee0d29b19e1b77b734791e4bd061d58c52a6b/src/NHibernate/Type/UriType.cs
            Uri u = new Uri((Config.GetHostSetting("FileShare")));
            BaseUri = new Uri(u.ToString());
            KeyCode = Guid.NewGuid().ToString().Substring(0,13);

            //Control auth types which can be chosen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidAuthTypes = new List<AuthenticationType>();
            ValidAuthTypes.Add(new BasicAuthentication());

            //Default created and modified to same datetime value
            DateTime curDTM = DateTime.Now;
            Created = curDTM;
            Modified = curDTM;
        }

        public override List<AuthenticationType> ValidAuthTypes { get; set; }
        public override bool IsUriEditable { get; set; }
        public override Uri CalcRelativeUri(RetrieverJob Job, NamedEnvironmentType datasetEnvironmentType, string CLA4260_QuartermasterNamedEnvironmentTypeFilter)
        {
            return new Uri(Path.Combine(BaseUri.AbsolutePath, Job.RelativeUri).ToString());
        }

        public override string GetDropPrefix(RetrieverJob Job)
        {
            throw new NotImplementedException();
        }

        public override void Validate(RetrieverJob job, ValidationResults validationResults)
        {
            throw new NotImplementedException();
        }
    }
}
