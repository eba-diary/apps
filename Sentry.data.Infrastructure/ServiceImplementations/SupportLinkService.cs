﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Interfaces;
using Sentry.data.Core.DTO.Admin;
using Sentry.data.Core;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure
{
    public class SupportLinkService : ISupportLinkService
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
            catch (Exception)
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
                SupportLink supportLinkExists = _datasetContext.SupportLinks.Where(w => w.SupportLinkId == id).FirstOrDefault();
                
                // if this link does exist with this specified id, delete it
                if(supportLinkExists != null)
                {
                    Logger.Error($"Found Support Link {id} to remove");
                    _datasetContext.Remove(supportLinkExists);
                    _datasetContext.SaveChanges();
                } 
            } 
            catch (Exception ex)
            {
                // the case where the supportLink id is not in the database
                Logger.Error($"Error removing Support Link {id}", ex);
                throw;
            }
        }

        
        public List<SupportLinkDto> GetSupportLinks()
        {
            List<SupportLink> supportLinks = _datasetContext.SupportLinks.ToList();
            List<SupportLinkDto> supportLinkDtos = new List<SupportLinkDto>();
            foreach(SupportLink supportLink in supportLinks)
            {
                SupportLinkDto supportLinkDto = new SupportLinkDto()
                {
                    SupportLinkId = supportLink.SupportLinkId,
                    Name = supportLink.Name,
                    Description = supportLink.Description,
                    Url = supportLink.Url,
                };
                supportLinkDtos.Add(supportLinkDto);
            }
            List<SupportLinkDto> sortedSupportLinkDtos = new List<SupportLinkDto>();
            var sortedQuery = supportLinkDtos.OrderBy(x => x.Name).ToList();
            foreach(var obj in sortedQuery)
            {
                var supportLinkDto = new SupportLinkDto()
                {
                    SupportLinkId=obj.SupportLinkId,
                    Name = obj.Name,
                    Url = obj.Url,
                    Description = obj.Description,
                };
                sortedSupportLinkDtos.Add(supportLinkDto);
            }

            return sortedSupportLinkDtos;
        }
        
    }
}
