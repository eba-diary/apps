using Nest;
using Sentry.data.Core.Entities.S3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

namespace Sentry.data.Core
{
    public static class NestHelper
    {
        public static Nest.Fields GetSearchFields<T>()
        {
            return Infer.Fields(CustomAttributeHelper.GetPropertiesWithAttribute<T, GlobalSearchFieldAttribute>().ToArray());
        }

        public static AggregationDictionary GetFilterAggregations<T>() where T : class
        {
            AggregationDictionary aggregations = new AggregationDictionary();

            foreach (PropertyInfo property in CustomAttributeHelper.GetPropertiesWithAttribute<T, FilterSearchFieldAttribute>())
            {
                FilterSearchFieldAttribute filterAttribute = property.GetCustomAttribute<FilterSearchFieldAttribute>();

                TermsAggregation termsAggregation = new TermsAggregation(filterAttribute.FilterCategoryName)
                {
                    Field = GetFilterCategoryField<T>(property),
                    Size = filterAttribute.IsPinnedFilter ? 10000 : 15
                };

                aggregations.Add(filterAttribute.FilterCategoryName, termsAggregation);
            }

            return aggregations;
        }

        //public static AggregationDictionary GetFilterAggregations2<T>() where T : class
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

        public static Field GetFilterCategoryField<T>(string categoryName) where T : class
        {
            if (CustomAttributeHelper.TryGetFilterSearchFieldProperty<T>(categoryName, out PropertyInfo property))
            {
                return GetFilterCategoryField<T>(property);
            }

            throw new InvalidOperationException($"Filter category name: {categoryName} does not match any category names registered to type of: {typeof(T).Name}");
        }

        public static BoolQuery ToSearchQuery<T>(this BaseFilterSearchDto filterSearchDto) where T : class
        {
            BoolQuery searchQuery = new BoolQuery();

            ParameterExpression originExpression = Expression.Parameter(typeof(T));

            if (!string.IsNullOrWhiteSpace(filterSearchDto.SearchText))
            {
                //split search terms regardless of amount of spaces between words
                List<string> terms = filterSearchDto.SearchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                searchQuery.Should = BuildShouldQuery<T>(typeof(T), terms, originExpression, originExpression);
                searchQuery.MinimumShouldMatch = searchQuery.Should.Any() ? 1 : 0;
            }

            if (filterSearchDto.FilterCategories?.Any() == true)
            {
                searchQuery.Filter = BuildFilterQuery<T>(typeof(T), filterSearchDto.FilterCategories, originExpression, originExpression);
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
        private static List<QueryContainer> BuildFilterQuery<T>(Type type, List<FilterCategoryDto> filterCategories, Expression parentExpression, ParameterExpression originParameter) where T : class
        {
            List<QueryContainer> queryContainers = new List<QueryContainer>();

            List<PropertyInfo> filterProperties = CustomAttributeHelper.GetPropertiesWithAttribute<FilterSearchFieldAttribute>(type).ToList();

            foreach (PropertyInfo property in filterProperties)
            {
                string categoryName = property.GetCustomAttribute<FilterSearchFieldAttribute>().FilterCategoryName;
                FilterCategoryDto filterCategory = filterCategories.FirstOrDefault(x => x.CategoryName == categoryName);

                if (filterCategory != null)
                {
                    Expression memberExpression = Expression.Property(parentExpression, property.Name);
                    Expression<Func<T, object>> fieldExpression = Expression.Lambda<Func<T, object>>(memberExpression, originParameter);

                    if (property.PropertyType == typeof(string))
                    {
                        fieldExpression = fieldExpression.AppendSuffix("keyword");
                    }

                    queryContainers.Add(new TermsQuery
                    {
                        Field = fieldExpression,
                        Terms = filterCategory.GetSelectedValues()
                    });
                }
            }

            IEnumerable<PropertyInfo> nestedFilterProperties = CustomAttributeHelper.GetPropertiesWithAttribute<FilterSearchNestedFieldAttribute>(type);

            foreach (PropertyInfo property in nestedFilterProperties)
            {
                Expression fieldExpression = Expression.Property(parentExpression, property.Name);
                Expression newParentExpression = AddFirstForEnumerable(fieldExpression, property);

                List<QueryContainer> nestedQueryContainers = BuildFilterQuery<T>(property.PropertyType, filterCategories, newParentExpression, originParameter);

                queryContainers.AddRange(nestedQueryContainers);
            }

            return queryContainers;
        }

        private static List<QueryContainer> BuildShouldQuery<T>(Type type, List<string> searchTerms, Expression parentExpression, ParameterExpression originParameter) where T : class
        {
            List<QueryContainer> queryContainers = new List<QueryContainer>();

            List<PropertyInfo> searchProperties = CustomAttributeHelper.GetPropertiesWithAttribute<GlobalSearchFieldAttribute>(type).ToList();

            if (searchProperties.Any())
            {
                List<Expression<Func<T, object>>> fieldExpressions = new List<Expression<Func<T, object>>>();

                foreach (PropertyInfo property in searchProperties)
                {
                    Expression memberExpression = Expression.Property(parentExpression, property.Name);
                    Expression<Func<T, object>> fieldExpression = Expression.Lambda<Func<T, object>>(memberExpression, originParameter);
                    fieldExpressions.Add(fieldExpression);
                }

                Nest.Fields fields = Infer.Fields(fieldExpressions.ToArray());

                foreach (Field field in fields)
                {
                    //???
                    field.Boost = field.Property.GetCustomAttribute<GlobalSearchFieldAttribute>().Boost;
                }

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
            }

            IEnumerable<PropertyInfo> nestedSearchProperties = CustomAttributeHelper.GetPropertiesWithAttribute<GlobalSearchNestedFieldAttribute>(type);

            foreach (PropertyInfo property in nestedSearchProperties)
            {
                Expression fieldExpression = Expression.Property(parentExpression, property.Name);
                Expression newParentExpression = AddFirstForEnumerable(fieldExpression, property);

                List<QueryContainer> nestedQueryContainers = BuildShouldQuery<T>(property.PropertyType, searchTerms, newParentExpression, originParameter);

                queryContainers.AddRange(nestedQueryContainers);
            }

            return queryContainers;
        }

        private static Expression AddFirstForEnumerable(Expression fieldExpression, PropertyInfo property)
        {
            //x.PropertyName.First()
            if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
            {
                MethodInfo first = property.PropertyType.GetMethod("First", Type.EmptyTypes);
                return Expression.Call(fieldExpression, first);
            }

            return fieldExpression;
        }

        private static Field GetFilterCategoryField<T>(PropertyInfo property) where T : class
        {
            if (property.PropertyType == typeof(string))
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T));
                Expression<Func<T, object>> expression = Expression.Lambda<Func<T, object>>(Expression.Property(parameter, property.Name), parameter);

                return Infer.Field(expression.AppendSuffix("keyword"));
            }

            return Infer.Field(property);
        }
        #endregion
    }
}
