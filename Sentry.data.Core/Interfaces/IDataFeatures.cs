﻿using Sentry.FeatureFlags;

namespace Sentry.data.Core
{
    public interface IDataFeatures
    {
        IFeatureFlag<bool> Expose_BusinessArea_Pages_CLA_1424 { get; }
    }
}
