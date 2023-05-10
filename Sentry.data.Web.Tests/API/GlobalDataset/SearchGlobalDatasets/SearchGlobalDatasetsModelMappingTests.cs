using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Web.API;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class SearchGlobalDatasetsModelMappingTests : BaseModelMappingTests
    {
        [TestMethod]
        public void Map_SearchGlobalDatasetsRequestModel_SearchGlobalDatasetsDto()
        {
            SearchGlobalDatasetsRequestModel request = new SearchGlobalDatasetsRequestModel
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
                ShouldSearchColumns = true,
            };

            SearchGlobalDatasetsDto dto = _mapper.Map<SearchGlobalDatasetsDto>(request);

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
        public void Map_SearchGlobalDatasetsResultDto_SearchGlobalDatasetsResponseModel()
        {
            SearchGlobalDatasetsResultsDto dto = new SearchGlobalDatasetsResultsDto
            {
                GlobalDatasets = new List<SearchGlobalDatasetDto>
                {
                    new SearchGlobalDatasetDto
                    {
                        GlobalDatasetId = 1,
                        DatasetName = "Name",
                        DatasetDescription = "Description",
                        DatasetSaidAssetCode = "SAID",
                        CategoryCode = "Category",
                        NamedEnvironments = new List<string> { "DEV" },
                        IsSecured = true,
                        IsFavorite = true,
                        TargetDatasetId = 2,
                        SearchHighlights = new List<SearchHighlightDto>
                        {
                            new SearchHighlightDto
                            {
                                PropertyName = SearchDisplayNames.GlobalDataset.DATASETNAME,
                                Highlights = new List<string> { "value", "value2" }
                            },
                            new SearchHighlightDto
                            {
                                PropertyName = SearchDisplayNames.GlobalDataset.SCHEMANAME,
                                Highlights = new List<string> { "value" }
                            }
                        }
                    },
                    new SearchGlobalDatasetDto
                    {
                        GlobalDatasetId = 2,
                        DatasetName = "Name 2",
                        DatasetDescription = "Description 2",
                        DatasetSaidAssetCode = "DATA",
                        CategoryCode = "Category",
                        NamedEnvironments = new List<string> { "DEV", "TEST" },
                        IsSecured = false,
                        IsFavorite = false,
                        TargetDatasetId = 1,
                        SearchHighlights = new List<SearchHighlightDto>()
                    }
                }
            };

            SearchGlobalDatasetsResponseModel model = _mapper.Map<SearchGlobalDatasetsResponseModel>(dto);

            Assert.AreEqual(2, model.GlobalDatasets.Count);

            SearchGlobalDatasetResponseModel globalDataset = model.GlobalDatasets[0];
            Assert.AreEqual(1, globalDataset.GlobalDatasetId);
            Assert.AreEqual("Name", globalDataset.DatasetName);
            Assert.AreEqual("Description", globalDataset.DatasetDescription);
            Assert.AreEqual("SAID", globalDataset.DatasetSaidAssetCode);
            Assert.AreEqual("Category", globalDataset.CategoryCode);
            Assert.AreEqual(1, globalDataset.NamedEnvironments.Count);
            Assert.AreEqual("DEV", globalDataset.NamedEnvironments.First());
            Assert.IsTrue(globalDataset.IsSecured);
            Assert.IsTrue(globalDataset.IsFavorite);
            Assert.AreEqual(2, globalDataset.TargetDatasetId);
            Assert.AreEqual(2, globalDataset.SearchHighlights.Count);

            SearchHighlightModel searchHighlight = globalDataset.SearchHighlights[0];
            Assert.AreEqual(SearchDisplayNames.GlobalDataset.DATASETNAME, searchHighlight.PropertyName);
            Assert.AreEqual(2, searchHighlight.Highlights.Count);
            Assert.AreEqual("value", searchHighlight.Highlights[0]);
            Assert.AreEqual("value2", searchHighlight.Highlights[1]);

            searchHighlight = globalDataset.SearchHighlights[1];
            Assert.AreEqual(SearchDisplayNames.GlobalDataset.SCHEMANAME, searchHighlight.PropertyName);
            Assert.AreEqual(1, searchHighlight.Highlights.Count);
            Assert.AreEqual("value", searchHighlight.Highlights[0]);

            globalDataset = model.GlobalDatasets[1];
            Assert.AreEqual(2, globalDataset.GlobalDatasetId);
            Assert.AreEqual("Name 2", globalDataset.DatasetName);
            Assert.AreEqual("Description 2", globalDataset.DatasetDescription);
            Assert.AreEqual("DATA", globalDataset.DatasetSaidAssetCode);
            Assert.AreEqual("Category", globalDataset.CategoryCode);
            Assert.AreEqual(2, globalDataset.NamedEnvironments.Count);
            Assert.AreEqual("DEV", globalDataset.NamedEnvironments.First());
            Assert.AreEqual("TEST", globalDataset.NamedEnvironments.Last());
            Assert.IsFalse(globalDataset.IsSecured);
            Assert.IsFalse(globalDataset.IsFavorite);
            Assert.AreEqual(1, globalDataset.TargetDatasetId);
            Assert.AreEqual(0, globalDataset.SearchHighlights.Count);
        }
    }
}
