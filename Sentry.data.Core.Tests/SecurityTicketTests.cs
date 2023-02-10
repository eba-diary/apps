using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SecurityTicketTests
    {
        /// <summary>
        /// Tests that a Security Ticket with an AdGroupName defined returns the correct Identity/IdentityType
        /// </summary>
        [TestMethod]
        public void SecurityTicket_Identity_Ad()
        {
            var ticket = new SecurityTicket() { AdGroupName = "MyADGroup" };
            Assert.AreEqual("MyADGroup", ticket.Identity);
            Assert.AreEqual(GlobalConstants.IdentityType.AD, ticket.IdentityType);
        }

        /// <summary>
        /// Tests that a Security Ticket with an UserId defined returns the correct Identity/IdentityType
        /// </summary>
        [TestMethod]
        public void SecurityTicket_Identity_User()
        {
            var ticket = new SecurityTicket() { GrantPermissionToUserId = "067664" };
            Assert.AreEqual("067664", ticket.Identity);
            Assert.AreEqual(GlobalConstants.IdentityType.AD, ticket.IdentityType);
        }

        /// <summary>
        /// Tests that a Security Ticket with an AWS Arn defined returns the correct Identity/IdentityType
        /// </summary>
        [TestMethod]
        public void SecurityTicket_Identity_Aws()
        {
            var ticket = new SecurityTicket() { AwsArn = "arn:aws:iam::376585054624:role/CA-DataEngineering" };
            Assert.AreEqual("arn:aws:iam::376585054624:role/CA-DataEngineering", ticket.Identity);
            Assert.AreEqual(GlobalConstants.IdentityType.AWS_IAM, ticket.IdentityType);
        }
        
        /// <summary>
        /// Tests that a Security Ticket with a Snowflake Account defined returns the correct Identity/IdentityType
        /// </summary>
        [TestMethod]
        public void SecurityTicket_Identity_Snowflake()
        {
            var ticket = new SecurityTicket() { SnowflakeAccount = "SampleSnowflakeAccount" };
            Assert.AreEqual("SampleSnowflakeAccount", ticket.Identity);
            Assert.AreEqual(GlobalConstants.IdentityType.SNOWFLAKE, ticket.IdentityType);
        }

        /// <summary>
        /// Tests that a Security Ticket with no identity defined returns a null Identity/IdentityType
        /// </summary>
        [TestMethod]
        public void SecurityTicket_Identity_None()
        {
            var ticket = new SecurityTicket();
            Assert.IsNull(ticket.Identity);
            Assert.IsNull(ticket.IdentityType);
        }
    }
}
