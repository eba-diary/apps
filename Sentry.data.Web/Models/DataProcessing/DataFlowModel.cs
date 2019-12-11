using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;


namespace Sentry.data.Web
{
    public class DataFlowModel
    {
        public DataFlowModel()
        {
            SchemaMaps = new List<SchemaMapModel>();
        }

        /// <summary>
        /// How is data getting into DSC (Push or Pull)
        /// </summary>
        /// 
        [DisplayName("How will data be ingested into DSC?")]
        public IngestionType IngestionType { get; set; }

        public string SelectedIngestionType { get; set; }

        /// <summary>
        /// Is the incoming data compressed?
        /// </summary>
        /// 
        [DisplayName("Is incoming data compressed?")]
        public bool IsCompressed { get; set; }

        /// <summary>
        /// Target
        /// </summary>
        public int SchemaId { get; set; }

        public List<SchemaMapModel> SchemaMaps { get; set; }
    }
}