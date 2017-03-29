using System;
using System.Linq;
using System.Text;
using System.IO;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using Sentry.data.Infrastructure.Mappings.Primary;
using NHibernate.Dialect;
using System.Configuration;
using System.Data;

namespace Sentry.data.DatabaseGenerator
{
    public static class Class1
    {
        public static void Main()
        {
            StringBuilder sb = new StringBuilder();

            TextWriter standardOut = Console.Out;
            Console.SetOut(new StringWriter(sb));

            global::NHibernate.Cfg.Configuration config = new global::NHibernate.Cfg.Configuration();
            config.DataBaseIntegration((db) =>
            {
                db.ConnectionString = ConfigurationManager.AppSettings["connectionString"];
                db.Dialect<MsSql2012Dialect>();
            });

            ModelMapper modelMapper = new ModelMapper();
            string strDefaultNamespace = typeof(UserMapping).Namespace;

            modelMapper.AddMappings(typeof(UserMapping).Assembly.GetExportedTypes().Where((t) => t.Namespace == strDefaultNamespace));
            //Make the model mapper default all strings to use AnsiString, instead of Unicode
            modelMapper.BeforeMapProperty += (mi, propertyPath, map) =>
            {
                if (propertyPath.LocalMember.GetPropertyOrFieldType() == typeof(string))
                {
                    map.Type(NHibernateUtil.AnsiString);
                }
            };

            NHibernate.Cfg.MappingSchema.HbmMapping mappings = modelMapper.CompileMappingForAllExplicitlyAddedEntities();
            config.AddMapping(mappings);

            //Set a breakpoint on this next line to get all the mapping hbm files
            string mappingXml = mappings.AsString();

            NHibernate.Tool.hbm2ddl.SchemaExport exporter = new NHibernate.Tool.hbm2ddl.SchemaExport(config);

            exporter.Execute(true, true, false);

            string primaryKeyConstraintRenameScript = GetPrimaryKeyConstraintRenameScript();

            ISessionFactory sf = config.BuildSessionFactory();
            using (IStatelessSession session = sf.OpenStatelessSession())
            {
                using (IDbConnection conn = session.Connection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = primaryKeyConstraintRenameScript;
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            //Set a breakpoint on this next line to get the DDL it generated
            string script = sb.ToString();

            Console.SetOut(standardOut);
            Console.WriteLine("Success.  Press Enter to continue.");
            Console.ReadLine();

        }

        public static string GetPrimaryKeyConstraintRenameScript()
        {
            return new System.Xml.Linq.XElement("string", "\n                   BEGIN TRANSACTION\nDECLARE @Rename nvarchar(MAX)\nDECLARE RenameCursor CURSOR FOR\n    SELECT\n            'EXEC sp_rename ''[' + c.CONSTRAINT_SCHEMA + '].[' + c.CONSTRAINT_NAME + ']'', ''PK_' + c.TABLE_NAME + ''', ''OBJECT'''\n        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS c\n        WHERE\n            c.CONSTRAINT_TYPE = 'PRIMARY KEY'\n            AND\n            c.TABLE_NAME IS NOT NULL\n        ORDER BY c.TABLE_NAME\nOPEN RenameCursor\nFETCH NEXT\n    FROM RenameCursor\n    INTO @Rename\nWHILE @@FETCH_STATUS = 0\nBEGIN\n    EXEC sp_executesql @Rename\n    FETCH NEXT\n        FROM RenameCursor\n        INTO @Rename\nEND\nCLOSE RenameCursor\nDEALLOCATE RenameCursor\nCOMMIT TRANSACTION\n               ").Value;

        }
    }
}
