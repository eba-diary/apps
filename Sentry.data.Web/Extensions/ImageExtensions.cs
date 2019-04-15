
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
            var prefix = Sentry.data.Core.GlobalConstants.StoragePrefixes.DATASET_IMAGE_STORAGE_PREFIX;
            prefix += "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + System.Guid.NewGuid().ToString();

            dto.StorageBucketName = bucket;
            dto.StoragePrefix = prefix;
            dto.StorageKey = prefix;
        }

        public static List<ImageModel> ToModel(this List<Core.ImageDto> dto)
        {
            List<ImageModel> modelList = new List<ImageModel>();
            if (dto != null && dto.Any())
            {
                foreach (Core.ImageDto img in dto.OrderBy(o => o.sortOrder))
                {
                    modelList.Add(img.ToModel());
                }
            }
            return modelList;
        }

        public static ImageModel ToModel(this Core.ImageDto dto)
        {
            return new ImageModel()
            {
                ImageId = dto.ImageId,
                StorageKey = dto.StorageKey,
                sortOrder = dto.sortOrder,
                ContentType = dto.ContentType,
                FileExtension = dto.FileExtension,
                StorageBucketName = dto.StorageBucketName,
                StoragePrefix = dto.StoragePrefix,
                FileName = dto.FileName
            };
        }
    }
}