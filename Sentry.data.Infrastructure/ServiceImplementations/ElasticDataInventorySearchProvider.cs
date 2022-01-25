using Nest;
using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public class ElasticDataInventorySearchProvider : DaleSearchProvider
    {
        private readonly IElasticContext _context;

        public ElasticDataInventorySearchProvider(IElasticContext context)
        {
            _context = context;
        }

        public override DaleResultDto GetSearchResults(DaleSearchDto dto)
        {
            DaleResultDto resultDto = new DaleResultDto();

            SearchRequest<DataInventory> searchRequest = BuildTextSearchRequest(dto, 1000);
            searchRequest.TrackTotalHits = true;

            ElasticResult<DataInventory> result = GetElasticResult(dto, resultDto, searchRequest);

            if (result.Documents?.Any() == true)
            {
                resultDto.DaleResults = result.Documents.Select(x => x.ToDto()).ToList();
                resultDto.SearchTotal = result.SearchTotal;
            }

            return resultDto;
        }

        public override FilterSearchDto GetSearchFilters(DaleSearchDto dto)
        {
            //get fields that are filterable via custom attribute
            Dictionary<string, Field> filterCategoryFields = NestHelper.FilterCategoryFields<DataInventory>();

            //build aggregation query
            AggregationDictionary aggregations = new AggregationDictionary();
            foreach (KeyValuePair<string, Field> field in filterCategoryFields)
            {
                aggregations.Add(field.Key, new TermsAggregation(field.Key) { Field = field.Value });
            }

            SearchRequest<DataInventory> request = BuildTextSearchRequest(dto, 0);
            request.Aggregations = aggregations;

            //get aggregation results
            FilterSearchDto resultDto = new FilterSearchDto();

            AggregateDictionary aggResults = GetElasticResult(dto, resultDto, request).Aggregations;

            //translate results to dto
            if (aggResults != null)
            {
                foreach (string categoryName in filterCategoryFields.Keys)
                {
                    TermsAggregate<string> categoryResults = aggResults.Terms(categoryName);
                    if (categoryResults?.Buckets?.Any() == true && categoryResults.SumOtherDocCount.HasValue && categoryResults.SumOtherDocCount == 0)
                    {
                        FilterCategoryDto categoryDto = new FilterCategoryDto() { CategoryName = categoryName };

                        foreach (var bucket in categoryResults.Buckets)
                        {
                            string bucketKey = bucket.KeyAsString ?? bucket.Key;
                            categoryDto.CategoryOptions.Add(new FilterCategoryOptionDto()
                            {
                                OptionValue = bucketKey,
                                ResultCount = bucket.DocCount.GetValueOrDefault(),
                                ParentCategoryName = categoryName,
                                Selected = dto.FilterCategories?.Any(x => x.CategoryName == categoryName && x.CategoryOptions?.Any(o => o.OptionValue == bucketKey && o.Selected) == true) == true
                            });
                        }

                        resultDto.FilterCategories.Add(categoryDto);
                    }
                }
            }

            return resultDto;
        }

        public override DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dto)
        {
            List<QueryContainer> must = new List<QueryContainer>();
            must.AddMatch<DataInventory>(x => x.IsSensitive, "true");

            switch (dto.Destiny)
            {
                case Core.GlobalEnums.DaleDestiny.SAID:
                    must.AddMatch<DataInventory>(x => x.AssetCode, dto.Criteria);
                    break;
                case Core.GlobalEnums.DaleDestiny.Server:
                    must.AddMatch<DataInventory>(x => x.ServerName, dto.Criteria);
                    break;
                default:
                    must.AddMatch<DataInventory>(x => x.DatabaseName, dto.Criteria);
                    break;
            }

            SearchRequest<DataInventory> request = new SearchRequest<DataInventory>()
            {
                Size = 1,
                Query = new BoolQuery()
                {
                    MustNot = new List<QueryContainer>()
                    {
                        new ExistsQuery()
                        {
                            Field = Infer.Field<DataInventory>(f => f.ExpirationDateTime)
                        }
                    },
                    Must = must
                }
            };

            DaleContainSensitiveResultDto resultDto = new DaleContainSensitiveResultDto();
            resultDto.DoesContainSensitiveResults = GetElasticResult(dto, resultDto, request).Documents?.Any() == true;

            return resultDto;
        }

        public override bool SaveSensitive(string sensitiveBlob)
        {
            //Currently not allowed at this time when source feature flag is set to ELASTIC
            //Solution for real-time updates has not been determined
            throw new NotImplementedException();
        }

        private ElasticResult<DataInventory> GetElasticResult(DaleSearchDto dto, DaleEventableDto resultDto, SearchRequest<DataInventory> searchRequest)
        {
            resultDto.DaleEvent = new DaleEventDto()
            {
                Criteria = dto.CriteriaToString(),
                Destiny = dto.Destiny.GetDescription(),
                QuerySuccess = true,
                Sensitive = dto.Sensitive.GetDescription()
            };

            try
            {
                return _context.Search(searchRequest);
            }
            catch (Exception ex)
            {
                resultDto.DaleEvent.QuerySuccess = false;
                resultDto.DaleEvent.QueryErrorMessage = $"Data Inventory Elasticsearch query failed. Exception: {ex.Message}";
                Logger.Error(resultDto.DaleEvent.QueryErrorMessage, ex);
            }

            return new ElasticResult<DataInventory>();
        }

        private SearchRequest<DataInventory> BuildTextSearchRequest(DaleSearchDto dto, int size)
        {
            List<QueryContainer> should = new List<QueryContainer>();

            if (!string.IsNullOrWhiteSpace(dto.Criteria))
            {
                //broad search for criteria across all searchable fields
                Nest.Fields fields = NestHelper.SearchFields<DataInventory>();

                //split search terms regardless of amount of spaces between words
                List<string> terms = dto.Criteria.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                //perform cross field search when multiple words in search criteria
                if (terms.Count > 1)
                {
                    should.Add(new QueryStringQuery()
                    {
                        Query = string.Join(" ", terms),
                        Fields = fields,
                        Fuzziness = Fuzziness.Auto,
                        Type = TextQueryType.CrossFields,
                        DefaultOperator = Operator.And
                    });

                    should.Add(new QueryStringQuery()
                    {
                        Query = string.Join(" ", terms.Select(x => $"*{x}*")),
                        Fields = fields,
                        AnalyzeWildcard = true,
                        Type = TextQueryType.CrossFields,
                        DefaultOperator = Operator.And
                    });
                }
                else
                {
                    should.Add(new QueryStringQuery()
                    {
                        Query = terms.First(),
                        Fields = fields,
                        Fuzziness = Fuzziness.Auto,
                        Type = TextQueryType.MostFields
                    });

                    should.Add(new QueryStringQuery()
                    {
                        Query = $"*{terms.First()}*",
                        Fields = fields,
                        AnalyzeWildcard = true,
                        Type = TextQueryType.MostFields
                    });
                }
            }

            List<QueryContainer> filter = new List<QueryContainer>();

            foreach (FilterCategoryDto category in dto.FilterCategories)
            {
                filter.Add(new QueryStringQuery()
                {
                    Query = string.Join(" OR ", category.CategoryOptions.Where(x => x.Selected).Select(x => x.OptionValue)),
                    DefaultField = NestHelper.FilterCategoryField<DataInventory>(category.CategoryName)
                });
            }

            return new SearchRequest<DataInventory>()
            {
                Size = size,
                Query = new BoolQuery()
                {
                    MustNot = new List<QueryContainer>() { new ExistsQuery() { Field = Infer.Field<DataInventory>(x => x.ExpirationDateTime) } },
                    Filter = filter,
                    Should = should,
                    MinimumShouldMatch = should.Any() ? 1 : 0
                }
            };
        }
    }
}
