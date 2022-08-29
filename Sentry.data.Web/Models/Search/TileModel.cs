using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class TileModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string TileTitle { get; set; }
        public string FavoriteTitle { get; set; }
        public bool IsFavorite { get; set; }
        public string Category { get; set; }
        public string AbbreviatedCategory { get; set; }
        public string Color { get; set; }
        public bool IsSecured { get; set; }
        public string LastActivityShortDate { get; set; }
        public bool IsReport { get; set; }
        public string AbbreviatedCategories { get; set; }
        public string ReportType { get; set; }
        public string UpdateFrequency { get; set; }
        public string ContactNames { get; set; }
        public List<string> BusinessUnits { get; set; }
        public List<string> Functions { get; set; }
        public List<string> Tags { get; set; }
    }
}