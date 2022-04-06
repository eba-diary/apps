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
        public void GetSearchResults_BasicSearch_DaleResultDto()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>())).ReturnsAsync(GetDataInventoryList());

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, null);

            DaleResultDto result = searchProvider.GetSearchResults(new DaleSearchDto() { Criteria = "Table" });

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsTrue(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("Table", result.DaleEvent.Criteria);
            Assert.IsTrue(string.IsNullOrEmpty(result.DaleEvent.QueryErrorMessage));

            Assert.AreEqual(1, result.SearchTotal);
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
        public void GetSearchResults_NoResultsSearch_EmptyDaleResultDto()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>())).Returns(Task.FromResult(new ElasticResult<DataInventory>()));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, null);

            DaleResultDto result = searchProvider.GetSearchResults(new DaleSearchDto());

            elasticContext.VerifyAll();

            Assert.AreEqual(0, result.SearchTotal);
            Assert.IsFalse(result.DaleResults.Any());
        }

        [TestMethod]
        public void GetSearchResults_ErrorSearch_QueryFailDaleEvent()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>()))
                .ThrowsAsync(new ElasticsearchClientException("FAIL"));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, null);

            DaleResultDto result = searchProvider.GetSearchResults(new DaleSearchDto() { Criteria = "Table" });

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsFalse(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("Table", result.DaleEvent.Criteria);
            Assert.AreEqual("Data Inventory Elasticsearch query failed. Exception: One or more errors occurred.", result.DaleEvent.QueryErrorMessage);

            Assert.AreEqual(0, result.SearchTotal);
            Assert.IsFalse(result.DaleResults.Any());
        }

        [TestMethod]
        public void DoesItemContainSensitive_DaleSearchDto_True()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>())).ReturnsAsync(GetDataInventoryList());

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, null);

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

        [TestMethod]
        public void GetSearchFilters_BasicSearch_FilterSearchDto()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>())).ReturnsAsync(GetAggregateDictionary());

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, null);

            FilterSearchDto result = searchProvider.GetSearchFilters(new DaleSearchDto() 
            {
                Criteria = "Table",
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = FilterCategoryOptions.ENVIRONMENT_PROD,
                                ParentCategoryName = FilterCategoryNames.ENVIRONMENT,
                                Selected = true
                            }
                        }
                    }
                }
            });

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsTrue(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("Table AND Environment:P", result.DaleEvent.Criteria);
            Assert.IsTrue(string.IsNullOrEmpty(result.DaleEvent.QueryErrorMessage));

            Assert.AreEqual(2, result.FilterCategories.Count);

            //assert dto mapping
            FilterCategoryDto category = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.ENVIRONMENT);
            Assert.IsNotNull(category);
            Assert.AreEqual(2, category.CategoryOptions.Count);

            FilterCategoryOptionDto option = category.CategoryOptions.First();
            Assert.AreEqual(FilterCategoryOptions.ENVIRONMENT_PROD, option.OptionValue);
            Assert.AreEqual(5, option.ResultCount);
            Assert.IsTrue(option.Selected);
            Assert.AreEqual(FilterCategoryNames.ENVIRONMENT, option.ParentCategoryName);

            option = category.CategoryOptions.Last();
            Assert.AreEqual(FilterCategoryOptions.ENVIRONMENT_NONPROD, option.OptionValue);
            Assert.AreEqual(3, option.ResultCount);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(FilterCategoryNames.ENVIRONMENT, option.ParentCategoryName);

            category = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.SENSITIVE);
            Assert.IsNotNull(category);
            Assert.AreEqual(2, category.CategoryOptions.Count);

            option = category.CategoryOptions.First();
            Assert.AreEqual("false", option.OptionValue);
            Assert.AreEqual(2, option.ResultCount);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(FilterCategoryNames.SENSITIVE, option.ParentCategoryName);

            option = category.CategoryOptions.Last();
            Assert.AreEqual("true", option.OptionValue);
            Assert.AreEqual(6, option.ResultCount);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(FilterCategoryNames.SENSITIVE, option.ParentCategoryName);
        }

        [TestMethod]
        public void GetSearchFilters_ErrorSearch_QueryFailDaleEvent()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>()))
                .ThrowsAsync(new ElasticsearchClientException("FAIL"));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, null);

            FilterSearchDto result = searchProvider.GetSearchFilters(new DaleSearchDto()
            {
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = FilterCategoryOptions.ENVIRONMENT_PROD,
                                ParentCategoryName = FilterCategoryNames.ENVIRONMENT,
                                Selected = true
                            },
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = FilterCategoryOptions.ENVIRONMENT_NONPROD,
                                ParentCategoryName = FilterCategoryNames.ENVIRONMENT,
                                Selected = true
                            }
                        }
                    },
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.SENSITIVE,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "true",
                                ParentCategoryName = FilterCategoryNames.SENSITIVE,
                                Selected = true
                            }
                        }
                    }
                }
            });

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsFalse(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("Environment:P OR D AND Sensitive:true", result.DaleEvent.Criteria);
            Assert.AreEqual("Data Inventory Elasticsearch query failed. Exception: One or more errors occurred.", result.DaleEvent.QueryErrorMessage);

            Assert.IsFalse(result.FilterCategories.Any());
        }

        [TestMethod]
        public void GetCategoriesByAsset_AssetHasSensitive_DaleCategoryResultDto()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.SetupSequence(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>()))
                .ReturnsAsync(GetCategoryAggregateDictionary(GetAllCategories()))
                .ReturnsAsync(GetCategoryAggregateDictionary(new List<string>() { "PCI", "User Setting" }));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, null);

            DaleCategoryResultDto result = searchProvider.GetCategoriesByAsset("DATA");

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsTrue(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("DATA", result.DaleEvent.Criteria);
            Assert.IsTrue(string.IsNullOrEmpty(result.DaleEvent.QueryErrorMessage));

            Assert.AreEqual(4, result.DaleCategories.Count);

            //assert dto mapping
            DaleCategoryDto category = result.DaleCategories.FirstOrDefault(x => x.Category == "PCI");
            Assert.IsNotNull(category);
            Assert.IsTrue(category.IsSensitive);

            category = result.DaleCategories.FirstOrDefault(x => x.Category == "User Setting");
            Assert.IsNotNull(category);
            Assert.IsTrue(category.IsSensitive);

            category = result.DaleCategories.FirstOrDefault(x => x.Category == "Financial Personal Information");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);

            category = result.DaleCategories.FirstOrDefault(x => x.Category == "Authentication Verifier");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);
        }

        [TestMethod]
        public void GetCategoriesByAsset_AssetNotFound_DaleCategoryResultDto()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.SetupSequence(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>()))
                .ReturnsAsync(GetCategoryAggregateDictionary(GetAllCategories()))
                .ReturnsAsync(GetCategoryAggregateDictionary(new List<string>()));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, null);

            DaleCategoryResultDto result = searchProvider.GetCategoriesByAsset("DATA");

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsTrue(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("DATA", result.DaleEvent.Criteria);
            Assert.IsTrue(string.IsNullOrEmpty(result.DaleEvent.QueryErrorMessage));

            Assert.AreEqual(4, result.DaleCategories.Count);

            //assert dto mapping
            DaleCategoryDto category = result.DaleCategories.FirstOrDefault(x => x.Category == "PCI");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);

            category = result.DaleCategories.FirstOrDefault(x => x.Category == "User Setting");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);

            category = result.DaleCategories.FirstOrDefault(x => x.Category == "Financial Personal Information");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);

            category = result.DaleCategories.FirstOrDefault(x => x.Category == "Authentication Verifier");
            Assert.IsNotNull(category);
            Assert.IsFalse(category.IsSensitive);
        }

        [TestMethod]
        public void GetCategoriesByAsset_Error_DaleCategoryResultDto()
        {
            Mock<IElasticContext> elasticContext = new Mock<IElasticContext>(MockBehavior.Strict);
            elasticContext.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<DataInventory>>()))
                .ThrowsAsync(new ElasticsearchClientException("FAIL"));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, null);

            DaleCategoryResultDto result = searchProvider.GetCategoriesByAsset("DATA");

            elasticContext.VerifyAll();

            Assert.IsNotNull(result.DaleEvent);
            Assert.IsFalse(result.DaleEvent.QuerySuccess);
            Assert.AreEqual("DATA", result.DaleEvent.Criteria);
            Assert.AreEqual("Data Inventory Elasticsearch query failed. Exception: One or more errors occurred.", result.DaleEvent.QueryErrorMessage);

            Assert.IsFalse(result.DaleCategories.Any());
        }

        [TestMethod]
        public void SaveSensitive_DaleSensitiveDtos_True()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDbExecuter> dbExecuter = mockRepository.Create<IDbExecuter>();
            dbExecuter.Setup(x => x.ExecuteCommand(It.IsAny<object>()));

            Mock<IElasticContext> elasticContext = mockRepository.Create<IElasticContext>();
            elasticContext.Setup(x => x.SearchAsync(It.IsAny<Func<SearchDescriptor<DataInventory>, ISearchRequest>>())).ReturnsAsync(GetDataInventoryListWithMultiple());
            elasticContext.Setup(x => x.Update(It.Is<DataInventory>(i => i.Id == 1))).Callback<DataInventory>(x =>
            {
                Assert.IsTrue(x.IsSensitive);
                Assert.IsFalse(x.IsOwnerVerified);
            }).ReturnsAsync(true);
            elasticContext.Setup(x => x.Update(It.Is<DataInventory>(i => i.Id == 2))).Callback<DataInventory>(x =>
            {
                Assert.IsFalse(x.IsSensitive);
                Assert.IsTrue(x.IsOwnerVerified);
            }).ReturnsAsync(true);

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, dbExecuter.Object);

            List<DaleSensitiveDto> dtos = new List<DaleSensitiveDto>()
            {
                new DaleSensitiveDto() { BaseColumnId = 1, IsSensitive = true, IsOwnerVerified = false },
                new DaleSensitiveDto() { BaseColumnId = 2, IsSensitive = false, IsOwnerVerified = true }
            };

            Assert.IsTrue(searchProvider.SaveSensitive(dtos));

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SaveSensitive_DaleSensitiveDtos_SqlFail_False()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDbExecuter> dbExecuter = mockRepository.Create<IDbExecuter>();
            dbExecuter.Setup(x => x.ExecuteCommand(It.IsAny<object>())).Throws<Exception>();

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(null, dbExecuter.Object);

            List<DaleSensitiveDto> dtos = new List<DaleSensitiveDto>()
            {
                new DaleSensitiveDto() { BaseColumnId = 1, IsSensitive = true, IsOwnerVerified = false },
                new DaleSensitiveDto() { BaseColumnId = 2, IsSensitive = false, IsOwnerVerified = true }
            };

            Assert.IsFalse(searchProvider.SaveSensitive(dtos));

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SaveSensitive_DaleSensitiveDtos_ElasticSearchFail_False()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDbExecuter> dbExecuter = mockRepository.Create<IDbExecuter>();
            dbExecuter.Setup(x => x.ExecuteCommand(It.IsAny<object>()));

            Mock<IElasticContext> elasticContext = mockRepository.Create<IElasticContext>();
            elasticContext.Setup(x => x.SearchAsync(It.IsAny<Func<SearchDescriptor<DataInventory>, ISearchRequest>>()))
                .ThrowsAsync(new ElasticsearchClientException("FAIL"));

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, dbExecuter.Object);

            List<DaleSensitiveDto> dtos = new List<DaleSensitiveDto>()
            {
                new DaleSensitiveDto() { BaseColumnId = 1, IsSensitive = true, IsOwnerVerified = false },
                new DaleSensitiveDto() { BaseColumnId = 2, IsSensitive = false, IsOwnerVerified = true }
            };

            Assert.IsFalse(searchProvider.SaveSensitive(dtos));

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SaveSensitive_DaleSensitiveDtos_ElasticUpdateFail_False()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDbExecuter> dbExecuter = mockRepository.Create<IDbExecuter>();
            dbExecuter.Setup(x => x.ExecuteCommand(It.IsAny<object>()));

            Mock<IElasticContext> elasticContext = mockRepository.Create<IElasticContext>();
            elasticContext.Setup(x => x.SearchAsync(It.IsAny<Func<SearchDescriptor<DataInventory>, ISearchRequest>>())).ReturnsAsync(GetDataInventoryListWithMultiple());
            elasticContext.Setup(x => x.Update(It.Is<DataInventory>(i => i.Id == 1))).ThrowsAsync(new ElasticsearchClientException("FAIL"));
            elasticContext.Setup(x => x.Update(It.Is<DataInventory>(i => i.Id == 2))).Callback<DataInventory>(x =>
            {
                Assert.IsFalse(x.IsSensitive);
                Assert.IsTrue(x.IsOwnerVerified);
            }).ReturnsAsync(true);

            ElasticDataInventorySearchProvider searchProvider = new ElasticDataInventorySearchProvider(elasticContext.Object, dbExecuter.Object);

            List<DaleSensitiveDto> dtos = new List<DaleSensitiveDto>()
            {
                new DaleSensitiveDto() { BaseColumnId = 1, IsSensitive = true, IsOwnerVerified = false },
                new DaleSensitiveDto() { BaseColumnId = 2, IsSensitive = false, IsOwnerVerified = true }
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
                    [FilterCategoryNames.ENVIRONMENT] = new BucketAggregate()
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
                    [FilterCategoryNames.SENSITIVE] = new BucketAggregate()
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
                    [FilterCategoryNames.DATABASE] = new BucketAggregate()
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
