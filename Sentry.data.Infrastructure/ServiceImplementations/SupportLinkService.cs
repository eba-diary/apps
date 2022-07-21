using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Interfaces;
using Sentry.data.Core.DTO.Admin;
using Sentry.data.Core;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure.ServiceImplementations
{
    public class SupportLinkService : ISupportLink
    {
        private readonly IDatasetContext _datasetContext;

        public SupportLinkService(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }
        public void AddSupportLink(SupportLinkDto supportLinkDto)
        {
            try
            {
                // create new SupportLink
                SupportLink supportLink = new SupportLink()
                {
                    SupportLinkId = supportLinkDto.SupportLinkId,
                    Name = supportLinkDto.Name,
                    Description = supportLinkDto.Description,
                    Url = supportLinkDto.Url,
                };

                // save SupportLink
                _datasetContext.Add(supportLink);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding SupportLink with id {supportLinkDto.SupportLinkId} with Name {supportLinkDto.Name} Description {supportLinkDto.Description} and Url {supportLinkDto.Url}");
                throw;
            }
        }

        public void RemoveSupportLink(int id)
        {
            try
            {
                // Does SupportLink with Id exist
                SupportLink exists = _datasetContext.SupportLinks.Where(w => w.SupportLinkId == id).FirstOrDefault();
                
                _datasetContext.Remove(exists);
                _datasetContext.SaveChanges();
            } 
            catch (Exception ex)
            {
                Logger.Error($"SupportLink with id {id} does not exist", ex);
                throw;
            }
        }
    }
}
