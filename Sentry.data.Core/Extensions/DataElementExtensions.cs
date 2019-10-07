using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sentry.data.Core
{
    public static class DataElementExtensions
    {

        public static string GenerateSASLibary(this DataElementDto dto, IDatasetContext dsContext)
        {
            return CommonExtensions.GenerateSASLibaryName(dsContext.GetById<Dataset>(dto.ParentDatasetId));
        }

        public static void SendIncludeInSasEmail(this SchemaRevision sr, bool IsNew, IApplicationUser user, IEmailService emailService)
        {
            FileSchema schema = sr.ParentSchema as FileSchema;
            if (schema.IsInSAS && Configuration.Config.GetHostSetting("SendIncludeInSasEmail") == "true")
            {
                StringBuilder bodySb = new StringBuilder();

                if (IsNew)
                {
                    bodySb.Append($"<p>{user.DisplayName} has requested {schema.HiveTable} be added to {schema.SasLibrary}.</p>");
                }
                else
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
