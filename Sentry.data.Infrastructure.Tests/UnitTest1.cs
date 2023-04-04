using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nest;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using IgnoreAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [Ignore("because")]
        [TestMethod]
        public void TestMethod1()
        {
            Bootstrapper.Init();

            IDatasetContext context = Bootstrapper.Container.GetInstance<IDatasetContext>();

            ConnectionSettings settings = new ConnectionSettings(new Uri("http://fit-d-elases-01.sentry.com:9200"));
            settings.DefaultMappingFor<GlobalDataset>(x => x.IndexName("data-dataset-082116").IdProperty(i => i.GlobalDatasetId));
            settings.ThrowExceptions();

            ElasticClient client = new ElasticClient(settings);

            var globalDatasets = context.Datasets.Where(x => x.ObjectStatus == Core.GlobalEnums.ObjectStatusEnum.Active && x.DatasetType == "DS").GroupBy(x => x.GlobalDatasetId).ToList();

            foreach (var globalDataset in globalDatasets)
            {
                GlobalDataset fullNested = new GlobalDataset
                {
                    GlobalDatasetId = globalDataset.First().GlobalDatasetId.Value,
                    DatasetName = globalDataset.First().DatasetName,
                    DatasetSaidAssetCode = globalDataset.First().Asset.SaidKeyCode,
                    EnvironmentDatasets = new List<EnvironmentDataset>()
                };

                foreach (var dataset in globalDataset)
                {
                    var fileConfigs = context.DatasetFileConfigs.Where(x => x.ParentDataset.DatasetId == dataset.DatasetId && x.ObjectStatus == Core.GlobalEnums.ObjectStatusEnum.Active).ToList();
                    var flows = context.DataFlow.Where(x => x.DatasetId == dataset.DatasetId && x.ObjectStatus == Core.GlobalEnums.ObjectStatusEnum.Active).ToList();

                    List<EnvironmentSchema> schemas = new List<EnvironmentSchema>();

                    foreach (var schema in fileConfigs.Select(x => x.Schema).ToList())
                    {
                        schemas.Add(new EnvironmentSchema
                        {
                            SchemaId = schema.SchemaId,
                            SchemaName = schema.Name,
                            SchemaDescription = schema.Description,
                            SchemaSaidAssetCode = flows.FirstOrDefault(x => x.SchemaId == schema.SchemaId)?.SaidKeyCode
                        });
                    }

                    EnvironmentDataset environmentDataset = new EnvironmentDataset
                    {
                        DatasetId = dataset.DatasetId,
                        DatasetDescription = dataset.DatasetDesc,
                        CategoryCode = dataset.DatasetCategories.First().Name,
                        NamedEnvironment = dataset.NamedEnvironment,
                        NamedEnvironmentType = dataset.NamedEnvironmentType.ToString(),
                        OriginationCode = dataset.OriginationCode,
                        IsSecured = dataset.IsSecured,
                        FavoriteUserIds = dataset.Favorities.Select(x => x.UserId).ToList(),
                        EnvironmentSchemas = schemas
                    };

                    fullNested.EnvironmentDatasets.Add(environmentDataset);
                }

                client.IndexDocument(fullNested);
            }
        }
    }
}
