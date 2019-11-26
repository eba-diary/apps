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

        UserSecurity GetUserSecurityById(int datasetId);
        List<KeyValuePair<string,string>> GetAllTagGroups();
        List<FavoriteDto> GetDatasetFavoritesDto(int id);
        byte[] GetImageData(string url, int? t);
        bool SaveTemporaryPreviewImage(ImageDto dto);
    }
}
