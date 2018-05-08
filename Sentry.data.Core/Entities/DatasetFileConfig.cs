using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sentry.data.Core
{
    public class DatasetFileConfig
    {

        public DatasetFileConfig()
        {
            //Default to false
            CreateCurrentFile = false;
        }        
        
        public virtual int ConfigId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string SearchCriteria { get; set; }
        public virtual string TargetFileName { get; set; }
        public virtual string DropPath { get; set; }
        public virtual Boolean IsRegexSearch { get; set; }
        public virtual Boolean OverwriteDatafile { get; set; }
        public virtual int FileTypeId { get; set; }
        public virtual Boolean IsGeneric { get; set; }
        public virtual Dataset ParentDataset { get; set; }
        public virtual DatasetScopeType DatasetScopeType { get; set; }
        public virtual Boolean CreateCurrentFile { get; set; }

        public virtual int DatasetScopeTypeID
        {
            get
            {
                return DatasetScopeType.ScopeTypeId;
            }
        }

        public virtual IList<RetrieverJob> RetrieverJobs { get; set; }



        /// <summary>
        /// Return path to current file
        /// </summary>
        public virtual Uri GetCurrentFileDir()
        {
            if (CreateCurrentFile)
            {
                //This is a Short-Term solution to maintaining a current file for access via SAS tool.

                //Need to add category, dataset name, and config name, to ensure we can delete all 
                // contents of directory to ensure only one file.
                var path = System.IO.Path.Combine(
                        Configuration.Config.GetHostSetting("PushToSASTargetPath"),
                        "current_files",
                        ParentDataset.DatasetCategory.Name.ToLower(),
                        ParentDataset.DatasetName.Replace(' ', '_').ToLower(),
                        Name.Replace(' ', '_').ToLower()
                        );

                return new Uri(path);
            }
            else
            {
                throw new InvalidOperationException("CreateCurrentFile is false on data file config");
            }                
        }
    }
}
