using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using System.IO;

namespace Sentry.data.Common
{
    /// <summary>
    /// Provides common code between projects
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Generates full drop location path for a dataset
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static string GenerateDatasetDropLocation(Dataset ds)
        {
            string filep = Path.Combine(Configuration.Config.GetHostSetting("DatasetLoaderBaseLocation"), ds.DatasetCategory.Name.ToLower());
            filep = Path.Combine(filep, ds.DatasetName.Replace(' ', '_').ToLower());
            //filep = Path.Combine(filep, GenerateDatasetFrequencyLocationName(ds.CreationFreqDesc).ToLower());
            return filep.ToString();
        }
        /// <summary>
        /// Generates full drop location path for a dataset.  <i>Specify all parameters</i>
        /// </summary>
        /// <param name="creationFrequency"></param>
        /// <param name="categoryName"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static string GenerateDatasetDropLocation(string creationFrequency, string categoryName, string datasetName)
        {
            string filep = Path.Combine(Configuration.Config.GetHostSetting("DatasetLoaderBaseLocation"), categoryName.ToLower());
            filep = Path.Combine(filep, datasetName.Replace(' ', '_').ToLower());
            //filep = Path.Combine(filep, creationFrequency.Replace(' ', '_').ToLower());
            return filep.ToString();
        }
        /// <summary>
        /// Generate storage location path for dataset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ds"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static string GenerateDatasetStorageLocation(Dataset ds)
        {
            return GenerateLocationKey(ds.CreationFreqDesc, ds.DatasetCategory.Name, ds.DatasetName);
        }
        /// <summary>
        /// Generate storage location path.  <i>Specify all parameters</i>
        /// </summary>
        /// <param name="creationFrequency"></param>
        /// <param name="categoryName"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static string GenerateDatasetStorageLocation(string creationFrequency, string categoryName, string datasetName)
        {
            return GenerateLocationKey(creationFrequency, categoryName, datasetName);
        }
        /// <summary>
        /// Generate storage key for datafile
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="now"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GenerateDatafileKey(Dataset ds, DateTime now, string filename)
        {
            StringBuilder location = new StringBuilder();
            location.Append(GenerateDatasetStorageLocation(ds));
            location.Append(now.Year.ToString());
            location.Append('/');
            location.Append(now.Month.ToString());
            location.Append('/');
            location.Append(now.Day.ToString());
            location.Append('/');
            location.Append(filename);

            return location.ToString();
        }


        /// <summary>
        /// Returns storage path
        /// </summary>
        /// <param name="creationFreqDesc"></param>
        /// <param name="category"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static string GenerateLocationKey(string creationFreqDesc, string category, string datasetName)
        {
            StringBuilder location = new StringBuilder();
            location.Append(Configuration.Config.GetHostSetting("S3DataPrefix"));
            location.Append(category.ToLower());
            location.Append('/');
            location.Append(FormatDatasetName(datasetName));
            location.Append('/');
            location.Append(GenerateDatasetFrequencyLocationName(creationFreqDesc));
            location.Append('/');

            return location.ToString();
        }
        /// <summary>
        /// Generates directory friendly dataset name
        /// </summary>
        /// <param name="dsName"></param>
        /// <returns></returns>
        public static string FormatDatasetName(string dsName)
        {
            string name = null;

            name = dsName.ToLower();
            name = name.Replace(' ', '_');

            return name;
        }

        /// <summary>
        /// Generates abbreviated frequency name
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static string GenerateDatasetFrequencyLocationName(string frequency)
        {
            string freq = null;
            switch (frequency.ToLower())
            {
                case "yearly":
                    freq = "yrly";
                    break;
                case "quarterly":
                    freq = "qrtly";
                    break;
                case "monthly":
                    freq = "mntly";
                    break;
                case "weekly":
                    freq = "wkly";
                    break;
                case "daily":
                    freq = "dly";
                    break;
                case "nonschedule":
                    freq = "nskd";
                    break;
                case "transaction":
                    freq = "trn";
                    break;
                default:
                    freq = "dflt";
                    break;
            };
            return freq;
        }
        /// <summary>
        /// Return distinct list of file extensions within a list of datasetfile objects
        /// </summary>
        /// <param name="dfList"></param>
        /// <returns></returns>
        public static List<string> GetDistinctFileExtensions(IList<DatasetFile> dfList)
        {
            List<string> extensions = new List<string>();
            foreach (DatasetFile df in dfList)
            {
                extensions.Add(Path.GetExtension(df.FileName));
            }
            
            return extensions.Distinct().ToList();
        }
        /// <summary>
        /// Get file extension of file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName).TrimStart('.').ToLower();
        }
    }
}
