using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Schema.Elastic;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class GlobalDatasetServiceTests
    {
        [TestMethod]
        public async Task SearchGlobalDatasetsAsync_SearchGlobalDatasetsDto_SearchGlobalDatasetsResultDto()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IGlobalDatasetProvider> globalDatasetProvider = mr.Create<IGlobalDatasetProvider>();

            SearchGlobalDatasetsDto searchGlobalDatasetsDto = new SearchGlobalDatasetsDto();

            List<GlobalDataset> globalDatasets = GetGlobalDatasets();

            globalDatasetProvider.Setup(x => x.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto)).ReturnsAsync(globalDatasets);

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().AssociateId).Returns("082116");

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(true);

            GlobalDatasetService globalDatasetService = new GlobalDatasetService(globalDatasetProvider.Object, null, userService.Object, dataFeatures.Object);

            SearchGlobalDatasetsResultsDto results = await globalDatasetService.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto);

            Assert.AreEqual(2, results.GlobalDatasets.Count);

            SearchGlobalDatasetDto result = results.GlobalDatasets[0];
            Assert.AreEqual(1, result.GlobalDatasetId);
            Assert.AreEqual("Name", result.DatasetName);
            Assert.AreEqual("SAID", result.DatasetSaidAssetCode);
            Assert.AreEqual("Description", result.DatasetDescription);
            Assert.AreEqual("Category", result.CategoryCode);
            Assert.AreEqual(2, result.NamedEnvironments.Count);
            Assert.AreEqual("PROD", result.NamedEnvironments[0]);
            Assert.AreEqual("DEV", result.NamedEnvironments[1]);
            Assert.IsTrue(result.IsSecured);
            Assert.IsTrue(result.IsFavorite);
            Assert.AreEqual(12, result.TargetDatasetId);
            Assert.AreEqual(2, result.SearchHighlights.Count);

            SearchHighlightDto highlight = result.SearchHighlights.First();
            Assert.AreEqual("DatasetName", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Name", highlight.Highlights.First());

            highlight = result.SearchHighlights.Last();
            Assert.AreEqual("DatasetDescription", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Description", highlight.Highlights.First());

            result = results.GlobalDatasets[1];
            Assert.AreEqual(2, result.GlobalDatasetId);
            Assert.AreEqual("Name 2", result.DatasetName);
            Assert.AreEqual("DATA", result.DatasetSaidAssetCode);
            Assert.AreEqual("Description 2", result.DatasetDescription);
            Assert.AreEqual("Category", result.CategoryCode);
            Assert.AreEqual(2, result.NamedEnvironments.Count);
            Assert.AreEqual("TEST", result.NamedEnvironments[0]);
            Assert.AreEqual("DEV", result.NamedEnvironments[1]);
            Assert.IsFalse(result.IsSecured);
            Assert.IsFalse(result.IsFavorite);
            Assert.AreEqual(22, result.TargetDatasetId);
            Assert.AreEqual(1, result.SearchHighlights.Count);

            highlight = result.SearchHighlights.First();
            Assert.AreEqual("DatasetDescription", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Description 2", highlight.Highlights.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task SearchGlobalDatasetsAsync_SearchGlobalDatasetsDto_SearchGlobalDatasetsResultDto_NoResults()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IGlobalDatasetProvider> globalDatasetProvider = mr.Create<IGlobalDatasetProvider>();

            SearchGlobalDatasetsDto searchGlobalDatasetsDto = new SearchGlobalDatasetsDto();

            List<GlobalDataset> globalDatasets = new List<GlobalDataset>();

            globalDatasetProvider.Setup(x => x.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto)).ReturnsAsync(globalDatasets);

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().AssociateId).Returns("082116");

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(true);

            GlobalDatasetService globalDatasetService = new GlobalDatasetService(globalDatasetProvider.Object, null, userService.Object, dataFeatures.Object);

            SearchGlobalDatasetsResultsDto results = await globalDatasetService.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto);

            Assert.IsFalse(results.GlobalDatasets.Any());

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task GetGlobalDatasetFiltersAsync_GetGlobalDatasetFiltersDto_GetGlobalDatasetFiltersResultDto()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IGlobalDatasetProvider> globalDatasetProvider = mr.Create<IGlobalDatasetProvider>();

            GetGlobalDatasetFiltersDto filtersDto = new GetGlobalDatasetFiltersDto();
            List<FilterCategoryDto> filterCategories = new List<FilterCategoryDto>();

            globalDatasetProvider.Setup(x => x.GetGlobalDatasetFiltersAsync(filtersDto)).ReturnsAsync(filterCategories);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(true);

            GlobalDatasetService globalDatasetService = new GlobalDatasetService(globalDatasetProvider.Object, null, null, dataFeatures.Object);

            GetGlobalDatasetFiltersResultDto results = await globalDatasetService.GetGlobalDatasetFiltersAsync(filtersDto);

            Assert.AreEqual(filterCategories, results.FilterCategories);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task GetGlobalDatasetFiltersAsync_GetGlobalDatasetFiltersDto_ShouldSearchColumns_GetGlobalDatasetFiltersResultDto()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IGlobalDatasetProvider> globalDatasetProvider = mr.Create<IGlobalDatasetProvider>();

            GetGlobalDatasetFiltersDto filtersDto = new GetGlobalDatasetFiltersDto 
            { 
                SearchText = "search",
                ShouldSearchColumns = true
            };

            DocumentsFiltersDto<GlobalDataset> documentsFiltersDto = new DocumentsFiltersDto<GlobalDataset>
            {
                FilterCategories = new List<FilterCategoryDto> 
                {
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.CATEGORY,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "Category",
                                Selected = true,
                                ResultCount = 1
                            }
                        }
                    }
                },
                Documents = new List<GlobalDataset>
                {
                    new GlobalDataset
                    {
                        EnvironmentDatasets = new List<EnvironmentDataset>
                        {
                            new EnvironmentDataset { DatasetId = 11 }
                        }
                    }
                }
            };

            globalDatasetProvider.Setup(x => x.GetGlobalDatasetsAndFiltersAsync(filtersDto)).ReturnsAsync(documentsFiltersDto);

            List<FilterCategoryDto> additionalFilters = new List<FilterCategoryDto>
            {
                new FilterCategoryDto
                {
                    CategoryName = FilterCategoryNames.Dataset.CATEGORY,
                    CategoryOptions = new List<FilterCategoryOptionDto>
                    {
                        new FilterCategoryOptionDto
                        {
                            OptionValue = "Category",
                            Selected = false,
                            ResultCount = 1
                        },
                        new FilterCategoryOptionDto
                        {
                            OptionValue = "Category 2",
                            Selected = false,
                            ResultCount = 1
                        }
                    }
                }
            };

            globalDatasetProvider.Setup(x => x.GetFiltersByEnvironmentDatasetIdsAsync(It.Is<List<int>>(i => i.Count == 1 && i[0] == 31))).ReturnsAsync(additionalFilters);
            
            Mock<ISchemaFieldProvider> schemaFieldProvider = mr.Create<ISchemaFieldProvider>();

            List<ElasticSchemaField> schemaFields = new List<ElasticSchemaField>
            {
                new ElasticSchemaField
                {
                    DatasetId = 11
                },
                new ElasticSchemaField
                {
                    DatasetId = 31
                },
                new ElasticSchemaField
                {
                    DatasetId = 11
                }
            };

            schemaFieldProvider.Setup(x => x.SearchSchemaFieldsAsync(filtersDto)).ReturnsAsync(schemaFields).Callback<BaseSearchDto>(x =>
            {
                Assert.AreEqual("search", x.SearchText);
            });

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(true);

            GlobalDatasetService globalDatasetService = new GlobalDatasetService(globalDatasetProvider.Object, schemaFieldProvider.Object, null, dataFeatures.Object);

            GetGlobalDatasetFiltersResultDto results = await globalDatasetService.GetGlobalDatasetFiltersAsync(filtersDto);

            Assert.AreEqual(1, results.FilterCategories.Count);

            FilterCategoryDto category = results.FilterCategories.First();
            Assert.AreEqual(FilterCategoryNames.Dataset.CATEGORY, category.CategoryName);
            Assert.AreEqual(2, category.CategoryOptions.Count);

            FilterCategoryOptionDto option = category.CategoryOptions[0];
            Assert.AreEqual("Category", option.OptionValue);
            Assert.IsTrue(option.Selected);
            Assert.AreEqual(2, option.ResultCount);

            option = category.CategoryOptions[1];
            Assert.AreEqual("Category 2", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(1, option.ResultCount);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task SearchGlobalDatasetsAsync_SearchGlobalDatasetsDto_SearchColumns_WithFilterCategories_SearchGlobalDatasetsResultDto()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IGlobalDatasetProvider> globalDatasetProvider = mr.Create<IGlobalDatasetProvider>();

            SearchGlobalDatasetsDto searchGlobalDatasetsDto = new SearchGlobalDatasetsDto
            {
                ShouldSearchColumns = true,
                SearchText = "search",
                FilterCategories = new List<FilterCategoryDto>
                {
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.CATEGORY,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "Category"
                            }
                        }
                    }
                }
            };

            List<GlobalDataset> globalDatasets = GetGlobalDatasets();

            List<GlobalDataset> globalDatasets2 = GetGlobalDatasets();
            GlobalDataset additionalGlobalDataset = new GlobalDataset
            {
                GlobalDatasetId = 3,
                DatasetName = "Name 3",
                DatasetSaidAssetCode = "DATA",
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 31,
                        DatasetDescription = "Description 3",
                        CategoryCode = "Category",
                        NamedEnvironment = "NRTEST",
                        NamedEnvironmentType = NamedEnvironmentType.NonProd.ToString(),
                        OriginationCode = DatasetOriginationCode.External.ToString(),
                        IsSecured = false,
                        FavoriteUserIds = new List<string>()
                    }
                },
                SearchHighlights = new List<SearchHighlight>
                {
                    new SearchHighlight
                    {
                        PropertyName = FilterCategoryNames.Dataset.DATASETASSET,
                        Highlights = new List<string> { "DATA"}
                    }
                }
            };
            globalDatasets2.Add(additionalGlobalDataset);

            globalDatasetProvider.Setup(x => x.SearchGlobalDatasetsAsync(It.IsAny<SearchGlobalDatasetsDto>())).ReturnsAsync(globalDatasets2);
            globalDatasetProvider.Setup(x => x.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto)).ReturnsAsync(globalDatasets);
            globalDatasetProvider.Setup(x => x.GetGlobalDatasetsByEnvironmentDatasetIdsAsync(It.Is<List<int>>(i => i.Count == 1 && i.First() == 31))).ReturnsAsync(new List<GlobalDataset> { additionalGlobalDataset });

            Mock<ISchemaFieldProvider> schemaFieldProvider = mr.Create<ISchemaFieldProvider>();
            List<ElasticSchemaField> schemaFields = new List<ElasticSchemaField>
            {
                new ElasticSchemaField
                {
                    DatasetId = 11,
                    SearchHighlights = new List<SearchHighlight>
                    {
                        new SearchHighlight
                        {
                            PropertyName = SearchDisplayNames.SchemaField.COLUMNNAME,
                            Highlights = new List<string> { "Field1" }
                        }
                    }
                },
                new ElasticSchemaField
                {
                    DatasetId = 31,
                    SearchHighlights = new List<SearchHighlight>
                    {
                        new SearchHighlight
                        {
                            PropertyName = SearchDisplayNames.SchemaField.COLUMNNAME,
                            Highlights = new List<string> { "Field2" }
                        }
                    }
                },
                new ElasticSchemaField
                {
                    DatasetId = 11,
                    SearchHighlights = new List<SearchHighlight>
                    {
                        new SearchHighlight
                        {
                            PropertyName = SearchDisplayNames.SchemaField.COLUMNNAME,
                            Highlights = new List<string> { "Field3" }
                        }
                    }
                }
            };
            schemaFieldProvider.Setup(x => x.SearchSchemaFieldsWithHighlightingAsync(It.IsAny<SearchSchemaFieldsDto>())).ReturnsAsync(schemaFields).Callback<SearchSchemaFieldsDto>(x =>
            {
                Assert.AreEqual(5, x.DatasetIds.Count);
                Assert.AreEqual("search", x.SearchText);
            });

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().AssociateId).Returns("082116");

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(true);

            GlobalDatasetService globalDatasetService = new GlobalDatasetService(globalDatasetProvider.Object, schemaFieldProvider.Object, userService.Object, dataFeatures.Object);

            SearchGlobalDatasetsResultsDto results = await globalDatasetService.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto);

            Assert.AreEqual(3, results.GlobalDatasets.Count);

            SearchGlobalDatasetDto result = results.GlobalDatasets[0];
            Assert.AreEqual(1, result.GlobalDatasetId);
            Assert.AreEqual("Name", result.DatasetName);
            Assert.AreEqual("SAID", result.DatasetSaidAssetCode);
            Assert.AreEqual("Description", result.DatasetDescription);
            Assert.AreEqual("Category", result.CategoryCode);
            Assert.AreEqual(2, result.NamedEnvironments.Count);
            Assert.AreEqual("PROD", result.NamedEnvironments[0]);
            Assert.AreEqual("DEV", result.NamedEnvironments[1]);
            Assert.IsTrue(result.IsSecured);
            Assert.IsTrue(result.IsFavorite);
            Assert.AreEqual(12, result.TargetDatasetId);
            Assert.AreEqual(3, result.SearchHighlights.Count);

            SearchHighlightDto highlight = result.SearchHighlights.First();
            Assert.AreEqual("DatasetName", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Name", highlight.Highlights.First());

            highlight = result.SearchHighlights[1];
            Assert.AreEqual("DatasetDescription", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Description", highlight.Highlights.First());

            highlight = result.SearchHighlights.Last();
            Assert.AreEqual(SearchDisplayNames.SchemaField.COLUMNNAME, highlight.PropertyName);
            Assert.AreEqual(2, highlight.Highlights.Count);
            Assert.AreEqual("Field1", highlight.Highlights.First());
            Assert.AreEqual("Field3", highlight.Highlights.Last());

            result = results.GlobalDatasets[1];
            Assert.AreEqual(2, result.GlobalDatasetId);
            Assert.AreEqual("Name 2", result.DatasetName);
            Assert.AreEqual("DATA", result.DatasetSaidAssetCode);
            Assert.AreEqual("Description 2", result.DatasetDescription);
            Assert.AreEqual("Category", result.CategoryCode);
            Assert.AreEqual(2, result.NamedEnvironments.Count);
            Assert.AreEqual("TEST", result.NamedEnvironments[0]);
            Assert.AreEqual("DEV", result.NamedEnvironments[1]);
            Assert.IsFalse(result.IsSecured);
            Assert.IsFalse(result.IsFavorite);
            Assert.AreEqual(22, result.TargetDatasetId);
            Assert.AreEqual(1, result.SearchHighlights.Count);

            highlight = result.SearchHighlights.First();
            Assert.AreEqual("DatasetDescription", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Description 2", highlight.Highlights.First());

            result = results.GlobalDatasets[2];
            Assert.AreEqual(3, result.GlobalDatasetId);
            Assert.AreEqual("Name 3", result.DatasetName);
            Assert.AreEqual("DATA", result.DatasetSaidAssetCode);
            Assert.AreEqual("Description 3", result.DatasetDescription);
            Assert.AreEqual("Category", result.CategoryCode);
            Assert.AreEqual(1, result.NamedEnvironments.Count);
            Assert.AreEqual("NRTEST", result.NamedEnvironments[0]);
            Assert.IsFalse(result.IsSecured);
            Assert.IsFalse(result.IsFavorite);
            Assert.AreEqual(31, result.TargetDatasetId);
            Assert.AreEqual(2, result.SearchHighlights.Count);

            highlight = result.SearchHighlights.First();
            Assert.AreEqual(FilterCategoryNames.Dataset.DATASETASSET, highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("DATA", highlight.Highlights.First());

            highlight = result.SearchHighlights.Last();
            Assert.AreEqual(SearchDisplayNames.SchemaField.COLUMNNAME, highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Field2", highlight.Highlights.First());

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task SearchGlobalDatasetsAsync_SearchGlobalDatasetsDto_SearchColumns_NoFilterCategories_SearchGlobalDatasetsResultDto()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IGlobalDatasetProvider> globalDatasetProvider = mr.Create<IGlobalDatasetProvider>();

            SearchGlobalDatasetsDto searchGlobalDatasetsDto = new SearchGlobalDatasetsDto
            {
                ShouldSearchColumns = true,
                SearchText = "search"
            };

            List<GlobalDataset> globalDatasets = GetGlobalDatasets();

            globalDatasetProvider.Setup(x => x.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto)).ReturnsAsync(globalDatasets);

            Mock<ISchemaFieldProvider> schemaFieldProvider = mr.Create<ISchemaFieldProvider>();
            List<ElasticSchemaField> schemaFields = new List<ElasticSchemaField>();
            schemaFieldProvider.Setup(x => x.SearchSchemaFieldsWithHighlightingAsync(It.IsAny<SearchSchemaFieldsDto>())).ReturnsAsync(schemaFields).Callback<SearchSchemaFieldsDto>(x =>
            {
                Assert.IsFalse(x.DatasetIds.Any());
                Assert.AreEqual("search", x.SearchText);
            });

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().AssociateId).Returns("082116");

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(true);

            GlobalDatasetService globalDatasetService = new GlobalDatasetService(globalDatasetProvider.Object, schemaFieldProvider.Object, userService.Object, dataFeatures.Object);

            SearchGlobalDatasetsResultsDto results = await globalDatasetService.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto);

            Assert.AreEqual(2, results.GlobalDatasets.Count);

            SearchGlobalDatasetDto result = results.GlobalDatasets[0];
            Assert.AreEqual(1, result.GlobalDatasetId);
            Assert.AreEqual("Name", result.DatasetName);
            Assert.AreEqual("SAID", result.DatasetSaidAssetCode);
            Assert.AreEqual("Description", result.DatasetDescription);
            Assert.AreEqual("Category", result.CategoryCode);
            Assert.AreEqual(2, result.NamedEnvironments.Count);
            Assert.AreEqual("PROD", result.NamedEnvironments[0]);
            Assert.AreEqual("DEV", result.NamedEnvironments[1]);
            Assert.IsTrue(result.IsSecured);
            Assert.IsTrue(result.IsFavorite);
            Assert.AreEqual(12, result.TargetDatasetId);
            Assert.AreEqual(2, result.SearchHighlights.Count);

            SearchHighlightDto highlight = result.SearchHighlights.First();
            Assert.AreEqual("DatasetName", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Name", highlight.Highlights.First());

            highlight = result.SearchHighlights[1];
            Assert.AreEqual("DatasetDescription", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Description", highlight.Highlights.First());

            result = results.GlobalDatasets[1];
            Assert.AreEqual(2, result.GlobalDatasetId);
            Assert.AreEqual("Name 2", result.DatasetName);
            Assert.AreEqual("DATA", result.DatasetSaidAssetCode);
            Assert.AreEqual("Description 2", result.DatasetDescription);
            Assert.AreEqual("Category", result.CategoryCode);
            Assert.AreEqual(2, result.NamedEnvironments.Count);
            Assert.AreEqual("TEST", result.NamedEnvironments[0]);
            Assert.AreEqual("DEV", result.NamedEnvironments[1]);
            Assert.IsFalse(result.IsSecured);
            Assert.IsFalse(result.IsFavorite);
            Assert.AreEqual(22, result.TargetDatasetId);
            Assert.AreEqual(1, result.SearchHighlights.Count);

            highlight = result.SearchHighlights.First();
            Assert.AreEqual("DatasetDescription", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Description 2", highlight.Highlights.First());

            mr.VerifyAll();
        }

        #region Helpers
        private List<GlobalDataset> GetGlobalDatasets()
        {
            return new List<GlobalDataset>
            {
                new GlobalDataset
                {
                    GlobalDatasetId = 1,
                    DatasetName = "Name",
                    DatasetSaidAssetCode = "SAID",
                    EnvironmentDatasets = new List<EnvironmentDataset>
                    {
                        new EnvironmentDataset
                        {
                            DatasetId = 11,
                            DatasetDescription = "Description",
                            CategoryCode = "Category",
                            NamedEnvironment = "DEV",
                            NamedEnvironmentType = NamedEnvironmentType.NonProd.ToString(),
                            OriginationCode = DatasetOriginationCode.Internal.ToString(),
                            IsSecured = true,
                            FavoriteUserIds = new List<string> { "082116" }
                        },
                        new EnvironmentDataset
                        {
                            DatasetId = 12,
                            DatasetDescription = "Description",
                            CategoryCode = "Category",
                            NamedEnvironment = "PROD",
                            NamedEnvironmentType = NamedEnvironmentType.Prod.ToString(),
                            OriginationCode = DatasetOriginationCode.Internal.ToString(),
                            IsSecured = true,
                            FavoriteUserIds = new List<string>()
                        }
                    },
                    SearchHighlights = new List<SearchHighlight>
                    {
                        new SearchHighlight
                        {
                            PropertyName = "DatasetName",
                            Highlights = new List<string> { "Name" }
                        },
                        new SearchHighlight
                        {
                            PropertyName = "DatasetDescription",
                            Highlights = new List<string> { "Description" }
                        }
                    }
                },
                new GlobalDataset
                {
                    GlobalDatasetId = 2,
                    DatasetName = "Name 2",
                    DatasetSaidAssetCode = "DATA",
                    EnvironmentDatasets = new List<EnvironmentDataset>
                    {
                        new EnvironmentDataset
                        {
                            DatasetId = 21,
                            DatasetDescription = "Description 2",
                            CategoryCode = "Category",
                            NamedEnvironment = "DEV",
                            NamedEnvironmentType = NamedEnvironmentType.NonProd.ToString(),
                            OriginationCode = DatasetOriginationCode.External.ToString(),
                            IsSecured = false,
                            FavoriteUserIds = new List<string>()
                        },
                        new EnvironmentDataset
                        {
                            DatasetId = 22,
                            DatasetDescription = "Description 2",
                            CategoryCode = "Category",
                            NamedEnvironment = "TEST",
                            NamedEnvironmentType = NamedEnvironmentType.NonProd.ToString(),
                            OriginationCode = DatasetOriginationCode.External.ToString(),
                            IsSecured = false,
                            FavoriteUserIds = new List<string>()
                        }
                    },
                    SearchHighlights = new List<SearchHighlight>
                    {
                        new SearchHighlight
                        {
                            PropertyName = "DatasetDescription",
                            Highlights = new List<string> { "Description 2"}
                        }
                    }
                }
            };
        }
        #endregion
    }
}
