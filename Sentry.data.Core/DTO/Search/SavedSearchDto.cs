﻿using Newtonsoft.Json.Linq;

namespace Sentry.data.Core
{
    public class SavedSearchDto : FilterSearchDto
    {
        public int SavedSearchId { get; set; }
        public string SearchType { get; set; }
        public string AssociateId { get; set; }
        public bool AddToFavorites { get; set; }
        public JObject ResultConfiguration { get; set; }
    }
}