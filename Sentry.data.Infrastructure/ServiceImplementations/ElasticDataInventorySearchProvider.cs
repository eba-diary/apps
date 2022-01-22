using Nest;
using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

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
            resultDto.DaleResults = SearchDataInventory(dto, BuildTextSearchRequest(dto, 10000), resultDto).Select(x => x.ToDto()).ToList();
            return resultDto;
        }

        public override List<FilterCategoryDto> GetSearchFilters(DaleSearchDto dto)
        {
            AggregationDictionary aggregations = new AggregationDictionary();

            Dictionary<string, Field> filterCategoryFields = NestHelper.FilterCategoryFields<DataInventory>();

            foreach (KeyValuePair<string, Field> field in filterCategoryFields)
            {
                aggregations.Add(field.Key, new TermsAggregation(field.Key) { Field = field.Value });
            }

            SearchRequest<DataInventory> request = BuildTextSearchRequest(dto, 0);
            request.Aggregations = aggregations;

            AggregateDictionary results = _context.Aggregate(request);

            //Go through the results and build the list of FilterCategoryDto

            return null;
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
            resultDto.DoesContainSensitiveResults = SearchDataInventory(dto, request, resultDto).Any();

            return resultDto;
        }

        public override bool SaveSensitive(string sensitiveBlob)
        {
            //Currently not allowed at this time when source feature flag is set to ELASTIC
            //Solution for real-time updates has not been determined
            throw new NotImplementedException();
        }

        private IList<DataInventory> SearchDataInventory(DaleSearchDto dto, SearchRequest<DataInventory> searchRequest, DaleEventableDto resultDto)
        {
            resultDto.DaleEvent = new DaleEventDto()
            {
                Criteria = dto.Destiny == Core.GlobalEnums.DaleDestiny.Advanced ? dto.AdvancedCriteria.ToEventString() : dto.Criteria,
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

            return new List<DataInventory>();
        }

        private SearchRequest<DataInventory> BuildTextSearchRequest(DaleSearchDto dto, int searchSize)
        {
            List<QueryContainer> must = new List<QueryContainer>();
            List<QueryContainer> should = new List<QueryContainer>();
            int minShould = 0;

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

                minShould = 1;
            }

            List<QueryContainer> filter = new List<QueryContainer>();

            if (dto.HasFilterFor("Sensitivity", "Sensitive"))
            {
                filter.AddMatch<DataInventory>(x => x.IsSensitive, "true");
            }
            else if (dto.HasFilterFor("Sensitivity", "Public"))
            {
                filter.AddMatch<DataInventory>(x => x.IsSensitive, "false");
            }

            if (dto.HasFilterFor("Environment", "Prod"))
            {
                filter.AddMatch<DataInventory>(x => x.ProdType, "P");
            }
            else if (dto.HasFilterFor("Environment", "NonProd"))
            {
                filter.AddMatch<DataInventory>(x => x.ProdType, "D");
            }

            return new SearchRequest<DataInventory>()
            {
                Size = searchSize,
                Query = new BoolQuery()
                {
                    MustNot = new List<QueryContainer>() { new ExistsQuery() { Field = Infer.Field<DataInventory>(x => x.ExpirationDateTime) } },
                    Filter = filter,
                    Must = must,
                    Should = should,
                    MinimumShouldMatch = minShould
                }
            };
        }
    }
}
