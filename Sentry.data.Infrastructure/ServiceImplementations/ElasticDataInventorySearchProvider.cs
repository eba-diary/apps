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
            List<QueryContainer> must = new List<QueryContainer>();
            List<QueryContainer> should = new List<QueryContainer>();
            int minShould = 0;

            if (dto.Destiny == Core.GlobalEnums.DaleDestiny.Advanced)
            {
                //targeted search of only fields with criteria            
                must.AddWildcard<DataInventory>(x => x.AssetCode, dto.AdvancedCriteria.Asset);
                must.AddWildcard<DataInventory>(x => x.ServerName, dto.AdvancedCriteria.Server);
                must.AddWildcard<DataInventory>(x => x.DatabaseName, dto.AdvancedCriteria.Database);
                must.AddWildcard<DataInventory>(x => x.BaseName, dto.AdvancedCriteria.Object);
                must.AddWildcard<DataInventory>(x => x.TypeDescription, dto.AdvancedCriteria.ObjectType);
                must.AddWildcard<DataInventory>(x => x.ColumnName, dto.AdvancedCriteria.Column);
                must.AddWildcard<DataInventory>(x => x.SourceName, dto.AdvancedCriteria.SourceType);

                should.AddMatch<DataInventory>(x => x.AssetCode, dto.AdvancedCriteria.Asset);
                should.AddMatch<DataInventory>(x => x.ServerName, dto.AdvancedCriteria.Server);
                should.AddMatch<DataInventory>(x => x.DatabaseName, dto.AdvancedCriteria.Database);
                should.AddMatch<DataInventory>(x => x.BaseName, dto.AdvancedCriteria.Object);
                should.AddMatch<DataInventory>(x => x.TypeDescription, dto.AdvancedCriteria.ObjectType);
                should.AddMatch<DataInventory>(x => x.ColumnName, dto.AdvancedCriteria.Column);
                should.AddMatch<DataInventory>(x => x.SourceName, dto.AdvancedCriteria.SourceType);
            }
            else
            {
                //broad search for criteria across all searchable fields
                should.AddTextSearch<DataInventory>(x => x.AssetCode, dto.Criteria);
                should.AddTextSearch<DataInventory>(x => x.ServerName, dto.Criteria);
                should.AddTextSearch<DataInventory>(x => x.DatabaseName, dto.Criteria);
                should.AddTextSearch<DataInventory>(x => x.BaseName, dto.Criteria);
                should.AddTextSearch<DataInventory>(x => x.TypeDescription, dto.Criteria);
                should.AddTextSearch<DataInventory>(x => x.ColumnName, dto.Criteria);
                should.AddTextSearch<DataInventory>(x => x.SourceName, dto.Criteria);

                if (should.Any())
                {
                    minShould = 1;
                }
            }

            List<QueryContainer> mustNot = new List<QueryContainer>();

            if (dto.Sensitive == Core.GlobalEnums.DaleSensitive.SensitiveOnly)
            {
                must.AddMatch<DataInventory>(x => x.IsSensitive, "true");
            }
            else
            {
                mustNot.Add(new ExistsQuery() { Field = Infer.Field<DataInventory>(x => x.ExpirationDateTime) });

                if (dto.Sensitive == Core.GlobalEnums.DaleSensitive.SensitiveNone)
                {
                    must.AddMatch<DataInventory>(x => x.IsSensitive, "false");
                }
            }

            SearchRequest<DataInventory> request = new SearchRequest<DataInventory>()
            {
                Size = 1000,
                Query = new BoolQuery()
                {
                    MustNot = mustNot,
                    Must = must,
                    Should = should,
                    MinimumShouldMatch = minShould
                }
            };

            return SearchDataInventory<DaleResultDto>(dto, request);
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

            return SearchDataInventory<DaleContainSensitiveResultDto>(dto, request);
        }

        public override bool SaveSensitive(string sensitiveBlob)
        {
            //Currently not allowed at this time when source feature flag is set to ELASTIC
            //Solution for real-time updates has not been determined
            throw new NotImplementedException();
        }

        private T SearchDataInventory<T>(DaleSearchDto dto, SearchRequest<DataInventory> searchRequest) where T : DaleEventableDto
        {
            T resultDto = Activator.CreateInstance<T>();
            resultDto.DaleEvent = new DaleEventDto()
            {
                Criteria = dto.Criteria,
                Destiny = dto.Destiny.GetDescription(),
                QuerySuccess = true,
                Sensitive = dto.Sensitive.GetDescription()
            };

            try
            {
                resultDto.SetResult(_context.Search(searchRequest));
            }
            catch (Exception ex)
            {
                resultDto.DaleEvent.QuerySuccess = false;
                resultDto.DaleEvent.QueryErrorMessage = $"Data Inventory Elasticsearch query failed. Exception: {ex}";
                Logger.Error(resultDto.DaleEvent.QueryErrorMessage, ex);
            }

            return resultDto;
        }
    }
}
