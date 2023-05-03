using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Infrastructure.FeatureFlags;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            List<GlobalDataset> globalDatasets = new List<GlobalDataset>
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

            globalDatasetProvider.Setup(x => x.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto)).ReturnsAsync(globalDatasets);

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().AssociateId).Returns("082116");

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(true);

            GlobalDatasetService globalDatasetService = new GlobalDatasetService(globalDatasetProvider.Object, userService.Object, dataFeatures.Object);

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

            GlobalDatasetService globalDatasetService = new GlobalDatasetService(globalDatasetProvider.Object, userService.Object, dataFeatures.Object);

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

            GlobalDatasetService globalDatasetService = new GlobalDatasetService(globalDatasetProvider.Object, null, dataFeatures.Object);

            GetGlobalDatasetFiltersResultDto results = await globalDatasetService.GetGlobalDatasetFiltersAsync(filtersDto);

            Assert.AreEqual(filterCategories, results.FilterCategories);

            mr.VerifyAll();
        }
    }
}
