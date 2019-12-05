using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Sentry.data.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class DataFlowModel
    {
        public DataFlowModel(DataFlowDto dto)
        {
            Id = dto.Id;
            FlowGuid = dto.FlowGuid;
            Name = dto.Name;
            CreatedBy = dto.CreatedBy;
            CreatedDTM = dto.CreateDTM;
        }

        public int Id { get; set; }
        public Guid FlowGuid { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDTM { get; set; }
    }
}