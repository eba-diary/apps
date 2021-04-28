﻿using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class QueryStorageAction : BaseAction
    {
        public QueryStorageAction() { }
        public QueryStorageAction(IQueryStorageProvider queryStorageProvider) : base(queryStorageProvider) { }
    }
}