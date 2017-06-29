using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class EditDatasetModel : BaseDatasetModel
    {
        public EditDatasetModel()
        {
        }

        public EditDatasetModel(Dataset ds) : base(ds)
        {
            //if (null != catList)
            //{
            //    List<SelectListItem> sliCatList = new List<SelectListItem>();

            //    foreach (String cat in catList.AsEnumerable())
            //    {
            //        SelectListItem sliCat = new SelectListItem();
            //        sliCat.Text = cat;
            //        sliCat.Value = cat;
            //        sliCatList.Add(sliCat);
            //    }
            //    this.AllCategories = sliCatList.AsEnumerable();
            //}

            //if (null != ds.Columns)
            //{
            //    this.md_Columns = new List<_DatasetMetadataModel>();
            //    foreach (DatasetMetadata dsm in ds.Columns)
            //    {
            //        md_Columns.Add(new _DatasetMetadataModel(dsm));
            //    }
            //}

            //if (null != ds.Metadata)
            //{
            //    this.md_Metadata = new List<_DatasetMetadataModel>();
            //    foreach (DatasetMetadata dsm in ds.Metadata)
            //    {
            //        md_Metadata.Add(new _DatasetMetadataModel(dsm));
            //    }
            //}
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

        [DisplayName("Frequency")]
        public int FreqencyID { get; set; }

        [DisplayName("Origination Code")]
        public int OriginationID { get; set; }
    }
}
