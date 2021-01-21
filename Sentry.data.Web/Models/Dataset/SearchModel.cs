using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public class SearchModel
    {
        public SearchModel(Dataset ds, IAssociateInfoProvider _associateInfoProvider)
        {

            Sentry.Associates.Associate sentryAssociate = String.IsNullOrWhiteSpace(ds.PrimaryOwnerId)? null : _associateInfoProvider.GetAssociateInfo(ds.PrimaryOwnerId);
            
            if (ds.DatasetCategories.Count > 1)
            {
                List<string> catNameList = new List<string>();

                foreach (Category cat in ds.DatasetCategories)
                {
                    // add either Name or Abbreviated Name (if exists)
                    catNameList.Add((!string.IsNullOrWhiteSpace(cat.AbbreviatedName)) ? cat.AbbreviatedName : cat.Name);
                }

                this.Color = "darkgray";
                this.Category = string.Join(", ", catNameList);
                this.AbbreviatedCategory = this.Category;
                this.BannerColor = "categoryBanner-" + this.Color;
                this.BorderColor = "borderSide_" + this.Color;
            }
            else
            {
                this.Color = ds.DatasetCategories.First().Color;
                this.Category = ds.DatasetCategories.First().Name;
                this.AbbreviatedCategory = (!String.IsNullOrWhiteSpace(ds.DatasetCategories.First().AbbreviatedName)) ? ds.DatasetCategories.First().AbbreviatedName : ds.DatasetCategories.First().Name;
                this.BannerColor = "categoryBanner-" + this.Color;
                this.BorderColor = "borderSide_" + this.Color;
            }

            this.Categories = ds.DatasetCategories.Select(x => x.Name).ToList();
            this.CategoryNames = string.Join(", ", this.Categories);
            this.IsSecured = ds.IsSecured;
            this.DatasetName = ds.DatasetName;
            this.DatasetId = ds.DatasetId;
            this.DatasetDesc = ds.DatasetDesc;
            this.DatasetInformation = ds.DatasetInformation;
            this.SentryOwnerName = (sentryAssociate == null) ? null : Sentry.data.Core.Helpers.DisplayFormatter.FormatAssociateName(sentryAssociate);
            //this.DistinctFileExtensions = ds.DatasetFiles.Select(x => Utilities.GetFileExtension(x.FileName).ToLower()).Distinct().ToList();
            this.Frequencies = null;
            this.BusinessUnits = ds.BusinessUnits.Select(x => x.Name).ToList();
            this.DatasetFunctions = ds.DatasetFunctions.Select(x => x.Name).ToList();

            //if (ds.DatasetFiles.Any())
            //{
            //    this.ChangedDtm = ds.DatasetFiles.Max(x => x.ModifiedDTM).ToShortDateString();
            //}
            //else
            //{
            //    this.ChangedDtm = ds.ChangedDtm.ToShortDateString();
            //}

            this.Type = ds.DatasetType;

            if (ds.DatasetType == GlobalConstants.DataEntityCodes.REPORT)
            {
                ReportType type = (ReportType)ds.DatasetFileConfigs.First().FileTypeId;
                this.DistinctFileExtensions = new List<string> { type.ToString() };
                this.Tags = ds.Tags.Select(s => s.GetSearchableTag()).ToList();
                
                if (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.Location))
                {
                    Location = (ds.Metadata.ReportMetadata.GetLatest) ?ds.Metadata.ReportMetadata.Location + GlobalConstants.BusinessObjectExhibit.GET_LATEST_URL_PARAMETER : ds.Metadata.ReportMetadata.Location;
                }
                else
                {
                    Location = null;
                }
                //Location = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.Location)) ? ds.Metadata.ReportMetadata.Location : null;
                LocationType = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.LocationType)) ? ds.Metadata.ReportMetadata.LocationType : null;
                this.UpdateFrequency = (Enum.GetName(typeof(ReportFrequency), ds.Metadata.ReportMetadata.Frequency) != null) ? Enum.GetName(typeof(ReportFrequency), ds.Metadata.ReportMetadata.Frequency) : "Not Specified";
                this.Link = "/BusinessIntelligence/Detail/" + ds.DatasetId;
                
                if(ds.Metadata.ReportMetadata.Contacts != null)
                {
                    List<ContactInfoModel> contactModels = new List<ContactInfoModel>();
                    foreach(var contact in ds.Metadata.ReportMetadata.Contacts)
                    {
                        Associates.Associate user = _associateInfoProvider.GetAssociateInfo(contact);
                        contactModels.Add(new ContactInfoModel()
                        {
                            Id = user.Id,
                            Name = user.FirstName + " " + user.LastName,
                            Email = user.WorkEmailAddress
                        });
                    }
                    this.ContactDetails = contactModels;
                }
                else
                {
                    this.ContactDetails = new List<ContactInfoModel>();
                }
            }
            else
            {
                this.Link = "/Dataset/Detail/" + ds.DatasetId;
                this.DistinctFileExtensions = new List<string>();//ds.DatasetFiles.Select(x => Utilities.GetFileExtension(x.FileName).ToLower()).Distinct().ToList();
            }

            this.CreatedDtm = ds.DatasetDtm.ToShortDateString();
        }

        public string Category { get; set; }
        public List<string> Categories { get; set; }
        public string AbbreviatedCategory { get; set; }
        public string CategoryNames { get; set; }

        public string DatasetName { get; set; }

        public int DatasetId { get; set; }

        public string DatasetDesc { get; set; }

        public string DatasetInformation { get; set; }

        public string SentryOwnerName { get; set; }

        public List<string> DistinctFileExtensions { get; set; }

        public List<string> Frequencies { get; set; }
        public string UpdateFrequency { get; set; }

        public bool IsSecured { get; set; }

        public string ChangedDtm { get; set; }
        public string CreatedDtm { get; set; }

        public string Color { get; set; }
        public string BannerColor { get; set; }
        public string BorderColor { get; set; }
        public string Location { get; set; }
        public string LocationType { get; set; }
        public string Type { get; set; }
        public string Link { get; set; }
        public List<SearchableTag> Tags { get; set; }
        public Boolean IsFavorite { get; set; }
        public Boolean CanEditDataset { get; set; }
        public List<string> BusinessUnits { get; set; }
        public List<string> DatasetFunctions { get; set; }
        public int PageViews { get; set; }
        public List<ContactInfoModel> ContactDetails { get; set; }
    }
}
