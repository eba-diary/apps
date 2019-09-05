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
    public class WallEService
    {
        private IDatasetContext _dsContext;
        private IS3ServiceProvider _s3ServiceProvider;
        private Guid _runGuid;

        public void Run()
        {
            _runGuid = Guid.NewGuid();
            Logger.Info($"walleservice-run-initiated - guid:{_runGuid}");
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IConfigService configService = container.GetInstance<IConfigService>();

                DeleteSchemas();

                //List<DatasetFileConfig> DeleteSchemaList = configService.GetSchemaMarkedDeleted();

                //if (DeleteSchemaList != null && DeleteSchemaList.Count > 0)
                //{
                //    Logger.Info($"walleservice-schemadeletes-detected - {DeleteSchemaList.Count} schemas found - guid:{_runGuid}");

                //}
                //else
                //{
                //    Logger.Info($"walleservice-schemadeletes-notdetected - guid:{_runGuid}");
                //}

                Logger.Info($"walleservice-run-completed - guid:{_runGuid}");
            }
        }

        private void DeleteSchemas()
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {

            }
        }
    }
}
