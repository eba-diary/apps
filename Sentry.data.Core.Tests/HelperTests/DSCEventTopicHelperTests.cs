﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            helper.Object.GetDSCTopic_Internal("TEST", true);
            helper.Object.GetDSCTopic_Internal("TEST", false);
            helper.Object.GetDSCTopic_Internal("QUAL", true);
            helper.Object.GetDSCTopic_Internal("PROD", true);

            //Assert
            helper.Verify(v => v.GetDSCEventTopicConfig("DSCEventTopic_Confluent"), Times.Exactly(4));
        }

        [TestMethod]
        public void GetDSCTopic_Internal_QUAL_and_PROD_NP()
        {
            //Arrange
            Mock<DscEventTopicHelper> helper = new Mock<DscEventTopicHelper>();
            helper.Setup(s => s.GetDSCEventTopicConfig(It.IsAny<string>())).Returns("ConfigString");

            //Act
            helper.Object.GetDSCTopic_Internal("QUAL", false);
            helper.Object.GetDSCTopic_Internal("PROD", false);

            //Assert
            helper.Verify(v => v.GetDSCEventTopicConfig("DSCEventTopic_Confluent_QUALNP"), Times.Once);
            helper.Verify(v => v.GetDSCEventTopicConfig("DSCEventTopic_Confluent_PRODNP"), Times.Once);
        }        
    }
}
