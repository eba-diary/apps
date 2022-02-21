﻿using Nest;
using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ElasticDataInventorySearchProvider : IDaleSearchProvider
    {
        private readonly IElasticContext _context;
        private readonly IDbExecuter _dbExecuter;
        private readonly string AssetCategoriesAggregationKey = "SaidListNames";

        public ElasticDataInventorySearchProvider(IElasticContext context, IDbExecuter dbExecuter)
        {
            _context = context;
            _dbExecuter = dbExecuter;
        }

        #region IDaleSearchProvider Implementation
        public DaleResultDto GetSearchResults(DaleSearchDto dto)
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

        public FilterSearchDto GetSearchFilters(DaleSearchDto dto)
        {           
            SearchRequest<DataInventory> request = BuildTextSearchRequest(dto, 0);
            request.Aggregations = NestHelper.GetFilterAggregations<DataInventory>();

            //get aggregation results
            FilterSearchDto resultDto = new FilterSearchDto();

            AggregateDictionary aggResults = GetElasticResult(dto, resultDto, request).Aggregations;

            //translate results to dto
            foreach (string categoryName in request.Aggregations.Select(x => x.Key).ToList())
            {
                TermsAggregate<string> categoryResults = aggResults?.Terms(categoryName);
                if (categoryResults?.Buckets?.Any() == true && categoryResults.SumOtherDocCount.HasValue && categoryResults.SumOtherDocCount == 0)
                {
                    resultDto.FilterCategories.Add(BuildFilterCategoryDto(categoryResults.Buckets, categoryName, dto.FilterCategories));
                }
            }

            return resultDto;
        }

        public DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dto)
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

            BoolQuery boolQuery = GetBaseBoolQuery();
            boolQuery.Must = must;

            SearchRequest<DataInventory> request = new SearchRequest<DataInventory>()
            {
                Size = 1,
                Query = boolQuery
            };

            DaleContainSensitiveResultDto resultDto = new DaleContainSensitiveResultDto();
            resultDto.DoesContainSensitiveResults = GetElasticResult(dto, resultDto, request).Documents?.Any() == true;

            return resultDto;
        }

        public DaleCategoryResultDto GetCategoriesByAsset(string search)
        {
            DaleCategoryResultDto resultDto = new DaleCategoryResultDto()
            {
                DaleCategories = new List<DaleCategoryDto>(),
                DaleEvent = new DaleEventDto()
                {
                    Criteria = search,
                    QuerySuccess = true
                }
            };

            Task<ElasticResult<DataInventory>> allCategoriesTask = _context.SearchAsync(GetBaseSaidListRequest(GetBaseBoolQuery()));

            List<QueryContainer> filters = new List<QueryContainer>();
            filters.AddMatch<DataInventory>(x => x.AssetCode, search);
            filters.AddMatch<DataInventory>(x => x.IsSensitive, "true");

            BoolQuery boolQuery = GetBaseBoolQuery();
            boolQuery.Filter = filters;

            Task<ElasticResult<DataInventory>> assetCategoriesTask = _context.SearchAsync(GetBaseSaidListRequest(boolQuery));

            try
            {
                List<string> allNames = ExtractAggregationBucketKeys(allCategoriesTask);
                List<string> assetNames = ExtractAggregationBucketKeys(assetCategoriesTask);

                resultDto.DaleCategories = allNames.Select(x => new DaleCategoryDto()
                {
                    Category = x,
                    IsSensitive = assetNames.Contains(x)
                }).ToList();
            }
            catch (AggregateException ex)
            {
                HandleAggregateException(ex, resultDto.DaleEvent);
            }

            return resultDto;
        }

        public bool SaveSensitive(List<DaleSensitiveDto> dtos)
        {
            try
            {
                //update SQL first
                _dbExecuter.ExecuteCommand(Newtonsoft.Json.JsonConvert.SerializeObject(dtos));

                //if SQL succeeds, update in elastic
                IList<DataInventory> diToUpdate = null;
                Dictionary<int, Task<bool>> tasks = new Dictionary<int, Task<bool>>();
                try
                {
                    //get documents to update by ids
                    diToUpdate = _context.SearchAsync<DataInventory>(x => x
                        .Query(q => q
                            .Ids(i => i
                                .Values(dtos.Select(dto => new Id(dto.BaseColumnId)).ToList())))).Result.Documents;

                    //submit async update requests per document to update
                    foreach (DataInventory di in diToUpdate)
                    {
                        DaleSensitiveDto dto = dtos.FirstOrDefault(x => x.BaseColumnId == di.Id);
                        di.IsSensitive = dto.IsSensitive;
                        di.IsOwnerVerified = dto.IsOwnerVerified;

                        tasks.Add(di.Id, _context.Update(di));
                    }

                    //wait for all requests to complete (WaitAll lets all complete before error is thrown)
                    Task.WaitAll(tasks.Values.ToArray());

                    return true;
                }
                catch (AggregateException aggEx)
                {
                    //search failed if diToUpdate is empty, else update(s) failed
                    Logger.Error(diToUpdate?.Any() != true
                        ? $"ElasticDataInventorySearchProvider failed to retrieve Id(s): {string.Join(", ", dtos.Select(x => x.BaseColumnId))} from index"
                        : $"ElasticDataInventorySearchProvider failed to update Id(s): {string.Join(", ", tasks.Where(x => x.Value.IsFaulted).Select(x => x.Key))}", aggEx);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"ElasticDataInventorySearchProvider SQL command failed to update Id(s): {string.Join(", ", dtos.Select(x => x.BaseColumnId))}", ex);
            }

            return false;
        }
        #endregion

        #region Methods
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
                return _context.SearchAsync(searchRequest).Result;
            }
            catch (AggregateException ex)
            {
                HandleAggregateException(ex, resultDto.DaleEvent);
            }

            return new ElasticResult<DataInventory>();
        }

        private SearchRequest<DataInventory> BuildTextSearchRequest(DaleSearchDto dto, int size)
        {
            BoolQuery boolQuery = GetBaseBoolQuery();

            if (!string.IsNullOrWhiteSpace(dto.Criteria))
            {
                //broad search for criteria across all searchable fields
                Nest.Fields fields = NestHelper.GetSearchFields<DataInventory>();

                //split search terms regardless of amount of spaces between words
                List<string> terms = dto.Criteria.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                //perform cross field search when multiple words in search criteria
                if (terms.Count > 1)
                {
                    boolQuery.Should = new List<QueryContainer>() 
                    {
                        new QueryStringQuery()
                        {
                            Query = string.Join(" ", terms),
                            Fields = fields,
                            Fuzziness = Fuzziness.Auto,
                            Type = TextQueryType.CrossFields,
                            DefaultOperator = Operator.And
                        },
                        new QueryStringQuery()
                        {
                            Query = string.Join(" ", terms.Select(x => $"*{x}*")),
                            Fields = fields,
                            AnalyzeWildcard = true,
                            Type = TextQueryType.CrossFields,
                            DefaultOperator = Operator.And
                        }
                    };
                }
                else
                {
                    boolQuery.Should = new List<QueryContainer>()
                    {
                        new QueryStringQuery()
                        {
                            Query = terms.First(),
                            Fields = fields,
                            Fuzziness = Fuzziness.Auto,
                            Type = TextQueryType.MostFields
                        },
                        new QueryStringQuery()
                        {
                            Query = $"*{terms.First()}*",
                            Fields = fields,
                            AnalyzeWildcard = true,
                            Type = TextQueryType.MostFields
                        }
                    };
                }

                boolQuery.MinimumShouldMatch = boolQuery.Should.Any() ? 1 : 0;
            }

            List<QueryContainer> filter = new List<QueryContainer>();

            foreach (FilterCategoryDto category in dto.FilterCategories)
            {
                filter.Add(new QueryStringQuery()
                {
                    Query = string.Join(" OR ", category.CategoryOptions.Where(x => x.Selected).Select(x => x.OptionValue)),
                    DefaultField = NestHelper.GetFilterCategoryField<DataInventory>(category.CategoryName)
                });
            }

            boolQuery.Filter = filter;

            return new SearchRequest<DataInventory>()
            {
                Size = size,
                Query = boolQuery
            };
        }

        private BoolQuery GetBaseBoolQuery()
        {
            return new BoolQuery()
            {
                MustNot = new List<QueryContainer>() { new ExistsQuery() { Field = Infer.Field<DataInventory>(x => x.ExpirationDateTime) } }
            };
        }

        private SearchRequest<DataInventory> GetBaseSaidListRequest(QueryContainer query)
        {
            return new SearchRequest<DataInventory>()
            {
                Size = 0,
                Query = query,
                Aggregations = new AggregationDictionary()
                {
                    { AssetCategoriesAggregationKey, new TermsAggregation(AssetCategoriesAggregationKey) { Field = Infer.Field<DataInventory>(x => x.SaidListName.Suffix("keyword")), Size = 40 } }
                }
            };
        }

        private void HandleAggregateException(AggregateException ex, DaleEventDto eventDto)
        {
            eventDto.QuerySuccess = false;
            eventDto.QueryErrorMessage = $"Data Inventory Elasticsearch query failed. Exception: {ex.Message}";
            Logger.Error(eventDto.QueryErrorMessage, ex);
        }

        private List<string> ExtractAggregationBucketKeys(Task<ElasticResult<DataInventory>> resultTask)
        {
            TermsAggregate<string> agg = resultTask.Result.Aggregations.Terms(AssetCategoriesAggregationKey);
            return agg.Buckets.SelectMany(x => x.Key.Split(',').Select(s => s.Trim())).Distinct().ToList();
        }

        private FilterCategoryDto BuildFilterCategoryDto(IReadOnlyCollection<KeyedBucket<string>> buckets, string categoryName, List<FilterCategoryDto> requestFilters)
        {
            FilterCategoryDto categoryDto = new FilterCategoryDto() { CategoryName = categoryName };

            foreach (var bucket in buckets)
            {
                string bucketKey = bucket.KeyAsString ?? bucket.Key;
                categoryDto.CategoryOptions.Add(new FilterCategoryOptionDto()
                {
                    OptionValue = bucketKey,
                    ResultCount = bucket.DocCount.GetValueOrDefault(),
                    ParentCategoryName = categoryName,
                    Selected = requestFilters?.Any(x => x.CategoryName == categoryName && x.CategoryOptions?.Any(o => o.OptionValue == bucketKey && o.Selected) == true) == true
                });
            }

            List<FilterCategoryOptionDto> selectedOptionsWithNoResults = requestFilters?.FirstOrDefault(x => x.CategoryName == categoryName)?.CategoryOptions?.Where(x => x.Selected && !categoryDto.CategoryOptions.Any(o => o.OptionValue == x.OptionValue)).ToList();
            if (selectedOptionsWithNoResults?.Any() == true)
            {
                categoryDto.CategoryOptions.AddRange(selectedOptionsWithNoResults);
            }

            return categoryDto;
        }
        #endregion
    }
}
