using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class FilterSearchExtensionsTests
    {
        [TestMethod]
        public void ToDto_SavedSearch_SavedSearchDto()
        {
            SavedSearch savedSearch = new SavedSearch()
            {
                SearchType = "SearchType",
                SearchName = "SearchName",
                SearchText = "SearchText",
                AssociateId = "000000",
                FilterCategoriesJson = GetFilterCategoriesJson(),
                ResultConfigurationJson = GetResultConfigurationJson()
            };

            SavedSearchDto dto = savedSearch.ToDto();

            Assert.AreEqual("SearchType", dto.SearchType);
            Assert.AreEqual("SearchName", dto.SearchName);
            Assert.AreEqual("SearchText", dto.SearchText);
            Assert.AreEqual("000000", dto.AssociateId);
            Assert.AreEqual(1, dto.FilterCategories.Count);

            FilterCategoryDto categoryDto = dto.FilterCategories.First();
            Assert.AreEqual(GlobalConstants.FilterCategoryNames.DataInventory.ENVIRONMENT, categoryDto.CategoryName);
            Assert.AreEqual(1, categoryDto.CategoryOptions.Count);

            FilterCategoryOptionDto optionDto = categoryDto.CategoryOptions.First();
            Assert.AreEqual(GlobalConstants.FilterCategoryOptions.ENVIRONMENT_PROD, optionDto.OptionValue);
            Assert.AreEqual(GlobalConstants.FilterCategoryNames.DataInventory.ENVIRONMENT, optionDto.ParentCategoryName);
            Assert.IsTrue(optionDto.Selected);
            Assert.AreEqual(0, optionDto.ResultCount);

            Assert.IsTrue(dto.ResultConfiguration.ContainsKey("VisibleColumns"));

            List<int> visibleColumns = dto.ResultConfiguration["VisibleColumns"].ToObject<List<int>>();
            Assert.AreEqual(3, visibleColumns.Count);
        }

        [TestMethod]
        public void ToEntity_SavedSearchDto_SavedSearch()
        {
            SavedSearchDto dto = new SavedSearchDto()
            {
                SearchType = "SearchType",
                SearchName = "SearchName",
                SearchText = "SearchText",
                AssociateId = "000000",
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

            SavedSearch savedSearch = dto.ToEntity();

            Assert.AreEqual("SearchType", savedSearch.SearchType);
            Assert.AreEqual("SearchName", savedSearch.SearchName);
            Assert.AreEqual("SearchText", savedSearch.SearchText);
            Assert.AreEqual("000000", savedSearch.AssociateId);
            Assert.AreEqual(GetFilterCategoriesJson(), savedSearch.FilterCategoriesJson);
            Assert.AreEqual(GetResultConfigurationJson(), savedSearch.ResultConfigurationJson);
        }

        [TestMethod]
        public void FilterBy_FilterCategoryDtos_DatasetTileDtos()
        {
            List<FilterCategoryDto> filters = GetFilterCategories();

            List<DatasetTileDto> dtos = GetDatasetTileDtosToFilter();

            List<DatasetTileDto> results = dtos.FilterBy(filters);

            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results.Any(x => x.Id == "1"));
            Assert.IsTrue(results.Any(x => x.Id == "2"));
            Assert.IsTrue(results.Any(x => x.Id == "5"));
        }

        [TestMethod]
        public void CreateFilters_DatasetTileDtos_FilterCategoryDtos()
        {
            List<DatasetTileDto> dtos = GetDatasetTileDtosToCreateFilters();

            List<FilterCategoryDto> searchedFilters = GetFilterCategories();

            List<FilterCategoryDto> filters = dtos.CreateFilters(searchedFilters);

            Assert.AreEqual(3, filters.Count);

            FilterCategoryDto filterCategory = filters.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.CATEGORY);
            Assert.AreEqual(3, filterCategory.CategoryOptions.Count);

            FilterCategoryOptionDto option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("Category", option.OptionValue);
            Assert.IsTrue(option.Selected);
            Assert.AreEqual(2, option.ResultCount);

            option = filterCategory.CategoryOptions[1];
            Assert.AreEqual("Category3", option.OptionValue);
            Assert.IsTrue(option.Selected);
            Assert.AreEqual(1, option.ResultCount);

            option = filterCategory.CategoryOptions.Last();
            Assert.AreEqual("Category4", option.OptionValue);
            Assert.IsTrue(option.Selected);
            Assert.AreEqual(0, option.ResultCount);

            filterCategory = filters.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.FAVORITE);
            Assert.AreEqual(1, filterCategory.CategoryOptions.Count);

            option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("True", option.OptionValue);
            Assert.IsTrue(option.Selected);
            Assert.AreEqual(3, option.ResultCount);

            filterCategory = filters.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.SECURED);
            Assert.AreEqual(2, filterCategory.CategoryOptions.Count);

            option = filterCategory.CategoryOptions.First();
            Assert.AreEqual("True", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(1, option.ResultCount);

            option = filterCategory.CategoryOptions.Last();
            Assert.AreEqual("False", option.OptionValue);
            Assert.IsFalse(option.Selected);
            Assert.AreEqual(2, option.ResultCount);
        }

        [TestMethod]
        public void HasSelectedValueOf_FilterCategoryOptionDtos_True()
        {
            List<FilterCategoryDto> filters = GetFilterCategories();
            List<FilterCategoryOptionDto> options = filters.FirstOrDefault(x => x.CategoryName == FilterCategoryNames.Dataset.CATEGORY).CategoryOptions;

            Assert.IsTrue(options.HasSelectedValueOf("Category3"));
            Assert.IsFalse(options.HasSelectedValueOf("Foo"));
        }

        [TestMethod]
        public void TryGetSlectedOptionsWithNoResultsIn_FilterCategoryOptionDtos_True()
        {
            List<FilterCategoryOptionDto> options = new List<FilterCategoryOptionDto>()
            {
                new FilterCategoryOptionDto()
                {
                    OptionValue = "Option",
                    Selected = true
                },
                new FilterCategoryOptionDto()
                {
                    OptionValue = "Option2",
                    Selected= true
                }
            };

            List<FilterCategoryOptionDto> newOptions = new List<FilterCategoryOptionDto>()
            {
                new FilterCategoryOptionDto()
                {
                    OptionValue = "Option2"
                }
            };

            Assert.IsTrue(options.TryGetSelectedOptionsWithNoResults(newOptions, out List<FilterCategoryOptionDto> results));
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Option", results.First().OptionValue);
        }

        #region Helpers
        private string GetFilterCategoriesJson()
        {
            return @"[{""CategoryName"":""Environment"",""CategoryOptions"":[{""OptionValue"":""P"",""ResultCount"":0,""ParentCategoryName"":""Environment"",""Selected"":true}],""DefaultCategoryOpen"":false,""HideResultCounts"":false}]";
        }

        private string GetResultConfigurationJson()
        {
            return @"{""VisibleColumns"":[1,2,3]}";
        }

        private List<FilterCategoryDto> GetFilterCategories()
        {
            return new List<FilterCategoryDto>()
            {
                new FilterCategoryDto()
                {
                    CategoryName = FilterCategoryNames.Dataset.CATEGORY,
                    CategoryOptions = new List<FilterCategoryOptionDto>()
                    {
                        new FilterCategoryOptionDto()
                        {
                            OptionValue = "Category",
                            ParentCategoryName = FilterCategoryNames.Dataset.CATEGORY,
                            Selected = true
                        },
                        new FilterCategoryOptionDto()
                        {
                            OptionValue = "Category3",
                            ParentCategoryName = FilterCategoryNames.Dataset.CATEGORY,
                            Selected = true
                        },
                        new FilterCategoryOptionDto()
                        {
                            OptionValue = "Category4",
                            ParentCategoryName = FilterCategoryNames.Dataset.CATEGORY,
                            Selected = true
                        }
                    }
                },
                new FilterCategoryDto()
                {
                    CategoryName = FilterCategoryNames.Dataset.FAVORITE,
                    CategoryOptions = new List<FilterCategoryOptionDto>()
                    {
                        new FilterCategoryOptionDto()
                        {
                            OptionValue = "True",
                            ParentCategoryName = FilterCategoryNames.Dataset.FAVORITE,
                            Selected = true
                        }
                    }
                }
            };
        }

        private List<DatasetTileDto> GetDatasetTileDtosToFilter()
        {
            return new List<DatasetTileDto>()
            {
                new DatasetTileDto()
                {
                    Id = "1",
                    Category = "Category",
                    IsFavorite = true,
                    IsSecured = true,
                },
                new DatasetTileDto()
                {
                    Id = "2",
                    Category = "Category",
                    IsFavorite = true,
                    IsSecured = false,
                },
                new DatasetTileDto()
                {
                    Id = "3",
                    Category = "Category2",
                    IsFavorite = true,
                    IsSecured = false,
                },
                new DatasetTileDto()
                {
                    Id = "4",
                    Category = "Category",
                    IsFavorite = false,
                    IsSecured = false,
                },
                new DatasetTileDto()
                {
                    Id = "5",
                    Category = "Category3",
                    IsFavorite = true,
                    IsSecured = false,
                }
            };
        }

        private List<DatasetTileDto> GetDatasetTileDtosToCreateFilters()
        {
            return new List<DatasetTileDto>()
            {
                new DatasetTileDto()
                {
                    Id = "1",
                    Category = "Category",
                    IsFavorite = true,
                    IsSecured = true,
                },
                new DatasetTileDto()
                {
                    Id = "2",
                    Category = "Category",
                    IsFavorite = true,
                    IsSecured = false,
                },
                new DatasetTileDto()
                {
                    Id = "5",
                    Category = "Category3",
                    IsFavorite = true,
                    IsSecured = false,
                }
            };
        }
        #endregion
    }
}
