﻿using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public class DaleEventDto
    {
        public string Criteria { get; set; }
        public string Destiny { get; set; }
        public int QuerySeconds { get; set; }
        public int QueryRows { get; set; }
        public bool QuerySuccess { get; set; }
    }
}
