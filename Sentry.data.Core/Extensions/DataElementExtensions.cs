using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sentry.data.Core
{
    public static class DataElementExtensions
    {

        public static void ToSchemaModel(this DataElement de, SchemaModel model)
        {
            model.SchemaID = de.DataElement_ID;
            model.Format = de.FileFormat;
            model.Header = "true";
            model.Delimiter = de.Delimiter;
            model.HiveDatabase = de.HiveDatabase;
            model.HiveTable = de.HiveTable;
            model.HiveLocation = de.HiveLocation;

            DataObject dObj = de.DataObjects.FirstOrDefault();

            List<ColumnModel> ColumnModelList = new List<ColumnModel>();

            if (dObj != null)
            {
                foreach (DataObjectField dof in dObj.DataObjectFields)
                {
                    ColumnModel cm = new ColumnModel();
                    cm.Name = dof.DataObjectField_NME;
                    cm.DataType = dof.DataType;
                    cm.Nullable = dof.Nullable.ToString();
                    cm.Length = dof.Length;
                    cm.Precision = dof.Precision;
                    cm.Scale = dof.Scale;

                    ColumnModelList.Add(cm);
                }
            }

            model.Columns = ColumnModelList;
        }

        public static void ToHiveCreateModel(this DataElement de, HiveTableCreateModel model)
        {
            SchemaModel schemaModel = new SchemaModel();
            de.ToSchemaModel(schemaModel);
            model.Schema = schemaModel;
        }
        public static void ToHiveDeleteModel(this DataElement de, HiveTableDeleteModel model)
        {
            model.SchemaId = de.DataElement_ID;
            model.HiveDatabase = de.HiveDatabase;
            model.HiveTable = de.HiveTable;
        }

        public static string GenerateSASLibary(this DataElementDto dto, IDatasetContext dsContext)
        {
            return CommonExtensions.GenerateSASLibaryName(dsContext.GetById<Dataset>(dto.ParentDatasetId));
        }

        public static void SendIncludeInSasEmail(this DataElement schema, IApplicationUser user, IEmailService emailService)
        {
            if (schema.IsInSAS && Configuration.Config.GetHostSetting("SendIncludeInSasEmail") == "true")
            {
                StringBuilder bodySb = new StringBuilder();

                if (schema.DataObjects.Count == 0)
                {
                    bodySb.Append($"<p>{user.DisplayName} has requested {schema.HiveTable} be added to {schema.SasLibrary}.</p>");
                }
                else if (schema.DataObjects.Count > 0)
                {
                    bodySb.Append($"<p>{user.DisplayName} has updated {schema.HiveTable} schema.  Can you please refresh {schema.HiveTable} within {schema.SasLibrary} library.</p>");
                }

                bodySb.Append($"<p>Thank you from your friendly data.sentry.com Administration team</p>");

                if (Configuration.Config.GetHostSetting("EmailDSCSupportAsCC") == "true")
                {
                    emailService.SendGenericEmail(Configuration.Config.GetHostSetting("SASAdministrationEmail"), $"Add {schema.HiveTable} to {schema.SasLibrary}", bodySb.ToString(), $"{user.EmailAddress};DSCSupport@sentry.com");
                }
                else
                {
                    emailService.SendGenericEmail(Configuration.Config.GetHostSetting("SASAdministrationEmail"), $"Add {schema.HiveTable} to {schema.SasLibrary}", bodySb.ToString(), $"{user.EmailAddress}");
                }
            }
        }
    }
}
