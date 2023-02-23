using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Associates;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using Sentry.data.Web.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class AddDatasetRequestValidatorTests
    {
        [TestMethod]
        public void Validate_Success()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = "Short",
                SaidAssetCode = "SAID",
                CategoryName = "Category",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                OriginalCreator = "Creator",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                PrimaryContactId = "000001",
                AlternateContactEmail = "me@sentry.com"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    DatasetName = "Name",
                    DatasetType = DataEntityCodes.DATASET,
                    NamedEnvironment = "TEST",
                    ShortName = "Short"
                }
            };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(new ValidationResults());

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsTrue(validationResponse.IsValid());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_Required_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                AlternateContactEmail = "me@sentry.com"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, null, null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(11, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.DatasetName));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.DatasetDescription));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.ShortName));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.SaidAssetCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.CategoryName));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(2, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", categories.Select(x => x.Name))}", fieldValidation.ValidationMessages.Last());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.OriginationCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(DatasetOriginationCode)))}", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.DataClassificationTypeCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(DataClassificationType)))}", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.OriginalCreator));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.NamedEnvironment));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.NamedEnvironmentTypeCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(NamedEnvironmentType)))}", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.PrimaryContactId));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_EnumValue_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = "Short",
                SaidAssetCode = "SAID",
                CategoryName = "Category",
                OriginationCode = null,
                DataClassificationTypeCode = "invalid",
                OriginalCreator = "Creator",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = "",
                PrimaryContactId = "000001",
                AlternateContactEmail = "me@sentry.com"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    DatasetName = "Name",
                    DatasetType = DataEntityCodes.DATASET,
                    NamedEnvironment = "TEST",
                    ShortName = "Short"
                }
            };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(3, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.OriginationCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(DatasetOriginationCode)))}", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.DataClassificationTypeCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(DataClassificationType)))}", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.NamedEnvironmentTypeCode));
            Assert.IsNotNull(fieldValidation);    
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(NamedEnvironmentType)))}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_MaxLength_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = "TooLongOfAShortName",
                SaidAssetCode = "SAID",
                CategoryName = "Category",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                OriginalCreator = "Creator",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                PrimaryContactId = "000001",
                AlternateContactEmail = "me@sentry.com"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    DatasetName = "Name",
                    DatasetType = DataEntityCodes.DATASET,
                    NamedEnvironment = "TEST",
                    ShortName = "Short"
                }
            };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(new ValidationResults());

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.First();
            Assert.AreEqual(nameof(AddDatasetRequestModel.ShortName), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Max length of 12 characters", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_RegularExpression_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = "Short Name",
                SaidAssetCode = "SAID",
                CategoryName = "Category",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                OriginalCreator = "Creator",
                NamedEnvironment = "NR DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                PrimaryContactId = "000001",
                AlternateContactEmail = "me@sentry.com"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            List<Category> categories = new List<Category>
            {       
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(2, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.ShortName));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Only alphanumeric characters are allowed", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.NamedEnvironment));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Must be alphanumeric, all caps, and less than 10 characters", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_DefaultShortName_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = SecurityConstants.ASSET_LEVEL_GROUP_NAME,
                SaidAssetCode = "SAID",
                CategoryName = "Category",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                OriginalCreator = "Creator",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                PrimaryContactId = "000001",
                AlternateContactEmail = "me@sentry.com"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    DatasetName = "Name",
                    DatasetType = DataEntityCodes.DATASET,
                    NamedEnvironment = "TEST",
                    ShortName = "Short"
                }
            };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(new ValidationResults());

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.First();
            Assert.AreEqual(nameof(AddDatasetRequestModel.ShortName), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Short name cannot be '{SecurityConstants.ASSET_LEVEL_GROUP_NAME}'", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_ContextChecks_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = "Short",
                SaidAssetCode = "SAID",
                CategoryName = "NotCategory",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                OriginalCreator = "Creator",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                PrimaryContactId = "000001"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    DatasetName = "Name",
                    DatasetType = DataEntityCodes.DATASET,
                    NamedEnvironment = "DEV",
                    ShortName = "Short"
                }
            };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(new ValidationResults());

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(3, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.DatasetName));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Dataset name already exists for the named environment", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.ShortName));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Short name is already in use by another Dataset for the named environment", fieldValidation.ValidationMessages.First());

            fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.CategoryName));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", categories.Select(x => x.Name))}", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_AlternateContactEmail_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = "Short",
                SaidAssetCode = "SAID",
                CategoryName = "Category",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                OriginalCreator = "Creator",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                PrimaryContactId = "000001",
                AlternateContactEmail = "notvalid@gmail.com"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    DatasetName = "Name",
                    DatasetType = DataEntityCodes.DATASET,
                    NamedEnvironment = "TEST",
                    ShortName = "Short"
                }
            };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(new ValidationResults());

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.First();
            Assert.AreEqual(nameof(AddDatasetRequestModel.AlternateContactEmail), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Must be valid sentry.com email address", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_PrimaryContactId_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = "Short",
                SaidAssetCode = "SAID",
                CategoryName = "Category",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                OriginalCreator = "Creator",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                PrimaryContactId = "000001"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    DatasetName = "Name",
                    DatasetType = DataEntityCodes.DATASET,
                    NamedEnvironment = "TEST",
                    ShortName = "Short"
                }
            };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(new ValidationResults());

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(() => null);

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.First();
            Assert.AreEqual(nameof(AddDatasetRequestModel.PrimaryContactId), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Must be a valid active associate", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_SaidAssetCode_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = "Short",
                SaidAssetCode = "SAID",
                CategoryName = "Category",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                OriginalCreator = "Creator",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                PrimaryContactId = "000001"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    DatasetName = "Name",
                    DatasetType = DataEntityCodes.DATASET,
                    NamedEnvironment = "TEST",
                    ShortName = "Short"
                }
            };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(false);

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.First();
            Assert.AreEqual(nameof(AddDatasetRequestModel.SaidAssetCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Must be a valid SAID asset code", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_NamedEnvironment_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = "Short",
                SaidAssetCode = "SAID",
                CategoryName = "Category",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                OriginalCreator = "Creator",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                PrimaryContactId = "000001"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    DatasetName = "Name",
                    DatasetType = DataEntityCodes.DATASET,
                    NamedEnvironment = "TEST",
                    ShortName = "Short"
                }
            };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            ValidationResults validationResults = new ValidationResults();
            validationResults.Add(ValidationErrors.NAMED_ENVIRONMENT_INVALID, $"Named Environment provided (\"DEV\") doesn't match a Quartermaster Named Environment for asset SAID");
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(validationResults);

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.First();
            Assert.AreEqual(nameof(AddDatasetRequestModel.NamedEnvironment), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Named Environment provided (\"DEV\") doesn't match a Quartermaster Named Environment for asset SAID", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_NamedEnvironmentType_Fail()
        {
            IRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description",
                ShortName = "Short",
                SaidAssetCode = "SAID",
                CategoryName = "Category",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                OriginalCreator = "Creator",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                PrimaryContactId = "000001"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    DatasetName = "Name",
                    DatasetType = DataEntityCodes.DATASET,
                    NamedEnvironment = "TEST",
                    ShortName = "Short"
                }
            };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<ISAIDService> saidService = mr.Create<ISAIDService>();
            saidService.Setup(x => x.VerifyAssetExistsAsync("SAID")).ReturnsAsync(true);

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            ValidationResults validationResults = new ValidationResults();
            validationResults.Add(ValidationErrors.NAMED_ENVIRONMENT_TYPE_INVALID, $"Named Environment Type provided (\"NonProd\") doesn't match Quartermaster (\"DEV\") for asset SAID");
            quartermasterService.Setup(x => x.VerifyNamedEnvironmentAsync("SAID", "DEV", NamedEnvironmentType.NonProd)).ReturnsAsync(validationResults);

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            AddDatasetRequestValidator validator = new AddDatasetRequestValidator(datasetContext.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.First();
            Assert.AreEqual(nameof(AddDatasetRequestModel.NamedEnvironmentTypeCode), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Named Environment Type provided (\"NonProd\") doesn't match Quartermaster (\"DEV\") for asset SAID", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }
    }
}
