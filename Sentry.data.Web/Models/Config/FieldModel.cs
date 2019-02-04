using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web.Models
{
    public class FieldModel
    {
        public FieldModel()
        {
            //schemaRows = new List<SchemaRow>();
            //hiveTypes = new List<SelectListItem>();
        }

        public List<FieldRowModel> schemaRows { get; set; }
    }

    [Serializable]
    public class FieldRowModel
    {
        public FieldRowModel()
        {
            List<String> numericHiveDataTypes = new List<string>()
            {
                "TINYINT", "SMALLINT", "INTEGER", "BIGINT", "FLOAT", "DOUBLE", "DECIMAL", "NUMERIC",
            };
            List<String> dateHiveDataTypes = new List<string>()
            {
                "TIMESTAMP", "DATE", "INTERVAL"
            };
            List<String> stringHiveDataTypes = new List<string>()
            {
                "STRING", "VARCHAR", "CHAR",
            };
            List<String> miscHiveDataTypes = new List<string>()
            {
                "BOOLEAN", "BINARY"
            };

            List<SelectListItem> sli = new List<SelectListItem>();

            SelectListGroup slg;
            slg = new SelectListGroup() { Name = "Numeric" };
            foreach (String s in numericHiveDataTypes)
            {
                sli.Add(new SelectListItem()
                {
                    Text = s,
                    Value = s,
                    Group = slg
                });
            }

            slg = new SelectListGroup() { Name = "Date/Time" };
            foreach (String s in dateHiveDataTypes)
            {
                sli.Add(new SelectListItem()
                {
                    Text = s,
                    Value = s,
                    Group = slg
                });
            }

            slg = new SelectListGroup() { Name = "String" };
            foreach (String s in stringHiveDataTypes)
            {
                sli.Add(new SelectListItem()
                {
                    Text = s,
                    Value = s,
                    Group = slg
                });
            }

            slg = new SelectListGroup() { Name = "Misc" };
            foreach (String s in miscHiveDataTypes)
            {
                sli.Add(new SelectListItem()
                {
                    Text = s,
                    Value = s,
                    Group = slg
                });
            }

            HiveTypes = sli.AsEnumerable();
        }

        public IEnumerable<SelectListItem> HiveTypes { get; set; }

        public virtual int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }


    }

}