using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using StructureMap.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.Tests.Extensions
{
    [TestClass]
    public class FilterSearchExtensionsTests
    {
        [TestMethod]
        public void ToDto_FilterSearchModel_FilterSearchDto()
        {
            FilterSearchModel model = new FilterSearchModel()
            {
                SearchName = "SearchName",
                SearchText = "Search",
                FilterCategories = new List<FilterCategoryModel>()
                {
                    new FilterCategoryModel()
                    {
                        CategoryName = "Category",
                        CategoryOptions = new List<FilterCategoryOptionModel>()
                        {
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = "Value",
                                ResultCount = 10,
                                ParentCategoryName = "Category",
                                Selected = true
                            },
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = "Value2",
                                ResultCount = 4,
                                ParentCategoryName = "Category",
                                Selected = false
                            }
                        }
                    },
                    new FilterCategoryModel()
                    {
                        CategoryName = "Category2",
                        CategoryOptions = new List<FilterCategoryOptionModel>()
                        {
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = "Value",
                                ResultCount = 5,
                                ParentCategoryName = "Category2",
                                Selected = false
                            },
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = "Value2",
                                ResultCount = 9,
                                ParentCategoryName = "Category2",
                                Selected = false
                            }
                        }
                    }
                }
            };

            FilterSearchDto dto = model.ToDto();

            Assert.AreEqual("SearchName", dto.SearchName);
            Assert.AreEqual("Search", dto.SearchText);
            Assert.AreEqual(2, dto.FilterCategories.Count);

            FilterCategoryDto categoryDto = dto.FilterCategories.First();
            Assert.AreEqual("Category", categoryDto.CategoryName);
            Assert.AreEqual(2, categoryDto.CategoryOptions.Count);

            FilterCategoryOptionDto optionDto = categoryDto.CategoryOptions.First();
            Assert.AreEqual("Value", optionDto.OptionValue);
            Assert.AreEqual(10, optionDto.ResultCount);
            Assert.AreEqual("Category", optionDto.ParentCategoryName);
            Assert.IsTrue(optionDto.Selected);

            optionDto = categoryDto.CategoryOptions.Last();
            Assert.AreEqual("Value2", optionDto.OptionValue);
            Assert.AreEqual(4, optionDto.ResultCount);
            Assert.AreEqual("Category", optionDto.ParentCategoryName);
            Assert.IsFalse(optionDto.Selected);

            categoryDto = dto.FilterCategories.Last();
            Assert.AreEqual("Category2", categoryDto.CategoryName);
            Assert.AreEqual(2, categoryDto.CategoryOptions.Count);

            optionDto = categoryDto.CategoryOptions.First();
            Assert.AreEqual("Value", optionDto.OptionValue);
            Assert.AreEqual(5, optionDto.ResultCount);
            Assert.AreEqual("Category2", optionDto.ParentCategoryName);
            Assert.IsFalse(optionDto.Selected);

            optionDto = categoryDto.CategoryOptions.Last();
            Assert.AreEqual("Value2", optionDto.OptionValue);
            Assert.AreEqual(9, optionDto.ResultCount);
            Assert.AreEqual("Category2", optionDto.ParentCategoryName);
            Assert.IsFalse(optionDto.Selected);
        }

        [TestMethod]
        public void ToModel_FilterSearchDto_FilterSearchModel()
        {
            FilterSearchDto dto = new FilterSearchDto()
            {
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = "Category",
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "Value",
                                ResultCount = 10,
                                ParentCategoryName = "Category",
                                Selected = true
                            },
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "Value2",
                                ResultCount = 4,
                                ParentCategoryName = "Category",
                                Selected = false
                            }
                        }
                    },
                    new FilterCategoryDto()
                    {
                        CategoryName = "Category2",
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "Value",
                                ResultCount = 5,
                                ParentCategoryName = "Category2",
                                Selected = false
                            },
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "Value2",
                                ResultCount = 9,
                                ParentCategoryName = "Category2",
                                Selected = false
                            }
                        }
                    }
                }
            };

            FilterSearchModel model = dto.ToModel();

            Assert.AreEqual(2, model.FilterCategories.Count);

            FilterCategoryModel categoryModel = model.FilterCategories.First();
            Assert.AreEqual("Category", categoryModel.CategoryName);
            Assert.AreEqual(2, categoryModel.CategoryOptions.Count);

            FilterCategoryOptionModel optionModel = categoryModel.CategoryOptions.First();
            Assert.AreEqual("Value", optionModel.OptionValue);
            Assert.AreEqual(10, optionModel.ResultCount);
            Assert.AreEqual("Category", optionModel.ParentCategoryName);
            Assert.IsTrue(optionModel.Selected);

            optionModel = categoryModel.CategoryOptions.Last();
            Assert.AreEqual("Value2", optionModel.OptionValue);
            Assert.AreEqual(4, optionModel.ResultCount);
            Assert.AreEqual("Category", optionModel.ParentCategoryName);
            Assert.IsFalse(optionModel.Selected);

            categoryModel = model.FilterCategories.Last();
            Assert.AreEqual("Category2", categoryModel.CategoryName);
            Assert.AreEqual(2, categoryModel.CategoryOptions.Count);

            optionModel = categoryModel.CategoryOptions.First();
            Assert.AreEqual("Value", optionModel.OptionValue);
            Assert.AreEqual(5, optionModel.ResultCount);
            Assert.AreEqual("Category2", optionModel.ParentCategoryName);
            Assert.IsFalse(optionModel.Selected);

            optionModel = categoryModel.CategoryOptions.Last();
            Assert.AreEqual("Value2", optionModel.OptionValue);
            Assert.AreEqual(9, optionModel.ResultCount);
            Assert.AreEqual("Category2", optionModel.ParentCategoryName);
            Assert.IsFalse(optionModel.Selected);
        }

        [TestMethod]
        public void ToDto_SaveSearchModel_SavedSearchDto()
        {
            SaveSearchModel model = new SaveSearchModel()
            {
                SearchType = GlobalConstants.SearchType.DATA_INVENTORY,
                SearchName = "Search",
                AddToFavorites = true,
                FilterCategories = new List<FilterCategoryModel>()
                {
                    new FilterCategoryModel()
                    {
                        CategoryName = "Category",
                        CategoryOptions = new List<FilterCategoryOptionModel>()
                        {
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = "Value",
                                ResultCount = 10,
                                ParentCategoryName = "Category",
                                Selected = true
                            },
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = "Value2",
                                ResultCount = 4,
                                ParentCategoryName = "Category",
                                Selected = false
                            }
                        }
                    }
                },
                ResultConfigurationJson = @"{""VisibleColumns"": [1,2,3]}"
            };

            SavedSearchDto dto = model.ToDto();

            Assert.AreEqual(GlobalConstants.SearchType.DATA_INVENTORY, dto.SearchType);
            Assert.AreEqual("Search", dto.SearchName);
            Assert.IsTrue(dto.AddToFavorites);
            Assert.AreEqual(1, dto.FilterCategories.Count);

            FilterCategoryDto categoryDto = dto.FilterCategories.First();
            Assert.AreEqual("Category", categoryDto.CategoryName);
            Assert.AreEqual(2, categoryDto.CategoryOptions.Count);

            FilterCategoryOptionDto optionDto = categoryDto.CategoryOptions.First();
            Assert.AreEqual("Value", optionDto.OptionValue);
            Assert.AreEqual(10, optionDto.ResultCount);
            Assert.AreEqual("Category", optionDto.ParentCategoryName);
            Assert.IsTrue(optionDto.Selected);

            optionDto = categoryDto.CategoryOptions.Last();
            Assert.AreEqual("Value2", optionDto.OptionValue);
            Assert.AreEqual(4, optionDto.ResultCount);
            Assert.AreEqual("Category", optionDto.ParentCategoryName);
            Assert.IsFalse(optionDto.Selected);

            Assert.IsTrue(dto.ResultConfiguration.ContainsKey("VisibleColumns"));

            List<int> visibleColumns = dto.ResultConfiguration["VisibleColumns"].ToObject<List<int>>();
            Assert.AreEqual(3, visibleColumns.Count);
        }

        [TestMethod]
        public void ToModel_SavedSearchOptionDto_SavedSearchOptionModel()
        {
            SavedSearchOptionDto dto = new SavedSearchOptionDto()
            {
                SavedSearchId = 1,
                SavedSearchName = "SearchName",
                SavedSearchUrl = "Url",
                IsFavorite = true
            };

            SavedSearchOptionModel model = dto.ToModel();

            Assert.AreEqual(1, model.SavedSearchId);
            Assert.AreEqual("SearchName", model.SavedSearchName);
            Assert.AreEqual("Url", model.SavedSearchUrl);
            Assert.IsTrue(model.IsFavorite);
        }

        [TestMethod]
        public void ToDto_TileSearchModel_TileSearchDto_DatasetTileDto()
        {
            TileSearchModel model = GetTileSearchModel();

            DateTime now = DateTime.Now;
            DatasetTileDto datasetTileDto = new DatasetTileDto()
            {
                LastActivityDateTime = now
            };

            TileSearchDto<DatasetTileDto> dto = model.ToDto<DatasetTileDto>();

            Assert.AreEqual("SearchName", dto.SearchName);
            Assert.AreEqual("SearchText", dto.SearchText);
            Assert.AreEqual(1, dto.FilterCategories.Count);
            Assert.AreEqual(2, dto.PageNumber);
            Assert.AreEqual(15, dto.PageSize);
            Assert.IsTrue(dto.UpdateFilters);
            Assert.IsTrue(dto.OrderByDescending);
            Assert.AreEqual(now, dto.OrderByField.Invoke(datasetTileDto));
        }

        [TestMethod]
        public void ToDto_TileSearchModel_TileSearchDto_SortByAlphabetical_DatasetTileDto()
        {
            TileSearchModel model = GetTileSearchModel();
            model.SortBy = 0;

            DatasetTileDto datasetTileDto = new DatasetTileDto() { Name = "SearchName" };

            TileSearchDto<DatasetTileDto> dto = model.ToDto<DatasetTileDto>();

            Assert.IsFalse(dto.OrderByDescending);
            Assert.AreEqual("SearchName", dto.OrderByField.Invoke(datasetTileDto));
        }

        [TestMethod]
        public void ToDto_TileSearchModel_TileSearchDto_SortByFavorite_DatasetTileDto()
        {
            TileSearchModel model = GetTileSearchModel();
            model.SortBy = 1;

            DatasetTileDto datasetTileDto = new DatasetTileDto() { IsFavorite = true };

            TileSearchDto<DatasetTileDto> dto = model.ToDto<DatasetTileDto>();

            Assert.IsTrue(dto.OrderByDescending);
            Assert.IsTrue((bool)dto.OrderByField.Invoke(datasetTileDto));
        }

        [TestMethod]
        public void ToEventDto_TileSearchModel_TileSearchEventDto()
        {
            TileSearchModel model = GetTileSearchModel();

            TileSearchEventDto dto = model.ToEventDto(20);

            Assert.AreEqual("SearchName", dto.SearchName);
            Assert.AreEqual("SearchText", dto.SearchText);
            Assert.AreEqual(1, dto.FilterCategories.Count);
            Assert.AreEqual(2, dto.PageNumber);
            Assert.AreEqual(15, dto.PageSize);
            Assert.AreEqual(2, dto.SortBy);
            Assert.AreEqual(1, dto.Layout);
            Assert.AreEqual(20, dto.TotalResults);
        }

        [TestMethod]
        public void ToDatasetTileDtos_TileModels_DatasetTileDtos()
        {
            List<TileModel> models = new List<TileModel>()
            {
                new TileModel()
                {
                    Id = "1",
                    Name = "TileName",
                    Description = "TileDescription",
                    Status = "Active",
                    IsFavorite = true,
                    Category = "Category",
                    AbbreviatedCategory = "Cat",
                    Color = "Blue",
                    IsSecured = false,
                    LastActivityShortDate = "8/18/2022",
                    OriginationCode = "Origin",
                    Environment = "DEV",
                    EnvironmentType = "NonProd"
                },
                new TileModel()
                {
                    Id = "2",
                    Name = "TileName2",
                    Description = "TileDescription2",
                    Status = "Deleted",
                    IsFavorite = false,
                    Category = "Category2",
                    AbbreviatedCategory = "Cat2",
                    Color = "Green",
                    IsSecured = true,
                    LastActivityShortDate = "8/18/2021",
                    OriginationCode = "Origin2",
                    Environment = "PROD",
                    EnvironmentType = "Prod"
                }
            };

            List<DatasetTileDto> dtos = models.ToDatasetTileDtos();

            Assert.AreEqual(2, dtos.Count);

            DatasetTileDto dto = dtos.First();
            Assert.AreEqual("1", dto.Id);
            Assert.AreEqual("TileName", dto.Name);
            Assert.AreEqual("TileDescription", dto.Description);
            Assert.AreEqual(ObjectStatusEnum.Active, dto.Status);
            Assert.IsTrue(dto.IsFavorite);
            Assert.AreEqual("Category", dto.Category);
            Assert.AreEqual("Cat", dto.AbbreviatedCategory);
            Assert.AreEqual("Blue", dto.Color);
            Assert.IsFalse(dto.IsSecured);
            Assert.AreEqual(DateTime.Parse("8/18/2022"), dto.LastActivityDateTime);
            Assert.AreEqual("Origin", dto.OriginationCode);
            Assert.AreEqual("DEV", dto.Environment);
            Assert.AreEqual("NonProd", dto.EnvironmentType);

            dto = dtos.Last();
            Assert.AreEqual("2", dto.Id);
            Assert.AreEqual("TileName2", dto.Name);
            Assert.AreEqual("TileDescription2", dto.Description);
            Assert.AreEqual(ObjectStatusEnum.Deleted, dto.Status);
            Assert.IsFalse(dto.IsFavorite);
            Assert.AreEqual("Category2", dto.Category);
            Assert.AreEqual("Cat2", dto.AbbreviatedCategory);
            Assert.AreEqual("Green", dto.Color);
            Assert.IsTrue(dto.IsSecured);
            Assert.AreEqual(DateTime.Parse("8/18/2021"), dto.LastActivityDateTime);
            Assert.AreEqual("Origin2", dto.OriginationCode);
            Assert.AreEqual("PROD", dto.Environment);
            Assert.AreEqual("Prod", dto.EnvironmentType);
        }

        private TileSearchModel GetTileSearchModel()
        {
            return new TileSearchModel()
            {
                SearchName = "SearchName",
                SearchText = "SearchText",
                FilterCategories = new List<FilterCategoryModel>() { new FilterCategoryModel() },
                PageNumber = 2,
                PageSize = 15,
                SortBy = 2,
                Layout = 1,
                UpdateFilters = true
            };
        }
    }
}
