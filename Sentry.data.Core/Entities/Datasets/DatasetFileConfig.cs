using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Sentry.data.Core.Entities.Metadata;

namespace Sentry.data.Core
{
    public class DatasetFileConfig
    {
        public DatasetFileConfig() {}        
 
        
        public virtual int ConfigId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int FileTypeId { get; set; }
        public virtual Dataset ParentDataset { get; set; }
        public virtual DatasetScopeType DatasetScopeType { get; set; }

        public virtual IList<DatasetFile> DatasetFiles { get; set; }
        public virtual IList<RetrieverJob> RetrieverJobs { get; set; }
        public virtual FileExtension FileExtension { get; set; }
        public virtual IList<DataElement> Schema { get; set; }

        /// <summary>
        /// Return path to current file
        /// </summary>
        public virtual Uri GetCurrentFileDir()
        {
            //This is a Short-Term solution to maintaining a current file for access via SAS tool.

            //Need to add category, dataset name, and config name, to ensure we can delete all 
            // contents of directory to ensure only one file.
            var path = System.IO.Path.Combine(
                    Configuration.Config.GetHostSetting("PushToSASTargetPath"),
                    "current_files",
                    ParentDataset.DatasetCategories.First().Name.ToLower(),
                    ParentDataset.DatasetName.Replace(' ', '_').ToLower(),
                    Name.Replace(' ', '_').ToLower()
                    );

            return new Uri(path);
              
        }

        public virtual DataElement GetLatestSchemaRevision()
        {
            return Schema.OrderByDescending(o => o.SchemaRevision).Take(1).SingleOrDefault();
        }
        public virtual string GetStorageCode()
        {
            if (ParentDataset.DatasetType == GlobalConstants.DataEntityTypes.DATASET)
            {
                return GetLatestSchemaRevision().StorageCode;
            }
            else
            {
                return null;
            }
        }
    }
}
