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

        [HttpGet]
        [Route("Migration/NamedEnvironment")]
        public async Task<PartialViewResult> _NamedEnvironment(string assetKeyCode, string namedEnvironment, int datasetId)
        {
            var model = new MigrationRequestModel()
            {
                DatasetId = datasetId,
                SAIDAssetKeyCode = assetKeyCode,
                DatasetNamedEnvironment = namedEnvironment
            };

            await model.SetNamedEnvironmentProperties(_datasetContext, _namedEnvironmentBuilder);

            return PartialView(model);
        }

    }
}