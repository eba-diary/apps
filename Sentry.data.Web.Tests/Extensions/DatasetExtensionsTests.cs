using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class DatasetExtensionsTests
    {
        [TestMethod]
        public void ToModel_DatasetFileConfigSchemaDto_DatasetFileConfigSchemaModel()
        {
            DatasetFileConfigSchemaDto dto = new DatasetFileConfigSchemaDto()
            {
                ConfigId = 1,
                SchemaId = 2,
                SchemaName = "SchemaName"
            };

            DatasetFileConfigSchemaModel model = dto.ToModel();

            Assert.AreEqual(1, model.ConfigId);
            Assert.AreEqual(2, model.SchemaId);
            Assert.AreEqual("SchemaName", model.SchemaName);
        }
    }
}
