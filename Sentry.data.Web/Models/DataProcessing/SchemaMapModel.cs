using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class SchemaMapModel
    {
        public SchemaMapModel()
        {
            AllDatasets = new List<SelectListItem>();
            AllSchemas = new List<SelectListItem>();
        }

        public int Id { get; set; }
        public string SearchCriteria { get; set; }        
        //[Required]
        public int SelectedDataset { get; set; }
        //[Required]
        public int SelectedSchema { get; set; }
        public bool IsDeleted { get; set; }

        public IEnumerable<SelectListItem> AllDatasets { get; set; }
        public IEnumerable<SelectListItem> AllSchemas { get; set; }
    }
}