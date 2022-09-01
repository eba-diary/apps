using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        public static List<T> FilterBy<T>(this IEnumerable<T> enumerable, List<FilterCategoryDto> filterCategoryDtos) where T : IFilterSearchable
        {
            foreach (FilterCategoryDto categoryDto in filterCategoryDtos)
            {
                if (CustomAttributeHelper.TryGetFilterSearchFieldProperty<T>(categoryDto.CategoryName, out PropertyInfo propertyInfo))
                {
                    ParameterExpression parameter = Expression.Parameter(typeof(T));
                    MethodCallExpression body;

                    if (IsList(propertyInfo))
                    {
                        LambdaExpression containsExpression = GetToStringParameterExpression(propertyInfo, categoryDto);
                        MethodInfo anyMethod = GetAnyMethodForType(propertyInfo);

                        //x => x.ListProperty.Any(a => selectedValues.Contains(a.ToString()))
                        body = Expression.Call(null, anyMethod, new Expression[] { Expression.Property(parameter, propertyInfo.Name), containsExpression });
                    }
                    else
                    {
                        //x => selectedValues.Contains(x.PropertyName.ToString())
                        body = GetSelectedContainsExpression(categoryDto, ToStringProperty(parameter, propertyInfo));
                    }

                    Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, new[] { parameter });
                    enumerable = enumerable.Where(lambda.Compile());
                }
            }

            return enumerable.ToList();
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

        public static bool TryGetSelectedOptionsWithNoResults(this List<FilterCategoryOptionDto> options, List<FilterCategoryOptionDto> newOptions, out List<FilterCategoryOptionDto> result)
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

                try
                {
                    List<FilterCategoryOptionDto> previousCategoryOptions = searchedFilters?.FirstOrDefault(x => x.CategoryName == categoryDto.CategoryName)?.CategoryOptions;

                    if (IsList(propertyInfo))
                    {
                        categoryDto.CategoryOptions = CreateCategoryOptionsFromList(results, propertyInfo, previousCategoryOptions, categoryDto);
                    }
                    else
                    {
                        categoryDto.CategoryOptions = CreateCategoryOptions(results, propertyInfo, previousCategoryOptions, categoryDto);
                    }

                    if (previousCategoryOptions.TryGetSelectedOptionsWithNoResults(categoryDto.CategoryOptions, out List<FilterCategoryOptionDto> selectedOptionsWithNoResults))
                    {
                        categoryDto.CategoryOptions.AddRange(selectedOptionsWithNoResults);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error creating filter for {propertyInfo.Name}", ex);
                }

                return categoryDto;
            }).ConfigureAwait(false);
        }

        private static List<FilterCategoryOptionDto> CreateCategoryOptions<T>(List<T> results, PropertyInfo propertyInfo, List<FilterCategoryOptionDto> previousCategoryOptions, FilterCategoryDto categoryDto) where T : IFilterSearchable
        {
            List<FilterCategoryOptionDto> categoryOptionDtos = new List<FilterCategoryOptionDto>();

            //results.GroupBy(x => x.PropertyName.ToString())
            ParameterExpression parameter = Expression.Parameter(typeof(T));
            Expression<Func<T, string>> groupByExpression = Expression.Lambda<Func<T, string>>(ToStringProperty(parameter, propertyInfo), parameter);
            List<IGrouping<string, T>> categoryOptions = results.GroupBy(groupByExpression.Compile()).ToList();

            foreach (IGrouping<string, T> optionValues in categoryOptions)
            {
                FilterCategoryOptionDto categoryOptionDto = new FilterCategoryOptionDto()
                {
                    OptionValue = optionValues.Key,
                    ResultCount = categoryDto.HideResultCounts ? 0 : optionValues.Count(),
                    ParentCategoryName = categoryDto.CategoryName,
                    Selected = previousCategoryOptions.HasSelectedValueOf(optionValues.Key)
                };

                categoryOptionDtos.Add(categoryOptionDto);
            }

            return categoryOptionDtos;
        }

        private static List<FilterCategoryOptionDto> CreateCategoryOptionsFromList<T>(List<T> results, PropertyInfo propertyInfo, List<FilterCategoryOptionDto> previousCategoryOptions, FilterCategoryDto categoryDto) where T : IFilterSearchable
        {
            List<FilterCategoryOptionDto> categoryOptionDtos = new List<FilterCategoryOptionDto>();

            ParameterExpression parameter = Expression.Parameter(typeof(T));
            MemberExpression property = Expression.Property(parameter, propertyInfo.Name);
            LambdaExpression toStringExpression = GetToStringParameterExpression(propertyInfo, null);
            MethodInfo selectMethod = GetSelectMethodForType(propertyInfo);
            MethodCallExpression select = Expression.Call(null, selectMethod, new Expression[] { property, toStringExpression });

            //x => x.ListProperty.Select(s => s.ToString())
            Expression<Func<T, IEnumerable<string>>> selectExpression = Expression.Lambda<Func<T, IEnumerable<string>>>(select, parameter);

            List<string> distinctOptions = results.SelectMany(selectExpression.Compile()).Distinct().ToList();

            foreach (string option in distinctOptions)
            {
                //x => x.ListProperty.Contains(option)
                MethodCallExpression contains = Expression.Call(Expression.Property(parameter, propertyInfo.Name), propertyInfo.PropertyType.GetMethod("Contains"), Expression.Constant(option));
                Expression<Func<T, bool>> countExpression = Expression.Lambda<Func<T, bool>>(contains, parameter);

                FilterCategoryOptionDto categoryOptionDto = new FilterCategoryOptionDto()
                {
                    OptionValue = option,
                    ResultCount = categoryDto.HideResultCounts ? 0 : results.Count(countExpression.Compile()),
                    ParentCategoryName = categoryDto.CategoryName,
                    Selected = previousCategoryOptions.HasSelectedValueOf(option)
                };

                categoryOptionDtos.Add(categoryOptionDto);
            }

            return categoryOptionDtos;
        }

        private static LambdaExpression GetToStringParameterExpression(PropertyInfo propertyInfo, FilterCategoryDto categoryDto)
        {
            //a => a.ToString()
            Type elementType = GetGenericArgumentType(propertyInfo);
            ParameterExpression toStringParameter = Expression.Parameter(elementType);
            MethodCallExpression toString = Expression.Call(toStringParameter, elementType.GetMethod("ToString", Type.EmptyTypes));

            if (categoryDto != null)
            {
                //a => selectedValues.Contains(a.ToString())
                toString = GetSelectedContainsExpression(categoryDto, toString);
            }

            return Expression.Lambda(toString, toStringParameter);
        }

        private static Type GetGenericArgumentType(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.GetGenericArguments()[0];
        }

        private static MethodInfo GetSelectMethodForType(PropertyInfo propertyInfo)
        {
            //used to create a direct reference to the generic GetSelectMethod in case of name change
            Type type = GetGenericArgumentType(propertyInfo);
            Func<MethodInfo> getSelectMethod = GetSelectMethod<string>;
            MethodInfo selectMethodInfo = typeof(FilterSearchExtensions).GetMethod(getSelectMethod.Method.Name, BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(type);
            return (MethodInfo)selectMethodInfo.Invoke(null, null);
        }

        private static MethodInfo GetSelectMethod<T>()
        {
            Expression<Func<IEnumerable<T>, IEnumerable<string>>> lambda = list => list.Select(e => default(string));
            return (lambda.Body as MethodCallExpression).Method;
        }

        private static MethodInfo GetAnyMethodForType(PropertyInfo propertyInfo)
        {
            //used to create a direct reference to the generic GetSelectMethod in case of name change
            Type type = GetGenericArgumentType(propertyInfo);
            Func<MethodInfo> getMethod = GetAnyMethod<string>;
            MethodInfo methodInfo = typeof(FilterSearchExtensions).GetMethod(getMethod.Method.Name, BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(type);
            return (MethodInfo)methodInfo.Invoke(null, null);
        }

        private static MethodInfo GetAnyMethod<T>()
        {
            Expression<Func<IEnumerable<T>, bool>> lambda = list => list.Any(e => default(bool));
            return (lambda.Body as MethodCallExpression).Method;
        }

        private static bool IsList(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
        }

        private static MethodCallExpression ToStringProperty(ParameterExpression parameter, PropertyInfo propertyInfo)
        {
            //x.PropertyName.ToString()
            MethodInfo toString = propertyInfo.PropertyType.GetMethod("ToString", Type.EmptyTypes);
            return Expression.Call(Expression.Property(parameter, propertyInfo.Name), toString);
        }

        private static MethodCallExpression GetSelectedContainsExpression(FilterCategoryDto categoryDto, MethodCallExpression toString)
        {
            //selectedValues.Contains(x{.PropertyName}.ToString())
            List<string> selectedValues = categoryDto.GetSelectedValues();
            MethodInfo contains = selectedValues.GetType().GetMethod(nameof(selectedValues.Contains));
            return Expression.Call(Expression.Constant(selectedValues), contains, toString);
        }
        #endregion
    }
}
