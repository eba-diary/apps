using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class FilterSearchExtensions
    {
        public static SavedSearchDto ToDto(this SavedSearch entity)
        {
            return new SavedSearchDto()
            {
                SearchType = entity.SearchType,
                SearchName = entity.SearchName,
                SearchText = entity.SearchText,
                AssociateId = entity.AssociateId,
                FilterCategories = !string.IsNullOrWhiteSpace(entity.FilterCategoriesJson) ? 
                                   JsonConvert.DeserializeObject<List<FilterCategoryDto>>(entity.FilterCategoriesJson) : 
                                   new List<FilterCategoryDto>(),
                ResultConfiguration = !string.IsNullOrWhiteSpace(entity.ResultConfigurationJson) ?
                                      JObject.Parse(entity.ResultConfigurationJson) :
                                      null
            };
        }

        public static SavedSearch ToEntity(this SavedSearchDto dto)
        {
            return new SavedSearch()
            {
                SearchType = dto.SearchType,
                SearchName = dto.SearchName,
                SearchText = dto.SearchText,
                AssociateId = dto.AssociateId,
                FilterCategoriesJson = dto.FilterCategories != null ? JsonConvert.SerializeObject(dto.FilterCategories) : null,
                ResultConfigurationJson = dto.ResultConfiguration != null ? dto.ResultConfiguration.ToString(Formatting.None) : null
            };
        }

        public static IEnumerable<T> FilterBy<T>(this IEnumerable<T> enumerable, List<FilterCategoryDto> filterCategoryDtos) where T : IFilterSearchable
        {
            foreach (FilterCategoryDto categoryDto in filterCategoryDtos)
            {
                if (CustomAttributeHelper.TryGetFilterSearchFieldProperty<T>(categoryDto.CategoryName, out PropertyInfo propertyInfo))
                {
                    ParameterExpression parameter = Expression.Parameter(typeof(T));

                    List<string> selectedValues = categoryDto.GetSelectedValues();
                    MethodInfo contains = selectedValues.GetType().GetMethod(nameof(selectedValues.Contains));

                    MethodCallExpression body = Expression.Call(Expression.Constant(selectedValues), contains, ToStringProperty(parameter, propertyInfo));
                    Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, new[] { parameter });
                    enumerable = enumerable.Where(lambda.Compile());
                }
            }

            return enumerable;
        }

        public static List<FilterCategoryDto> CreateFilters<T>(this List<T> results, List<FilterCategoryDto> previousFilters) where T : IFilterSearchable
        {
            List<FilterCategoryDto> filterCategories = new List<FilterCategoryDto>();

            List<PropertyInfo> filterableProperties = CustomAttributeHelper.GetPropertiesWithAttribute<T, FilterSearchField>().ToList();

            List<Task<FilterCategoryDto>> tasks = filterableProperties.Select(x => CreateFilterCategoryAsync(results, previousFilters, x)).ToList();

            filterCategories.AddRange(tasks.Where(x => x.Result.CategoryOptions.Any()).Select(x => x.Result).ToList());

            return filterCategories;
        }

        public static bool HasSelectedValueOf(this List<FilterCategoryOptionDto> options, string value)
        {
            return options?.Any(o => o.OptionValue == value && o.Selected) == true;
        }

        public static bool TryGetSelectedOptionsWithNoResultsIn(this List<FilterCategoryOptionDto> options, List<FilterCategoryOptionDto> newOptions, out List<FilterCategoryOptionDto> result)
        {
            result = options?.Where(x => x.Selected && !newOptions.Any(o => o.OptionValue == x.OptionValue)).ToList();
            return result?.Any() == true;
        }

        #region Private Methods
        private static async Task<FilterCategoryDto> CreateFilterCategoryAsync<T>(List<T> results, List<FilterCategoryDto> searchedFilters, PropertyInfo propertyInfo) where T : IFilterSearchable
        {
            return await Task.Run(() =>
            {
                FilterSearchField filterAttribute = propertyInfo.GetCustomAttribute<FilterSearchField>();
                FilterCategoryDto categoryDto = new FilterCategoryDto() 
                { 
                    CategoryName = filterAttribute.FilterCategoryName,
                    DefaultCategoryOpen = filterAttribute.DefaultOpen,
                    HideResultCounts = filterAttribute.HideResultCounts
                };

                ParameterExpression parameter = Expression.Parameter(typeof(T));
                Expression<Func<T, string>> groupByExpression = Expression.Lambda<Func<T, string>>(ToStringProperty(parameter, propertyInfo), parameter);

                List<IGrouping<string, T>> categoryOptions = results.GroupBy(groupByExpression.Compile()).ToList();

                List<FilterCategoryOptionDto> previousCategoryOptions = searchedFilters?.FirstOrDefault(x => x.CategoryName == categoryDto.CategoryName)?.CategoryOptions;

                foreach (IGrouping<string, T> optionValues in categoryOptions)
                {
                    FilterCategoryOptionDto categoryOptionDto = new FilterCategoryOptionDto()
                    {
                        OptionValue = optionValues.Key,
                        ResultCount = optionValues.Count(),
                        ParentCategoryName = categoryDto.CategoryName,
                        Selected = previousCategoryOptions.HasSelectedValueOf(optionValues.Key)
                    };

                    categoryDto.CategoryOptions.Add(categoryOptionDto);
                }

                if (previousCategoryOptions.TryGetSelectedOptionsWithNoResultsIn(categoryDto.CategoryOptions, out List<FilterCategoryOptionDto> selectedOptionsWithNoResults))
                {
                    categoryDto.CategoryOptions.AddRange(selectedOptionsWithNoResults);
                }

                return categoryDto;
            }).ConfigureAwait(false);
        }

        private static MethodCallExpression ToStringProperty(ParameterExpression parameter, PropertyInfo propertyInfo)
        {
            MethodInfo toString = propertyInfo.PropertyType.GetMethod("ToString", Type.EmptyTypes);
            return Expression.Call(Expression.Property(parameter, propertyInfo.Name), toString);
        }
        #endregion
    }
}
