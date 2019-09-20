using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Schema
{
    public class SchemaInfoModel
    {
        public int ConfigId { get; set; }
        public int SchemaId { get; set; }
        public string SchemaEntity_NME { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StorageCode { get; set; }
        public string FileType { get; set; }
        public bool CreateCurrentView { get; set; }
        public bool IsInSAS { get; set; }
        public string Delimiter { get; set; }
        public bool HasHeader { get; set; }
        public bool IsTrackableSchema { get; set; }
        public int RevisionCount { get; set; }
    }
}