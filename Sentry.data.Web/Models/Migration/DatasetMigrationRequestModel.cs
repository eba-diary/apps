using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class DatasetMigrationRequestModel
    {
        public int DatasetId { get; set; }
        public string TargetNamedEnvironment { get; set; }
        public List<string> AllAssetNamedEnvironments { get; set; } = new List<string>();
        public List<int> SelectedSchema { get; set; } = new List<int>();

        public string SAIDAssetKeyCode { get; set; }
        public IEnumerable<SelectListItem> DatasetNamedEnvironmentDropDown { get; set; }
        public IEnumerable<SelectListItem> DatasetNamedEnvironmentTypeDropDown { get; set; }
        public NamedEnvironmentType DatasetNamedEnvironmentType { get; set; }
        public IEnumerable<SelectListItem> SchemaList { get; set; }
    }
}