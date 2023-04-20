using Nest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sentry.data.Core
{
    public static class NestHelper
    {
        //public static Nest.Fields GetSearchFields<T>()
        //{
        //    return Infer.Fields(CustomAttributeHelper.GetPropertiesWithAttribute<T, GlobalSearchFieldAttribute>().ToArray());
        //}

        //public static AggregationDictionary GetFilterAggregations<T>() where T : class
        //{
        //    AggregationDictionary aggregations = new AggregationDictionary();

        //    foreach (PropertyInfo property in CustomAttributeHelper.GetPropertiesWithAttribute<T, FilterSearchFieldAttribute>())
        //    {
        //        FilterSearchFieldAttribute filterAttribute = property.GetCustomAttribute<FilterSearchFieldAttribute>();

        //        TermsAggregation termsAggregation = new TermsAggregation(filterAttribute.FilterCategoryName)
        //        {
        //            Field = GetFilterCategoryField<T>(property),
        //            Size = filterAttribute.IsPinnedFilter ? 10000 : 15
        //        };

        //        aggregations.Add(filterAttribute.FilterCategoryName, termsAggregation);
        //    }

        //    return aggregations;
        //}

        //public static Field GetFilterCategoryField<T>(string categoryName) where T : class
        //{
        //    if (CustomAttributeHelper.TryGetFilterSearchFieldProperty<T>(categoryName, out PropertyInfo property))
        //    {
        //        return GetFilterCategoryField<T>(property);
        //    }

        //    throw new InvalidOperationException($"Filter category name: {categoryName} does not match any category names registered to type of: {typeof(T).Name}");
        //}

        public static AggregationDictionary GetFilterAggregations<T>() where T : class
        {
            AggregationDictionary aggregations = new AggregationDictionary();

            List<KeyValuePair<string, TermsAggregation>> termsAggregations = GetAllByAttribute<KeyValuePair<string, TermsAggregation>, FilterSearchFieldAttribute>(typeof(T), null, BuildAggregation);

            foreach (var termsAggregation in termsAggregations)
            {
                aggregations.Add(termsAggregation.Key, termsAggregation.Value);
            }

            return aggregations;
        }

        public static BoolQuery ToSearchQuery<T>(this BaseFilterSearchDto filterSearchDto) where T : class
        {
            BoolQuery searchQuery = new BoolQuery();

            if (!string.IsNullOrWhiteSpace(filterSearchDto.SearchText))
            {
                //split search terms regardless of amount of spaces between words
                List<string> terms = filterSearchDto.SearchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                searchQuery.Should = GetShouldQueries<T>(terms);
                searchQuery.MinimumShouldMatch = searchQuery.Should.Any() ? 1 : 0;
            }

            if (filterSearchDto.FilterCategories?.Any() == true)
            {
                searchQuery.Filter = GetAllByAttribute<QueryContainer, FilterSearchFieldAttribute>(typeof(T), null, (prop, field, attr) => GetFilterTermsQuery(prop, field, attr, filterSearchDto.FilterCategories));
            }

            return searchQuery;
        }

        public static List<FilterCategoryDto> ToFilterCategories(this AggregateDictionary aggregations, List<FilterCategoryDto> requestedFilterCategories)
        {
            List<FilterCategoryDto> filterCategories = new List<FilterCategoryDto>();

            foreach (KeyValuePair<string, IAggregate> aggregation in aggregations)
            {
                TermsAggregate<string> termsAggregate = (TermsAggregate<string>)aggregation.Value;

                if (termsAggregate.Buckets.Any())
                {
                    FilterCategoryDto filterCategory = new FilterCategoryDto
                    {
                        CategoryName = aggregation.Key
                    };

                    List<FilterCategoryOptionDto> previousCategoryOptions = requestedFilterCategories?.FirstOrDefault(x => x.CategoryName == aggregation.Key).CategoryOptions;

                    foreach (var bucket in termsAggregate.Buckets)
                    {
                        string bucketKey = bucket.KeyAsString ?? bucket.Key;
                        filterCategory.CategoryOptions.Add(new FilterCategoryOptionDto()
                        {
                            OptionValue = bucketKey,
                            ResultCount = bucket.DocCount.GetValueOrDefault(),
                            ParentCategoryName = aggregation.Key,
                            Selected = previousCategoryOptions.HasSelectedValueOf(bucketKey)
                        });
                    }

                    if (previousCategoryOptions.TryGetSelectedOptionsWithNoResults(filterCategory.CategoryOptions, out List<FilterCategoryOptionDto> selectedOptionsWithNoResults))
                    {
                        filterCategory.CategoryOptions.AddRange(selectedOptionsWithNoResults);
                    }

                    filterCategories.Add(filterCategory);
                }

            }

            return filterCategories;
        }

        #region Private
        private static QueryContainer GetFilterTermsQuery(PropertyInfo property, string fieldName, FilterSearchFieldAttribute filterAttribute, List<FilterCategoryDto> filterCategories)
        {
            string categoryName = filterAttribute.FilterCategoryName;
            FilterCategoryDto filterCategory = filterCategories.FirstOrDefault(x => x.CategoryName == categoryName);

            if (filterCategory != null)
            {
                if (property.PropertyType == typeof(string))
                {
                    fieldName = $"{fieldName}.keyword";
                }

                return new TermsQuery
                {
                    Field = fieldName,
                    Terms = filterCategory.GetSelectedValues()
                };
            }

            return null;
        }

        private static List<QueryContainer> GetShouldQueries<T>(List<string> searchTerms)
        {
            List<QueryContainer> queryContainers = new List<QueryContainer>();

            List<string> searchFields = GetAllByAttribute<string, GlobalSearchFieldAttribute>(typeof(T), null, GetSearchField);

            Nest.Fields fields = Infer.Fields(searchFields.ToArray());

            if (searchTerms.Count > 1)
            {
                queryContainers.Add(new QueryStringQuery()
                {
                    Query = string.Join(" ", searchTerms),
                    Fields = fields,
                    Fuzziness = Fuzziness.Auto,
                    Type = TextQueryType.CrossFields,
                    DefaultOperator = Operator.And
                });

                queryContainers.Add(new QueryStringQuery()
                {
                    Query = string.Join(" ", searchTerms.Select(x => $"*{x}*")),
                    Fields = fields,
                    AnalyzeWildcard = true,
                    Type = TextQueryType.CrossFields,
                    DefaultOperator = Operator.And
                });
            }
            else
            {
                queryContainers.Add(new QueryStringQuery()
                {
                    Query = searchTerms.First(),
                    Fields = fields,
                    Fuzziness = Fuzziness.Auto,
                    Type = TextQueryType.MostFields
                });

                queryContainers.Add(new QueryStringQuery()
                {
                    Query = $"*{searchTerms.First()}*",
                    Fields = fields,
                    AnalyzeWildcard = true,
                    Type = TextQueryType.MostFields
                });
            }

            return queryContainers;
        }

        private static string GetSearchField(PropertyInfo property, string fieldName, GlobalSearchFieldAttribute searchAttribute)
        {
            double? boost = searchAttribute.Boost;

            if (boost.HasValue)
            {
                fieldName = $"{fieldName}^{boost}";
            }

            return fieldName;
        }
        private static KeyValuePair<string, TermsAggregation> BuildAggregation(PropertyInfo property, string fieldName, FilterSearchFieldAttribute filterAttribute)
        {
            if (property.PropertyType == typeof(string))
            {
                fieldName = $"{fieldName}.keyword";
            }

            TermsAggregation termsAggregation = new TermsAggregation(filterAttribute.FilterCategoryName)
            {
                Field = fieldName,
                Size = filterAttribute.IsPinnedFilter ? 10000 : 15
            };

            return new KeyValuePair<string, TermsAggregation>(filterAttribute.FilterCategoryName, termsAggregation);
        }

        private static List<TResult> GetAllByAttribute<TResult, TAttribute>(Type type, string parentFieldName, Func<PropertyInfo, string, TAttribute, TResult> createResult) where TAttribute : Attribute
        {
            List<TResult> results = new List<TResult>();

            List<PropertyInfo> searchProperties = CustomAttributeHelper.GetPropertiesWithAttribute<TAttribute>(type).ToList();

            foreach (PropertyInfo property in searchProperties)
            {
                string fieldName = property.GetCustomAttribute<PropertyNameAttribute>().Name;

                if (!string.IsNullOrEmpty(parentFieldName))
                {
                    fieldName = $"{parentFieldName}.{fieldName}";
                }

                if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                {
                    TAttribute attribute = property.GetCustomAttribute<TAttribute>();

                    TResult result = createResult(property, fieldName, attribute);

                    if (result != null)
                    {
                        results.Add(result);
                    }
                }
                else
                {
                    Type propertyType = property.PropertyType;

                    if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
                    {
                        propertyType = propertyType.GetGenericArguments()[0];
                    }

                    List<TResult> nestedSearchFields = GetAllByAttribute(propertyType, fieldName, createResult);
                    results.AddRange(nestedSearchFields);
                }
            }

            return results;
        }

        //private static Field GetFilterCategoryField<T>(PropertyInfo property) where T : class
        //{
        //    if (property.PropertyType == typeof(string))
        //    {
        //        ParameterExpression parameter = Expression.Parameter(typeof(T));
        //        Expression<Func<T, object>> expression = Expression.Lambda<Func<T, object>>(Expression.Property(parameter, property.Name), parameter);

        //        return Infer.Field(expression.AppendSuffix("keyword"));
        //    }

        //    return Infer.Field(property);
        //}

        //private static List<QueryContainer> BuildFilterQuery(Type type, List<FilterCategoryDto> filterCategories, string parentFieldName)
        //{
        //    List<QueryContainer> queryContainers = new List<QueryContainer>();

        //    List<PropertyInfo> filterProperties = CustomAttributeHelper.GetPropertiesWithAttribute<FilterSearchFieldAttribute>(type).ToList();

        //    foreach (PropertyInfo property in filterProperties)
        //    {
        //        string fieldName = property.GetFieldName(parentFieldName);

        //        if (property.IsSearchableType())
        //        {
        //            string categoryName = property.GetCustomAttribute<FilterSearchFieldAttribute>().FilterCategoryName;
        //            FilterCategoryDto filterCategory = filterCategories.FirstOrDefault(x => x.CategoryName == categoryName);

        //            if (filterCategory != null)
        //            {
        //                if (property.PropertyType == typeof(string))
        //                {
        //                    fieldName = $"{fieldName}.keyword";
        //                }

        //                queryContainers.Add(new TermsQuery
        //                {
        //                    Field = fieldName,
        //                    Terms = filterCategory.GetSelectedValues()
        //                });
        //            }
        //        }
        //        else
        //        {
        //            Type propertyType = property.GetNestedSearchType();
        //            List<QueryContainer> nestedQueryContainers = BuildFilterQuery(propertyType, filterCategories, fieldName);
        //            queryContainers.AddRange(nestedQueryContainers);
        //        }                
        //    }

        //    return queryContainers;
        //}

        //private static List<string> GetSearchFields(Type type, string parentFieldName)
        //{
        //    List<string> searchFields = new List<string>();

        //    List<PropertyInfo> searchProperties = CustomAttributeHelper.GetPropertiesWithAttribute<GlobalSearchFieldAttribute>(type).ToList();

        //    foreach (PropertyInfo property in searchProperties)
        //    {
        //        string fieldName = property.GetFieldName(parentFieldName);

        //        if (property.IsSearchableType())
        //        {
        //            double? boost = property.GetCustomAttribute<GlobalSearchFieldAttribute>().Boost;

        //            if (boost.HasValue)
        //            {
        //                fieldName = $"{fieldName}^{boost}";
        //            }

        //            searchFields.Add(fieldName);
        //        }
        //        else
        //        {
        //            Type propertyType = property.GetNestedSearchType();
        //            List<string> nestedSearchFields = GetSearchFields(propertyType, fieldName);
        //            searchFields.AddRange(nestedSearchFields);
        //        }
        //    }

        //    return searchFields;
        //}

        //private static Dictionary<string, TermsAggregation> BuildAggregations(Type type, string parentFieldName)
        //{
        //    Dictionary<string, TermsAggregation> aggregations = new Dictionary<string, TermsAggregation>();

        //    List<PropertyInfo> filterProperties = CustomAttributeHelper.GetPropertiesWithAttribute<FilterSearchFieldAttribute>(type).ToList();

        //    foreach (PropertyInfo property in filterProperties)
        //    {
        //        string fieldName = property.GetFieldName(parentFieldName);

        //        if (property.IsSearchableType())
        //        {
        //            FilterSearchFieldAttribute filterAttribute = property.GetCustomAttribute<FilterSearchFieldAttribute>();

        //            if (property.PropertyType == typeof(string))
        //            {
        //                fieldName = $"{fieldName}.keyword";
        //            }

        //            TermsAggregation termsAggregation = new TermsAggregation(filterAttribute.FilterCategoryName)
        //            {
        //                Field = fieldName,
        //                Size = filterAttribute.IsPinnedFilter ? 10000 : 15
        //            };

        //            aggregations.Add(filterAttribute.FilterCategoryName, termsAggregation);
        //        }
        //        else
        //        {
        //            Type propertyType = property.GetNestedSearchType();
        //            Dictionary<string, TermsAggregation> nestedAggregations = BuildAggregations(propertyType, fieldName);

        //            foreach (var nestedAggregation in nestedAggregations)
        //            {
        //                aggregations.Add(nestedAggregation.Key, nestedAggregation.Value);
        //            }
        //        }                
        //    }

        //    return aggregations;
        //}
        #endregion
    }
}
