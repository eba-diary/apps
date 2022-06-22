using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System.IO;
using System.Web;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class DatasetFileExtensionsTests
    {
        [TestMethod]
        public void ToDto_UploadDatasetFileModel_UploadDatasetFileDto()
        {
            Mock<HttpPostedFileBase> file = new Mock<HttpPostedFileBase>(MockBehavior.Strict);
            file.SetupGet(x => x.FileName).Returns("Filename.json");

            Mock<Stream> stream = new Mock<Stream>();
            file.SetupGet(x => x.InputStream).Returns(stream.Object);

            UploadDatasetFileModel model = new UploadDatasetFileModel()
            {
                DatasetId = 1,
                ConfigId = 2,
                SchemaName = "SchemaName",
                DatasetFile = file.Object
            };

            UploadDatasetFileDto dto = model.ToDto();

            Assert.AreEqual(1, dto.DatasetId);
            Assert.AreEqual(2, dto.ConfigId);
            Assert.AreEqual("Filename.json", dto.FileName);
            Assert.AreEqual(stream.Object, dto.FileInputStream);
        }
    }
}
