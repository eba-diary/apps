using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Web.API;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class GetGlobalDatasetFiltersModelMappingTests : BaseModelMappingTests
    {
        [TestMethod]
        public void Map_GetGlobalDatasetFiltersRequestModel_SearchGlobalDatasetsDto()
        {
            GetGlobalDatasetFiltersRequestModel request = new GetGlobalDatasetFiltersRequestModel
            {
                SearchText = "search text",
                FilterCategories = new List<FilterCategoryRequestModel>
                {
                    new FilterCategoryRequestModel
                    {
                        CategoryName = "Category1",
                        CategoryOptions = new List<FilterCategoryOptionRequestModel>
                        {
                            new FilterCategoryOptionRequestModel
                            {
                                OptionValue = "Option1"
                            },
                            new FilterCategoryOptionRequestModel
                            {
                                OptionValue = "Option2"
                            }
                        }
                    },
                    new FilterCategoryRequestModel
                    {
                        CategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionRequestModel>
                        {
                            new FilterCategoryOptionRequestModel
                            {
                                OptionValue = "Prod"
                            }
                        }
                    }
                },
                ShouldSearchColumns = true
            };

            GetGlobalDatasetFiltersDto dto = _mapper.Map<GetGlobalDatasetFiltersDto>(request);

            Assert.AreEqual("search text", dto.SearchText);
            Assert.IsTrue(dto.ShouldSearchColumns);
            Assert.AreEqual(2, dto.FilterCategories.Count);

            FilterCategoryDto categoryDto = dto.FilterCategories[0];
            Assert.AreEqual("Category1", categoryDto.CategoryName);
            Assert.IsFalse(categoryDto.DefaultCategoryOpen);
            Assert.IsFalse(categoryDto.HideResultCounts);
            Assert.AreEqual(2, categoryDto.CategoryOptions.Count);

            FilterCategoryOptionDto optionDto = categoryDto.CategoryOptions[0];
            Assert.AreEqual("Option1", optionDto.OptionValue);
            Assert.AreEqual(0, optionDto.ResultCount);
            Assert.AreEqual("Category1", optionDto.ParentCategoryName);
            Assert.IsTrue(optionDto.Selected);

            optionDto = categoryDto.CategoryOptions[1];
            Assert.AreEqual("Option2", optionDto.OptionValue);
            Assert.AreEqual(0, optionDto.ResultCount);
            Assert.AreEqual("Category1", optionDto.ParentCategoryName);
            Assert.IsTrue(optionDto.Selected);

            categoryDto = dto.FilterCategories[1];
            Assert.AreEqual(FilterCategoryNames.DataInventory.ENVIRONMENT, categoryDto.CategoryName);
            Assert.IsFalse(categoryDto.DefaultCategoryOpen);
            Assert.IsFalse(categoryDto.HideResultCounts);
            Assert.AreEqual(1, categoryDto.CategoryOptions.Count);

            optionDto = categoryDto.CategoryOptions[0];
            Assert.AreEqual("P", optionDto.OptionValue);
            Assert.AreEqual(0, optionDto.ResultCount);
            Assert.AreEqual(FilterCategoryNames.DataInventory.ENVIRONMENT, optionDto.ParentCategoryName);
            Assert.IsTrue(optionDto.Selected);
        }

        [TestMethod]
        public void Map_GetGlobalDatasetFiltersResultDto_GetGlobalDatasetFiltersResponseModel()
        {
            GetGlobalDatasetFiltersResultDto dto = new GetGlobalDatasetFiltersResultDto
            {
                FilterCategories = new List<FilterCategoryDto>
                {
                    new FilterCategoryDto
                    {
                        CategoryName = "Category1",
                        HideResultCounts = true,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "Option1",
                                Selected = true,
                                ResultCount = 2,
                                ParentCategoryName = "Category1"
                            },
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "Option2",
                                Selected = false,
                                ResultCount = 8,
                                ParentCategoryName = "Category1"
                            }
                        }
                    },
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT,
                        DefaultCategoryOpen = true,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "P",
                                Selected = true,
                                ResultCount = 10,
                                ParentCategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT
                            }
                        }
                    }
                }
            };

            GetGlobalDatasetFiltersResponseModel model = _mapper.Map<GetGlobalDatasetFiltersResponseModel>(dto);

            Assert.AreEqual(2, model.FilterCategories.Count);

            FilterCategoryResponseModel category = model.FilterCategories[0];
            Assert.AreEqual("Category1", category.CategoryName);
            Assert.IsFalse(category.DefaultCategoryOpen);
            Assert.IsTrue(category.HideResultCounts);
            Assert.AreEqual(2, category.CategoryOptions.Count);

            FilterCategoryOptionResponseModel option = category.CategoryOptions[0];
            Assert.AreEqual("Option1", option.OptionValue);
            Assert.AreEqual(2, option.ResultCount);
            Assert.AreEqual("Category1", option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            option = category.CategoryOptions[1];
            Assert.AreEqual("Option2", option.OptionValue);
            Assert.AreEqual(8, option.ResultCount);
            Assert.AreEqual("Category1", option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            category = model.FilterCategories[1];
            Assert.AreEqual(FilterCategoryNames.DataInventory.ENVIRONMENT, category.CategoryName);
            Assert.IsTrue(category.DefaultCategoryOpen);
            Assert.IsFalse(category.HideResultCounts);
            Assert.AreEqual(1, category.CategoryOptions.Count);

            option = category.CategoryOptions[0];
            Assert.AreEqual("Prod", option.OptionValue);
            Assert.AreEqual(10, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.DataInventory.ENVIRONMENT, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);
        }
    }
}
