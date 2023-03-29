using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.DTO.Retriever
{
    public class DataSourceTokenDto
    {
        public int Id { get; set; }
        public string CurrentToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? CurrentTokenExp { get; set; }
        public string TokenName { get; set; }
        public string TokenUrl { get; set; }
        public int TokenExp { get; set; }
        public string Scope { get; set; }
        public bool ToDelete { get; set; }
        public bool Enabled { get; set; }
    }
}
