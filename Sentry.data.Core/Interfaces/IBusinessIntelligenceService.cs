using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IBusinessIntelligenceService
    {
        BusinessIntelligenceDto GetBusinessIntelligenceDto(int datasetId);
        BusinessIntelligenceHomeDto GetHomeDto();
        List<string> Validate(BusinessIntelligenceDto dto);
        bool CreateAndSaveBusinessIntelligenceDataset(BusinessIntelligenceDto dto);
    }
}
