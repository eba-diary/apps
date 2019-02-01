using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class ColumnModel
    {
        public ColumnModel(ColumnDTO dto)
        {
            Name = dto.Name;
            DataType = dto.DataType;
            Nullable = dto.Nullable;
            Length = dto.Length;
            Precision = dto.Precision;
            Scale = dto.Scale;
        }

        public string Name { get; set; }
        public string DataType { get; set; }
        public bool? Nullable { get; set; }
        public string Length { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }
    }
}