using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using System;
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
                AssociateId = "000000",
                ResultConfigurationJson = @"{""VisibleColumns"":[1,2,3]}"
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
            Assert.AreEqual(GlobalConstants.FilterCategoryNames.DataInventory.ENVIRONMENT, categoryDto.CategoryName);
            Assert.AreEqual(1, categoryDto.CategoryOptions.Count);

            FilterCategoryOptionDto optionDto = categoryDto.CategoryOptions.First();
            Assert.AreEqual(GlobalConstants.FilterCategoryOptions.ENVIRONMENT_PROD, optionDto.OptionValue);
            Assert.AreEqual(GlobalConstants.FilterCategoryNames.DataInventory.ENVIRONMENT, optionDto.ParentCategoryName);
            Assert.IsTrue(optionDto.Selected);
            Assert.AreEqual(0, optionDto.ResultCount);

            Assert.IsTrue(result.ResultConfiguration.ContainsKey("VisibleColumns"));

            List<int> visibleColumns = result.ResultConfiguration["VisibleColumns"].ToObject<List<int>>();
            Assert.AreEqual(3, visibleColumns.Count);
        }

        [TestMethod]
        public void GetSavedSearch_ThrowsException()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);

            datasetContext.SetupGet(x => x.SavedSearches).Throws<Exception>();

            FilterSearchService service = new FilterSearchService(datasetContext.Object, null);

            Assert.ThrowsException<Exception>(() => service.GetSavedSearch(GlobalConstants.SearchType.DATA_INVENTORY, "SearchName", "000000"));

            datasetContext.VerifyAll();
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
            userFavoriteService.Setup(x => x.GetUserFavorite(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, 1, "000000")).Returns(new UserFavorite());

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
        public void GetSavedSearchOptions_ThrowsException()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);

            datasetContext.SetupGet(x => x.SavedSearches).Throws<Exception>();

            FilterSearchService service = new FilterSearchService(datasetContext.Object, null);

            Assert.ThrowsException<Exception>(() => service.GetSavedSearchOptions(GlobalConstants.SearchType.DATA_INVENTORY, "000000"));

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void SaveSearch_SavedSearchDto_Add()
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
                Assert.AreEqual(@"[{""CategoryName"":""Environment"",""CategoryOptions"":[{""OptionValue"":""P"",""ResultCount"":0,""ParentCategoryName"":""Environment"",""Selected"":true}],""DefaultCategoryOpen"":false,""HideResultCounts"":false}]", x.FilterCategoriesJson);
                Assert.AreEqual(@"{""VisibleColumns"":[1,2,3]}", x.ResultConfigurationJson);
            });
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IUserFavoriteService> userFavoriteService = mockRepository.Create<IUserFavoriteService>();
            userFavoriteService.Setup(x => x.AddUserFavorite(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, 1, "000000"));

            FilterSearchService service = new FilterSearchService(datasetContext.Object, userFavoriteService.Object);

            SavedSearchDto dto = new SavedSearchDto()
            {
                SavedSearchId = 0,
                SearchType = GlobalConstants.SearchType.DATA_INVENTORY,
                SearchName = "SearchName",
                AssociateId = "000000",
                SearchText = "text search",
                AddToFavorites = true,
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = GlobalConstants.FilterCategoryNames.DataInventory.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = GlobalConstants.FilterCategoryOptions.ENVIRONMENT_PROD,
                                ParentCategoryName = GlobalConstants.FilterCategoryNames.DataInventory.ENVIRONMENT,
                                Selected = true,
                                ResultCount = 0
                            }
                        }
                    }
                },
                ResultConfiguration = new JObject()
                {
                    { "VisibleColumns", new JArray() { 1, 2, 3 } }
                }
            };

            string result = service.SaveSearch(dto);

            mockRepository.VerifyAll();

            Assert.AreEqual(GlobalConstants.SaveSearchResults.NEW, result);
        }

        [TestMethod]
        public void SaveSearch_SavedSearchDto_Update()
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
                AssociateId = "000000",
                ResultConfigurationJson = @"{""VisibleColumns"":[1,2,3]}"
            };
            
            datasetContext.SetupGet(x => x.SavedSearches).Returns(new List<SavedSearch>() { savedSearch }.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IUserFavoriteService> userFavoriteService = mockRepository.Create<IUserFavoriteService>();
            userFavoriteService.Setup(x => x.RemoveUserFavorite(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, 1, "000000"));

            FilterSearchService service = new FilterSearchService(datasetContext.Object, userFavoriteService.Object);

            SavedSearchDto dto = new SavedSearchDto()
            {
                SavedSearchId = 1,
                SearchType = GlobalConstants.SearchType.DATA_INVENTORY,
                SearchName = "SearchName",
                AssociateId = "000000",
                SearchText = "new search",
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = GlobalConstants.FilterCategoryNames.DataInventory.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = GlobalConstants.FilterCategoryOptions.ENVIRONMENT_NONPROD,
                                ParentCategoryName = GlobalConstants.FilterCategoryNames.DataInventory.ENVIRONMENT,
                                Selected = true,
                                ResultCount = 0
                            }
                        }
                    }
                },
                ResultConfiguration = new JObject()
                {
                    { "VisibleColumns", new JArray() { 2, 3 } }
                }
            };

            string result = service.SaveSearch(dto);

            mockRepository.VerifyAll();

            Assert.AreEqual(GlobalConstants.SaveSearchResults.UPDATE, result);
            Assert.AreEqual("new search", savedSearch.SearchText);
            Assert.AreEqual(@"[{""CategoryName"":""Environment"",""CategoryOptions"":[{""OptionValue"":""D"",""ResultCount"":0,""ParentCategoryName"":""Environment"",""Selected"":true}],""DefaultCategoryOpen"":false,""HideResultCounts"":false}]", savedSearch.FilterCategoriesJson);
            Assert.AreEqual(@"{""VisibleColumns"":[2,3]}", savedSearch.ResultConfigurationJson);
        }

        [TestMethod]
        public void SaveSearch_SavedSearchDto_Exists()
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

            FilterSearchService service = new FilterSearchService(datasetContext.Object, null);

            SavedSearchDto dto = new SavedSearchDto()
            {
                SavedSearchId = 0,
                SearchType = GlobalConstants.SearchType.DATA_INVENTORY,
                SearchName = "SearchName",
                AssociateId = "000000"
            };

            string result = service.SaveSearch(dto);

            mockRepository.VerifyAll();

            Assert.AreEqual(GlobalConstants.SaveSearchResults.EXISTS, result);
        }

        [TestMethod]
        public void SaveSearch_ThrowsException()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);

            datasetContext.SetupGet(x => x.SavedSearches).Throws<Exception>();

            FilterSearchService service = new FilterSearchService(datasetContext.Object, null);

            Assert.ThrowsException<Exception>(() => service.SaveSearch(new SavedSearchDto()));

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void RemoveSavedSearch_1_000000_Success()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();

            SavedSearch savedSearch = new SavedSearch()
            {
                SavedSearchId = 1,
                AssociateId = "000000"
            };

            datasetContext.SetupGet(x => x.SavedSearches).Returns(new List<SavedSearch>() { savedSearch }.AsQueryable());
            datasetContext.Setup(x => x.Remove(It.IsAny<SavedSearch>())).Callback<SavedSearch>(x =>
            {
                Assert.AreEqual(1, x.SavedSearchId);
                Assert.AreEqual("000000", x.AssociateId);
            });
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IUserFavoriteService> userFavoriteService = mockRepository.Create<IUserFavoriteService>();
            userFavoriteService.Setup(x => x.RemoveUserFavorite(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, 1, "000000"));

            FilterSearchService service = new FilterSearchService(datasetContext.Object, userFavoriteService.Object);

            service.RemoveSavedSearch(1, "000000");

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void RemoveSavedSearch_ThrowsException()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);

            datasetContext.SetupGet(x => x.SavedSearches).Throws<Exception>();

            FilterSearchService service = new FilterSearchService(datasetContext.Object, null);

            Assert.ThrowsException<Exception>(() => service.RemoveSavedSearch(1, "000000"));

            datasetContext.VerifyAll();
        }
    }
}
