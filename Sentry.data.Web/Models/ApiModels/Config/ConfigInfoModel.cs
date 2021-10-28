using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Config
{
    public class ConfigInfoModel
    {
        public int ConfigId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StorageCode { get; set; }
        public string FileType { get; set; }
        public bool CreateCurrentView { get; set; }
        //CLA-3306: We are removing IsInSAS from the UI and DSC model. However, the API still exposes this, so we are defaulting to false to maintain the API.
        public bool IsInSAS { get; set; } = false; 
        public string Delimiter { get; set; }
        public bool HasHeader { get; set; }
        public bool IsTrackableSchema { get; set; }
        public int SchemaId { get; set; }
    }
}