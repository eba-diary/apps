using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.API;
using System;
using System.Linq;
using System.Linq.Dynamic;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class ValidationExtensionsTests
    {
        [TestMethod]
        public void Validate_FromValidationResponseModel_FluentValidationResponse()
        {
            AddDatasetRequestModel requestModel = new AddDatasetRequestModel
            {
                DatasetName = "Name"
            };

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = requestModel.Validate(x => x.DatasetName);

            Assert.IsNotNull(fluent.ValidationResponse);
            Assert.AreEqual("Name", fluent.PropertyValue);
            Assert.AreEqual(nameof(requestModel.DatasetName), fluent.PropertyName);
            Assert.AreEqual(requestModel, fluent.RequestModel);
        }

        [TestMethod]
        public void Validate_FromFluentValidationResponse_FluentValidationResponse()
        {
            AddDatasetRequestModel requestModel = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                DatasetDescription = "Description"
            };

            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "DatasetName",
                PropertyValue = "Name",
                RequestModel = requestModel
            };

            fluent = fluent.Validate(x => x.DatasetDescription);

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.AreEqual("Description", fluent.PropertyValue);
            Assert.AreEqual(nameof(requestModel.DatasetDescription), fluent.PropertyName);
            Assert.AreEqual(requestModel, fluent.RequestModel);
        }

        [TestMethod]
        public void Required_Fail_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = null
            };

            fluent = fluent.Required();

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.IsNull(fluent.PropertyValue);
            Assert.AreEqual(1, fluent.ValidationResponse.FieldValidations.Count);

            Assert.IsTrue(fluent.ValidationResponse.FieldValidations.TryDequeue(out ConcurrentFieldValidationResponse fieldValidation));
            Assert.AreEqual("Field", fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);

            Assert.IsTrue(fieldValidation.ValidationMessages.TryDequeue(out string message));
            Assert.AreEqual("Required field", message);
        }

        [TestMethod]
        public void Required_Success_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = "Value"
            };

            fluent = fluent.Required();

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.IsNotNull(fluent.ValidationResponse);
            Assert.AreEqual("Value", fluent.PropertyValue);
            Assert.AreEqual("Field", fluent.PropertyName);
            Assert.IsNull(fluent.ValidationResponse.FieldValidations);
        }

        [TestMethod]
        public void MaxLength_Fail_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = "Value"
            };

            fluent = fluent.MaxLength(3);

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.AreEqual("Value", fluent.PropertyValue);
            Assert.AreEqual(1, fluent.ValidationResponse.FieldValidations.Count);

            Assert.IsTrue(fluent.ValidationResponse.FieldValidations.TryDequeue(out ConcurrentFieldValidationResponse fieldValidation));
            Assert.AreEqual("Field", fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);

            Assert.IsTrue(fieldValidation.ValidationMessages.TryDequeue(out string message));
            Assert.AreEqual("Max length of 3 characters", message);
        }

        [TestMethod]
        public void MaxLength_Success_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = "Value"
            };

            fluent = fluent.MaxLength(5);

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.AreEqual("Value", fluent.PropertyValue);
            Assert.AreEqual("Field", fluent.PropertyName);
            Assert.IsNull(fluent.ValidationResponse.FieldValidations);
        }

        [TestMethod]
        public void MaxLength_Null_Success_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = null
            };

            fluent = fluent.MaxLength(3);

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.IsNull(fluent.PropertyValue);
            Assert.AreEqual("Field", fluent.PropertyName);
            Assert.IsNull(fluent.ValidationResponse.FieldValidations);
        }

        [TestMethod]
        public void RegularExpression_Fail_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = "Va lue"
            };

            fluent = fluent.RegularExpression("^[0-9a-zA-Z]*$", "Message");

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.AreEqual("Va lue", fluent.PropertyValue);
            Assert.AreEqual(1, fluent.ValidationResponse.FieldValidations.Count);

            Assert.IsTrue(fluent.ValidationResponse.FieldValidations.TryDequeue(out ConcurrentFieldValidationResponse fieldValidation));
            Assert.AreEqual("Field", fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);

            Assert.IsTrue(fieldValidation.ValidationMessages.TryDequeue(out string message));
            Assert.AreEqual("Message", message);
        }

        [TestMethod]
        public void RegularExpression_Success_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = "Value"
            };

            fluent = fluent.RegularExpression("^[0-9a-zA-Z]*$", "Message");

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.AreEqual("Value", fluent.PropertyValue);
            Assert.AreEqual("Field", fluent.PropertyName);
            Assert.IsNull(fluent.ValidationResponse.FieldValidations);
        }

        [TestMethod]
        public void RegularExpression_Null_Success_FluentValidationResponse()
        {

            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = null
            };

            fluent = fluent.RegularExpression("^[0-9a-zA-Z]*$", "Message");

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.IsNull(fluent.PropertyValue);
            Assert.AreEqual("Field", fluent.PropertyName);
            Assert.IsNull(fluent.ValidationResponse.FieldValidations);
        }

        [TestMethod]
        public void EnumValue_Fail_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = "Value"
            };

            fluent = fluent.EnumValue(typeof(NamedEnvironmentType));

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.AreEqual("Value", fluent.PropertyValue);
            Assert.AreEqual(1, fluent.ValidationResponse.FieldValidations.Count);

            Assert.IsTrue(fluent.ValidationResponse.FieldValidations.TryDequeue(out ConcurrentFieldValidationResponse fieldValidation));
            Assert.AreEqual("Field", fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);

            Assert.IsTrue(fieldValidation.ValidationMessages.TryDequeue(out string message));
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(NamedEnvironmentType)))}", message);
        }

        [TestMethod]
        public void EnumValue_Success_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = NamedEnvironmentType.Prod.ToString()
            };

            fluent = fluent.EnumValue(typeof(NamedEnvironmentType));

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.AreEqual(NamedEnvironmentType.Prod.ToString(), fluent.PropertyValue);
            Assert.AreEqual("Field", fluent.PropertyName);
            Assert.IsNull(fluent.ValidationResponse.FieldValidations);
        }

        [TestMethod]
        public void EnumValue_CaseInsensitive_Success_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = "nonprod"
            };

            fluent = fluent.EnumValue(typeof(NamedEnvironmentType));

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.AreEqual("nonprod", fluent.PropertyValue);
            Assert.AreEqual("Field", fluent.PropertyName);
            Assert.IsNull(fluent.ValidationResponse.FieldValidations);
        }

        [TestMethod]
        public void EnumValue_NotRequired_Null_Success_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = null
            };

            fluent = fluent.EnumValue(typeof(NamedEnvironmentType));

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.IsNull(fluent.PropertyValue);
            Assert.AreEqual("Field", fluent.PropertyName);
            Assert.IsNull(fluent.ValidationResponse.FieldValidations);
        }

        [TestMethod]
        public void EnumValue_Required_Null_Fail_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = null,
                IsRequiredProperty = true
            };

            fluent = fluent.EnumValue(typeof(NamedEnvironmentType));

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.IsNull(fluent.PropertyValue);
            Assert.AreEqual(1, fluent.ValidationResponse.FieldValidations.Count);

            Assert.IsTrue(fluent.ValidationResponse.FieldValidations.TryDequeue(out ConcurrentFieldValidationResponse fieldValidation));
            Assert.AreEqual("Field", fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);

            Assert.IsTrue(fieldValidation.ValidationMessages.TryDequeue(out string message));
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(NamedEnvironmentType)))}", message);
        }

        [TestMethod]
        public void EnumValue_InvalidOption_Fail_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = "none"
            };

            fluent = fluent.EnumValue(typeof(DataClassificationType), DataClassificationType.None.ToString());

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.AreEqual("none", fluent.PropertyValue);
            Assert.AreEqual(1, fluent.ValidationResponse.FieldValidations.Count);

            Assert.IsTrue(fluent.ValidationResponse.FieldValidations.TryDequeue(out ConcurrentFieldValidationResponse fieldValidation));
            Assert.AreEqual("Field", fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);

            Assert.IsTrue(fieldValidation.ValidationMessages.TryDequeue(out string message));
            Assert.AreEqual($"Must provide a valid value - {string.Join(" | ", Enum.GetNames(typeof(DataClassificationType)).Where(x => x != DataClassificationType.None.ToString()))}", message);
        }

        [TestMethod]
        public void EnumValue_InvalidOption_Success_FluentValidationResponse()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            FluentValidationResponse<AddDatasetRequestModel, string> fluent = new FluentValidationResponse<AddDatasetRequestModel, string>
            {
                ValidationResponse = validationResponse,
                PropertyName = "Field",
                PropertyValue = "public"
            };

            fluent = fluent.EnumValue(typeof(DataClassificationType), DataClassificationType.None.ToString());

            Assert.AreEqual(validationResponse, fluent.ValidationResponse);
            Assert.AreEqual("public", fluent.PropertyValue);
            Assert.AreEqual("Field", fluent.PropertyName);
            Assert.IsNull(fluent.ValidationResponse.FieldValidations);
        }
    }
}
