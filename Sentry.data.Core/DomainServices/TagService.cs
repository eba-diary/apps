using Microsoft.Extensions.Logging;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class TagService : BaseDomainService<TagService>, ITagService
    {
        private readonly IDatasetContext _datasetContext;

        public TagService(IDatasetContext datasetContext, DomainServiceCommonDependency<TagService> commonDependency) : base (commonDependency)
        {
            _datasetContext = datasetContext;
        }

        public bool CreateAndSaveNewTag(TagDto dto)
        {
            try
            {
                CreateMetadataTag(dto);

                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving tag - ");
                return false;
            }

            return true;            
        }

        public bool UpdateAndSaveTag(TagDto dto)
        {
            try
            {
                MetadataTag tag = _datasetContext.GetById<MetadataTag>(dto.TagGroupId);

                UpdateTag(dto, tag);

                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving tag - ");
                return false;
            }

            return true;
        }

        public List<string> Validate(TagDto dto)
        {
            List<string> errors = new List<string>();
            if (dto.TagId == 0 && _datasetContext.Tags.Where(w => w.Name.ToLower() == dto.TagName.ToLower()).Count() > 0)
            {
                errors.Add("Tag name already exists");
            }

            return errors;
        }



        #region Private Methods
        private void CreateMetadataTag(TagDto dto)
        {
            MetadataTag tag = MapMetadataTag(dto, new MetadataTag());
            _datasetContext.Add(tag);
        }

        private void UpdateTag(TagDto dto, MetadataTag tag)
        {
            MapMetadataTag(dto, tag);
        }

        private MetadataTag MapMetadataTag(TagDto dto, MetadataTag tag)
        {
            tag.Name = dto.TagName;
            tag.Description = dto.Description;
            tag.Group = _datasetContext.GetById<TagGroup>(dto.TagGroupId);
            tag.Created = dto.Created;
            tag.CreatedBy = dto.CreatedBy;

            return tag;
        }
        #endregion

    }
}
