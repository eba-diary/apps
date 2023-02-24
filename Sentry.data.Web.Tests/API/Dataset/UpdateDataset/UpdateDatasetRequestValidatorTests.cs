using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core;
using Sentry.data.Web.API;
using System;
using static Sentry.data.Core.GlobalConstants;
using System.Collections.Generic;
using System.Linq;
using Sentry.Associates;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class UpdateDatasetRequestValidatorTests
    {
        [TestMethod]
        public void Validate_Success()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                CategoryCode = "Category",
                DatasetDescription = "Description",
                UsageInformation = "Usage",
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                IsSecured = true,
                PrimaryContactId = "000001",
                AlternateContactEmail = "me@sentry.com",
                OriginationCode = DatasetOriginationCode.External.ToString(),
                OriginalCreator = "Creator"
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>(); List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000001")).ReturnsAsync(new Associate());

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(datasetContext.Object, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsTrue(validationResponse.IsValid());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_Minium_Success()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                OriginationCode = DatasetOriginationCode.External.ToString()
            };

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsTrue(validationResponse.IsValid());
        }

        [TestMethod]
        public void Validate_DatasetDescription_Fail()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                DatasetDescription = StringOfLength(4100),
                OriginationCode = DatasetOriginationCode.External.ToString()
            };

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.DatasetDescription));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Max length of 4096 characters", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_OriginationCode_Required_Fail()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                OriginationCode = null
            };

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.OriginationCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(2, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());
            Assert.AreEqual($"Must provide a valid value - { string.Join(" | ", Enum.GetNames(typeof(DatasetOriginationCode))) }", fieldValidation.ValidationMessages.Last());
        }

        [TestMethod]
        public void Validate_OriginationCode_Invalid_Fail()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                OriginationCode = "invalid"
            };

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.OriginationCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(DatasetOriginationCode)))}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_DataClassificationTypeCode_Invalid_Fail()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                DataClassificationTypeCode = "invalid",
                OriginationCode = DatasetOriginationCode.External.ToString()
            };

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.DataClassificationTypeCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(DataClassificationType)).Where(x => x != DataClassificationType.None.ToString()))}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_DataClassificationTypeCode_None_Fail()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                DataClassificationTypeCode = DataClassificationType.None.ToString(),
                OriginationCode = DatasetOriginationCode.External.ToString()
            };

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.DataClassificationTypeCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(DataClassificationType)).Where(x => x != DataClassificationType.None.ToString()))}", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_OriginalCreator_Fail()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                OriginalCreator = StringOfLength(130),
                OriginationCode = DatasetOriginationCode.External.ToString()
            };

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.OriginalCreator));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Max length of 128 characters", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_UsageInformation_Fail()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                UsageInformation = StringOfLength(4100),
                OriginationCode = DatasetOriginationCode.External.ToString()
            };

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.UsageInformation));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Max length of 4096 characters", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_PrimaryContactId_Fail()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                PrimaryContactId = "000000",
                OriginationCode = DatasetOriginationCode.External.ToString()
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IAssociateInfoProvider> associateInfoProvider = mr.Create<IAssociateInfoProvider>();

            Associate associate = null;
            associateInfoProvider.Setup(x => x.GetActiveAssociateByIdAsync("000000")).ReturnsAsync(associate);

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(null, associateInfoProvider.Object);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.PrimaryContactId));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Must be a valid active associate", fieldValidation.ValidationMessages.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public void Validate_AlternateContactEmail_Fail()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                AlternateContactEmail = "me@gmail.com",
                OriginationCode = DatasetOriginationCode.External.ToString()
            };

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(null, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.AlternateContactEmail));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Must be valid sentry.com email address", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public void Validate_CategoryCode_Fail()
        {
            IRequestModel model = new UpdateDatasetRequestModel
            {
                CategoryCode = "other",
                OriginationCode = DatasetOriginationCode.External.ToString()
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>(); List<Category> categories = new List<Category>
            {
                new Category { Name = "Category", ObjectType = DataEntityCodes.DATASET }
            };

            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            UpdateDatasetRequestValidator validator = new UpdateDatasetRequestValidator(datasetContext.Object, null);

            ConcurrentValidationResponse validationResponse = validator.ValidateAsync(model).Result;

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddDatasetRequestModel.CategoryCode));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", categories.Select(x => x.Name))}", fieldValidation.ValidationMessages.First());
        }

        private string StringOfLength(int length)
        {
            return new string('*', length);
        }
    }
}
