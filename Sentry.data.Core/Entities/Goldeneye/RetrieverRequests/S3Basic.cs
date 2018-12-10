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
            Bucket = Configuration.Config.GetHostSetting("AWSRootBucket");
            string url = Path.Combine($"http://s3-sa-east-1.amazonaws.com/", Bucket, Configuration.Config.GetHostSetting("S3DataPrefix"), "droplocation");
            BaseUri = new Uri(url);
            IsUriEditable = false;
        }

        public override string SourceType
        {
            get
            {
                return "S3Basic";
            }
        }

        public override Uri CalcRelativeUri(RetrieverJob Job)
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
