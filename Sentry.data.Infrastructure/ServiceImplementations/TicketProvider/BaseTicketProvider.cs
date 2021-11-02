﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public abstract class BaseTicketProvider : IBaseTicketProvider
    {
        public abstract string CreateChangeTicket(AccessRequest model);
        public abstract HpsmTicket RetrieveTicket(string ticketId);
        public abstract void CloseTicket(string ticketId, bool wasTicketDenied = false);
    }
}