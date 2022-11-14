using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Helpers;

namespace Sentry.data.Core.Tests.HelperTests
{
    [TestClass]
    public class DSCEventTopicHelperTests
    {
        [TestMethod]
        public void GetDSCTopic_Internal()
        {
            //Arrange
            

            Mock<DscEventTopicHelper> helper = new Mock<DscEventTopicHelper>();
            helper.Setup(s => s.GetDSCEventTopicConfig(It.IsAny<string>())).Returns("ConfigString");

            //Act
            helper.Object.GetDSCTopic_Internal(GlobalConstants.Environments.TEST, true);
            helper.Object.GetDSCTopic_Internal(GlobalConstants.Environments.TEST, false);
            helper.Object.GetDSCTopic_Internal(GlobalConstants.Environments.QUAL, true);
            helper.Object.GetDSCTopic_Internal(GlobalConstants.Environments.PROD, true);

            //Assert
            helper.Verify(v => v.GetDSCEventTopicConfig("DSCEventTopic_Confluent"), Times.Exactly(4));
            helper.Verify(v => v.GetDSCEventTopicConfig("DSCEventTopic_Confluent_NP"), Times.Never);
        }

        [TestMethod]
        public void GetDSCTopic_Internal_QUAL_and_PROD_NP()
        {
            //Arrange
            Mock<DscEventTopicHelper> helper = new Mock<DscEventTopicHelper>();
            helper.Setup(s => s.GetDSCEventTopicConfig(It.IsAny<string>())).Returns("ConfigString");

            //Act
            helper.Object.GetDSCTopic_Internal(GlobalConstants.Environments.QUAL, false);
            helper.Object.GetDSCTopic_Internal(GlobalConstants.Environments.PROD, false);

            //Assert
            helper.Verify(v => v.GetDSCEventTopicConfig("DSCEventTopic_Confluent"), Times.Never);
            helper.Verify(v => v.GetDSCEventTopicConfig("DSCEventTopic_Confluent_NP"), Times.Exactly(2));
        }        
    }
}
