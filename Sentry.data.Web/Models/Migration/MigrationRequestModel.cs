using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class MigrationRequestModel
    {
        public int DatasetId { get; set; }
        [DisplayName("Dataset Name")]
        public string DatasetName { get; set; }
        [DisplayName("Target Named Environment")]
        public string TargetNamedEnvironment { get; set; }
        public List<string> AllAssetNamedEnvironments { get; set; } = new List<string>();
        [DisplayName("Schema ")]
        public List<int> SelectedSchema { get; set; } = new List<int>();
        public bool QuartermasterManagedNamedEnvironments { get; set; }

        public string SAIDAssetKeyCode { get; set; }
        public IEnumerable<SelectListItem> DatasetNamedEnvironmentDropDown { get; set; }
        public IEnumerable<SelectListItem> DatasetNamedEnvironmentTypeDropDown { get; set; }
        public NamedEnvironmentType DatasetNamedEnvironmentType { get; set; }
        public IEnumerable<SelectListItem> SchemaList { get; set; }
    }
}