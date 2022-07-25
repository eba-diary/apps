using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class DatasetFileConfig : ITrackableSchema
    {
        public DatasetFileConfig()
        {
            DeleteInd = false;
        }       
 
        
        public virtual int ConfigId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int FileTypeId { get; set; }
        public virtual Dataset ParentDataset { get; set; }
        public virtual DatasetScopeType DatasetScopeType { get; set; }
        public virtual ObjectStatusEnum ObjectStatus { get; set; }
        public virtual bool DeleteInd { get; set; }
        public virtual string DeleteIssuer { get; set; }
        public virtual DateTime DeleteIssueDTM { get; set; }

        public virtual IList<DatasetFile> DatasetFiles { get; set; }
        public virtual IList<RetrieverJob> RetrieverJobs { get; set; }
        public virtual FileExtension FileExtension { get; set; }

        /* ITrackableSchema implementation */
        public virtual bool IsSchemaTracked { get; set; }
        public virtual FileSchema Schema { get; set; }


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

        public virtual SchemaRevision GetLatestSchemaRevision()
        {
            return Schema.Revisions.OrderByDescending(o => o.Revision_NBR).Take(1).SingleOrDefault();
        }
        public virtual string GetStorageCode()
        {
            if (Schema is FileSchema scm)
            {
                return scm.StorageCode;
            }
            else
            {
                return null;
            }
        }
    }
}
