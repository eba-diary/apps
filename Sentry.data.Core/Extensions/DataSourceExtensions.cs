using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class DataSourceExtensions
    {
        public static string Encrpyt(this DataSource source, IEncryptionService eService, IDatasetContext dsContext)
        {
            if (source.Is<HTTPSSource>())
            {
                string newIV = eService.GenerateNewIV();

                HTTPSSource newSource = (HTTPSSource)source;
                HTTPSSource origSource = (newSource.Id == 0)? null : (HTTPSSource)dsContext.GetById<DataSource>(source.Id);


                // Check CurrentToken
                // Initial setting of values
                // CurrentToken
                if (origSource == null && newSource.CurrentToken != null)
                {
                    newSource.CurrentToken = eService.EncryptString(newSource.CurrentToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), newIV).Item1;
                }

                //ClientPrivateId
                if (origSource == null && newSource.ClientPrivateId != null)
                {
                    newSource.ClientPrivateId = eService.EncryptString(newSource.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), newIV).Item1;
                }


                // Update of values
                // Check CurrentToken
                if (origSource != null && origSource.CurrentToken != null && newSource.CurrentToken != null && origSource.CurrentToken != newSource.CurrentToken)
                {
                    newSource.CurrentToken = eService.DecryptEncryptUsingNewIV(origSource.CurrentToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), origSource.IVKey, newIV);
                }

                // Check ClientPrivateId
                if (origSource != null && origSource.ClientPrivateId != null && newSource.ClientPrivateId != null && origSource.ClientPrivateId != newSource.ClientPrivateId)
                {
                    newSource.CurrentToken = eService.DecryptEncryptUsingNewIV(origSource.CurrentToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), origSource.IVKey, newIV);
                }

                return newIV;
            }
            else
            {
                throw new InvalidOperationException("Encrypt method not valid for this type of DataSource");
            }
        }
    }
}
