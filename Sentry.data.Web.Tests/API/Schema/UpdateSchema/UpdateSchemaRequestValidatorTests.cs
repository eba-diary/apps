using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Associates;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class UpdateSchemaRequestValidatorTests
    {
        [TestMethod]
        public void Validate_Minimum_Success()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel();

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsTrue(validationResponse.IsValid());
        }

        [TestMethod]
        public void Validate_All_Success()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                SchemaDescription = "Description",
                Delimiter = ",",
                HasHeader = true,
                ScopeTypeCode = "Appending",
                FileTypeCode = ExtensionNames.CSV,
                SchemaRootPath = "root,path",
                CreateCurrentView = true,
                IngestionTypeCode = IngestionType.Topic.ToString(),
                IsCompressed = true,
                CompressionTypeCode = CompressionTypes.ZIP.ToString(),
                IsPreprocessingRequired = true,
                PreprocessingTypeCode = DataFlowPreProcessingTypes.googleapi.ToString(),
                KafkaTopicName = "TopicName",
                PrimaryContactId = "000001",
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>().AsQueryable());
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsTrue(validationResponse.IsValid());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_Delimiter_Space_Success()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                SchemaDescription = "Description",
                Delimiter = " ",
                HasHeader = true,
                ScopeTypeCode = "Appending",
                FileTypeCode = ExtensionNames.DELIMITED,
                SchemaRootPath = "root,path",
                CreateCurrentView = true,
                IngestionTypeCode = IngestionType.Topic.ToString(),
                IsCompressed = true,
                CompressionTypeCode = CompressionTypes.ZIP.ToString(),
                IsPreprocessingRequired = true,
                PreprocessingTypeCode = DataFlowPreProcessingTypes.googleapi.ToString(),
                KafkaTopicName = "TopicName",
                PrimaryContactId = "000001",
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>().AsQueryable());
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsTrue(validationResponse.IsValid());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_IngestionTypeCode_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                IngestionTypeCode = "invalid"
            };

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.IngestionTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(IngestionType)).Where(x => x != IngestionType.DSC_Pull.ToString()))}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_IngestionTypeCode_RequiredWhenKafkaTopicName_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                KafkaTopicName = "TopicName"
            };

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.IngestionTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be {IngestionType.Topic} to set {nameof(UpdateSchemaRequestModel.KafkaTopicName)}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_PrimaryContactId_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                PrimaryContactId = "000001"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(() => null);

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.PrimaryContactId), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must be a valid active associate", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_CompressionTypeCode_Invalid_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                IsCompressed = true,
                CompressionTypeCode = "invalid"
            };

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.CompressionTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(CompressionTypes)))}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_CompressionTypeCode_Null_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                IsCompressed = true
            };

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.CompressionTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value when {nameof(UpdateSchemaRequestModel.IsCompressed)} is true - {string.Join(" | ", Enum.GetNames(typeof(CompressionTypes)))}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_IsCompressed_Fail()
        {
            UpdateSchemaRequestModel requestModel = new UpdateSchemaRequestModel
            {
                IsCompressed = false,
                CompressionTypeCode = CompressionTypes.ZIP.ToString()
            };

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.IsCompressed), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must set to true to use {nameof(BaseSchemaModel.CompressionTypeCode)}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_PreprocessingTypeCode_Invalid_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                IsPreprocessingRequired = true,
                PreprocessingTypeCode = "invalid"
            };

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.PreprocessingTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(DataFlowPreProcessingTypes)))}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_PreprocessingTypeCode_Null_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                IsPreprocessingRequired = true
            };

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.PreprocessingTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value when {nameof(UpdateSchemaRequestModel.IsPreprocessingRequired)} is true - {string.Join(" | ", Enum.GetNames(typeof(DataFlowPreProcessingTypes)))}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_IsPreprocessingRequired_Fail()
        {
            UpdateSchemaRequestModel requestModel = new UpdateSchemaRequestModel
            {
                IsPreprocessingRequired = false,
                PreprocessingTypeCode = DataFlowPreProcessingTypes.claimiq.ToString()
            };

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.IsPreprocessingRequired), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must set to true to use {nameof(BaseSchemaModel.PreprocessingTypeCode)}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_KafkaTopicName_Fail()
        {
            UpdateSchemaRequestModel requestModel = new UpdateSchemaRequestModel
            {
                IngestionTypeCode = IngestionType.Topic.ToString()
            };

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.KafkaTopicName), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field when {nameof(UpdateSchemaRequestModel.IngestionTypeCode)} is {IngestionType.Topic}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_KafkaTopicName_Exists_Fail()
        {
            UpdateSchemaRequestModel requestModel = new UpdateSchemaRequestModel
            {
                IngestionTypeCode = IngestionType.Topic.ToString(),
                KafkaTopicName = "TopicName"
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>
            {
                new DataFlow { TopicName = "TopicName" }
            }.AsQueryable());

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.KafkaTopicName), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Kafka topic name already exists", fieldValidation.ValidationMessages.First());

            datasetContext.Verify();
        }

        [TestMethod]
        public void Validate_IngestionTypeCode_WithKafkaTopicName_Fail()
        {
            UpdateSchemaRequestModel requestModel = new UpdateSchemaRequestModel
            {
                IngestionTypeCode = IngestionType.S3_Drop.ToString(),
                KafkaTopicName = "TopicName"
            };

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.IngestionTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be {IngestionType.Topic} to set {nameof(UpdateSchemaRequestModel.KafkaTopicName)}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_FileTypeCode_Invalid_Fail()
        {
            UpdateSchemaRequestModel requestModel = new UpdateSchemaRequestModel
            {
                FileTypeCode = "invalid"
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>();

            List<FileExtension> fileTypes = new List<FileExtension>
            {
                new FileExtension { Name = ExtensionNames.JSON },
                new FileExtension { Name = ExtensionNames.CSV },
                new FileExtension { Name = ExtensionNames.DELIMITED }
            };
            datasetContext.SetupGet(x => x.FileExtensions).Returns(fileTypes.AsQueryable());

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.FileTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", fileTypes.Select(x => x.Name))}", fieldValidation.ValidationMessages.First());

            datasetContext.Verify();
        }

        [TestMethod]
        public void Validate_Delimiter_Null_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                FileTypeCode = ExtensionNames.DELIMITED
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>();

            List<FileExtension> fileTypes = new List<FileExtension>
            {
                new FileExtension { Name = ExtensionNames.JSON },
                new FileExtension { Name = ExtensionNames.CSV },
                new FileExtension { Name = ExtensionNames.DELIMITED }
            };
            datasetContext.SetupGet(x => x.FileExtensions).Returns(fileTypes.AsQueryable());

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.Delimiter), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field when {nameof(BaseSchemaModel.FileTypeCode)} is {ExtensionNames.DELIMITED}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_FileTypeCode_DelimitedComma_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                FileTypeCode = ExtensionNames.DELIMITED,
                Delimiter = ","
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>();

            List<FileExtension> fileTypes = new List<FileExtension>
            {
                new FileExtension { Name = ExtensionNames.JSON },
                new FileExtension { Name = ExtensionNames.CSV },
                new FileExtension { Name = ExtensionNames.DELIMITED }
            };
            datasetContext.SetupGet(x => x.FileExtensions).Returns(fileTypes.AsQueryable());

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.FileTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be {ExtensionNames.CSV} if {nameof(BaseSchemaModel.Delimiter)} is a comma (,)", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_Delimiter_CSVNoComma_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                FileTypeCode = ExtensionNames.CSV,
                Delimiter = "|"
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>();

            List<FileExtension> fileTypes = new List<FileExtension>
            {
                new FileExtension { Name = ExtensionNames.JSON },
                new FileExtension { Name = ExtensionNames.CSV },
                new FileExtension { Name = ExtensionNames.DELIMITED }
            };
            datasetContext.SetupGet(x => x.FileExtensions).Returns(fileTypes.AsQueryable());

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.Delimiter), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be a comma (,) when {nameof(BaseSchemaModel.FileTypeCode)} is {ExtensionNames.CSV}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_FileTypeCode_FileTypeWithDelimiter_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                FileTypeCode = ExtensionNames.JSON,
                Delimiter = "|"
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>();

            List<FileExtension> fileTypes = new List<FileExtension>
            {
                new FileExtension { Name = ExtensionNames.JSON },
                new FileExtension { Name = ExtensionNames.CSV },
                new FileExtension { Name = ExtensionNames.DELIMITED }
            };
            datasetContext.SetupGet(x => x.FileExtensions).Returns(fileTypes.AsQueryable());

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.FileTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be {ExtensionNames.CSV} or {ExtensionNames.DELIMITED} to set {nameof(BaseSchemaModel.Delimiter)}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_FileTypeCode_RequiredWithDelimiter_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                Delimiter = "|"
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>();

            List<FileExtension> fileTypes = new List<FileExtension>
            {
                new FileExtension { Name = ExtensionNames.JSON },
                new FileExtension { Name = ExtensionNames.CSV },
                new FileExtension { Name = ExtensionNames.DELIMITED }
            };
            datasetContext.SetupGet(x => x.FileExtensions).Returns(fileTypes.AsQueryable());

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.FileTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be {ExtensionNames.CSV} or {ExtensionNames.DELIMITED} to set {nameof(BaseSchemaModel.Delimiter)}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_HasHeader_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                FileTypeCode = ExtensionNames.JSON,
                HasHeader = true
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>();

            List<FileExtension> fileTypes = new List<FileExtension>
            {
                new FileExtension { Name = ExtensionNames.JSON },
                new FileExtension { Name = ExtensionNames.CSV },
                new FileExtension { Name = ExtensionNames.DELIMITED }
            };
            datasetContext.SetupGet(x => x.FileExtensions).Returns(fileTypes.AsQueryable());

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.HasHeader), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be false when {nameof(BaseSchemaModel.FileTypeCode)} is {ExtensionNames.JSON}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_ScopeTypeCode_Fail()
        {
            IRequestModel requestModel = new UpdateSchemaRequestModel
            {
                ScopeTypeCode = "invalid"
            };

            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>();
            List<DatasetScopeType> scopeTypes = new List<DatasetScopeType>
            {
                new DatasetScopeType { Name = "Appending" }
            };
            datasetContext.SetupGet(x => x.DatasetScopeTypes).Returns(scopeTypes.AsQueryable());

            UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(UpdateSchemaRequestModel.ScopeTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", scopeTypes.Select(x => x.Name))}", fieldValidation.ValidationMessages.First());
        }

        #region Helpers
        private Mock<IDatasetContext> GetDatasetContext(MockRepository mr)
        {
            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            List<FileExtension> fileExtensions = new List<FileExtension>
            {
                new FileExtension { Name = ExtensionNames.JSON },
                new FileExtension { Name = ExtensionNames.CSV },
                new FileExtension { Name = ExtensionNames.DELIMITED }
            };
            datasetContext.SetupGet(x => x.FileExtensions).Returns(fileExtensions.AsQueryable());

            List<DatasetScopeType> scopeTypes = new List<DatasetScopeType>
            {
                new DatasetScopeType { Name = "Appending" }
            };
            datasetContext.SetupGet(x => x.DatasetScopeTypes).Returns(scopeTypes.AsQueryable());

            return datasetContext;
        }

        private Mock<IAssociateInfoProvider> GetAssociateInfoProvider(MockRepository mr)
        {
            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            return associateInfoProvider;
        }
        #endregion
    }
}
