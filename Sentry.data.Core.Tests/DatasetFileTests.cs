using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers.Paginate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sentry.data.Core.Interfaces;
using System.Linq.Expressions;
using System.Text;


namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetFileTests
    {
        [TestMethod]
        public void GetDatasetFileDropIdListByDatasetFile_MatchExists()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();

            DatasetFile datasetFile = new DatasetFile()
            {
                Dataset = new Dataset()
                {
                    DatasetId = 99
                },
                Schema = new FileSchema()
                {
                    SchemaId = 222
                },
                OriginalFileName = "abc.txt"
            };

            DatasetFileQuery datasetFileQuery_1 = new DatasetFileQuery()
            {
                DatasetFileDrop = 33,
                DatasetID = 99,
                SchemaId = 222,
                FileNME = "abc.txt"
            };

            DatasetFileQuery datasetFileQuery_2 = new DatasetFileQuery()
            {
                DatasetFileDrop = 11,
                DatasetID = 66,
                SchemaId = 222,
                FileNME = "abc.txt"
            };

            context.Setup(s => s.DatasetFileQuery).Returns(new List<DatasetFileQuery>() { datasetFileQuery_1, datasetFileQuery_2 }.AsQueryable());

            //Act
            int fileIds = datasetFile.GetDatasetFileDropIdListByDatasetFile(context.Object);

            //Assert
            Assert.AreEqual(33, fileIds);
        }

        [TestMethod]
        public void GetDatasetFileDropIdListByDatasetFile_NoMatch()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();

            DatasetFile datasetFile = new DatasetFile()
            {
                Dataset = new Dataset()
                {
                    DatasetId = 99
                },
                Schema = new FileSchema()
                {
                    SchemaId = 222
                },
                OriginalFileName = "abc.txt"
            };

            DatasetFileQuery datasetFileQuery_1 = new DatasetFileQuery()
            {
                DatasetFileDrop = 11,
                DatasetID = 66,
                SchemaId = 222,
                FileNME = "abc.txt"
            };

            context.Setup(s => s.DatasetFileQuery).Returns(new List<DatasetFileQuery>() { datasetFileQuery_1 }.AsQueryable());

            //Act
            int fileIds = datasetFile.GetDatasetFileDropIdListByDatasetFile(context.Object);

            //Assert
            Assert.IsTrue(fileIds == 0);
        }
    }
}
