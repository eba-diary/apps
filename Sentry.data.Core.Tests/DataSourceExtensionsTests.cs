using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataSourceExtensionsTests
    {
        [TestMethod]
        public void ToDto_AuthenticationType_AuthenticationTypeDto()
        {
            AuthenticationType authenticationType = new OAuthAuthentication
            {
                AuthID = 1,
                AuthName = "OAuth 2.0 Authentication"
            };

            AuthenticationTypeDto dto = authenticationType.ToDto();

            Assert.AreEqual(1, dto.AuthID);
            Assert.AreEqual("OAuth 2.0 Authentication", dto.AuthName);
        }
    }
}
