using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IBusinessIntelligenceService
    {
        BusinessIntelligenceDto GetBusinessIntelligenceDto(int datasetId);
        BusinessIntelligenceDetailDto GetBusinessIntelligenceDetailDto(int datasetId);
        BusinessIntelligenceHomeDto GetHomeDto();
        List<string> Validate(BusinessIntelligenceDto dto);
        bool CreateAndSaveBusinessIntelligence(BusinessIntelligenceDto dto);
        bool UpdateAndSaveBusinessIntelligence(BusinessIntelligenceDto dto);
        void Delete(int id);
    }
}
