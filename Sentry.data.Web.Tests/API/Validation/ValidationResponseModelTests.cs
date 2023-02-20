using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NHibernate.Util;
using Sentry.data.Web.API;
using System.Linq;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class ValidationResponseModelTests
    {
        [TestMethod]
        public void IsValid_NoFieldValidations_True()
        {
            ValidationResponseModel model = new ValidationResponseModel();

            Assert.IsTrue(model.IsValid());
        }

        [TestMethod]
        public void AddFieldValidation_NewField()
        {
            ValidationResponseModel model = new ValidationResponseModel();

            model.AddFieldValidation("Field", "Message");

            Assert.AreEqual(1, model.FieldValidations.Count);
            Assert.AreEqual(1, model.FieldValidations.First().ValidationMessages.Count);
            Assert.IsFalse(model.IsValid());
        }

        [TestMethod]
        public void AddFieldValidation_ExistingField()
        {
            ValidationResponseModel model = new ValidationResponseModel();

            model.AddFieldValidation("Field", "Message");
            model.AddFieldValidation("Field", "Message 2");

            Assert.AreEqual(1, model.FieldValidations.Count);
            Assert.AreEqual(2, model.FieldValidations.First().ValidationMessages.Count);
            Assert.IsFalse(model.IsValid());
        }

        [TestMethod]
        public void AddFieldValidation_AdditionalField()
        {
            ValidationResponseModel model = new ValidationResponseModel();

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
            ValidationResponseModel model = new ValidationResponseModel();

            model.AddFieldValidation("Field", "Message");

            Assert.IsTrue(model.HasValidationsFor("Field"));
        }

        [TestMethod]
        public void HasValidationsFor_Field_False()
        {
            ValidationResponseModel model = new ValidationResponseModel();

            model.AddFieldValidation("Field", "Message");

            Assert.IsFalse(model.HasValidationsFor("Field2"));
        }
    }
}
