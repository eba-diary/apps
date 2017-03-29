using System;
using System.ComponentModel.DataAnnotations;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class DataFeedItemModel
    {
        public DataFeedItemModel()
        {

        }

        public DataFeedItemModel(DataFeedItem dfi)
        {
            this.PublishDate = dfi.PublishDate;
            this.URL = dfi.URL;
            this.ShortDescription = dfi.ShortDescription;
            this.LongDescription = dfi.LongDescription;
        }

        [Required()]
        [MaxLength(255)]
        public DateTime PublishDate { get; set; }

        public string DisplayPublishDate
        {
            get
            {
                return this.PublishDate.ToShortDateString() + " " + this.PublishDate.ToShortTimeString();
            }
        }

        [Required()]
        [MaxLength(255)]
        public string URL { get; set; }

        [Required()]
        [MaxLength(255)]
        public string ShortDescription { get; set; }

        [Required()]
        [MaxLength(255)]
        public string LongDescription { get; set; }


    }
}