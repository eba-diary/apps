using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetFileConfigExtensionsTests
    {
        [TestMethod]
        public void ToDatasetFileConfigSchemaDto_DatasetFileConfig_DatasetFileConfigSchemaDto()
        {
            DatasetFileConfig datasetFileConfig = new DatasetFileConfig()
            {
                ConfigId = 1,
                Name = "SchemaName",
                Schema = new FileSchema()
                {
                    SchemaId = 2
                }
            };

            DatasetFileConfigSchemaDto dto = datasetFileConfig.ToDatasetFileConfigSchemaDto();

            Assert.AreEqual(1, dto.ConfigId);
            Assert.AreEqual(2, dto.SchemaId);
            Assert.AreEqual("SchemaName", dto.SchemaName);
        }
    }
}
