using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Web.Extensions;
using Newtonsoft.Json;

namespace Sentry.data.Web.Controllers
{
    public class MigrationController : Controller
    {
        private readonly IDataFeatures _featureFlags;
        private readonly IDatasetContext _datasetContext;
        private readonly NamedEnvironmentBuilder _namedEnvironmentBuilder;
        private readonly IDatasetService _datasetService;
               
        public MigrationController(IDataFeatures featureFlags, IDatasetContext datasetContext, NamedEnvironmentBuilder namedEnvironmentBuilder, IDatasetService datasetService)
        {
            _featureFlags = featureFlags;
            _datasetContext = datasetContext;
            _namedEnvironmentBuilder = namedEnvironmentBuilder;
            _datasetService = datasetService;
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

            await model.SetNamedEnvironmentProperties(_datasetContext, _namedEnvironmentBuilder);

            model.SchemaList = Utility.BuildSchemaDropDown(_datasetContext, datasetId, 0);

            return PartialView("_MigrationRequest", model);
        }

        [HttpGet]
        [Route("Migration/Dataset/{id}")]
        public ActionResult MigrationHistory(int id)
        {
            if (!_featureFlags.CLA1797_DatasetSchemaMigration.GetValue())
            {
                return Json(new { Success = false, Message = "Unauthorized access" });
            }

            List<MigrationHistory> migrationHistories = _datasetContext.MigrationHistory.Where(w => w.SourceDatasetId == id || w.TargetDatasetId == id).OrderByDescending(o => o.CreateDateTime).ToList();
            Dataset dataset = _datasetContext.Datasets.FirstOrDefault(w => w.DatasetId == id);
            MigrationHistoryPageModel pageModel = new MigrationHistoryPageModel()
            { 
                SourceDatasetId = id,
                SourceDatasetName = (dataset == null)? null : dataset.DatasetName,
                MigrationHistoryModels = migrationHistories.ToMigrationHistoryModels(),
                Security = _datasetService.GetUserSecurityForDataset(id)
            };

            return View("_MigrationHistory", pageModel);
        }

        //CONTROLLER ACTION called from JS to return the Migration History JSON
        [HttpPost]
        public ActionResult MagicModalMigrationHistory(int migrationHistoryId)
        {
            MigrationHistory migrationHistory = _datasetContext.MigrationHistory.FirstOrDefault(w => w.MigrationHistoryId == migrationHistoryId);
            string migrationHistoryJson = JsonConvert.SerializeObject(migrationHistory);
            return Json(new { migrationHistoryJson = migrationHistoryJson });
        }



        #region Private Methods

        //private async Task SetNamedEnvironmentProperties(MigrationRequestModel model)
        //{
        //    var sourceNamedEnvironment = _datasetContext.Datasets
        //        .Where(w => w.DatasetId == model.DatasetId && w.ObjectStatus == ObjectStatusEnum.Active)
        //        .Select(s => s.NamedEnvironment)
        //        .FirstOrDefault();
        //    var (namedEnvironmentList, namedEnvironmentTypeList) = await _namedEnvironmentBuilder.BuildNamedEnvironmentDropDownsAsync(model.SAIDAssetKeyCode, sourceNamedEnvironment);

        //    if (namedEnvironmentList == null || !namedEnvironmentList.Any())
        //    {
        //        var datasetName = _datasetContext.Datasets.Where(w => w.DatasetId == model.DatasetId).Select(s => s.DatasetName).FirstOrDefault();
        //        List<NamedEnvironmentDto> datasetNamedEnvironmentDtoList = _datasetContext.Datasets
        //            .Where(w => w.Asset.SaidKeyCode == model.SAIDAssetKeyCode && w.DatasetName == datasetName && w.ObjectStatus == ObjectStatusEnum.Active)
        //            .Select(s => new NamedEnvironmentDto() { NamedEnvironment = s.NamedEnvironment, NamedEnvironmentType = s.NamedEnvironmentType })
        //            .ToList();
        //        model.DatasetNamedEnvironmentDropDown = NamedEnvironmentBuilder.BuildNamedEnvironmentDropDown(sourceNamedEnvironment, datasetNamedEnvironmentDtoList)
        //                                                                            .Where(w => w.Value != sourceNamedEnvironment).OrderBy(o => o.Text);
        //        model.DatasetNamedEnvironmentTypeDropDown = _namedEnvironmentBuilder.BuildNamedEnvironmentTypeDropDown(sourceNamedEnvironment, datasetNamedEnvironmentDtoList);
        //        model.QuartermasterManagedNamedEnvironments = false;
        //    }
        //    else
        //    {
        //        //Filter out the source dataset named environment from list and order
        //        model.DatasetNamedEnvironmentDropDown = namedEnvironmentList.Where(w => w.Value != sourceNamedEnvironment).OrderBy(o => o.Text);
        //        model.DatasetNamedEnvironmentTypeDropDown = namedEnvironmentTypeList;
        //        model.DatasetNamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironmentTypeList.First(l => l.Selected).Value);
        //        model.QuartermasterManagedNamedEnvironments = true;
        //    }
        //}

        #endregion

    }
}