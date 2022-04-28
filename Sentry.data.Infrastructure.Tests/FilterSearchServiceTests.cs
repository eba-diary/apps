﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class FilterSearchServiceTests
    {
        [TestMethod]
        public void GetSavedSearch_DataInventory_SearchName_000000_SavedSearchDto()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);

            SavedSearch savedSearch = new SavedSearch()
            {
                SavedSearchId = 1,
                SearchType = GlobalConstants.SearchType.DATA_INVENTORY,
                SearchName = "SearchName",
                SearchText = "text search",
                FilterCategoriesJson = @"[{""CategoryName"":""Environment"",""CategoryOptions"":[{""OptionValue"":""P"",""ResultCount"":0,""ParentCategoryName"":""Environment"",""Selected"":true}]}]",
                AssociateId = "000000"
            };

            datasetContext.SetupGet(x => x.SavedSearches).Returns(new List<SavedSearch>() { savedSearch }.AsQueryable());

            FilterSearchService service = new FilterSearchService(datasetContext.Object, null);

            SavedSearchDto result = service.GetSavedSearch(GlobalConstants.SearchType.DATA_INVENTORY, "SearchName", "000000");

            datasetContext.VerifyAll();

            Assert.AreEqual("text search", result.SearchText);
            Assert.AreEqual("SearchName", result.SearchName);
            Assert.AreEqual(GlobalConstants.SearchType.DATA_INVENTORY, result.SearchType);
            Assert.AreEqual("000000", result.AssociateId);
            Assert.AreEqual(1, result.FilterCategories.Count);

            FilterCategoryDto categoryDto = result.FilterCategories.First();
            Assert.AreEqual(GlobalConstants.FilterCategoryNames.ENVIRONMENT, categoryDto.CategoryName);
            Assert.AreEqual(1, categoryDto.CategoryOptions.Count);

            FilterCategoryOptionDto optionDto = categoryDto.CategoryOptions.First();
            Assert.AreEqual(GlobalConstants.FilterCategoryOptions.ENVIRONMENT_PROD, optionDto.OptionValue);
            Assert.AreEqual(GlobalConstants.FilterCategoryNames.ENVIRONMENT, optionDto.ParentCategoryName);
            Assert.IsTrue(optionDto.Selected);
            Assert.AreEqual(0, optionDto.ResultCount);
        }

        [TestMethod]
        public void GetSavedSearchOptions_DataInventory_000000_SavedSearchOptions()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);
            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();

            SavedSearch savedSearch = new SavedSearch()
            {
                SavedSearchId = 1,
                SearchType = GlobalConstants.SearchType.DATA_INVENTORY,
                SearchName = "SearchName",
                AssociateId = "000000"
            };

            datasetContext.SetupGet(x => x.SavedSearches).Returns(new List<SavedSearch>() { savedSearch }.AsQueryable());

            Mock<IUserFavoriteService> userFavoriteService = mockRepository.Create<IUserFavoriteService>();
            userFavoriteService.Setup(x => x.GetUserFavoriteByEntity(1, "000000")).Returns(new UserFavorite());

            FilterSearchService service = new FilterSearchService(datasetContext.Object, userFavoriteService.Object);

            List<SavedSearchOptionDto> results = service.GetSavedSearchOptions(GlobalConstants.SearchType.DATA_INVENTORY, "000000");

            mockRepository.VerifyAll();

            Assert.AreEqual(1, results.Count);

            SavedSearchOptionDto result = results.First();
            Assert.AreEqual("SearchName", result.SavedSearchName);
            Assert.AreEqual(1, result.SavedSearchId);
            Assert.IsTrue(result.IsFavorite);
        }

        [TestMethod]
        public void SaveSearch_SavedSearchDto_AddNew()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.SavedSearches).Returns(new List<SavedSearch>().AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<SavedSearch>())).Callback<SavedSearch>(x =>
            {
                x.SavedSearchId = 1;
                Assert.AreEqual("SearchName", x.SearchName);
                Assert.AreEqual("text search", x.SearchText);
                Assert.AreEqual(GlobalConstants.SearchType.DATA_INVENTORY, x.SearchType);
                Assert.AreEqual("000000", x.AssociateId);
                Assert.AreEqual(@"[{""CategoryName"":""Environment"",""CategoryOptions"":[{""OptionValue"":""P"",""ResultCount"":0,""ParentCategoryName"":""Environment"",""Selected"":true}]}]", x.FilterCategoriesJson);
            });
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IUserFavoriteService> userFavoriteService = mockRepository.Create<IUserFavoriteService>();
            userFavoriteService.Setup(x => x.AddUserFavorite(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, 1, "000000"));

            FilterSearchService service = new FilterSearchService(datasetContext.Object, userFavoriteService.Object);

            SavedSearchDto dto = new SavedSearchDto()
            {
                SearchType = GlobalConstants.SearchType.DATA_INVENTORY,
                SearchName = "SearchName",
                AssociateId = "000000",
                SearchText = "text search",
                AddToFavorites = true,
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = GlobalConstants.FilterCategoryNames.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = GlobalConstants.FilterCategoryOptions.ENVIRONMENT_PROD,
                                ParentCategoryName = GlobalConstants.FilterCategoryNames.ENVIRONMENT,
                                Selected = true,
                                ResultCount = 0
                            }
                        }
                    }
                }
            };

            string result = service.SaveSearch(dto);

            mockRepository.VerifyAll();

            Assert.AreEqual(GlobalConstants.SaveSearchResults.NEW, result);
        }

        [TestMethod]
        public void SaveSearch_SavedSearchDto_UpdateExisting()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();

            SavedSearch savedSearch = new SavedSearch()
            {
                SavedSearchId = 1,
                SearchType = GlobalConstants.SearchType.DATA_INVENTORY,
                SearchName = "SearchName",
                SearchText = "text search",
                FilterCategoriesJson = @"[{""CategoryName"":""Environment"",""CategoryOptions"":[{""OptionValue"":""P"",""ResultCount"":0,""ParentCategoryName"":""Environment"",""Selected"":true}]}]",
                AssociateId = "000000"
            };
            
            datasetContext.SetupGet(x => x.SavedSearches).Returns(new List<SavedSearch>() { savedSearch }.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            FilterSearchService service = new FilterSearchService(datasetContext.Object, null);

            SavedSearchDto dto = new SavedSearchDto()
            {
                SearchType = GlobalConstants.SearchType.DATA_INVENTORY,
                SearchName = "SearchName",
                AssociateId = "000000",
                SearchText = "new search",
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = GlobalConstants.FilterCategoryNames.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = GlobalConstants.FilterCategoryOptions.ENVIRONMENT_NONPROD,
                                ParentCategoryName = GlobalConstants.FilterCategoryNames.ENVIRONMENT,
                                Selected = true,
                                ResultCount = 0
                            }
                        }
                    }
                }
            };

            string result = service.SaveSearch(dto);

            mockRepository.VerifyAll();

            Assert.AreEqual(GlobalConstants.SaveSearchResults.UPDATE, result);
            Assert.AreEqual("new search", savedSearch.SearchText);
            Assert.AreEqual(@"[{""CategoryName"":""Environment"",""CategoryOptions"":[{""OptionValue"":""D"",""ResultCount"":0,""ParentCategoryName"":""Environment"",""Selected"":true}]}]", savedSearch.FilterCategoriesJson);
        }
    }
}
