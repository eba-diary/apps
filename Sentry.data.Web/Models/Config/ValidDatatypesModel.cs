using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class ValidDatatypesModel
    {
        public ValidDatatypesModel()
        {
            ValidDatatypes = new List<DataTypeModel>();
        }
        public FileExtension FileExtension { get; set; }
        public string FileExtensionName { get; set; }
        public bool IsPositional { get; set; }
        public List<DataTypeModel> ValidDatatypes { get; set; }
        public bool IsFixedWidth { get; set; }
        public bool AllowArrays {
            get
            {
                return FileExtensionName == GlobalConstants.ExtensionNames.JSON || FileExtensionName == GlobalConstants.ExtensionNames.XML || FileExtensionName == GlobalConstants.ExtensionNames.PARQUET;
            }          
        }
    }

    public class DataTypeModel
    {
        public DataTypeModel(string name, string desc, string tag)
        {
            Name = name;
            Description = desc;
            Tag = tag;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
    }
}