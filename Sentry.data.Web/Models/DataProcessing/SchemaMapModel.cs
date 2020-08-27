﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

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
        [Range(1, int.MaxValue, ErrorMessage = "Please Select a Dataset")]
        public int SelectedDataset { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please Select a Schema")]
        public int SelectedSchema { get; set; }
        public bool IsDeleted { get; set; }

        public string Index { get; set; }

        public IEnumerable<SelectListItem> AllDatasets { get; set; }
        public IEnumerable<SelectListItem> AllSchemas { get; set; }
    }
}