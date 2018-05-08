using System;
using System.IO;
using Sentry.Configuration;


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

            //Default created and modified to same datetime value
            DateTime curDTM = DateTime.Now;
            Created = curDTM;
            Modified = curDTM;
        }

        public override bool IsUriEditable { get; set; }
        public override Uri CalcRelativeUri(RetrieverJob Job)
        {
            return new Uri(Path.Combine(BaseUri.AbsolutePath, Job.RelativeUri).ToString());
        }

        public override string GetDropPrefix(RetrieverJob Job)
        {
            throw new NotImplementedException();
        }
    }
}
