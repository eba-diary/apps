using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using Sentry.data.Web.Models.ApiModels.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Web.Extensions
{
    public static class MigrationExtensions
    {
        public static Core.SchemaMigrationRequest ToDto(this SchemaMigrationRequestModel model)
        {
            return new Core.SchemaMigrationRequest()
            {
                SourceSchemaId = model.SourceSchemaId,
                TargetDataFlowNamedEnvironment = model.TargetDataFlowNamedEnviornment,
                TargetDatasetId = model.TargetDatasetId,
                TargetDatasetNamedEnvironment = model.TargetDatasetNamedEnvironment
            };
        }

        public static Core.DatasetSchemaMigrationRequest MapToDatasetSchemaMigrationRequest(this DatasetSchemaMigrationRequestModel model)
        {
            return new Core.DatasetSchemaMigrationRequest()
            {
                SourceSchemaId = model.SourceSchemaId,
                TargetDataFlowNamedEnvironment = model.TargetDataFlowNamedEnviornment
            };
        }

        public static Core.DatasetMigrationRequest ToDto(this DatasetMigrationRequestModel model)
        {
            Core.DatasetMigrationRequest request = new Core.DatasetMigrationRequest()
            {
                SourceDatasetId = model.SourceDatasetId,
                TargetDatasetNamedEnvironment = model.TargetDatasetNamedEnvironment,
                TargetDatasetNamedEnvironmentType = model.TargetDatasetNamedEnvironmentType,
                TargetDatasetId = model.TargetDatasetId,
                SchemaMigrationRequests = new List<Core.DatasetSchemaMigrationRequest>()
            };

            foreach (DatasetSchemaMigrationRequestModel datasetSchemaMigrationRequestModel in model.SchemaMigrationRequests)
            {
                request.SchemaMigrationRequests.Add(datasetSchemaMigrationRequestModel.MapToDatasetSchemaMigrationRequest());
            }

            return request;
        }

        public static DatasetMigrationResponseModel ToDatasetMigrationResponseModel(this Core.DatasetMigrationRequestResponse response)
        {
            DatasetMigrationResponseModel model = new DatasetMigrationResponseModel()
            {
                IsDatasetMigrated = response.IsDatasetMigrated,
                DatasetMigrationReason = response.DatasetMigrationReason,
                DatasetId = response.DatasetId,
                DatasetName = response.DatasetName,
                SchemaMigrationResponse = new List<SchemaMigrationResponseModel>()
            };

            foreach (Core.SchemaMigrationRequestResponse schemaResponse in response.SchemaMigrationResponses)
            {
                model.SchemaMigrationResponse.Add(schemaResponse.ToSchemaMigrationRequestModel());
            }

            return model;
        }

        public static SchemaMigrationResponseModel ToSchemaMigrationRequestModel(this Core.SchemaMigrationRequestResponse response)
        {
            return new SchemaMigrationResponseModel()
            {
                SourceSchemaId = response.SourceSchemaId,
                IsSchemaMigrated = response.MigratedSchema,
                SchemaId = response.TargetSchemaId,
                SchemaName = response.SchemaName,
                SchemaMigrationMessage = response.SchemaMigrationReason,
                IsSchemaRevisionMigrated = response.MigratedSchemaRevision,
                SchemaRevisionId = response.TargetSchemaRevisionId,
                SchemaRevisionName = response.SchemaRevisionName,
                SchemaRevisionMigrationMessage = response.SchemaRevisionMigrationReason,
                IsDataFlowMigrated = response.MigratedDataFlow,
                DataFlowId = response.TargetDataFlowId,
                DataFlowName = response.DataFlowName,
                DataFlowMigrationMessage = response.DataFlowMigrationReason
            };
        }

        public static async Task SetNamedEnvironmentProperties(this MigrationRequestModel model, IDatasetContext context, NamedEnvironmentBuilder namedEnvironmentBuilder)
        {
            var sourceNamedEnvironment = string.Empty;
            sourceNamedEnvironment = context.Datasets
                    .Where(w => w.DatasetId == model.DatasetId && w.ObjectStatus == ObjectStatusEnum.Active)
                    .Select(s => s.NamedEnvironment)
                    .FirstOrDefault();           
            
            var (namedEnvironmentList, namedEnvironmentTypeList) = await namedEnvironmentBuilder.BuildNamedEnvironmentDropDownsAsync(model.SAIDAssetKeyCode, model.DatasetNamedEnvironment);

            if (namedEnvironmentList == null || !namedEnvironmentList.Any())
            {
                //Build out model properties based on SAID asset named environments defined in DSC for the given dataset name

                var datasetName = context.Datasets.Where(w => w.DatasetId == model.DatasetId).Select(s => s.DatasetName).FirstOrDefault();

                List<NamedEnvironmentDto> datasetNamedEnvironmentDtoList = context.Datasets
                    .Where(w => w.Asset.SaidKeyCode == model.SAIDAssetKeyCode && w.DatasetName == datasetName && w.ObjectStatus == ObjectStatusEnum.Active)
                    .Select(s => new NamedEnvironmentDto() { NamedEnvironment = s.NamedEnvironment, NamedEnvironmentType = s.NamedEnvironmentType })
                    .ToList();

                if (model.DatasetNamedEnvironment != null && !datasetNamedEnvironmentDtoList.Any(w => w.NamedEnvironment == model.DatasetNamedEnvironment))
                {
                    // DSC does not have any knowledge of the target named environment
                    // Add it to the drop down as a selected entry
                    datasetNamedEnvironmentDtoList.Add(new NamedEnvironmentDto() { NamedEnvironment = model.DatasetNamedEnvironment.ToUpper() });
                    model.NewNonQManagedNamedEnvironment = true;
                }

                model.DatasetNamedEnvironmentDropDown = NamedEnvironmentBuilder.BuildNamedEnvironmentDropDown(model.DatasetNamedEnvironment, datasetNamedEnvironmentDtoList)
                                                                                    .Where(w => w.Value != sourceNamedEnvironment).OrderBy(o => o.Text);

                List<System.Web.Mvc.SelectListItem> itemList = namedEnvironmentBuilder.BuildNamedEnvironmentTypeDropDown(model.DatasetNamedEnvironment, datasetNamedEnvironmentDtoList);
                if (model.NewNonQManagedNamedEnvironment)
                {
                    itemList.ForEach(w => w.Selected = false);
                    itemList.Add(new System.Web.Mvc.SelectListItem { Text = "Select Environment Type", Value = "Select Type", Selected = true, Disabled = false });
                }
                model.DatasetNamedEnvironmentTypeDropDown = itemList.OrderByDescending(o => o.Selected).ThenBy(o => o.Text);

                model.QuartermasterManagedNamedEnvironments = false;
            }
            else
            {
                //Build out model properties based on SAID asset named environments defined within Quartermaster

                //Filter out the source dataset named environment from list and order
                model.DatasetNamedEnvironmentDropDown = namedEnvironmentList.Where(w => w.Value != sourceNamedEnvironment).OrderBy(o => o.Text);
                model.DatasetNamedEnvironmentTypeDropDown = namedEnvironmentTypeList;
                model.DatasetNamedEnvironmentType = (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), namedEnvironmentTypeList.First(l => l.Selected).Value);
                model.QuartermasterManagedNamedEnvironments = true;
                model.NewNonQManagedNamedEnvironment = false;
            }
        }
    }
}