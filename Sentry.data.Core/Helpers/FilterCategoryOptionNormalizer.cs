using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public static class FilterCategoryOptionNormalizer
    {
        private static readonly Lazy<Dictionary<string, Dictionary<string, string>>> _config = new Lazy<Dictionary<string, Dictionary<string, string>>>(() => new Dictionary<string, Dictionary<string, string>>() 
        {
            { 
                FilterCategoryNames.ENVIRONMENT, new Dictionary<string, string>()
                {
                    { FilterCategoryOptions.ENVIRONMENT_PROD, "Prod" },
                    { FilterCategoryOptions.ENVIRONMENT_NONPROD, "NonProd" }
                }
            }
        });

        public static string Normalize(string category, string value)
        {
            if (_config.Value.TryGetValue(category, out Dictionary<string, string> categoryConfig) && categoryConfig.TryGetValue(value, out string result))
            {
                return result;
            }

            return value;
        }

        public static string Denormalize(string category, string value)
        {
            if (_config.Value.TryGetValue(category, out Dictionary<string, string> categoryConfig) && categoryConfig.ContainsValue(value))
            {
                return categoryConfig.FirstOrDefault(x => x.Value == value).Key;
            }

            return value;
        }
    }
}
