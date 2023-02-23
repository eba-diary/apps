using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web.API;
using System.Linq;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class ValidationModelMappingTests : BaseModelMappingTests
    {
        [TestMethod]
        public void Map_ConcurrentFieldValidationResponse_FieldValidationResponseModel()
        {
            ConcurrentFieldValidationResponse fieldValidation = new ConcurrentFieldValidationResponse
            {
                Field = "Field"
            };

            fieldValidation.AddValidationMessage("Message 1");
            fieldValidation.AddValidationMessage("Message 2");

            FieldValidationResponseModel model = _mapper.Map<FieldValidationResponseModel>(fieldValidation);

            Assert.AreEqual("Field", model.Field);
            Assert.AreEqual(2, model.ValidationMessages.Count);
            Assert.AreEqual("Message 1", model.ValidationMessages.First());
            Assert.AreEqual("Message 2", model.ValidationMessages.Last());
        }

        [TestMethod]
        public void Map_ConcurrentValidationResponse_ValidationResponseModel()
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            validationResponse.AddFieldValidation("Field", "Message 1");
            validationResponse.AddFieldValidation("Field 2", "Message 1");
            validationResponse.AddFieldValidation("Field", "Message 2");

            ValidationResponseModel model = _mapper.Map<ValidationResponseModel>(validationResponse);

            Assert.AreEqual(2, model.FieldValidations.Count);

            FieldValidationResponseModel fieldValidation = model.FieldValidations.First();
            Assert.AreEqual("Field", fieldValidation.Field);
            Assert.AreEqual(2, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Message 1", fieldValidation.ValidationMessages.First());
            Assert.AreEqual("Message 2", fieldValidation.ValidationMessages.Last());

            fieldValidation = model.FieldValidations.Last();
            Assert.AreEqual("Field 2", fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Message 1", fieldValidation.ValidationMessages.First());
        }
    }
}
