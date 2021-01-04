using Sentry.data.Core;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class DFModel
    {
        public DFModel(DataFlowDto dto)
        {
            Id = dto.Id;
            FlowGuid = dto.FlowGuid;
            SaidKeyCode = dto.SaidKeyCode;
            Name = dto.Name;
            CreatedBy = dto.CreatedBy;
            CreatedDTM = dto.CreateDTM;
            FlowStorageCode = dto.FlowStorageCode;
            AssociatedJobs = dto.AssociatedJobs;
        }

        public int Id { get; set; }
        public Guid FlowGuid { get; set; }
        public string SaidKeyCode { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDTM { get; set; }
        public string FlowStorageCode { get; set; }
        /// <summary>
        /// Associated retriever job which pull data from external sources
        /// </summary>
        public List<int> AssociatedJobs { get; set; }
    }
}