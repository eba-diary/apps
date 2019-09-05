using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure
{
    public static class WallEService
    {
        private static Guid _runGuid;

        public static async Task Run()
        {
            _runGuid = Guid.NewGuid();
            Logger.Info($"walleservice-run-initiated - guid:{_runGuid}");

            await Task.Factory.StartNew(() => { DeleteSchemas(); });
            
        }

        private static void DeleteSchemas()
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IConfigService configService = container.GetInstance<IConfigService>();
                List<DatasetFileConfig> DeleteSchemaList = configService.GetSchemaMarkedDeleted();

                if (DeleteSchemaList != null && DeleteSchemaList.Count > 0)
                {
                    Logger.Info($"walleservice-schemadeletes-detected - {DeleteSchemaList.Count} schemas found - guid:{_runGuid}");
                    foreach (DatasetFileConfig config in DeleteSchemaList)
                    {
                        configService.Delete(config.ConfigId, false);
                    }
                }
                else
                {
                    Logger.Info($"walleservice-schemadeletes-notdetected - guid:{_runGuid}");
                }

                Logger.Info($"walleservice-run-completed - guid:{_runGuid}");
            }            
        }
    }
}
