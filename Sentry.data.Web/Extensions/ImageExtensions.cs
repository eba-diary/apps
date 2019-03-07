
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public static class ImageExtensions
    {
        public static void GenerateStorageInfo(this Core.ImageDto dto)
        {
            var bucket = Configuration.Config.GetHostSetting("AWSRootBucket");
            var prefix = "/" + Sentry.data.Core.GlobalConstants.StoragePrefixes.DATASET_IMAGE_STORAGE_PREFIX + "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + System.Guid.NewGuid().ToString();

            dto.StorageBucketName = bucket;
            dto.StoragePrefix = prefix;
            dto.StorageKey = prefix;
        }
    }
}