using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class BaseAssetModel
    {
        /// <summary>
        /// Parameterless constructor is needed for view binding
        /// </summary>
        public BaseAssetModel()
        {

        }

        public BaseAssetModel(Asset asset)
        {
            this.Id = asset.Id;
            this.Name = asset.Name;
            this.Description = asset.Description;

            //this.DynamicDetails = asset.DynamicDetails;
            //this.DynamicDetails = asset.DynamicDetails;

            //this.State = asset.DynamicDetails.State;
            //this.LastRefreshDate = asset.DynamicDetails.LastRefreshDate;
        }

        public int Id { get; set; }

        [Required()]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(4000)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string Description { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string ShortDescription
        {
            get
            {
                if ((Description != null) && Description.Length > 100)
                {
                    return Description.Substring(0, 99) + "...";
                }
                else
                {
                    return Description;
                }
            }
        }

        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string LastRefreshDateDisplay
        {
            get
            {
                if (LastRefreshDate.Date == DateTime.Today)
                {
                    return "Today: " + LastRefreshDate.ToShortTimeString();
                }
                else
                {
                    return LastRefreshDate.ToShortDateString() + " " + LastRefreshDate.ToShortTimeString();
                }
            }
        }

        public AssetDynamicDetails DynamicDetails { get; set; }

        [Required]
        [DisplayName("Asset State")]
        public AssetState State
        {
            get
            {
                return this.DynamicDetails.State;
            }
        }

        [Required]
        [DisplayName("Asset Last Refresh Date")]
        public DateTime LastRefreshDate
        {
            get
            {
                return this.DynamicDetails.LastRefreshDate;
            }
        }

    }
}
