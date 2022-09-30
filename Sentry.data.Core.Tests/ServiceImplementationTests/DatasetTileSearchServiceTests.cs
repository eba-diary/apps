using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetTileSearchServiceTests : BaseCoreUnitTest
    {
        [TestMethod]
        public void PublishSearchEventAsync_TileSearchEventDto_Success()
        {
            JObject searchJson = GetData("DatasetTileSearchService_TileSearchEventDto.json");

            Mock<IEventService> eventService = new Mock<IEventService>(MockBehavior.Strict);
            eventService.Setup(x => x.PublishSuccessEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Callback<string, string, string>((type, reason, search) =>
            {
                Assert.AreEqual(GlobalConstants.EventType.SEARCH, type);
                Assert.AreEqual("Searched Datasets", reason);
                Assert.AreEqual(searchJson.ToString(Formatting.None), search);
            }).Returns(Task.CompletedTask);

            TileSearchEventDto dto = new TileSearchEventDto()
            {
                SearchName = "SearchName",
                SearchText = "SearchText",
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.Dataset.CATEGORY,
                        DefaultCategoryOpen = true,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                ParentCategoryName = FilterCategoryNames.Dataset.CATEGORY,
                                OptionValue = "Sentry",
                                Selected = true
                            }
                        }
                    }
                },
                PageNumber = 2,
                PageSize = 15,
                SortBy = 3,
                Layout = 1,
                TotalResults = 50
            };

            DatasetTileSearchService service = new DatasetTileSearchService(null, null, eventService.Object);
            service.PublishSearchEventAsync(dto).Wait();

            eventService.VerifyAll();
        }

        [TestMethod]
        public void GetSearchableTiles_DatasetTileDtos_Admin()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);
            DatasetTileSearchService searchService = GetSearchService(repository, true);

            List<DatasetTileDto> tileDtos = searchService.GetSearchableTiles();

            Assert.AreEqual(5, tileDtos.Count);

            DatasetTileDto tile = tileDtos.First(x => x.Name == "Dataset1");
            Assert.IsTrue(tile.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 8), tile.LastActivityDateTime);
            Assert.AreEqual(1, tile.ProducerAssets.Count);
            Assert.AreEqual("SAID", tile.ProducerAssets.First());
            Assert.AreEqual(ObjectStatusEnum.Active, tile.Status);

            tile = tileDtos.First(x => x.Name == "Dataset2");
            Assert.IsFalse(tile.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 8), tile.LastActivityDateTime);
            Assert.AreEqual(2, tile.ProducerAssets.Count);
            Assert.AreEqual("SAID", tile.ProducerAssets.First());
            Assert.AreEqual("SAID3", tile.ProducerAssets.Last());
            Assert.AreEqual(ObjectStatusEnum.Active, tile.Status);

            tile = tileDtos.First(x => x.Name == "Dataset3");
            Assert.IsTrue(tile.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 12), tile.LastActivityDateTime);
            Assert.AreEqual(1, tile.ProducerAssets.Count);
            Assert.AreEqual("SAID3", tile.ProducerAssets.First());
            Assert.AreEqual(ObjectStatusEnum.Active, tile.Status);

            tile = tileDtos.First(x => x.Name == "Foo Bar");
            Assert.IsTrue(tile.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 20), tile.LastActivityDateTime);
            Assert.AreEqual(0, tile.ProducerAssets.Count);
            Assert.AreEqual(ObjectStatusEnum.Active, tile.Status);

            tile = tileDtos.First(x => x.Name == "Pending Delete");
            Assert.IsFalse(tile.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 21), tile.LastActivityDateTime);
            Assert.AreEqual(0, tile.ProducerAssets.Count);
            Assert.AreEqual(ObjectStatusEnum.Pending_Delete, tile.Status);

            repository.VerifyAll();
        }

        [TestMethod]
        public void GetSearchableTiles_DatasetTileDtos_NonAdmin()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);
            DatasetTileSearchService searchService = GetSearchService(repository, false);

            List<DatasetTileDto> tileDtos = searchService.GetSearchableTiles();

            Assert.AreEqual(4, tileDtos.Count);

            DatasetTileDto tile = tileDtos.First(x => x.Name == "Dataset1");
            Assert.IsTrue(tile.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 8), tile.LastActivityDateTime);
            Assert.AreEqual(1, tile.ProducerAssets.Count);
            Assert.AreEqual("SAID", tile.ProducerAssets.First());
            Assert.AreEqual(ObjectStatusEnum.Active, tile.Status);

            tile = tileDtos.First(x => x.Name == "Dataset2");
            Assert.IsFalse(tile.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 8), tile.LastActivityDateTime);
            Assert.AreEqual(2, tile.ProducerAssets.Count);
            Assert.AreEqual("SAID", tile.ProducerAssets.First());
            Assert.AreEqual("SAID3", tile.ProducerAssets.Last());
            Assert.AreEqual(ObjectStatusEnum.Active, tile.Status);

            tile = tileDtos.First(x => x.Name == "Dataset3");
            Assert.IsTrue(tile.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 12), tile.LastActivityDateTime);
            Assert.AreEqual(1, tile.ProducerAssets.Count);
            Assert.AreEqual("SAID3", tile.ProducerAssets.First());
            Assert.AreEqual(ObjectStatusEnum.Active, tile.Status);

            tile = tileDtos.First(x => x.Name == "Foo Bar");
            Assert.IsTrue(tile.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 20), tile.LastActivityDateTime);
            Assert.AreEqual(0, tile.ProducerAssets.Count);
            Assert.AreEqual(ObjectStatusEnum.Active, tile.Status);

            repository.VerifyAll();
        }

        [TestMethod]
        public void GetSearchableTiles_ThrowsException()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Throws(new Exception("No bueno!"));

            DatasetTileSearchService searchService = new DatasetTileSearchService(datasetContext.Object, null, null);

            List<DatasetTileDto> dtos = searchService.GetSearchableTiles();

            Assert.AreEqual(0, dtos.Count);
        }

        [TestMethod]
        public void SearchTiles_TileSearchDto_DatasetTileDto_EmptySearchableTiles_SortByRecentActivity_TileSearchResultDto()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);
            DatasetTileSearchService searchService = GetSearchService(repository, false);

            TileSearchDto<DatasetTileDto> tileSearchDto = new TileSearchDto<DatasetTileDto>()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchText = "Dataset",
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.Dataset.CATEGORY,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "Category1",
                                Selected = true
                            }
                        }
                    }
                },
                UpdateFilters = true,
                OrderByDescending = true,
                OrderByField = x => x.LastActivityDateTime
            };

            TileSearchResultDto<DatasetTileDto> result = searchService.SearchTiles(tileSearchDto);

            Assert.AreEqual(2, result.TotalResults);
            Assert.AreEqual(10, result.PageSize);
            Assert.AreEqual(1, result.PageNumber);
            Assert.AreEqual(2, result.Tiles.Count);
            Assert.AreEqual(5, result.FilterCategories.Count);
            Assert.AreEqual(2, result.FilterCategories.First(x => x.CategoryName == FilterCategoryNames.Dataset.CATEGORY).CategoryOptions.Count);
            Assert.AreEqual(2, result.FilterCategories.First(x => x.CategoryName == FilterCategoryNames.Dataset.SECURED).CategoryOptions.Count);
            Assert.AreEqual(2, result.FilterCategories.First(x => x.CategoryName == FilterCategoryNames.Dataset.FAVORITE).CategoryOptions.Count);
            Assert.AreEqual(1, result.FilterCategories.First(x => x.CategoryName == FilterCategoryNames.Dataset.ENVIRONMENTTYPE).CategoryOptions.Count);
            Assert.AreEqual(2, result.FilterCategories.First(x => x.CategoryName == FilterCategoryNames.Dataset.PRODUCERASSET).CategoryOptions.Count);

            DatasetTileDto tileDto = result.Tiles.First();
            Assert.AreEqual("Dataset3", tileDto.Name);
            Assert.IsTrue(tileDto.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 12), tileDto.LastActivityDateTime);
            Assert.AreEqual(1, tileDto.ProducerAssets.Count);
            Assert.AreEqual("SAID3", tileDto.ProducerAssets.First());

            tileDto = result.Tiles.Last();
            Assert.AreEqual("Dataset1", tileDto.Name);
            Assert.IsTrue(tileDto.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 8), tileDto.LastActivityDateTime);
            Assert.AreEqual(1, tileDto.ProducerAssets.Count);
            Assert.AreEqual("SAID", tileDto.ProducerAssets.First());

            repository.VerifyAll();
        }

        [TestMethod]
        public void SearchTiles_TileSearchDto_DatasetTileDto_SearchableTiles_SortByAlphabetically_TileSearchResultDto()
        {
            DatasetTileSearchService searchService = new DatasetTileSearchService(null, null, null);

            TileSearchDto<DatasetTileDto> tileSearchDto = new TileSearchDto<DatasetTileDto>()
            {
                PageNumber = 2,
                PageSize = 2,
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.Dataset.CATEGORY,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "Category1",
                                Selected = true
                            }
                        }
                    }
                },
                UpdateFilters = false,
                OrderByField = x => x.Name,
                SearchableTiles = GetSearchableTiles()
            };

            TileSearchResultDto<DatasetTileDto> result = searchService.SearchTiles(tileSearchDto);

            Assert.AreEqual(3, result.TotalResults);
            Assert.AreEqual(2, result.PageSize);
            Assert.AreEqual(2, result.PageNumber);
            Assert.AreEqual(1, result.Tiles.Count);
            Assert.AreEqual(0, result.FilterCategories.Count);

            DatasetTileDto tileDto = result.Tiles.First();
            Assert.AreEqual("Foo Bar", tileDto.Name);
            Assert.IsTrue(tileDto.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 23), tileDto.LastActivityDateTime);
        }

        [TestMethod]
        public void SearchTiles_TileSearchDto_DatasetTileDto_SearchableTiles_SortByFavorite_TileSearchResultDto()
        {
            DatasetTileSearchService searchService = new DatasetTileSearchService(null, null, null);

            TileSearchDto<DatasetTileDto> tileSearchDto = new TileSearchDto<DatasetTileDto>()
            {
                PageNumber = 1,
                PageSize = 1,
                SearchText = "Dataset",
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.Dataset.SECURED,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "False",
                                Selected = true
                            }
                        }
                    }
                },
                UpdateFilters = true,
                OrderByDescending = true,
                OrderByField = x => x.IsFavorite,
                SearchableTiles = GetSearchableTiles()
            };

            TileSearchResultDto<DatasetTileDto> result = searchService.SearchTiles(tileSearchDto);

            Assert.AreEqual(2, result.TotalResults);
            Assert.AreEqual(1, result.PageSize);
            Assert.AreEqual(1, result.PageNumber);
            Assert.AreEqual(1, result.Tiles.Count);
            Assert.AreEqual(8, result.FilterCategories.Count);

            FilterCategoryDto filterCategory = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.CATEGORY);
            Assert.AreEqual(2, filterCategory.CategoryOptions.Count);

            FilterCategoryOptionDto option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("Category1", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(2, option.ResultCount);

            option = filterCategory.CategoryOptions.Last();
            Assert.AreEqual("Category2", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(1, option.ResultCount);

            filterCategory = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.SECURED);
            Assert.AreEqual(2, filterCategory.CategoryOptions.Count);

            option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("True", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            option = filterCategory.CategoryOptions.Last();
            Assert.AreEqual("False", option.OptionValue);
            Assert.IsTrue(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            filterCategory = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.FAVORITE);
            Assert.AreEqual(2, filterCategory.CategoryOptions.Count);

            option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("True", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            option = filterCategory.CategoryOptions.Last();
            Assert.AreEqual("False", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            filterCategory = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.ORIGIN);
            Assert.AreEqual(2, filterCategory.CategoryOptions.Count);

            option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("Internal", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            option = filterCategory.CategoryOptions.Last();
            Assert.AreEqual("External", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            filterCategory = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.ENVIRONMENT);
            Assert.AreEqual(3, filterCategory.CategoryOptions.Count);

            option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("PROD", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            option = filterCategory.CategoryOptions[1];
            Assert.AreEqual("TEST", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            option = filterCategory.CategoryOptions.Last();
            Assert.AreEqual("DEV", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            filterCategory = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.ENVIRONMENTTYPE);
            Assert.AreEqual(2, filterCategory.CategoryOptions.Count);

            option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("Prod", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            option = filterCategory.CategoryOptions.Last();
            Assert.AreEqual("NonProd", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            filterCategory = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.DATASETASSET);
            Assert.AreEqual(2, filterCategory.CategoryOptions.Count);

            option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("SAID", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            option = filterCategory.CategoryOptions.Last();
            Assert.AreEqual("SAID2", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            filterCategory = result.FilterCategories.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.PRODUCERASSET);
            Assert.AreEqual(2, filterCategory.CategoryOptions.Count);

            option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("SAID", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            option = filterCategory.CategoryOptions.Last();
            Assert.AreEqual("SAID2", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            DatasetTileDto tileDto = result.Tiles.First();
            Assert.AreEqual("Dataset3", tileDto.Name);
            Assert.IsTrue(tileDto.IsFavorite);
            Assert.AreEqual(new DateTime(2022, 8, 15), tileDto.LastActivityDateTime);
        }

        [TestMethod]
        public void SearchTiles_ThrowsException()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Throws(new Exception("No bueno!"));

            DatasetTileSearchService searchService = new DatasetTileSearchService(datasetContext.Object, null, null);

            TileSearchDto<DatasetTileDto> tileSearchDto = new TileSearchDto<DatasetTileDto>();

            TileSearchResultDto<DatasetTileDto> result = searchService.SearchTiles(tileSearchDto);

            Assert.AreEqual(0, result.Tiles.Count);
        }

        #region Private
        private DatasetTileSearchService GetSearchService(MockRepository repository, bool isAdmin)
        {
            List<Dataset> datasets = new List<Dataset>()
            {
                GetDataset(1, "Dataset1", "Category1", "000000", true, 5, ObjectStatusEnum.Active),
                GetDataset(2, "Dataset2", "Category2", "000001", false, 8, ObjectStatusEnum.Active),
                GetDataset(3, "Dataset3", "Category1", "000000", false, 12, ObjectStatusEnum.Active),
                GetDataset(4, "Foo Bar", "Category1", "000000", false, 20, ObjectStatusEnum.Active),
                GetDataset(5, "Pending Delete", "Category3", "000002", false, 21, ObjectStatusEnum.Pending_Delete),
                new Dataset()
                {
                    DatasetId = 98,
                    DatasetType = DataEntityCodes.REPORT,
                    ObjectStatus = ObjectStatusEnum.Active
                },
                new Dataset()
                {
                    DatasetId = 99,
                    DatasetType = DataEntityCodes.DATASET,
                    ObjectStatus = ObjectStatusEnum.Deleted
                }
            };

            List<DatasetFile> files = new List<DatasetFile>()
            {
                new DatasetFile()
                {
                    Dataset = datasets.First(),
                    CreatedDTM = new DateTime(2022, 8, 8)
                },
                new DatasetFile()
                {
                    Dataset = datasets.First(),
                    CreatedDTM = new DateTime(2022, 7, 8)
                },
                new DatasetFile()
                {
                    Dataset = datasets[2],
                    CreatedDTM = new DateTime(2022, 8, 10)
                }
            };

            List<DataFlow> dataFlows = new List<DataFlow>()
            {
                new DataFlow()
                {
                    ObjectStatus = ObjectStatusEnum.Active,
                    DatasetId = 1,
                    SaidKeyCode = "SAID"
                },
                new DataFlow()
                {
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DatasetId = 1,
                    SaidKeyCode = "SAID2"
                },
                new DataFlow()
                {
                    ObjectStatus = ObjectStatusEnum.Active,
                    DatasetId = 2,
                    SaidKeyCode = "SAID"
                },
                new DataFlow()
                {
                    ObjectStatus = ObjectStatusEnum.Active,
                    DatasetId = 2,
                    SaidKeyCode = "SAID3"
                },
                new DataFlow()
                {
                    ObjectStatus = ObjectStatusEnum.Active,
                    DatasetId = 3,
                    SaidKeyCode = "SAID3"
                }
            };

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig>().AsQueryable());
            datasetContext.SetupGet(x => x.Security).Returns(new List<Security>().AsQueryable());
            datasetContext.SetupGet(x => x.SecurityTicket).Returns(new List<SecurityTicket>().AsQueryable());
            datasetContext.SetupGet(x => x.SecurityTicket).Returns(new List<SecurityTicket>().AsQueryable());
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());
            datasetContext.SetupGet(x => x.DatasetFileStatusActive).Returns(files.AsQueryable());
            datasetContext.SetupGet(x => x.DataFlow).Returns(dataFlows.AsQueryable());

            Mock<IApplicationUser> appUser = repository.Create<IApplicationUser>();
            appUser.SetupGet(x => x.AssociateId).Returns("000000");
            appUser.SetupGet(x => x.IsAdmin).Returns(isAdmin);

            Mock<IUserService> userService = repository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object);

            return new DatasetTileSearchService(datasetContext.Object, userService.Object, null);
        }

        private Dataset GetDataset(int id, string name, string category, string favoriteId, bool isSecured, int changeDay, ObjectStatusEnum status)
        {
            return new Dataset()
            {
                DatasetId = id,
                DatasetType = DataEntityCodes.DATASET,
                Security = new Security(),
                DatasetName = name,
                ObjectStatus = status,
                IsSecured = isSecured,
                ChangedDtm = new DateTime(2022, 8, changeDay),
                DatasetCategories = new List<Category>()
                {
                    new Category()
                    {
                        Name = category
                    }
                },
                Favorities = new List<Favorite>()
                {
                    new Favorite()
                    {
                        UserId = favoriteId
                    }
                }
            };
        }

        private List<DatasetTileDto> GetSearchableTiles()
        {
            return new List<DatasetTileDto>()
            {
                GetDatasetTileDto(1, "Dataset1", "Category1", true, true, 5, "SAID", "Internal", "PROD"),
                GetDatasetTileDto(2, "Dataset2", "Category2", false, false, 8, "SAID2", "External", "TEST"),
                GetDatasetTileDto(3, "Dataset3", "Category1", true, false, 12, "SAID", "External", "DEV"),
                GetDatasetTileDto(4, "Foo Bar", "Category1", true, false, 20, "SAID3", "Internal", "TEST")
            };
        }

        private DatasetTileDto GetDatasetTileDto(int id, string name, string category, bool isFavorite, bool isSecured, int createDay, string asset, string origin, string env)
        {
            return new DatasetTileDto()
            {
                Id = id.ToString(),
                Name = name,
                IsFavorite = isFavorite,
                Category = category,
                IsSecured = isSecured,
                LastActivityDateTime = new DateTime(2022, 8, createDay + 3),
                DatasetAsset = asset,
                ProducerAssets = new List<string>() { asset },
                OriginationCode = origin,
                Environment = env,
                EnvironmentType = env.Contains("PROD") ? NamedEnvironmentType.Prod.GetDescription() : NamedEnvironmentType.NonProd.GetDescription()
            };
        }
        #endregion
    }
}
