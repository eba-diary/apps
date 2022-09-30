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
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.Add(It.IsAny<SupportLink>())).Callback<SupportLink>(x =>
            {
                Assert.AreEqual(12, x.SupportLinkId);
                Assert.AreEqual("New Link", x.Name);
                Assert.AreEqual("This is the new link", x.Description);
                Assert.AreEqual("New Link Url", x.Url);
            });

            datasetContext.Setup(x => x.SaveChanges(true));

            SupportLinkService supportLinkService = new SupportLinkService(datasetContext.Object);

            SupportLinkDto supportLinkDto = new SupportLinkDto()
            {
                SupportLinkId = 12,
                Name = "New Link",
                Description = "This is the new link",
                Url = "New Link Url"
            };

            // Act
            supportLinkService.AddSupportLink(supportLinkDto);

            // Assert
            datasetContext.VerifyAll();
        }


        [TestMethod]
        public void TestRemoveSupportLink()
        {
            // Arrange
            SupportLink supportLink1 = new SupportLink()
            {
                SupportLinkId = 13,
                Name = "Remove New Link",
                Description = "This is the remove link",
                Url = "New Link Url to remove"
            };

            List<SupportLink> supportLinksList = new List<SupportLink>();
            supportLinksList.Add(supportLink1);
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.Setup(x => x.SupportLinks).Returns(supportLinksList.AsQueryable());
            datasetContext.Setup(x => x.Remove(It.IsAny<SupportLink>())).Callback<SupportLink>(x =>
            {
                Assert.AreEqual(13, x.SupportLinkId);
                Assert.AreEqual("Remove New Link", x.Name);
                Assert.AreEqual("This is the remove link", x.Description);
                Assert.AreEqual("New Link Url to remove", x.Url);
            }); 
            datasetContext.Setup(x => x.SaveChanges(true));

            SupportLinkService supportLinkService = new SupportLinkService(datasetContext.Object);

            // Act
            supportLinkService.RemoveSupportLink(13);

            // Assert
            datasetContext.VerifyAll();
        }

        
    }
}
