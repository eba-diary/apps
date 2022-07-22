using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.DTO.Admin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class SupportLinkServiceTests
    {
        [TestMethod]
        public void TestAddSupportLink()
        {
            // Arrange
            var supportLinksList = new List<SupportLink>();
            var context = new Mock<IDatasetContext>();
            SupportLinkDto supportLinkDto = new SupportLinkDto()
            {
                SupportLinkId = 12,
                Name = "New Link",
                Description = "This is the new link",
                Url = "New Link Url"
            };
            context.Setup(f => f.SupportLinks).Returns(supportLinksList.AsQueryable());
            
            // Act
            var supportLinkService = new SupportLinkService(context.Object);
            supportLinkService.AddSupportLink(supportLinkDto);

            // Assert
            Assert.AreEqual(1, supportLinksList.Count);
            Assert.AreEqual(12, supportLinksList[0].SupportLinkId);
            Assert.AreEqual("New Link", supportLinksList[0].Name);
            Assert.AreEqual("This is the new link", supportLinksList[0].Description);
            Assert.AreEqual("New Link Url", supportLinksList[0].Url);
        }


        [TestMethod]
        public void TestRemoveSupportLink()
        {

            // Arrange
            var context = new Mock<IDatasetContext>();
            var supportLinksList = new List<SupportLink>();
            SupportLinkDto supportLinkDto1 = new SupportLinkDto()
            {
                SupportLinkId = 12,
                Name = "New Link",
                Description = "This is the new link",
                Url = "New Link Url"
            };
            SupportLinkDto supportLinkDto2 = new SupportLinkDto()
            {
                SupportLinkId = 13,
                Name = "New Link Two",
                Description = "This is the 2nd new link",
                Url = "New Link Url Two"
            };

            context.Setup(F => F.SupportLinks).Returns(supportLinksList.AsQueryable());

            // Act
            var supportLinkService = new SupportLinkService(context.Object);
            supportLinkService.AddSupportLink(supportLinkDto1);
            supportLinkService.AddSupportLink(supportLinkDto2);

            supportLinkService.RemoveSupportLink(12);

            // Assert
            Assert.AreEqual(1, supportLinksList.Count);
            Assert.AreEqual(13, supportLinksList[0].SupportLinkId);
            Assert.AreEqual("New Link Two", supportLinksList[0].Name);
            Assert.AreEqual("This is the 2nd new link", supportLinksList[0].Description);
            Assert.AreEqual("New Link Url Two", supportLinksList[0].Url);
            context.VerifyAll();

        }
    }
}
