using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class S3Basic : S3Source
    {
        public S3Basic()
        {
            IsUriEditable = false;
        }

        public override string SourceType
        {
            get
            {
                return GlobalConstants.DataSourceDiscriminator.DEFAULT_S3_DROP_LOCATION;
            }
        }

        public override Uri CalcRelativeUri(RetrieverJob Job, NamedEnvironmentType datasetEnvironmentType, string CLA4260_QuartermasterNamedEnvironmentTypeFilter)
        {
            string storageCode = Job.DatasetConfig.GetStorageCode();
            //string cat = Job.DatasetConfig.ParentDataset.DatasetCategory.Id.ToString();
            //string dsname = Job.DatasetConfig.ParentDataset.DatasetId.ToString();
            //string dfcname = Job.DatasetConfig.ConfigId.ToString();

            Uri u = new Uri(Path.Combine(new string[] { BaseUri.ToString(), storageCode }).ToString());

            return u;
        }
        public override string GetDropPrefix(RetrieverJob Job)
        {
            string storageCode = Job.DatasetConfig.GetStorageCode();
            //string cat = Job.DatasetConfig.ParentDataset.DatasetCategory.Id.ToString();
            //string dsname = Job.DatasetConfig.ParentDataset.DatasetId.ToString();
            //string dfcname = Job.DatasetConfig.ConfigId.ToString();

            string url = $"{Configuration.Config.GetHostSetting("S3DataPrefix")}droplocation/{storageCode}/";

            return url;
        }
    }
}
