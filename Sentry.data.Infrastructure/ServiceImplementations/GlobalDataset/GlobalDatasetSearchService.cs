using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class GlobalDatasetSearchService : IGlobalDatasetSearchService
    {
        private readonly IDataFeatures _dataFeatures;

        public GlobalDatasetSearchService(IDataFeatures dataFeatures)
        {
            _dataFeatures = dataFeatures;
        }

        public GlobalDatasetPageResultDto SetGlobalDatasetPageResults(GlobalDatasetPageRequestDto globalDatasetPageRequestDto)
        {
            IEnumerable<SearchGlobalDatasetDto> globalDatasetEnumerable = globalDatasetPageRequestDto.GlobalDatasets;

            switch ((GlobalDatasetSortByOption)globalDatasetPageRequestDto.SortBy)
            {
                case GlobalDatasetSortByOption.Alphabetical:
                    globalDatasetEnumerable = globalDatasetPageRequestDto.GlobalDatasets.OrderBy(x => x.DatasetName);
                    break;
                case GlobalDatasetSortByOption.Favorites:
                    globalDatasetEnumerable = globalDatasetPageRequestDto.GlobalDatasets.OrderByDescending(x => x.IsFavorite);
                    break;
            }

            if (globalDatasetPageRequestDto.PageSize > 0)
            {
                globalDatasetEnumerable = globalDatasetEnumerable.Skip(globalDatasetPageRequestDto.PageSize * (globalDatasetPageRequestDto.PageNumber - 1)).Take(globalDatasetPageRequestDto.PageSize);
            }

            GlobalDatasetPageResultDto resultDto = new GlobalDatasetPageResultDto
            {
                PageNumber = globalDatasetPageRequestDto.PageNumber,
                PageSize = globalDatasetPageRequestDto.PageSize,
                SortBy = globalDatasetPageRequestDto.SortBy,
                Layout = globalDatasetPageRequestDto.Layout,
                TotalResults = globalDatasetPageRequestDto.GlobalDatasets.Count,
                GlobalDatasets = globalDatasetEnumerable.ToList()
            };

            return resultDto;
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
