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
                should.AddMatchAndWildcard<DataInventory>(x => x.AssetCode, dto.Criteria);
                should.AddMatchAndWildcard<DataInventory>(x => x.ServerName, dto.Criteria);
                should.AddMatchAndWildcard<DataInventory>(x => x.DatabaseName, dto.Criteria);
                should.AddMatchAndWildcard<DataInventory>(x => x.BaseName, dto.Criteria);
                should.AddMatchAndWildcard<DataInventory>(x => x.TypeDescription, dto.Criteria);
                should.AddMatchAndWildcard<DataInventory>(x => x.ColumnName, dto.Criteria);
                should.AddMatchAndWildcard<DataInventory>(x => x.SourceName, dto.Criteria);

                if (should.Any())
                {
                    minShould = 1;
                }
            }

            if (dto.Sensitive == Core.GlobalEnums.DaleSensitive.SensitiveOnly)
            {
                must.AddMatch<DataInventory>(x => x.IsSensitive, "true");
            }
            else if (dto.Sensitive == Core.GlobalEnums.DaleSensitive.SensitiveNone)
            {
                must.AddMatch<DataInventory>(x => x.IsSensitive, "false");
            }

            SearchRequest<DataInventory> request = new SearchRequest<DataInventory>()
            {
                Size = 1000,
                Query = new BoolQuery()
                {
                    MustNot = new List<QueryContainer>() { new ExistsQuery() { Field = Infer.Field<DataInventory>(x => x.ExpirationDateTime) } },
                    Must = must,
                    Should = should,
                    MinimumShouldMatch = minShould
                }
            };

            DaleResultDto resultDto = new DaleResultDto();
            resultDto.DaleResults = SearchDataInventory(dto, request, resultDto).Select(x => x.ToDto()).ToList();

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
    }
}
