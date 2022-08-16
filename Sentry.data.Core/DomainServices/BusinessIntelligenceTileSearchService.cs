using Sentry.Associates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceTileSearchService : TileSearchService<BusinessIntelligenceTileDto>
    {
        private readonly IAssociateInfoProvider _associateInfoProvider;

        public BusinessIntelligenceTileSearchService(IAssociateInfoProvider associateInfoProvider, IDatasetContext datasetContext, IUserService userService) : base(datasetContext, userService)
        {
            _associateInfoProvider = associateInfoProvider;
        }

        protected override IQueryable<Dataset> GetDatasets()
        {
            return _datasetContext.Datasets.Where(x => x.DatasetType == GlobalConstants.DataEntityCodes.REPORT && x.CanDisplay);
        }

        protected override BusinessIntelligenceTileDto MapToTileDto(Dataset dataset)
        {
            BusinessIntelligenceTileDto biTileDto = dataset.ToBusinessIntelligenceTileDto();

            if (dataset.Metadata?.ReportMetadata?.Contacts?.Any() == true)
            {
                List<string> contactNames = new List<string>();

                foreach (string contact in dataset.Metadata.ReportMetadata.Contacts)
                {
                    Associate associate = _associateInfoProvider.GetAssociateInfo(contact);
                    if (associate != null)
                    {
                        contactNames.Add(associate.FullName);
                    }
                }

                biTileDto.ContactNames = string.Join(", ", contactNames);
            }

            return biTileDto;
        }
    }
}
