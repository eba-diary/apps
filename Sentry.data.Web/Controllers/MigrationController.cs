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
        private readonly IMigrationService _migrationService;

        public MigrationController(IDataFeatures featureFlags, IDatasetContext datasetContext, NamedEnvironmentBuilder namedEnvironmentBuilder, IDatasetService datasetService, IMigrationService migrationService)
        {
            _featureFlags = featureFlags;
            _datasetContext = datasetContext;
            _namedEnvironmentBuilder = namedEnvironmentBuilder;
            _datasetService = datasetService;
            _migrationService = migrationService;

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
            //EXIT IF FEATURE FLAG OFF
            if (!_featureFlags.CLA1797_DatasetSchemaMigration.GetValue())
            {
                return Json(new { Success = false, Message = "Unauthorized access" });
            }
            
            //GRAB DTO FOR THIS DatasetId
            DatasetDetailDto dto = _datasetService.GetDatasetDetailDto(id);

            //GET RELATIVES WITH MIGRATION HISTORY ONLY
            List<DatasetRelativeDto> relativesWithMigrationHistory = _migrationService.GetRelativesWithMigrationHistory(dto.DatasetRelatives);
            
            //CREATE LIST<int> OF RELATIVES
            List<int> datasetIdList = new List<int>(relativesWithMigrationHistory.Select(s => s.DatasetId));
            
            //DETERMINE IF WE SHOULD SHOW DROP DOWN FILTER (MIGRATION HISTORY)
            //IF THERE IS NO MIGRATION HISTORIES THEN WHY SHOW A FILTER
            bool showDropDownFilter = (_migrationService.GetMigrationHistory(datasetIdList).Count >= 1);

            //CREATE MODEL WHICH IS MIGRATION HISTORY HEADER
            MigrationHistoryPageModel pageModel = new MigrationHistoryPageModel()
            { 
                SourceDatasetId = id,
                SourceDatasetName = dto.DatasetName,
                ShowNamedEnvironmentFilter = showDropDownFilter,
                DatasetRelatives = relativesWithMigrationHistory?.Select(s => s.ToModel()).OrderBy(o => o.DatasetNamedEnvironment).ToList()
            };

            return View("_MigrationHistory", pageModel);
        }


        [HttpPost]
        [Route("Migration/Detail/{id}/{namedenvironment}/")]
        public ActionResult MigrationHistoryDetail(int id, string namedEnvironment)
        {
            //EXIT IF FEATURE FLAG OFF
            if (!_featureFlags.CLA1797_DatasetSchemaMigration.GetValue())
            {
                return Json(new { Success = false, Message = "Unauthorized access" });
            }

            //GRAB DTO FOR THIS DatasetId
            DatasetDetailDto dto = _datasetService.GetDatasetDetailDto(id);

            //TURN DatasetRelatives INTO datasetIdList AND FILTER ON NAMEDENVIRONMENT SELECTED
            //NOTE: IF namedEnvironment==ALL THEN IT WILL AUTOMATICALLY GRAB EVERYTHING
            //THE OR IN LINQ IS A TRICK TO BRING IN ALL RELATIVES IF namedEnvironment==ALL
            List<int> datasetIdList = new List<int>(dto.DatasetRelatives.Where(w => w.NamedEnvironment == namedEnvironment || namedEnvironment == GlobalConstants.MigrationHistoryNamedEnvFilter.ALL_NAMED_ENV)
                                                                        .Select(s => s.DatasetId));

            //GET ALL MIGRATION HISTORY FOR THESE DATASET RELATIVES
            List<MigrationHistory> migrationHistories = _migrationService.GetMigrationHistory(datasetIdList);

            //CREATE MODEL WHICH IS MIGRATION HISTORY DETIAL AKA PARTIAL VIEW OF PAGE
            MigrationHistoryDetailPageModel detailPageModel = new MigrationHistoryDetailPageModel()
            {
                MigrationHistoryModels = migrationHistories.ToMigrationHistoryModels(),
                Security = _datasetService.GetUserSecurityForDataset(id)
            };

            return PartialView("_MigrationHistoryDetail", detailPageModel);
        }



        //CONTROLLER ACTION called from JS to return the Migration History JSON
        [HttpPost]
        public ActionResult MagicModalMigrationHistory(int migrationHistoryId)
        {
            MigrationHistory migrationHistory = _datasetContext.MigrationHistory.FirstOrDefault(w => w.MigrationHistoryId == migrationHistoryId);
            string migrationHistoryJson = JsonConvert.SerializeObject(migrationHistory);
            return Json(new { migrationHistoryJson = migrationHistoryJson });
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