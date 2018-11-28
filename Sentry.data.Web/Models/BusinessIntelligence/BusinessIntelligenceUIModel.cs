using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web.Models
{
    public class BusinessIntelligenceUIModel
    {
        public BusinessIntelligenceUIModel(Category cat)
        {
            Id = cat.Id;
            Name = cat.Name;
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
    }
}