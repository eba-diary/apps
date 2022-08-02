using Sentry.Core;
using System.Collections.Generic;
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

        public SchemaMapModel(Core.SchemaMapDto dto)
        {
            Id = dto.Id;
            SelectedDataset = dto.DatasetId;
            SelectedSchema = dto.SchemaId;
        }

        public int Id { get; set; }
        public string SearchCriteria { get; set; }        
        //[Range(1, int.MaxValue, ErrorMessage = "Please Select a Dataset")]
        public int SelectedDataset { get; set; }
        //[Range(1, int.MaxValue, ErrorMessage = "Please Select a Schema")]
        public int SelectedSchema { get; set; }
        public bool IsDeleted { get; set; }

        public string Index { get; set; }
        public IEnumerable<SelectListItem> AllDatasets { get; set; }
        public IEnumerable<SelectListItem> AllSchemas { get; set; }

        public ValidationException Validate()
        {
            ValidationResults results = new ValidationResults();
            if (!IsDeleted)
            {
                if (SelectedDataset == 0)
                {
                    results.Add("SelectedDataset", "Must select dataset for schema mapping");
                }
                if (SelectedSchema == 0)
                {
                    results.Add("SelectedSchema", "Must select schema for schema mapping");
                }
            }

            return new ValidationException(results);
        }
    }
}