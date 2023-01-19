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
        public PartialViewResult MigrationResponse()
        {
            return PartialView("_MigrationResponse");
        }

        [HttpGet]
        public async Task<ActionResult> MigrationRequest(int datasetId)
        {
            if (!_featureFlags.CLA1797_DatasetSchemaMigration.GetValue())
            {
                return Json(new { Success = false, Message = "Unauthorized access" });
            }

            (string datasetName, string saidAssetKeyCode) = _datasetContext.Datasets.Where(w => w.DatasetId == datasetId).Select(s => new Tuple<string, string>(s.DatasetName, s.Asset.SaidKeyCode)).FirstOrDefault();

            MigrationRequestModel model = new MigrationRequestModel()
            {
                DatasetId = datasetId,
                DatasetName = datasetName,
                SAIDAssetKeyCode = saidAssetKeyCode
            };

            await SetNamedEnvironmentProperties(model);

            model.SchemaList = Utility.BuildSchemaDropDown(_datasetContext, datasetId, 0);

            return PartialView("_MigrationRequest", model);
        }

        #region Private Methods

        private async Task SetNamedEnvironmentProperties(MigrationRequestModel model)
        {
            var sourceNamedEnvironment = _datasetContext.Datasets
                .Where(w => w.DatasetId == model.DatasetId && w.ObjectStatus == ObjectStatusEnum.Active)
                .Select(s => s.NamedEnvironment)
                .FirstOrDefault();
            var (namedEnvironmentList, namedEnvironmentTypeList) = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDownsAsync(model.SAIDAssetKeyCode, sourceNamedEnvironment);

            if (namedEnvironmentList == null || !namedEnvironmentList.Any())
            {
                var datasetName = _datasetContext.Datasets.Where(w => w.DatasetId == model.DatasetId).Select(s => s.DatasetName).FirstOrDefault();
                List<NamedEnvironmentDto> datasetNamedEnvironmentDtoList = _datasetContext.Datasets
                    .Where(w => w.Asset.SaidKeyCode == model.SAIDAssetKeyCode && w.DatasetName == datasetName && w.ObjectStatus == ObjectStatusEnum.Active)
                    .Select(s => new NamedEnvironmentDto() { NamedEnvironment = s.NamedEnvironment, NamedEnvironmentType = s.NamedEnvironmentType })
                    .ToList();
                model.DatasetNamedEnvironmentDropDown = NamedEnvironmentBuilder.BuildNamedEnvironmentDropDown(sourceNamedEnvironment, datasetNamedEnvironmentDtoList)
                                                                                    .Where(w => w.Value != sourceNamedEnvironment).OrderBy(o => o.Text);
                model.DatasetNamedEnvironmentTypeDropDown = _namedEnvironmentBuilder.BuildNamedEnvironmentTypeDropDown(sourceNamedEnvironment, datasetNamedEnvironmentDtoList);
                model.QuartermasterManagedNamedEnvironments = false;
            }
            else
            {
                //Filter out the source dataset named environment from list and order
                model.DatasetNamedEnvironmentDropDown = namedEnvironmentList.Where(w => w.Value != sourceNamedEnvironment).OrderBy(o => o.Text);
                model.DatasetNamedEnvironmentTypeDropDown = namedEnvironmentTypeList;
                model.DatasetNamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironmentTypeList.First(l => l.Selected).Value);
                model.QuartermasterManagedNamedEnvironments = true;
            }
        }

        #endregion

    }
}