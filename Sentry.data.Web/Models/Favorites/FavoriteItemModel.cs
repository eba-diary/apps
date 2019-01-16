﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class FavoriteItemModel
    {
        public FavoriteItemModel()
        {

        }

        public string DisplayTitle
        {
            get
            {
                return Core.Helpers.DisplayFormatter.FormatFavoriteTitle(this.Title);
            }
        }

        public string Title { get; set; }
        public string Url { get; set; }
        public int Sequence { get; set; }
        public string Img { get; set; }
        public string FeedName { get; set; }
        public string FeedUrlType { get; set; }
        public string FeedUrl { get; set; }
        public int FeedId { get; set; }
    }
}