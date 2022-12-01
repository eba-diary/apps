using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class EncryptionServiceTests
    {
        [TestMethod]
        public void IsEncrypted_NoIndicator_False()
        {
            EncryptionService service = new EncryptionService();

            bool result = service.IsEncrypted("plaintext");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEncrypted_OnlyBeginningIndicator_False()
        {
            EncryptionService service = new EncryptionService();

            bool result = service.IsEncrypted($"{Indicators.ENCRYPTIONINDICATOR}plaintext");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEncrypted_OnlyEndIndicator_False()
        {
            EncryptionService service = new EncryptionService();

            bool result = service.IsEncrypted($"plaintext{Indicators.ENCRYPTIONINDICATOR}");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEncrypted_EmptyString_False()
        {
            EncryptionService service = new EncryptionService();

            bool result = service.IsEncrypted($"");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEncrypted_WithIndicators_True()
        {
            EncryptionService service = new EncryptionService();

            bool result = service.IsEncrypted($"{Indicators.ENCRYPTIONINDICATOR}text{Indicators.ENCRYPTIONINDICATOR}");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PrepEncryptedForDisplay_EmptyString_EmptyString()
        {
            EncryptionService service = new EncryptionService();

            string result = service.PrepEncryptedForDisplay("");

            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void PrepEncryptedForDisplay_Text_TextWithIndicators()
        {
            EncryptionService service = new EncryptionService();

            string result = service.PrepEncryptedForDisplay("text");

            Assert.AreEqual($"{Indicators.ENCRYPTIONINDICATOR}text{Indicators.ENCRYPTIONINDICATOR}", result);
        }
    }
}
