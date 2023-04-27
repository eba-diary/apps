using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class GlobalDatasetSearchServiceTests
    {
        [TestMethod]
        public void SetGlobalDatasetPageResults_SortAlphabetically()
        {
            GlobalDatasetPageRequestDto requestDto = new GlobalDatasetPageRequestDto
            {
                PageNumber = 1,
                PageSize = 15,
                SortBy = (int)GlobalDatasetSortByOption.Alphabetical,
                Layout = (int)LayoutOption.List,
                GlobalDatasets = new List<SearchGlobalDatasetDto>
                {
                    new SearchGlobalDatasetDto
                    {
                        DatasetName = "Name 2"
                    },
                    new SearchGlobalDatasetDto
                    {
                        DatasetName = "Name 1"
                    },
                    new SearchGlobalDatasetDto
                    {
                        DatasetName = "Name 3"
                    }
                }
            };

            GlobalDatasetSearchService searchService = new GlobalDatasetSearchService(null);

            GlobalDatasetPageResultDto resultDto = searchService.SetGlobalDatasetPageResults(requestDto);

            Assert.AreEqual(1, resultDto.PageNumber);
            Assert.AreEqual(15, resultDto.PageSize);
            Assert.AreEqual((int)GlobalDatasetSortByOption.Alphabetical, resultDto.SortBy);
            Assert.AreEqual((int)LayoutOption.List, resultDto.Layout);
            Assert.AreEqual(3, resultDto.GlobalDatasets.Count);
            Assert.AreEqual(3, resultDto.TotalResults);

            Assert.AreEqual("Name 1", resultDto.GlobalDatasets[0].DatasetName);
            Assert.AreEqual("Name 2", resultDto.GlobalDatasets[1].DatasetName);
            Assert.AreEqual("Name 3", resultDto.GlobalDatasets[2].DatasetName);
        }

        [TestMethod]
        public void SetGlobalDatasetPageResults_SortFavorite()
        {
            GlobalDatasetPageRequestDto requestDto = new GlobalDatasetPageRequestDto
            {
                PageNumber = 1,
                PageSize = 15,
                SortBy = (int)GlobalDatasetSortByOption.Favorites,
                Layout = (int)LayoutOption.List,
                GlobalDatasets = new List<SearchGlobalDatasetDto>
                {
                    new SearchGlobalDatasetDto
                    {
                        DatasetName = "Name 3",
                        IsFavorite = false
                    },
                    new SearchGlobalDatasetDto
                    {
                        DatasetName = "Name 2",
                        IsFavorite = true
                    },
                    new SearchGlobalDatasetDto
                    {
                        DatasetName = "Name 1",
                        IsFavorite = false
                    }
                }
            };

            GlobalDatasetSearchService searchService = new GlobalDatasetSearchService(null);

            GlobalDatasetPageResultDto resultDto = searchService.SetGlobalDatasetPageResults(requestDto);

            Assert.AreEqual(1, resultDto.PageNumber);
            Assert.AreEqual(15, resultDto.PageSize);
            Assert.AreEqual((int)GlobalDatasetSortByOption.Favorites, resultDto.SortBy);
            Assert.AreEqual((int)LayoutOption.List, resultDto.Layout);
            Assert.AreEqual(3, resultDto.GlobalDatasets.Count);
            Assert.AreEqual(3, resultDto.TotalResults);

            Assert.AreEqual("Name 2", resultDto.GlobalDatasets[0].DatasetName);
            Assert.AreEqual("Name 3", resultDto.GlobalDatasets[1].DatasetName);
            Assert.AreEqual("Name 1", resultDto.GlobalDatasets[2].DatasetName);
        }

        [TestMethod]
        public void SetGlobalDatasetPageResults_Page2()
        {
            GlobalDatasetPageRequestDto requestDto = new GlobalDatasetPageRequestDto
            {
                PageNumber = 2,
                PageSize = 1,
                SortBy = (int)GlobalDatasetSortByOption.Relevance,
                Layout = (int)LayoutOption.List,
                GlobalDatasets = new List<SearchGlobalDatasetDto>
                {
                    new SearchGlobalDatasetDto
                    {
                        DatasetName = "Name 1"
                    },
                    new SearchGlobalDatasetDto
                    {
                        DatasetName = "Name 2"
                    },
                    new SearchGlobalDatasetDto
                    {
                        DatasetName = "Name 3"
                    }
                }
            };

            GlobalDatasetSearchService searchService = new GlobalDatasetSearchService(null);

            GlobalDatasetPageResultDto resultDto = searchService.SetGlobalDatasetPageResults(requestDto);

            Assert.AreEqual(2, resultDto.PageNumber);
            Assert.AreEqual(1, resultDto.PageSize);
            Assert.AreEqual((int)GlobalDatasetSortByOption.Relevance, resultDto.SortBy);
            Assert.AreEqual((int)LayoutOption.List, resultDto.Layout);
            Assert.AreEqual(1, resultDto.GlobalDatasets.Count);
            Assert.AreEqual(3, resultDto.TotalResults);

            Assert.AreEqual("Name 2", resultDto.GlobalDatasets[0].DatasetName);
        }

        [TestMethod]
        public void GetInitialFilters_NoFilters_DefaultProd()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4258_DefaultProdSearchFilter.GetValue()).Returns(true);

            GlobalDatasetSearchService searchService = new GlobalDatasetSearchService(dataFeatures.Object);

            List<FilterCategoryDto> filters = searchService.GetInitialFilters(null);

            Assert.AreEqual(1, filters.Count);

            FilterCategoryDto filterCategory = filters[0];
            Assert.AreEqual(FilterCategoryNames.Dataset.ENVIRONMENTTYPE, filterCategory.CategoryName);
            Assert.AreEqual(1, filterCategory.CategoryOptions.Count);

            FilterCategoryOptionDto option = filterCategory.CategoryOptions[0];
            Assert.AreEqual(NamedEnvironmentType.Prod.GetDescription(), option.OptionValue);
            Assert.AreEqual(FilterCategoryNames.Dataset.ENVIRONMENTTYPE, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            mr.VerifyAll();
        }

        [TestMethod]
        public void GetInitialFilters_MultipleFilters()
        {
            GlobalDatasetSearchService searchService = new GlobalDatasetSearchService(null);

            List<string> filters = new List<string>
            {
                $"{FilterCategoryNames.Dataset.ENVIRONMENT}_DEV",
                $"{FilterCategoryNames.Dataset.CATEGORY}_Category"
            };

            List<FilterCategoryDto> results = searchService.GetInitialFilters(filters);

            Assert.AreEqual(2, results.Count);

            FilterCategoryDto filterCategory = results[0];
            Assert.AreEqual(FilterCategoryNames.Dataset.ENVIRONMENT, filterCategory.CategoryName);
            Assert.AreEqual(1, filterCategory.CategoryOptions.Count);

            FilterCategoryOptionDto option = filterCategory.CategoryOptions[0];
            Assert.AreEqual("DEV", option.OptionValue);
            Assert.AreEqual(FilterCategoryNames.Dataset.ENVIRONMENT, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            filterCategory = results[1];
            Assert.AreEqual(FilterCategoryNames.Dataset.CATEGORY, filterCategory.CategoryName);
            Assert.AreEqual(1, filterCategory.CategoryOptions.Count);

            option = filterCategory.CategoryOptions[0];
            Assert.AreEqual("Category", option.OptionValue);
            Assert.AreEqual(FilterCategoryNames.Dataset.CATEGORY, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);
        }
    }
}
