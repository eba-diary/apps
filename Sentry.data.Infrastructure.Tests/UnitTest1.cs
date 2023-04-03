﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nest;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore("because")]
        [TestMethod]
        public void TestMethod1()
        {
            Bootstrapper.Init();

            IDatasetContext context = Bootstrapper.Container.GetInstance<IDatasetContext>();

            ConnectionSettings settings = new ConnectionSettings(new Uri("http://fit-d-elases-01.sentry.com:9200"));
            settings.DefaultMappingFor<GlobalDataset>(x => x.IndexName("data-dataset-082116").IdProperty(i => i.GlobalDatasetId));
            settings.ThrowExceptions();

            ElasticClient client = new ElasticClient(settings);

            foreach (var dataset in context.Datasets.Where(x => x.ObjectStatus == Core.GlobalEnums.ObjectStatusEnum.Active && x.DatasetType == "DS").ToList())
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

                GlobalDataset fullNested = new GlobalDataset
                {
                    GlobalDatasetId = dataset.GlobalDatasetId.Value,
                    DatasetName = dataset.DatasetName,
                    DatasetSaidAssetCode = dataset.Asset.SaidKeyCode,
                    EnvironmentDatasets = new List<EnvironmentDataset>
                    {
                        new EnvironmentDataset
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
                        }
                    }
                };

                client.IndexDocument(fullNested);
            }
        }
    }
}
