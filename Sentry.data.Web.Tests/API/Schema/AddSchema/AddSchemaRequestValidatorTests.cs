using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Associates;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using Sentry.data.Web.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class AddSchemaRequestValidatorTests
    {
        [TestMethod]
        public void Validate_Required_Success()
        {
            IRequestModel requestModel = GetBaseSuccessModel();

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            Assert.IsTrue(validationResponse.IsValid());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_Optional_Success()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.IngestionTypeCode = IngestionType.Topic.ToString();
            requestModel.IsCompressed = true;
            requestModel.CompressionTypeCode = CompressionTypes.ZIP.ToString();
            requestModel.IsPreprocessingRequired = true;
            requestModel.PreprocessingTypeCode = DataFlowPreProcessingTypes.googleapi.ToString();
            requestModel.KafkaTopicName = "TopicName";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>().AsQueryable());

            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsTrue(validationResponse.IsValid());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_Compressed_NullTypeCode_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.IsCompressed = true;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.CompressionTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value when {nameof(AddSchemaRequestModel.IsCompressed)} is true - {string.Join(" | ", Enum.GetNames(typeof(CompressionTypes)))}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_Compressed_InvalidTypeCode_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.IsCompressed = true;
            requestModel.CompressionTypeCode = "invalid";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.CompressionTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(CompressionTypes)))}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_NotCompressed_ValidTypeCode_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.CompressionTypeCode = CompressionTypes.ZIP.ToString();

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.IsCompressed), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must set to true to use {nameof(BaseSchemaModel.CompressionTypeCode)}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_Preprocessing_NullTypeCode_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.IsPreprocessingRequired = true;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.PreprocessingTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value when {nameof(AddSchemaRequestModel.IsPreprocessingRequired)} is true - {string.Join(" | ", Enum.GetNames(typeof(DataFlowPreProcessingTypes)))}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_Preprocessing_InvalidTypeCode_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.IsPreprocessingRequired = true;
            requestModel.PreprocessingTypeCode = "invalid";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.PreprocessingTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(DataFlowPreProcessingTypes)))}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_Preprocessing_ValidTypeCode_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.PreprocessingTypeCode = DataFlowPreProcessingTypes.googleapi.ToString();

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.IsPreprocessingRequired), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must set to true to use {nameof(BaseSchemaModel.PreprocessingTypeCode)}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_FileTypeCode_Required_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.FileTypeCode = null;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.FileTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - JSON | CSV | DELIMITED", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_FileTypeCode_Invalid_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.FileTypeCode = "invalid";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.FileTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - JSON | CSV | DELIMITED", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_FileTypeCode_Delimited_NoDelimiter_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.FileTypeCode = ExtensionNames.DELIMITED;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.Delimiter), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field when {nameof(requestModel.FileTypeCode)} is {ExtensionNames.DELIMITED}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_FileTypeCode_Delimited_Success()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.FileTypeCode = ExtensionNames.DELIMITED;
            requestModel.Delimiter = "|";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsTrue(validationResponse.IsValid());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_FileTypeCode_CSV_NoDelimiter_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.FileTypeCode = ExtensionNames.CSV;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.Delimiter), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be a comma (,) when {nameof(requestModel.FileTypeCode)} is {ExtensionNames.CSV}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_FileTypeCode_CSV_NotComma_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.FileTypeCode = ExtensionNames.CSV;
            requestModel.Delimiter = "|";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.Delimiter), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be a comma (,) when {nameof(requestModel.FileTypeCode)} is {ExtensionNames.CSV}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_FileTypeCode_FileTypeWithDelimiter_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.FileTypeCode = ExtensionNames.JSON;
            requestModel.Delimiter = "|";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.FileTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be {ExtensionNames.CSV} or {ExtensionNames.DELIMITED} to set {nameof(BaseSchemaModel.Delimiter)}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_FileTypeCode_CSV_Success()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.FileTypeCode = ExtensionNames.CSV;
            requestModel.Delimiter = ",";
            requestModel.HasHeader = true;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsTrue(validationResponse.IsValid());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_HasHeader_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.FileTypeCode = ExtensionNames.JSON;
            requestModel.HasHeader = true;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.HasHeader), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be false when {nameof(BaseSchemaModel.FileTypeCode)} is {ExtensionNames.JSON}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_IngestionTypeCode_Required_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.IngestionTypeCode = null;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.IngestionTypeCode), fieldValidation.Field);
            Assert.AreEqual(2, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field", fieldValidation.ValidationMessages.First());
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(IngestionType)).Where(x => x != IngestionType.DSC_Pull.ToString()))}", fieldValidation.ValidationMessages.Last());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_IngestionTypeCode_Invalid_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.IngestionTypeCode = "invalid";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.IngestionTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(IngestionType)).Where(x => x != IngestionType.DSC_Pull.ToString()))}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_IngestionTypeCode_Topic_NullKafkaTopicName_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.IngestionTypeCode = IngestionType.Topic.ToString();

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.KafkaTopicName), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field when {nameof(requestModel.IngestionTypeCode)} is {IngestionType.Topic}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_IngestionTypeCode_NotTopic_KafkaTopicName_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.IngestionTypeCode = IngestionType.S3_Drop.ToString();
            requestModel.KafkaTopicName = "TopicName";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.IngestionTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value must be {IngestionType.Topic} to set {nameof(requestModel.KafkaTopicName)}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_IngestionTypeCode_Topic_KafkaTopicName_Exists_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.IngestionTypeCode = IngestionType.Topic.ToString();
            requestModel.KafkaTopicName = "TopicName";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            List<DataFlow> dataFlows = new List<DataFlow>
            {
                new DataFlow { TopicName = "TopicName" }
            };
            datasetContext.SetupGet(x => x.DataFlow).Returns(dataFlows.AsQueryable());

            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.KafkaTopicName), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Kafka topic name already exists", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_SchemaName_Required_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.SchemaName = null;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.SchemaName), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_SchemaName_MaxLength_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.SchemaName = StringOfLength(101);

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.SchemaName), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Max length of 100 characters", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_SchemaDescription_Required_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.SchemaDescription = null;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.SchemaDescription), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_DatasetId_NotFound_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr, true);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.DatasetId), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Dataset does not exist", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_SchemaName_Exists_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.SchemaName = "Other";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.SchemaName), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Schema name already exists for the Dataset", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_SchemaName_FileSchemaFlow_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.SchemaName = "FileSchemaFlow";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.SchemaName), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"FileSchemaFlow is a reserved name", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_ScopeTypeCode_Required_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.ScopeTypeCode = null;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.ScopeTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - Appending", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_ScopeTypeCode_Invalid_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.ScopeTypeCode = "invalid";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.ScopeTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - Appending", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_PrimaryContactId_Required_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.PrimaryContactId = null;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.PrimaryContactId), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_PrimaryContactId_NotExists_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IQuartermasterService> quartermasterService = GetQuartermasterService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(() => null);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.PrimaryContactId), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must be a valid active associate", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_SaidAssetCode_Required_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.SaidAssetCode = null;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, null, null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.SaidAssetCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_SaidAssetCode_Exists_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(false);

            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.SaidAssetCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must be a valid SAID asset code", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_NamedEnvironment_Required_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.NamedEnvironment = null;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.NamedEnvironment), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_NamedEnvironment_ReqularExpression_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.NamedEnvironment = "NR DEV";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.NamedEnvironment), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must be alphanumeric, all caps, and less than 10 characters", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_NamedEnvironment_Verify_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            ValidationResults validationResults = new ValidationResults();
            validationResults.Add(ValidationErrors.NAMED_ENVIRONMENT_INVALID, $"Named Environment provided (\"DEV\") doesn't match a Quartermaster Named Environment for asset SAID");
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(validationResults);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.NamedEnvironment), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Named Environment provided (\"DEV\") doesn't match a Quartermaster Named Environment for asset SAID", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_NamedEnvironmentTypeCode_Required_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.NamedEnvironmentTypeCode = null;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.NamedEnvironmentTypeCode), fieldValidation.Field);
            Assert.AreEqual(2, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(NamedEnvironmentType)))}", fieldValidation.ValidationMessages.Last());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_NamedEnvironmentTypeCode_Invalid_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();
            requestModel.NamedEnvironmentTypeCode = "invalid";

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.NamedEnvironmentTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(NamedEnvironmentType)))}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_NamedEnvironmentType_Verify_Fail()
        {
            AddSchemaRequestModel requestModel = GetBaseSuccessModel();

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            Mock<ISAIDService> saidService = GetSaidService(mr);
            Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            ValidationResults validationResults = new ValidationResults();
            validationResults.Add(ValidationErrors.NAMED_ENVIRONMENT_TYPE_INVALID, $"Named Environment Type provided (\"NonProd\") doesn't match Quartermaster (\"DEV\") for asset SAID");
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(validationResults);

            AddSchemaRequestValidator validator = new AddSchemaRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync((IRequestModel)requestModel).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(requestModel.NamedEnvironmentTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Named Environment Type provided (\"NonProd\") doesn't match Quartermaster (\"DEV\") for asset SAID", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        #region Helpers
        private AddSchemaRequestModel GetBaseSuccessModel()
        {
            return new AddSchemaRequestModel
            {
                SchemaName = "Name",
                SchemaDescription = "Description",
                FileTypeCode = ExtensionNames.JSON,
                IngestionTypeCode = IngestionType.S3_Drop.ToString(),
                SaidAssetCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                DatasetId = 1,
                PrimaryContactId = "000001",
                ScopeTypeCode = "Appending"
            };
        }

        private Mock<IDatasetContext> GetDatasetContext(MockRepository mr, bool nullDataset = false)
        {
            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            Dataset dataset = nullDataset ? null : new Dataset
            {
                DatasetFileConfigs = new List<DatasetFileConfig>
                {
                    new DatasetFileConfig { Name = "Name", DeleteInd = true },
                    new DatasetFileConfig { Name = "Other" }
                }
            };
            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(dataset);

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

        private Mock<ISAIDService> GetSaidService(MockRepository mr)
        {
            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            return saidService;
        }

        private Mock<IQuartermasterService> GetQuartermasterService(MockRepository mr)
        {
            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(new ValidationResults());

            return quartermasterService;
        }

        private Mock<IAssociateInfoProvider> GetAssociateInfoProvider(MockRepository mr)
        {
            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            return associateInfoProvider;
        }

        private string StringOfLength(int length)
        {
            return new string('*', length);
        }
        #endregion
    }
}
