using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SchemaTests
    {
        [TestMethod]
        public void AddRevision()
        {
            //Arrange
            FileSchema schema = new FileSchema();
            SchemaRevision revision = new SchemaRevision();

            //Act
            schema.AddRevision(revision);

            //Assert
            Assert.AreEqual(1, schema.Revisions.Count);          
        }
    }
}
