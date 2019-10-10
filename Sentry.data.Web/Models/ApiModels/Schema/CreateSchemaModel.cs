using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Schema
{
    public class CreateSchemaModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileFormat { get; set; }
        public bool CurrentView { get; set; }
        public bool AddToSAS { get; set; }
        public string Delimiter { get; set; }
        public bool HasHeader { get; set; }
    }
}