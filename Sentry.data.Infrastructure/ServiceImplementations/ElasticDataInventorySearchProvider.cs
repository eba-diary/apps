using Nest;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public class ElasticDataInventorySearchProvider : IDaleSearchProvider
    {
        private readonly IElasticContext _context;

        public ElasticDataInventorySearchProvider(IElasticContext context)
        {
            _context = context;
        }

        public DaleResultDto GetSearchResults(DaleSearchDto dto)
        {
            List<QueryContainer> should = new List<QueryContainer>();            

            if (dto.Destiny == Core.GlobalEnums.DaleDestiny.Advanced)
            {
                //only add specific properties to be searched             
                should.AddWildcard<DataInventory>(x => x.AssetCode, dto.AdvancedCriteria.Asset);
                should.AddWildcard<DataInventory>(x => x.ServerName, dto.AdvancedCriteria.Server);
                should.AddWildcard<DataInventory>(x => x.DatabaseName, dto.AdvancedCriteria.Database);
                should.AddWildcard<DataInventory>(x => x.BaseName, dto.AdvancedCriteria.Object);
                should.AddWildcard<DataInventory>(x => x.TypeDescription, dto.AdvancedCriteria.ObjectType);
                should.AddWildcard<DataInventory>(x => x.ColumnName, dto.AdvancedCriteria.Column);
                should.AddWildcard<DataInventory>(x => x.SourceName, dto.AdvancedCriteria.SourceType);
            }
            else
            {
                //Add all properties to be searched based on criteria
                should.AddWildcard<DataInventory>(x => x.AssetCode, dto.Criteria);
                should.AddWildcard<DataInventory>(x => x.ServerName, dto.Criteria);
                should.AddWildcard<DataInventory>(x => x.DatabaseName, dto.Criteria);
                should.AddWildcard<DataInventory>(x => x.BaseName, dto.Criteria);
                should.AddWildcard<DataInventory>(x => x.TypeDescription, dto.Criteria);
                should.AddWildcard<DataInventory>(x => x.ColumnName, dto.Criteria);
                should.AddWildcard<DataInventory>(x => x.SourceName, dto.Criteria);
            }

            List<QueryContainer> must = new List<QueryContainer>();

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
                From = 0,
                Size = 1000,
                Query = new BoolQuery() 
                { 
                    Must = must,
                    Should = should,
                    MinimumShouldMatch = should.Any() ? 1 : 0
                }
            };

            IList<DataInventory> results = _context.Search(request);

            DaleResultDto resultDto = new DaleResultDto() { DaleResults = results.Select(x => x.ToDto()).ToList() };

            return resultDto;
        }

        public DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dto)
        {
            throw new NotImplementedException();
        }

        public DaleCategoryResultDto GetCategoriesByAsset(string search)
        {
            throw new NotImplementedException();
        }

        public bool SaveSensitive(string sensitiveBlob)
        {
            throw new NotImplementedException();
        }
    }
}
