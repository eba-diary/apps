using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Util;
using Sentry.Core;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void FilterCategoryOptionModel_OptionId_Encode()
        {
            FilterCategoryOptionModel model = new FilterCategoryOptionModel()
            {
                OptionValue = "Value w/ special ch@r@cters",
                ParentCategoryName = "Category"
            };

            Assert.AreEqual("Category_Value+w%2f+special+ch%40r%40cters", model.OptionId);
        }

        [TestMethod]
        public void DataFlowModel_Validate_TokenPagingHTTPS_Success()
        {
            DataFlowModel model = new DataFlowModel
            {
                Name = "DataFlowModel",
                IngestionTypeSelection = 2,
                RetrieverJob = new JobModel
                {
                    SelectedSourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                    SelectedDataSource = "3",
                    SchedulePicker = "4",
                    Schedule = "* * * * *",
                    PagingType = Core.PagingType.Token,
                    PageParameterName = "ParameterName",
                    PageTokenField = "PageToken"
                },
                SchemaMaps = new List<SchemaMapModel>
                {
                    new SchemaMapModel
                    {
                        SelectedDataset = 5,
                        SelectedSchema = 6
                    }
                },
                IsPreProcessingRequired = false,
                SAIDAssetKeyCode = "SAID"
            };

            ValidationException result = model.Validate();

            Assert.IsTrue(result.ValidationResults.IsValid());
        }

        [TestMethod]
        public void DataFlowModel_Validate_TokenPagingHTTPS_Fail()
        {
            DataFlowModel model = new DataFlowModel
            {
                Name = "DataFlowModel",
                IngestionTypeSelection = 2,
                RetrieverJob = new JobModel
                {
                    SelectedSourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                    SelectedDataSource = "3",
                    SchedulePicker = "4",
                    Schedule = "* * * * *",
                    PagingType = Core.PagingType.Token
                },
                SchemaMaps = new List<SchemaMapModel>
                {
                    new SchemaMapModel
                    {
                        SelectedDataset = 5,
                        SelectedSchema = 6
                    }
                },
                IsPreProcessingRequired = false,
                SAIDAssetKeyCode = "SAID"
            };

            ValidationException result = model.Validate();

            Assert.IsFalse(result.ValidationResults.IsValid());
            Assert.AreEqual(2, result.ValidationResults.GetAll().Count);

            ValidationResult validationResult = result.ValidationResults.GetAll()[0];
            Assert.AreEqual("PageParameterName", validationResult.Id);
            Assert.AreEqual("Page Parameter Name is required", validationResult.Description);

            validationResult = result.ValidationResults.GetAll()[1];
            Assert.AreEqual("PageTokenField", validationResult.Id);
            Assert.AreEqual("Page Token Field is required", validationResult.Description);
        }

        [TestMethod]
        public void DataFlowModel_Validate_PageNumberPagingHTTPS_Success()
        {
            DataFlowModel model = new DataFlowModel
            {
                Name = "DataFlowModel",
                IngestionTypeSelection = 2,
                RetrieverJob = new JobModel
                {
                    SelectedSourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                    SelectedDataSource = "3",
                    SchedulePicker = "4",
                    Schedule = "* * * * *",
                    PagingType = Core.PagingType.PageNumber,
                    PageParameterName = "ParameterName"
                },
                SchemaMaps = new List<SchemaMapModel>
                {
                    new SchemaMapModel
                    {
                        SelectedDataset = 5,
                        SelectedSchema = 6
                    }
                },
                IsPreProcessingRequired = false,
                SAIDAssetKeyCode = "SAID"
            };

            ValidationException result = model.Validate();

            Assert.IsTrue(result.ValidationResults.IsValid());
        }

        [TestMethod]
        public void DataFlowModel_Validate_PageNumberPagingHTTPS_Fail()
        {
            DataFlowModel model = new DataFlowModel
            {
                Name = "DataFlowModel",
                IngestionTypeSelection = 2,
                RetrieverJob = new JobModel
                {
                    SelectedSourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                    SelectedDataSource = "3",
                    SchedulePicker = "4",
                    Schedule = "* * * * *",
                    PagingType = Core.PagingType.PageNumber
                },
                SchemaMaps = new List<SchemaMapModel>
                {
                    new SchemaMapModel
                    {
                        SelectedDataset = 5,
                        SelectedSchema = 6
                    }
                },
                IsPreProcessingRequired = false,
                SAIDAssetKeyCode = "SAID"
            };

            ValidationException result = model.Validate();

            Assert.IsFalse(result.ValidationResults.IsValid());
            Assert.AreEqual(1, result.ValidationResults.GetAll().Count);

            ValidationResult validationResult = result.ValidationResults.GetAll()[0];
            Assert.AreEqual("PageParameterName", validationResult.Id);
            Assert.AreEqual("Page Parameter Name is required", validationResult.Description);
        }
    }
}