using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using Sentry.data.Core;
using Nest;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using static Sentry.data.Core.GlobalConstants;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class ElasticDataInventorySearchProviderTests
    {
        #region Tests
        [TestMethod]
        public void GetSearchResults_BasicSearch_DataInventorySearchResultDto()
        {
            Mock<IElasticDocumentClient> elasticDocumentClient = new Mock<IElasticDocumentClient>(MockBehavior.Strict);
            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>())).ReturnsAsync(GetDataInventoryList());

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, null);

            DataInventorySearchResultDto result = searchProvider.GetSearchResults(new FilterSearchDto() { SearchText = "Table" });

            elasticDocumentClient.VerifyAll();

            Assert.IsNotNull(result.DataInventoryEvent);
            Assert.IsTrue(result.DataInventoryEvent.QuerySuccess);
            Assert.AreEqual("Table", result.DataInventoryEvent.SearchCriteria);
            Assert.IsTrue(string.IsNullOrEmpty(result.DataInventoryEvent.QueryErrorMessage));

            Assert.AreEqual(1, result.SearchTotal);
            Assert.IsTrue(result.DataInventoryResults.Any());

            //assert dto mapping
            DataInventorySearchResultRowDto rowDto = result.DataInventoryResults.First();
            Assert.AreEqual("CODE", rowDto.Asset);
            Assert.AreEqual("server.sentry.com", rowDto.Server);
            Assert.AreEqual("DBName", rowDto.Database);
            Assert.AreEqual("TableName", rowDto.Object);
            Assert.AreEqual("Table", rowDto.ObjectType);
            Assert.AreEqual("column_nme", rowDto.Column);
            Assert.IsFalse(rowDto.IsSensitive);
            Assert.AreEqual("Prod", rowDto.ProdType);
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
        public void GetSearchResults_NoResultsSearch_EmptyDataInventorySearchResultDto()
        {
            Mock<IElasticDocumentClient> elasticDocumentClient = new Mock<IElasticDocumentClient>(MockBehavior.Strict);
            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>())).Returns(Task.FromResult(new ElasticResult<DataInventory>()));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, null);

            DataInventorySearchResultDto result = searchProvider.GetSearchResults(new FilterSearchDto());

            elasticDocumentClient.VerifyAll();

            Assert.AreEqual(0, result.SearchTotal);
            Assert.IsFalse(result.DataInventoryResults.Any());
        }

        [TestMethod]
        public void GetSearchResults_ErrorSearch_QueryFailDataInventoryEvent()
        {
            Mock<IElasticDocumentClient> elasticDocumentClient = new Mock<IElasticDocumentClient>(MockBehavior.Strict);
            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>()))
                .ThrowsAsync(new ElasticsearchClientException("FAIL"));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, null);

            DataInventorySearchResultDto result = searchProvider.GetSearchResults(new FilterSearchDto() { SearchText = "Table" });

            elasticDocumentClient.VerifyAll();

            Assert.IsNotNull(result.DataInventoryEvent);
            Assert.IsFalse(result.DataInventoryEvent.QuerySuccess);
            Assert.AreEqual("Table", result.DataInventoryEvent.SearchCriteria);
            Assert.AreEqual("Data Inventory Elasticsearch query failed. Exception: One or more errors occurred.", result.DataInventoryEvent.QueryErrorMessage);

            Assert.AreEqual(0, result.SearchTotal);
            Assert.IsFalse(result.DataInventoryResults.Any());
        }

        [TestMethod]
        public void DoesItemContainSensitive_FilterSearchDto_True()
        {
            Mock<IElasticDocumentClient> elasticDocumentClient = new Mock<IElasticDocumentClient>(MockBehavior.Strict);
            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>())).ReturnsAsync(GetDataInventoryList());

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, null);

            DataInventorySensitiveSearchDto dto = new DataInventorySensitiveSearchDto()
            {
                SearchText = "DATA",
                SearchTarget = DataInventorySearchTargets.SAID
            };

            DataInventorySensitiveSearchResultDto result = searchProvider.DoesItemContainSensitive(dto);

            elasticDocumentClient.VerifyAll();

            Assert.IsNotNull(result.DataInventoryEvent);
            Assert.IsTrue(result.DataInventoryEvent.QuerySuccess);
            Assert.AreEqual("DATA", result.DataInventoryEvent.SearchCriteria);
            Assert.IsTrue(string.IsNullOrEmpty(result.DataInventoryEvent.QueryErrorMessage));

            Assert.IsTrue(result.HasSensitive);
        }

        [TestMethod]
        public void GetSearchFilters_BasicSearch_FilterSearchDto()
        {
            Mock<IElasticDocumentClient> elasticDocumentClient = new Mock<IElasticDocumentClient>(MockBehavior.Strict);
            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>())).ReturnsAsync(GetAggregateDictionary());

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, null);

            FilterSearchDto result = searchProvider.GetSearchFilters(new FilterSearchDto() 
            {
                SearchText = "Table",
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = FilterCategoryOptions.ENVIRONMENT_PROD,
                                ParentCategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT,
                                Selected = true
                            }
                        }
                    }
                }
            });

            elasticDocumentClient.VerifyAll();

            Assert.AreEqual(2, result.FilterCategories.Count);

            //assert dto mapping
            FilterCategoryDto category = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.DataInventory.ENVIRONMENT);
            Assert.IsNotNull(category);
            Assert.AreEqual(2, category.CategoryOptions.Count);

            FilterCategoryOptionDto option = category.CategoryOptions.First();
            Assert.AreEqual(FilterCategoryOptions.ENVIRONMENT_PROD, option.OptionValue);
            Assert.AreEqual(5, option.ResultCount);
            Assert.IsTrue(option.Selected);
            Assert.AreEqual(FilterCategoryNames.DataInventory.ENVIRONMENT, option.ParentCategoryName);

            option = category.CategoryOptions.Last();
            Assert.AreEqual(FilterCategoryOptions.ENVIRONMENT_NONPROD, option.OptionValue);
            Assert.AreEqual(3, option.ResultCount);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(FilterCategoryNames.DataInventory.ENVIRONMENT, option.ParentCategoryName);

            category = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.DataInventory.SENSITIVE);
            Assert.IsNotNull(category);
            Assert.AreEqual(2, category.CategoryOptions.Count);

            option = category.CategoryOptions.First();
            Assert.AreEqual("false", option.OptionValue);
            Assert.AreEqual(2, option.ResultCount);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(FilterCategoryNames.DataInventory.SENSITIVE, option.ParentCategoryName);

            option = category.CategoryOptions.Last();
            Assert.AreEqual("true", option.OptionValue);
            Assert.AreEqual(6, option.ResultCount);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(FilterCategoryNames.DataInventory.SENSITIVE, option.ParentCategoryName);
        }

        [TestMethod]
        public void GetSearchFilters_ErrorSearch_QueryFailDataInventoryEvent()
        {
            Mock<IElasticDocumentClient> elasticDocumentClient = new Mock<IElasticDocumentClient>(MockBehavior.Strict);
            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>()))
                .ThrowsAsync(new ElasticsearchClientException("FAIL"));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, null);

            FilterSearchDto result = searchProvider.GetSearchFilters(new FilterSearchDto()
            {
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = FilterCategoryOptions.ENVIRONMENT_PROD,
                                ParentCategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT,
                                Selected = true
                            },
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = FilterCategoryOptions.ENVIRONMENT_NONPROD,
                                ParentCategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT,
                                Selected = true
                            }
                        }
                    },
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.DataInventory.SENSITIVE,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "true",
                                ParentCategoryName = FilterCategoryNames.DataInventory.SENSITIVE,
                                Selected = true
                            }
                        }
                    }
                }
            });

            elasticDocumentClient.VerifyAll();

            Assert.IsFalse(result.FilterCategories.Any());
        }

        [TestMethod]
        public void GetCategoriesByAsset_AssetHasSensitive_DataInventoryAssetCategoriesDto()
        {
            Mock<IElasticDocumentClient> elasticDocumentClient = new Mock<IElasticDocumentClient>(MockBehavior.Strict);
            elasticDocumentClient.SetupSequence(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>()))
                .ReturnsAsync(GetCategoryAggregateDictionary(GetAllCategories()))
                .ReturnsAsync(GetCategoryAggregateDictionary(new List<string>() { "PCI", "User Setting" }));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, null);

            DataInventoryAssetCategoriesDto result = searchProvider.GetCategoriesByAsset("DATA");

            elasticDocumentClient.VerifyAll();

            Assert.IsNotNull(result.DataInventoryEvent);
            Assert.IsTrue(result.DataInventoryEvent.QuerySuccess);
            Assert.AreEqual("DATA", result.DataInventoryEvent.SearchCriteria);
            Assert.IsTrue(string.IsNullOrEmpty(result.DataInventoryEvent.QueryErrorMessage));

            Assert.AreEqual(4, result.DataInventoryCategories.Count);

            //assert dto mapping
            DataInventoryCategoryDto category = result.DataInventoryCategories.FirstOrDefault(x => x.Category == "PCI");
            Assert.IsNotNull(category);
            Assert.IsTrue(category.IsSensitive);

            category = result.DataInventoryCategories.FirstOrDefault(x => x.Category == "User Setting");
            Assert.IsNotNull(category);
            Assert.IsTrue(category.IsSensitive);

            category = result.DataInventoryCategories.FirstOrDefault(x => x.Category == "Financial Personal Information");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);

            category = result.DataInventoryCategories.FirstOrDefault(x => x.Category == "Authentication Verifier");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);
        }

        [TestMethod]
        public void GetCategoriesByAsset_AssetNotFound_DataInventoryAssetCategoriesDto()
        {
            Mock<IElasticDocumentClient> elasticDocumentClient = new Mock<IElasticDocumentClient>(MockBehavior.Strict);
            elasticDocumentClient.SetupSequence(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>()))
                .ReturnsAsync(GetCategoryAggregateDictionary(GetAllCategories()))
                .ReturnsAsync(GetCategoryAggregateDictionary(new List<string>()));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, null);

            DataInventoryAssetCategoriesDto result = searchProvider.GetCategoriesByAsset("DATA");

            elasticDocumentClient.VerifyAll();

            Assert.IsNotNull(result.DataInventoryEvent);
            Assert.IsTrue(result.DataInventoryEvent.QuerySuccess);
            Assert.AreEqual("DATA", result.DataInventoryEvent.SearchCriteria);
            Assert.IsTrue(string.IsNullOrEmpty(result.DataInventoryEvent.QueryErrorMessage));

            Assert.AreEqual(4, result.DataInventoryCategories.Count);

            //assert dto mapping
            DataInventoryCategoryDto category = result.DataInventoryCategories.FirstOrDefault(x => x.Category == "PCI");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);

            category = result.DataInventoryCategories.FirstOrDefault(x => x.Category == "User Setting");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);

            category = result.DataInventoryCategories.FirstOrDefault(x => x.Category == "Financial Personal Information");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);

            category = result.DataInventoryCategories.FirstOrDefault(x => x.Category == "Authentication Verifier");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);
        }

        [TestMethod]
        public void GetCategoriesByAsset_Error_DataInventoryAssetCategoriesDto()
        {
            Mock<IElasticDocumentClient> elasticDocumentClient = new Mock<IElasticDocumentClient>(MockBehavior.Strict);
            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>()))
                .ThrowsAsync(new ElasticsearchClientException("FAIL"));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, null);

            DataInventoryAssetCategoriesDto result = searchProvider.GetCategoriesByAsset("DATA");

            elasticDocumentClient.VerifyAll();

            Assert.IsNotNull(result.DataInventoryEvent);
            Assert.IsFalse(result.DataInventoryEvent.QuerySuccess);
            Assert.AreEqual("DATA", result.DataInventoryEvent.SearchCriteria);
            Assert.AreEqual("Data Inventory Elasticsearch query failed. Exception: One or more errors occurred.", result.DataInventoryEvent.QueryErrorMessage);

            Assert.IsFalse(result.DataInventoryCategories.Any());
        }

        [TestMethod]
        public void SaveSensitive_DataInventoryUpdateDtos_True()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDbExecuter> dbExecuter = mockRepository.Create<IDbExecuter>();
            dbExecuter.Setup(x => x.ExecuteCommand(It.IsAny<object>()));

            Mock<IElasticDocumentClient> elasticDocumentClient = mockRepository.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<Func<SearchDescriptor<DataInventory>, ISearchRequest>>())).ReturnsAsync(GetDataInventoryListWithMultiple());
            elasticDocumentClient.Setup(x => x.Update(It.Is<DataInventory>(i => i.Id == 1))).Callback<DataInventory>(x =>
            {
                Assert.IsTrue(x.IsSensitive);
                Assert.IsFalse(x.IsOwnerVerified);
            }).ReturnsAsync(true);
            elasticDocumentClient.Setup(x => x.Update(It.Is<DataInventory>(i => i.Id == 2))).Callback<DataInventory>(x =>
            {
                Assert.IsFalse(x.IsSensitive);
                Assert.IsTrue(x.IsOwnerVerified);
            }).ReturnsAsync(true);

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, dbExecuter.Object);

            List<DataInventoryUpdateDto> dtos = new List<DataInventoryUpdateDto>()
            {
                new DataInventoryUpdateDto() { BaseColumnId = 1, IsSensitive = true, IsOwnerVerified = false },
                new DataInventoryUpdateDto() { BaseColumnId = 2, IsSensitive = false, IsOwnerVerified = true }
            };

            Assert.IsTrue(searchProvider.SaveSensitive(dtos));

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SaveSensitive_DataInventoryUpdateDtos_SqlFail_False()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDbExecuter> dbExecuter = mockRepository.Create<IDbExecuter>();
            dbExecuter.Setup(x => x.ExecuteCommand(It.IsAny<object>())).Throws<Exception>();

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(null, dbExecuter.Object);

            List<DataInventoryUpdateDto> dtos = new List<DataInventoryUpdateDto>()
            {
                new DataInventoryUpdateDto() { BaseColumnId = 1, IsSensitive = true, IsOwnerVerified = false },
                new DataInventoryUpdateDto() { BaseColumnId = 2, IsSensitive = false, IsOwnerVerified = true }
            };

            Assert.IsFalse(searchProvider.SaveSensitive(dtos));

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SaveSensitive_DataInventoryUpdateDtos_ElasticSearchFail_False()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDbExecuter> dbExecuter = mockRepository.Create<IDbExecuter>();
            dbExecuter.Setup(x => x.ExecuteCommand(It.IsAny<object>()));

            Mock<IElasticDocumentClient> elasticDocumentClient = mockRepository.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<Func<SearchDescriptor<DataInventory>, ISearchRequest>>()))
                .ThrowsAsync(new ElasticsearchClientException("FAIL"));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, dbExecuter.Object);

            List<DataInventoryUpdateDto> dtos = new List<DataInventoryUpdateDto>()
            {
                new DataInventoryUpdateDto() { BaseColumnId = 1, IsSensitive = true, IsOwnerVerified = false },
                new DataInventoryUpdateDto() { BaseColumnId = 2, IsSensitive = false, IsOwnerVerified = true }
            };

            Assert.IsFalse(searchProvider.SaveSensitive(dtos));

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SaveSensitive_DataInventoryUpdateDtos_ElasticUpdateFail_False()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDbExecuter> dbExecuter = mockRepository.Create<IDbExecuter>();
            dbExecuter.Setup(x => x.ExecuteCommand(It.IsAny<object>()));

            Mock<IElasticDocumentClient> elasticDocumentClient = mockRepository.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<Func<SearchDescriptor<DataInventory>, ISearchRequest>>())).ReturnsAsync(GetDataInventoryListWithMultiple());
            elasticDocumentClient.Setup(x => x.Update(It.Is<DataInventory>(i => i.Id == 1))).ThrowsAsync(new ElasticsearchClientException("FAIL"));
            elasticDocumentClient.Setup(x => x.Update(It.Is<DataInventory>(i => i.Id == 2))).Callback<DataInventory>(x =>
            {
                Assert.IsFalse(x.IsSensitive);
                Assert.IsTrue(x.IsOwnerVerified);
            }).ReturnsAsync(true);

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticDocumentClient.Object, dbExecuter.Object);

            List<DataInventoryUpdateDto> dtos = new List<DataInventoryUpdateDto>()
            {
                new DataInventoryUpdateDto() { BaseColumnId = 1, IsSensitive = true, IsOwnerVerified = false },
                new DataInventoryUpdateDto() { BaseColumnId = 2, IsSensitive = false, IsOwnerVerified = true }
            };

            Assert.IsFalse(searchProvider.SaveSensitive(dtos));

            mockRepository.VerifyAll();
        }
        #endregion

        #region Methods
        private ElasticResult<DataInventory> GetDataInventoryList()
        {
            return new ElasticResult<DataInventory>()
            {
                SearchTotal = 1,
                Documents = new List<DataInventory>()
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
                        ProdType = FilterCategoryOptions.ENVIRONMENT_PROD,
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
                }
            };
        }
        private ElasticResult<DataInventory> GetDataInventoryListWithMultiple()
        {
            return new ElasticResult<DataInventory>()
            {
                SearchTotal = 2,
                Documents = new List<DataInventory>()
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
                        ProdType = FilterCategoryOptions.ENVIRONMENT_PROD,
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
                    },
                    new DataInventory()
                    {
                        Id = 2,
                        AssetCode = "CODE2",
                        BaseName = "TableName2",
                        ServerName = "server.sentry.com",
                        DatabaseName = "DBName2",
                        TypeDescription = "Table2",
                        ColumnName = "column_nme2",
                        IsSensitive = true,
                        ProdType = FilterCategoryOptions.ENVIRONMENT_PROD,
                        ColumnType = "Type",
                        MaxLength = 10,
                        Precision = 0,
                        Scale = 2,
                        IsNullable = null,
                        EffectiveDateTime = DateTime.Parse("2022-01-10T05:00:00.000"),
                        IsOwnerVerified = false,
                        SourceName = "Source2",
                        ScanListName = "Scan2",
                        SaidListName = "SaidList2"
                    }
                }
            };
        }

        private ElasticResult<DataInventory> GetAggregateDictionary()
        {
            return new ElasticResult<DataInventory>()
            {
                Aggregations = new AggregateDictionary(new Dictionary<string, IAggregate>
                {
                    [FilterCategoryNames.DataInventory.ENVIRONMENT] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 5,
                                Key = FilterCategoryOptions.ENVIRONMENT_PROD
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 3,
                                Key = FilterCategoryOptions.ENVIRONMENT_NONPROD
                            }
                        }.AsReadOnly()
                    },
                    [FilterCategoryNames.DataInventory.SENSITIVE] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 2,
                                Key = "0",
                                KeyAsString = "false"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 6,
                                Key = "1",
                                KeyAsString = "true"
                            }
                        }.AsReadOnly()
                    },
                    ["NoMatchAgg"] = new BucketAggregate()
                    {
                        Items = new List<KeyedBucket<string>>().AsReadOnly()
                    },
                    [FilterCategoryNames.DataInventory.DATABASE] = new BucketAggregate()
                    {
                        SumOtherDocCount = 10,
                        Items = new List<KeyedBucket<string>>().AsReadOnly()
                    }
                })
            };
        }

        private ElasticResult<DataInventory> GetCategoryAggregateDictionary(List<string> categories)
        {
            List<KeyedBucket<object>> buckets = new List<KeyedBucket<object>>();

            foreach (string category in categories)
            {
                buckets.Add(new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                {
                    DocCount = 5,
                    Key = category
                });
            }

            return new ElasticResult<DataInventory>()
            {
                Aggregations = new AggregateDictionary(new Dictionary<string, IAggregate>
                {
                    ["SaidListNames"] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = buckets.AsReadOnly()
                    }
                })
            };
        }

        private List<string> GetAllCategories()
        {
            return new List<string>()
            {
                "Financial Personal Information",
                "PCI",
                "User Setting",
                "Authentication Verifier",
                "Financial Personal Information, PCI",
                "Financial Personal Information, PCI, User Setting",
                "Financial Personal Information, User Setting",
				"Authentication Verifier, Financial Personal Information",
				"PCI, User Setting"
            };
        }
        #endregion
    }
}
