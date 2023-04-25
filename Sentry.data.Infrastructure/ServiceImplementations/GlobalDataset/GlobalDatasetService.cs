using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;
using System.Web;

namespace Sentry.data.Infrastructure
{
    public class GlobalDatasetService : IGlobalDatasetService
    {
        private readonly IGlobalDatasetProvider _globalDatasetProvider;
        private readonly IUserService _userService;
        private readonly IDataFeatures _dataFeatures;

        public GlobalDatasetService(IGlobalDatasetProvider globalDatasetProvider, IUserService userService, IDataFeatures dataFeatures)
        {
            _globalDatasetProvider = globalDatasetProvider;
            _userService = userService;
            _dataFeatures = dataFeatures;
        }

        public async Task<SearchGlobalDatasetsResultsDto> SearchGlobalDatasetsAsync(SearchGlobalDatasetsDto searchGlobalDatasetsDto)
        {
            if (!_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                throw new ResourceFeatureDisabledException(nameof(_dataFeatures.CLA4789_ImprovedSearchCapability), "SearchGlobalDatasets");
            }

            List<GlobalDataset> globalDatasets = await _globalDatasetProvider.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto);

            string currentUserId = _userService.GetCurrentUser().AssociateId;
            SearchGlobalDatasetsResultsDto resultDto = new SearchGlobalDatasetsResultsDto
            {
                GlobalDatasets = globalDatasets.Select(x => x.ToSearchResult(currentUserId)).ToList()
            };

            return resultDto;
        }

        public async Task<GetGlobalDatasetFiltersResultDto> GetGlobalDatasetFiltersAsync(GetGlobalDatasetFiltersDto getGlobalDatasetFiltersDto)
        {
            if (!_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                throw new ResourceFeatureDisabledException(nameof(_dataFeatures.CLA4789_ImprovedSearchCapability), "SearchGlobalDatasets");
            }

            GetGlobalDatasetFiltersResultDto resultsDto = new GetGlobalDatasetFiltersResultDto
            {
                FilterCategories = await _globalDatasetProvider.GetGlobalDatasetFiltersAsync(getGlobalDatasetFiltersDto)
            };

            return resultsDto;
        }

        public List<FilterCategoryDto> GetInitialFilters(List<string> filters)
        {
            List<FilterCategoryDto> categories = new List<FilterCategoryDto>();

            if (filters != null)
            {
                foreach (string filter in filters)
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        List<string> parts = filter.Split('_').ToList();
                        string category = parts.First();

                        FilterCategoryOptionDto optionModel = new FilterCategoryOptionDto()
                        {
                            OptionValue = HttpUtility.UrlDecode(parts.Last()),
                            ParentCategoryName = category,
                            Selected = true
                        };

                        FilterCategoryDto existingCategory = categories.FirstOrDefault(x => x.CategoryName == category);

                        if (existingCategory != null)
                        {
                            if (!existingCategory.CategoryOptions.Any(x => x.OptionValue == optionModel.OptionValue))
                            {
                                existingCategory.CategoryOptions.Add(optionModel);
                            }
                        }
                        else
                        {
                            FilterCategoryDto newCategory = new FilterCategoryDto() { CategoryName = category };
                            newCategory.CategoryOptions.Add(optionModel);
                            categories.Add(newCategory);
                        }
                    }
                }
            }
            else if (_dataFeatures.CLA4258_DefaultProdSearchFilter.GetValue())
            {
                FilterCategoryDto defaultProd = new FilterCategoryDto() { CategoryName = FilterCategoryNames.Dataset.ENVIRONMENTTYPE };
                defaultProd.CategoryOptions.Add(new FilterCategoryOptionDto()
                {
                    OptionValue = NamedEnvironmentType.Prod.GetDescription(),
                    ParentCategoryName = defaultProd.CategoryName,
                    Selected = true
                });
                categories.Add(defaultProd);
            }

            return categories;
        }
    }
}
