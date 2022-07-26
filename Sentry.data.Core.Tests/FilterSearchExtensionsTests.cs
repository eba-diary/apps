using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

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

        #region Helpers
        private string GetFilterCategoriesJson()
        {
            return @"[{""CategoryName"":""Environment"",""CategoryOptions"":[{""OptionValue"":""P"",""ResultCount"":0,""ParentCategoryName"":""Environment"",""Selected"":true}]}]";
        }

        private string GetResultConfigurationJson()
        {
            return @"{""VisibleColumns"":[1,2,3]}";
        }
        #endregion
    }
}
