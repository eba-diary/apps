﻿using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ClaimIQAction : BaseAction
    {
        public ClaimIQAction() { }
        public ClaimIQAction(IClaimIQActionProvider claimIqActionProvider) : base(claimIqActionProvider) { }
    }
}