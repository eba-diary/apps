using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class AssetDynamicDetails
    {
        private DateTime _lastRefreshDate;
        public DateTime LastRefreshDate
        {
            get
            {
                return _lastRefreshDate;
            }
        }

        public AssetState State { get; set; }

        public AssetDynamicDetails(AssetState state, DateTime lastRefreshDate)
        {
            State = state;
            _lastRefreshDate = lastRefreshDate;
        }
    }
}
