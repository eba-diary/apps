using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using Sentry.data.Core;
using Nest;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class ElasticDataInventorySearchProviderTests
    {
        [TestMethod]
        public void GetSearchResults_BasicSearch_DaleResultDto()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.Search(It.IsAny<SearchRequest<DataInventory>>())).Returns(GetDataInventoryList());

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object);

            DaleSearchDto searchDto = new DaleSearchDto()
            {
                Criteria = "Table",
                Destiny = Core.GlobalEnums.DaleDestiny.Object,
                Sensitive = Core.GlobalEnums.DaleSensitive.SensitiveAll
            };

            DaleResultDto result = searchProvider.GetSearchResults(searchDto);

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsTrue(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("Table", result.DaleEvent.Criteria);
            Assert.AreEqual("Object", result.DaleEvent.Destiny);
            Assert.AreEqual("SensitiveAll", result.DaleEvent.Sensitive);
            Assert.IsTrue(string.IsNullOrEmpty(result.DaleEvent.QueryErrorMessage));

            Assert.IsTrue(result.DaleResults.Any());

            //assert dto mapping
            DaleResultRowDto rowDto = result.DaleResults.First();
            Assert.AreEqual("CODE", rowDto.Asset);
            Assert.AreEqual("server.sentry.com", rowDto.Server);
            Assert.AreEqual("DBName", rowDto.Database);
            Assert.AreEqual("TableName", rowDto.Object);
            Assert.AreEqual("Table", rowDto.ObjectType);
            Assert.AreEqual("column_nme", rowDto.Column);
            Assert.IsFalse(rowDto.IsSensitive);
            Assert.AreEqual("P", rowDto.ProdType);
            Assert.AreEqual("Type", rowDto.ColumnType);
            Assert.AreEqual(100, rowDto.MaxLength);
            Assert.AreEqual(7, rowDto.Precision);
            Assert.AreEqual(2, rowDto.Scale);
            Assert.IsFalse(rowDto.IsNullable);
            Assert.AreEqual("2022-01-05 05:00:00.000", rowDto.EffectiveDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            Assert.AreEqual(1, rowDto.BaseColumnId);
            Assert.IsTrue(rowDto.IsOwnerVerified);
            Assert.AreEqual("Source", rowDto.SourceType);
            Assert.AreEqual("Scan", rowDto.ScanCategory);
            Assert.AreEqual("SaidList", rowDto.ScanType);
        }

        [TestMethod]
        public void GetSearchResults_AdvancedSearch_DaleResultDto()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.Search(It.IsAny<SearchRequest<DataInventory>>())).Returns(GetDataInventoryList());

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object);

            DaleSearchDto searchDto = new DaleSearchDto()
            {
                Destiny = Core.GlobalEnums.DaleDestiny.Advanced,
                Sensitive = Core.GlobalEnums.DaleSensitive.SensitiveNone,
                AdvancedCriteria = new DaleAdvancedCriteriaDto()
                {
                    Asset = "CODE",
                    Object = "Table",
                    Column = "column_nme"
                }
            };

            DaleResultDto result = searchProvider.GetSearchResults(searchDto);

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsTrue(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("Asset:CODE AND Object:Table AND Column:column_nme", result.DaleEvent.Criteria);
            Assert.AreEqual("Advanced", result.DaleEvent.Destiny);
            Assert.AreEqual("SensitiveNone", result.DaleEvent.Sensitive);
            Assert.IsTrue(string.IsNullOrEmpty(result.DaleEvent.QueryErrorMessage));

            Assert.IsTrue(result.DaleResults.Any());

            //assert dto mapping
            DaleResultRowDto rowDto = result.DaleResults.First();
            Assert.AreEqual("CODE", rowDto.Asset);
            Assert.AreEqual("server.sentry.com", rowDto.Server);
            Assert.AreEqual("DBName", rowDto.Database);
            Assert.AreEqual("TableName", rowDto.Object);
            Assert.AreEqual("Table", rowDto.ObjectType);
            Assert.AreEqual("column_nme", rowDto.Column);
            Assert.IsFalse(rowDto.IsSensitive);
            Assert.AreEqual("P", rowDto.ProdType);
            Assert.AreEqual("Type", rowDto.ColumnType);
            Assert.AreEqual(100, rowDto.MaxLength);
            Assert.AreEqual(7, rowDto.Precision);
            Assert.AreEqual(2, rowDto.Scale);
            Assert.IsFalse(rowDto.IsNullable);
            Assert.AreEqual("2022-01-05 05:00:00.000", rowDto.EffectiveDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            Assert.AreEqual(1, rowDto.BaseColumnId);
            Assert.IsTrue(rowDto.IsOwnerVerified);
            Assert.AreEqual("Source", rowDto.SourceType);
            Assert.AreEqual("Scan", rowDto.ScanCategory);
            Assert.AreEqual("SaidList", rowDto.ScanType);
        }

        [TestMethod]
        public void GetSearchResults_NoResultsSearch_EmptyDaleResultDto()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.Search(It.IsAny<SearchRequest<DataInventory>>())).Returns(new List<DataInventory>());

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object);

            DaleSearchDto searchDto = new DaleSearchDto()
            {
                Destiny = Core.GlobalEnums.DaleDestiny.Object,
                Sensitive = Core.GlobalEnums.DaleSensitive.SensitiveNone
            };

            DaleResultDto result = searchProvider.GetSearchResults(searchDto);

            elasticContext.VerifyAll();

            Assert.IsFalse(result.DaleResults.Any());
        }

        [TestMethod]
        public void GetSearchResults_ErrorSearch_QueryFailDaleEvent()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.Search(It.IsAny<SearchRequest<DataInventory>>())).Throws(new ElasticsearchClientException("FAIL"));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object);

            DaleSearchDto searchDto = new DaleSearchDto()
            {
                Destiny = Core.GlobalEnums.DaleDestiny.Object,
                Sensitive = Core.GlobalEnums.DaleSensitive.SensitiveNone,
                Criteria = "Table"
            };

            DaleResultDto result = searchProvider.GetSearchResults(searchDto);

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsFalse(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("Table", result.DaleEvent.Criteria);
            Assert.AreEqual("Object", result.DaleEvent.Destiny);
            Assert.AreEqual("SensitiveNone", result.DaleEvent.Sensitive);
            Assert.AreEqual("Data Inventory Elasticsearch query failed. Exception: FAIL", result.DaleEvent.QueryErrorMessage);

            Assert.IsFalse(result.DaleResults.Any());
        }

        [TestMethod]
        public void DoesItemContainSensitive_DaleSearchDto_True()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.Search(It.IsAny<SearchRequest<DataInventory>>())).Returns(GetDataInventoryList());

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object);

            DaleSearchDto searchDto = new DaleSearchDto()
            {
                Destiny = Core.GlobalEnums.DaleDestiny.SAID,
                Sensitive = Core.GlobalEnums.DaleSensitive.SensitiveAll,
                Criteria = "DATA"
            };

            DaleContainSensitiveResultDto result = searchProvider.DoesItemContainSensitive(searchDto);

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsTrue(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("DATA", result.DaleEvent.Criteria);
            Assert.AreEqual("SAID", result.DaleEvent.Destiny);
            Assert.AreEqual("SensitiveAll", result.DaleEvent.Sensitive);
            Assert.IsTrue(string.IsNullOrEmpty(result.DaleEvent.QueryErrorMessage));

            Assert.IsTrue(result.DoesContainSensitiveResults);
        }

        #region Methods
        private IList<DataInventory> GetDataInventoryList()
        {
            return new List<DataInventory>()
            {
                new DataInventory()
                {
                    Id = 1,
                    AssetCode = "CODE",
                    BaseName = "TableName",
                    ServerName = "server.sentry.com",
                    DatabaseName = "DBName",
                    TypeDescription = "Table",
                    ColumnName = "column_nme",
                    IsSensitive = false,
                    ProdType = "P",
                    ColumnType = "Type",
                    MaxLength = 100,
                    Precision = 7,
                    Scale = 2,
                    IsNullable = null,
                    EffectiveDateTime = DateTime.Parse("2022-01-05T05:00:00.000"),
                    IsOwnerVerified = true,
                    SourceName = "Source",
                    ScanListName = "Scan",
                    SaidListName = "SaidList"
                }
            };
        }
        #endregion
    }
}
