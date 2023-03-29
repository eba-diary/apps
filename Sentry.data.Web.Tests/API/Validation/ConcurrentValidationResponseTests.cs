using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web.API;
using System.Linq;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class ConcurrentValidationResponseTests
    {
        [TestMethod]
        public void IsValid_NoFieldValidations_True()
        {
            ConcurrentValidationResponse model = new ConcurrentValidationResponse();

            Assert.IsTrue(model.IsValid());
        }

        [TestMethod]
        public void AddFieldValidation_NewField()
        {
            ConcurrentValidationResponse model = new ConcurrentValidationResponse();

            model.AddFieldValidation("Field", "Message");

            Assert.AreEqual(1, model.FieldValidations.Count);
            Assert.AreEqual(1, model.FieldValidations.First().ValidationMessages.Count);
            Assert.IsFalse(model.IsValid());
        }

        [TestMethod]
        public void AddFieldValidation_ExistingField()
        {
            ConcurrentValidationResponse model = new ConcurrentValidationResponse();

            model.AddFieldValidation("Field", "Message");
            model.AddFieldValidation("Field", "Message 2");

            Assert.AreEqual(1, model.FieldValidations.Count);
            Assert.AreEqual(2, model.FieldValidations.First().ValidationMessages.Count);
            Assert.IsFalse(model.IsValid());
        }

        [TestMethod]
        public void AddFieldValidation_AdditionalField()
        {
            ConcurrentValidationResponse model = new ConcurrentValidationResponse();

            model.AddFieldValidation("Field", "Message");
            model.AddFieldValidation("Field", "Message 2");
            model.AddFieldValidation("Field2", "Message");

            Assert.AreEqual(2, model.FieldValidations.Count);
            Assert.AreEqual(2, model.FieldValidations.First().ValidationMessages.Count);
            Assert.AreEqual(1, model.FieldValidations.Last().ValidationMessages.Count);
            Assert.IsFalse(model.IsValid());
        }

        [TestMethod]
        public void HasValidationsFor_Field_True()
        {
            ConcurrentValidationResponse model = new ConcurrentValidationResponse();

            model.AddFieldValidation("Field", "Message");

            Assert.IsTrue(model.HasValidationsFor("Field"));
        }

        [TestMethod]
        public void HasValidationsFor_Field_False()
        {
            ConcurrentValidationResponse model = new ConcurrentValidationResponse();

            model.AddFieldValidation("Field", "Message");

            Assert.IsFalse(model.HasValidationsFor("Field2"));
        }

        [TestMethod]
        public void AddValidationsFor_ConcurrentValidationResponse_Null()
        {
            ConcurrentValidationResponse toValidationResponse = new ConcurrentValidationResponse();

            ConcurrentValidationResponse fromValidationResponse = new ConcurrentValidationResponse();

            toValidationResponse.AddValidationsFrom(fromValidationResponse);

            Assert.IsTrue(toValidationResponse.IsValid());
            Assert.IsNull(toValidationResponse.FieldValidations);
        }

        [TestMethod]
        public void AddValidationsFor_ConcurrentValidationResponse_FieldValidations()
        {
            ConcurrentValidationResponse toValidationResponse = new ConcurrentValidationResponse();
            toValidationResponse.AddFieldValidation("Field1", "F1 Message 1");

            ConcurrentValidationResponse fromValidationResponse = new ConcurrentValidationResponse();
            fromValidationResponse.AddFieldValidation("Field2", "F2 Message 1");
            fromValidationResponse.AddFieldValidation("Field1", "F1 Message 2");
            fromValidationResponse.AddFieldValidation("Field2", "F2 Message 2");

            toValidationResponse.AddValidationsFrom(fromValidationResponse);

            Assert.IsFalse(toValidationResponse.IsValid());
            Assert.IsNotNull(toValidationResponse.FieldValidations);
            Assert.AreEqual(2, toValidationResponse.FieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = toValidationResponse.FieldValidations.First();
            Assert.AreEqual("Field1", fieldValidation.Field);
            Assert.AreEqual(2, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("F1 Message 1", fieldValidation.ValidationMessages.First());
            Assert.AreEqual("F1 Message 2", fieldValidation.ValidationMessages.Last());

            fieldValidation = toValidationResponse.FieldValidations.Last();
            Assert.AreEqual("Field2", fieldValidation.Field);
            Assert.AreEqual(2, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual("F2 Message 1", fieldValidation.ValidationMessages.First());
            Assert.AreEqual("F2 Message 2", fieldValidation.ValidationMessages.Last());
        }
    }
}
