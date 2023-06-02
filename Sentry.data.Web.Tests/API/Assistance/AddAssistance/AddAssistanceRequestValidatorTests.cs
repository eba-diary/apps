using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web.API;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class AddAssistanceRequestValidatorTests
    {
        [TestMethod]
        public async Task Validate_Success()
        {
            IRequestModel model = new AddAssistanceRequestModel
            {
                Summary = "Summary",
                Description = "Description"
            };

            AddAssistanceRequestValidator validator = new AddAssistanceRequestValidator();

            ConcurrentValidationResponse validationResponse = await validator.ValidateAsync(model);

            Assert.IsTrue(validationResponse.IsValid());
        }

        [TestMethod]
        public async Task Validate_Summary_Fail()
        {
            IRequestModel model = new AddAssistanceRequestModel
            {
                Description = "Description"
            };

            AddAssistanceRequestValidator validator = new AddAssistanceRequestValidator();

            ConcurrentValidationResponse validationResponse = await validator.ValidateAsync(model);

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddAssistanceRequestModel.Summary));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public async Task Validate_Description_Fail()
        {
            IRequestModel model = new AddAssistanceRequestModel
            {
                Summary = "Summary"
            };

            AddAssistanceRequestValidator validator = new AddAssistanceRequestValidator();

            ConcurrentValidationResponse validationResponse = await validator.ValidateAsync(model);

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault(x => x.Field == nameof(AddAssistanceRequestModel.Description));
            Assert.IsNotNull(fieldValidation);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("Required field", fieldValidation.ValidationMessages.First());
        }
    }
}
