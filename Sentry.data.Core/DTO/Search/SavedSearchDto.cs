﻿namespace Sentry.data.Core
{
    public class SavedSearchDto : FilterSearchDto
    {
        public string SearchName { get; set; }
        public string AssociateId { get; set; }
        public bool AddToFavorites { get; set; }
    }
}
