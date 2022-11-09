using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Configuration;
using System.IO;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    /// <summary>
    /// This class is for data.sentry.com controlled DFS drop locations.  Provides 
    /// </summary>
    public class DfsBasic : DfsSource
    {
        public DfsBasic()
        {
            IsUriEditable = false;
            BaseUri = new Uri($"{BaseUri.ToString()}DatasetLoader/");
        }

        //Setting Discriminator Value for NHibernate
        public override string SourceType
        {
            get
            {
                return GlobalConstants.DataSourceDiscriminator.DEFAULT_DROP_LOCATION;
            }
        }
        public override Uri CalcRelativeUri(RetrieverJob job, NamedEnvironmentType datasetEnvironmentType, string CLA4260_QuartermasterNamedEnvironmentTypeFilter)
        {
            string cat = job.DatasetConfig.ParentDataset.DatasetCategories.First().Name.ToLower();
            string dsname = job.DatasetConfig.ParentDataset.DatasetName.Replace(' ', '_').ToLower();
            string dfcname = job.DatasetConfig.Name.Replace(' ', '_').ToLower();

            Uri u = new Uri(Path.Combine(new string[] { BaseUri.ToString(), cat, dsname, dfcname }).ToString());

            return u;
        }        
    }
}
