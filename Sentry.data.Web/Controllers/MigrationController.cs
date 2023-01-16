using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class MigrationController : Controller
    {
        private readonly IDataFeatures _featureFlags;
        private readonly IDatasetContext _datasetContext;
        private readonly NamedEnvironmentBuilder _namedEnvironmentBuilder;

        public MigrationController(IDataFeatures featureFlags, IDatasetContext datasetContext, NamedEnvironmentBuilder namedEnvironmentBuilder)
        {
            _featureFlags = featureFlags;
            _datasetContext = datasetContext;
            _namedEnvironmentBuilder = namedEnvironmentBuilder;
        }

        [HttpGet]
        public async Task<ActionResult> DatasetMigrationRequest(int datasetId)
        {
            if (!_featureFlags.CLA1797_DatasetSchemaMigration.GetValue())
            {
                return Json(new { Success = false, Message = "Unauthorized access" });
            }
            DatasetMigrationRequestModel model = new DatasetMigrationRequestModel
            {
                DatasetId = datasetId,
                SAIDAssetKeyCode = _datasetContext.Datasets.Where(w => w.DatasetId == datasetId).Select(s => s.Asset.SaidKeyCode).FirstOrDefault()
            };

            await SetNamedEnvironmentProperties(model);

            model.SchemaList = Utility.BuildSchemaDropDown(_datasetContext, datasetId, 0);

            return PartialView("Dataset/_DatasetMigrationRequest", model);
        }

        [HttpGet]
        public PartialViewResult DatasetMigrationResponse()
        {
            return PartialView("Dataset/_DatasetMigrationResponse");
        }

        #region Private Methods
        private async Task SetNamedEnvironmentProperties(DatasetMigrationRequestModel model)
        {
            var sourceNamedEnvironment = _datasetContext.Datasets.Where(w => w.DatasetId == model.DatasetId).Select(s => s.NamedEnvironment).FirstOrDefault();
            var namedEnvironments = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDownsAsync(model.SAIDAssetKeyCode, sourceNamedEnvironment);

            if (namedEnvironments.namedEnvironmentList == null || !namedEnvironments.namedEnvironmentList.Any())
            {
                var datasetName = _datasetContext.Datasets.Where(w => w.DatasetId == model.DatasetId).Select(s => s.DatasetName).FirstOrDefault();
                List<NamedEnvironmentDto> datasetNamedEnvironmentDtoList = _datasetContext.Datasets.Where(w => w.Asset.SaidKeyCode == model.SAIDAssetKeyCode && w.DatasetName == datasetName).Select(s => new NamedEnvironmentDto() { NamedEnvironment = s.NamedEnvironment, NamedEnvironmentType = s.NamedEnvironmentType }).ToList();
                model.DatasetNamedEnvironmentDropDown = NamedEnvironmentBuilder.BuildNamedEnvironmentDropDown(sourceNamedEnvironment, datasetNamedEnvironmentDtoList);
                model.DatasetNamedEnvironmentTypeDropDown = _namedEnvironmentBuilder.BuildNamedEnvironmentTypeDropDown(sourceNamedEnvironment, datasetNamedEnvironmentDtoList);
                model.QuartermasterManagedNamedEnvironments = false;
            }
            else
            {
                //Filter out the source dataset named environment from list and order
                model.DatasetNamedEnvironmentDropDown = namedEnvironments.namedEnvironmentList.Where(w => w.Value != sourceNamedEnvironment).OrderBy(o => o.Text);
                model.DatasetNamedEnvironmentTypeDropDown = namedEnvironments.namedEnvironmentTypeList;
                model.DatasetNamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironments.namedEnvironmentTypeList.First(l => l.Selected).Value);
                model.QuartermasterManagedNamedEnvironments = true;
            }
        }

        #endregion

    }
}