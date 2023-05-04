using Nest;
using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ElasticDataInventorySearchProvider : IDataInventorySearchProvider
    {
        private readonly IElasticDocumentClient _context;
        private readonly IDbExecuter _dbExecuter;
        private readonly string AssetCategoriesAggregationKey = "SaidListNames";

        public ElasticDataInventorySearchProvider(IElasticDocumentClient context, IDbExecuter dbExecuter)
        {
            _context = context;
            _dbExecuter = dbExecuter;
        }

        #region IDataInventorySearchProvider Implementation
        public DataInventorySearchResultDto GetSearchResults(FilterSearchDto dto)
        {
            DataInventorySearchResultDto resultDto = new DataInventorySearchResultDto();

            SearchRequest<DataInventory> searchRequest = GetSearchRequest(dto);
            searchRequest.Size = 1000;
            searchRequest.TrackTotalHits = true;

            ElasticResult<DataInventory> result = GetElasticResult(dto, resultDto, searchRequest);

            if (result.Documents?.Any() == true)
            {
                resultDto.DataInventoryResults = result.Documents.Select(x => x.ToDto()).ToList();
                resultDto.SearchTotal = result.SearchTotal;
            }

            return resultDto;
        }

        public FilterSearchDto GetSearchFilters(FilterSearchDto dto)
        {           
            SearchRequest<DataInventory> searchRequest = GetSearchRequest(dto);
            searchRequest.Aggregations = NestHelper.GetFilterAggregations<DataInventory>();
            searchRequest.Size = 0;

            //get aggregation results
            FilterSearchDto resultDto = new FilterSearchDto();

            try
            {
                ElasticResult<DataInventory> elasticResult = _context.SearchAsync(searchRequest).Result;
                resultDto.FilterCategories = elasticResult.Aggregations.ToFilterCategories<DataInventory>(dto.FilterCategories);
            }
            catch (AggregateException ex)
            {
                Logger.Error($"Data Inventory Elasticsearch query failed. Exception: {ex.Message}", ex);
            }            

            return resultDto;
        }

        public DataInventorySensitiveSearchResultDto DoesItemContainSensitive(DataInventorySensitiveSearchDto dto)
        {
            List<QueryContainer> must = new List<QueryContainer>();
            must.AddMatch<DataInventory>(x => x.IsSensitive, "true");

            if (string.Equals(dto.SearchTarget, GlobalConstants.DataInventorySearchTargets.SAID, StringComparison.OrdinalIgnoreCase))
            {
                must.AddMatch<DataInventory>(x => x.AssetCode, dto.SearchText);
            }
            else if (string.Equals(dto.SearchTarget, GlobalConstants.DataInventorySearchTargets.SERVER, StringComparison.OrdinalIgnoreCase))
            {
                must.AddMatch<DataInventory>(x => x.ServerName, dto.SearchText);
            }
            else
            {
                must.AddMatch<DataInventory>(x => x.DatabaseName, dto.SearchText);
            }

            BoolQuery boolQuery = new BoolQuery
            {
                Must = must,
                MustNot = BuildMustNotQuery()
            };

            SearchRequest<DataInventory> request = new SearchRequest<DataInventory>()
            {
                Size = 1,
                Query = boolQuery
            };

            DataInventorySensitiveSearchResultDto resultDto = new DataInventorySensitiveSearchResultDto();
            resultDto.HasSensitive = GetElasticResult(dto, resultDto, request).Documents?.Any() == true;

            return resultDto;
        }

        public DataInventoryAssetCategoriesDto GetCategoriesByAsset(string search)
        {
            DataInventoryAssetCategoriesDto resultDto = new DataInventoryAssetCategoriesDto()
            {
                DataInventoryCategories = new List<DataInventoryCategoryDto>(),
                DataInventoryEvent = new DataInventoryEventDto()
                {
                    SearchCriteria = search,
                    QuerySuccess = true
                }
            };

            BoolQuery mustNotOnly = new BoolQuery
            {
                MustNot = BuildMustNotQuery()
            };

            Task<ElasticResult<DataInventory>> allCategoriesTask = _context.SearchAsync(GetBaseSaidListRequest(mustNotOnly));

            List<QueryContainer> filters = new List<QueryContainer>();
            filters.AddMatch<DataInventory>(x => x.AssetCode, search);
            filters.AddMatch<DataInventory>(x => x.IsSensitive, "true");

            BoolQuery mustNotAndFilters = new BoolQuery
            {
                Filter = filters,
                MustNot = BuildMustNotQuery()
            };

            Task<ElasticResult<DataInventory>> assetCategoriesTask = _context.SearchAsync(GetBaseSaidListRequest(mustNotAndFilters));

            try
            {
                List<string> allNames = ExtractAggregationBucketKeys(allCategoriesTask);
                List<string> assetNames = ExtractAggregationBucketKeys(assetCategoriesTask);

                resultDto.DataInventoryCategories = allNames.Select(x => new DataInventoryCategoryDto()
                {
                    Category = x,
                    IsSensitive = assetNames.Contains(x)
                }).ToList();
            }
            catch (AggregateException ex)
            {
                HandleAggregateException(ex, resultDto.DataInventoryEvent);
            }

            return resultDto;
        }

        public bool SaveSensitive(List<DataInventoryUpdateDto> dtos)
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
                        DataInventoryUpdateDto dto = dtos.FirstOrDefault(x => x.BaseColumnId == di.Id);
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
        private ElasticResult<DataInventory> GetElasticResult(FilterSearchDto dto, DataInventoryEventableDto resultDto, SearchRequest<DataInventory> searchRequest)
        {
            resultDto.DataInventoryEvent = new DataInventoryEventDto()
            {
                SearchCriteria = dto.ToString(),
                QuerySuccess = true
            };

            try
            {
                return _context.SearchAsync(searchRequest).Result;
            }
            catch (AggregateException ex)
            {
                HandleAggregateException(ex, resultDto.DataInventoryEvent);
            }

            return new ElasticResult<DataInventory>();
        }

        private SearchRequest<DataInventory> GetSearchRequest(FilterSearchDto dto)
        {
            BoolQuery searchQuery = dto.ToSearchQuery<DataInventory>();
            searchQuery.MustNot = BuildMustNotQuery();

            return new SearchRequest<DataInventory>
            {
                Query = searchQuery
            };
        }

        private List<QueryContainer> BuildMustNotQuery()
        {
            return new List<QueryContainer>()
            {
                new ExistsQuery()
                {
                    Field = Infer.Field<DataInventory>(x => x.ExpirationDateTime)
                }
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

        private void HandleAggregateException(AggregateException ex, DataInventoryEventDto eventDto)
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
        #endregion
    }
}
