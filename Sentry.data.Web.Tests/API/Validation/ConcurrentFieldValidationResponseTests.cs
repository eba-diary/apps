using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Util;
using Sentry.data.Web.API;
using System.Linq;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class ConcurrentFieldValidationResponseTests
    {
        [TestMethod]
        public void AddValdiationMessage_AddNewMessage()
        {
            ConcurrentFieldValidationResponse model = new ConcurrentFieldValidationResponse
            {
                Field = "Field"
            };

            model.AddValidationMessage("Message");

            Assert.AreEqual("Field", model.Field);
            Assert.AreEqual(1, model.ValidationMessages.Count);
            Assert.AreEqual("Message", model.ValidationMessages.First());
        }

        [TestMethod]
        public void AddValdiationMessage_AddAdditionalMessage()
        {
            ConcurrentFieldValidationResponse model = new ConcurrentFieldValidationResponse
            {
                Field = "Field"
            };

            model.AddValidationMessage("Message");
            model.AddValidationMessage("Message 2");

            Assert.AreEqual("Field", model.Field);
            Assert.AreEqual(2, model.ValidationMessages.Count);
            Assert.IsNotNull("Message", model.ValidationMessages.First());
            Assert.AreEqual("Message 2", model.ValidationMessages.Last());
        }
    }
}
