using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.Core;
using Sentry.data.Core;
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
                    PagingType = PagingType.PageNumber,
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

        [TestMethod]
        public void JobModel_NoRequestVariablesUsed_Success()
        {
            JobModel job = new JobModel
            {
                SelectedSourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                SelectedDataSource = "3",
                SchedulePicker = "4",
                Schedule = "* * * * *",
                RelativeUri = "NoVariables",
                RequestVariables = new List<RequestVariableModel>()
            };

            ValidationException result = job.Validate();

            Assert.IsTrue(result.ValidationResults.IsValid());
        }

        [TestMethod]
        public void JobModel_VariablesInRelativeUri_NoRequestVariables_Fail()
        {
            string var1 = string.Format(Indicators.REQUESTVARIABLEINDICATOR, "Var1");
            string var2 = string.Format(Indicators.REQUESTVARIABLEINDICATOR, "Var2");

            JobModel job = new JobModel
            {
                SelectedSourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                SelectedDataSource = "3",
                SchedulePicker = "4",
                Schedule = "* * * * *",
                RelativeUri = $"Search/{var1}/Data?param={var2}&param2={var1}"
            };

            ValidationException result = job.Validate();

            Assert.IsFalse(result.ValidationResults.IsValid());
            Assert.AreEqual(1, result.ValidationResults.GetAll().Count);

            ValidationResult validationResult = result.ValidationResults.GetAll()[0];
            Assert.AreEqual("RetrieverJob.RelativeUri", validationResult.Id);
            Assert.AreEqual($"Request Variable(s) not defined for {var1}, {var2}", validationResult.Description);
        }

        [TestMethod]
        public void JobModel_NoVariablesInRelativeUri_RequestVariables_Fail()
        {
            JobModel job = new JobModel
            {
                SelectedSourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                SelectedDataSource = "3",
                SchedulePicker = "4",
                Schedule = "* * * * *",
                RelativeUri = $"Search/Data",
                RequestVariables = new List<RequestVariableModel>
                {
                    new RequestVariableModel
                    {
                        Index = "1",
                        VariableName = "Var1",
                        VariableValue = "2022-12-01",
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    },
                    new RequestVariableModel
                    {
                        Index = "2",
                        VariableName = "Var2",
                        VariableValue = "2022-12-02",
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    }
                }
            };

            ValidationException result = job.Validate();

            Assert.IsFalse(result.ValidationResults.IsValid());
            Assert.AreEqual(2, result.ValidationResults.GetAll().Count);

            ValidationResult validationResult = result.ValidationResults.GetAll()[0];
            Assert.AreEqual("RetrieverJob.RequestVariables[1].VariableName", validationResult.Id);
            Assert.AreEqual($"{string.Format(Indicators.REQUESTVARIABLEINDICATOR, "Var1")} is not used in Relative URI", validationResult.Description);

            validationResult = result.ValidationResults.GetAll()[1];
            Assert.AreEqual("RetrieverJob.RequestVariables[2].VariableName", validationResult.Id);
            Assert.AreEqual($"{string.Format(Indicators.REQUESTVARIABLEINDICATOR, "Var2")} is not used in Relative URI", validationResult.Description);
        }

        [TestMethod]
        public void JobModel_MultipleVariablesInRelativeUri_MultipleRequestVariables_Fail()
        {
            string var1 = string.Format(Indicators.REQUESTVARIABLEINDICATOR, "Var1");
            string var2 = string.Format(Indicators.REQUESTVARIABLEINDICATOR, "Var2");

            JobModel job = new JobModel
            {
                SelectedSourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                SelectedDataSource = "3",
                SchedulePicker = "4",
                Schedule = "* * * * *",
                RelativeUri = $"Search/{var1}/Data?param={var2}",
                RequestVariables = new List<RequestVariableModel>
                {
                    new RequestVariableModel
                    {
                        Index = "1",
                        VariableName = "Var1",
                        VariableValue = "2022-12-01",
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    },
                    new RequestVariableModel
                    {
                        Index = "3",
                        VariableName = "Var3",
                        VariableValue = "2022-12-03",
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    }
                }
            };

            ValidationException result = job.Validate();

            Assert.IsFalse(result.ValidationResults.IsValid());
            Assert.AreEqual(2, result.ValidationResults.GetAll().Count);

            ValidationResult validationResult = result.ValidationResults.GetAll()[0];
            Assert.AreEqual("RetrieverJob.RelativeUri", validationResult.Id);
            Assert.AreEqual($"Request Variable(s) not defined for {var2}", validationResult.Description);

            validationResult = result.ValidationResults.GetAll()[1];
            Assert.AreEqual("RetrieverJob.RequestVariables[3].VariableName", validationResult.Id);
            Assert.AreEqual($"{string.Format(Indicators.REQUESTVARIABLEINDICATOR, "Var3")} is not used in Relative URI", validationResult.Description);
        }

        [TestMethod]
        public void JobModel_MultipleVariablesInRelativeUri_MultipleRequestVariables_Success()
        {
            string var1 = string.Format(Indicators.REQUESTVARIABLEINDICATOR, "Var1");
            string var2 = string.Format(Indicators.REQUESTVARIABLEINDICATOR, "Var2");

            JobModel job = new JobModel
            {
                SelectedSourceType = DataSourceDiscriminator.HTTPS_SOURCE,
                SelectedDataSource = "3",
                SchedulePicker = "4",
                Schedule = "* * * * *",
                RelativeUri = $"Search/{var1}/Data?param={var2}&param2={var1}",
                RequestVariables = new List<RequestVariableModel>
                {
                    new RequestVariableModel
                    {
                        Index = "1",
                        VariableName = "Var1",
                        VariableValue = "2022-12-01",
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    },
                    new RequestVariableModel
                    {
                        Index = "2",
                        VariableName = "Var2",
                        VariableValue = "2022-12-02",
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    }
                }
            };

            ValidationException result = job.Validate();

            Assert.IsTrue(result.ValidationResults.IsValid());
        }

        [TestMethod]
        public void RequestVariableModel_RequiredFields_Fail()
        {
            RequestVariableModel model = new RequestVariableModel
            {
                Index = "1",
                VariableName = "",
                VariableValue = "",
                VariableIncrementType = RequestVariableIncrementType.None
            };

            ValidationResults results = model.Validate();

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(3, results.GetAll().Count);

            ValidationResult validationResult = results.GetAll()[0];
            Assert.AreEqual("RetrieverJob.RequestVariables[1].VariableName", validationResult.Id);
            Assert.AreEqual($"Variable Name is required", validationResult.Description);

            validationResult = results.GetAll()[1];
            Assert.AreEqual("RetrieverJob.RequestVariables[1].VariableValue", validationResult.Id);
            Assert.AreEqual($"Variable Value is required", validationResult.Description);

            validationResult = results.GetAll()[2];
            Assert.AreEqual("RetrieverJob.RequestVariables[1].VariableIncrementType", validationResult.Id);
            Assert.AreEqual($"Variable Increment Type is required", validationResult.Description);
        }

        [TestMethod]
        public void RequestVariableModel_NonAlphanumericVariableName_Fail()
        {
            RequestVariableModel model = new RequestVariableModel
            {
                Index = "1",
                VariableName = "v@r$123",
                VariableValue = "2022-12-01",
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            ValidationResults results = model.Validate();

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(1, results.GetAll().Count);

            ValidationResult validationResult = results.GetAll()[0];
            Assert.AreEqual("RetrieverJob.RequestVariables[1].VariableName", validationResult.Id);
            Assert.AreEqual($"Variable Name must be alphanumeric", validationResult.Description);
        }

        [TestMethod]
        public void RequestVariableModel_InvalidFormatVariableValue_Fail()
        {
            RequestVariableModel model = new RequestVariableModel
            {
                Index = "1",
                VariableName = "var1",
                VariableValue = "20221201",
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            ValidationResults results = model.Validate();

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(1, results.GetAll().Count);

            ValidationResult validationResult = results.GetAll()[0];
            Assert.AreEqual("RetrieverJob.RequestVariables[1].VariableValue", validationResult.Id);
            Assert.AreEqual($"Variable Value must be in yyyy-MM-dd format to use with '{RequestVariableIncrementType.DailyExcludeToday.GetDescription()}'", validationResult.Description);
        }

        [TestMethod]
        public void RequestVariableModel_Success()
        {
            RequestVariableModel model = new RequestVariableModel
            {
                Index = "1",
                VariableName = "var1",
                VariableValue = "2022-12-01",
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            ValidationResults results = model.Validate();

            Assert.IsTrue(results.IsValid());
        }
    }
}