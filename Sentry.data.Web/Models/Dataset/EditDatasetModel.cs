using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web
{
    public class EditDatasetModel : BaseDatasetModel
    {
        public EditDatasetModel()
        {
        }

        public EditDatasetModel(Dataset ds, IAssociateInfoProvider associateService) : base(ds, associateService)
        {

        }

        //[DisplayName("Column Metadata")]
        //[Required]
        //public IList<_DatasetMetadataModel> md_Columns { get; set; }

        //[DisplayName("User Metadata")]
        //[Required]
        //public IList<_DatasetMetadataModel> md_Metadata { get; set; }

        ///// <summary>
        ///// CategoryIDs holds the IDs of the selected categories.  
        ///// It is needed for model binding and MVC editor helpers
        ///// </summary>
        //[DisplayName("Categories")]
        //[Required]
        //public int[] CategoryIDs { get; set; }

        /// <summary>
        /// AllCategories holds the sorted list of all possible categories.
        /// </summary>
        public IEnumerable<SelectListItem> AllCategories { get; set; }

        public IEnumerable<SelectListItem> AllFrequencies { get; set; }

        public IEnumerable<SelectListItem> AllOriginationCodes { get; set; }

        public IEnumerable<SelectListItem> AllDatasetScopeTypes { get; set; }

        [DisplayName("Frequency")]
        public int FreqencyID { get; set; }

        [DisplayName("Origination Code")]
        public int OriginationID { get; set; }

        [DisplayName("Dataset Scope Type")]
        public int DatasetScopeTypeID { get; set; }
    }
}
