using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;
using System.IO;
using Sentry.data.Core;
using System.Net;
using Sentry.Common.Logging;
using StructureMap;

namespace Sentry.data.Infrastructure
{
    public class SftpProvider
    {
        SessionOptions _sessionOptions;
        private IContainer Container { get; set; }

        private void createSessionOptions(string hostname, NetworkCredential creds)
        {
            _sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = hostname,
                UserName = creds.UserName,
                Password = creds.Password
            };
        }

        public void GetFile(RetrieverJob job, string targetFile)
        {
            string fingerprint = null;
            DataSource source = job.DataSource;

            createSessionOptions(job.DataSource.BaseUri.ToString(), job.DataSource.SourceAuthType.GetCredentials(job));

            if (source.HostFingerPrintKey != null)
            {
                Logger.Info($"Connecting to known Host - Job:{job.Id} | DataSource:{source.Name} | DataSourceId:{source.Id} | Url:{source.BaseUri}");                
                _sessionOptions.SshHostKeyFingerprint = source.HostFingerPrintKey;
            }
            else
            {
                Logger.Info($"Connecting to new SFTP Source - Job:{job.Id} | DataSource:{source.Name} | DataSource:{source.Id} | Url:{source.BaseUri}");
                
                using (Session session = new Session())
                {
                    fingerprint = session.ScanFingerprint(_sessionOptions, "SHA-256");
                    _sessionOptions.SshHostKeyFingerprint = fingerprint;
                }

                Logger.Info($"Adding SFTP source SSH fingerprint key - Job:{job.Id} | DataSource:{source.Name} | DataSourceId:{source.Id} | Url:{source.BaseUri}");
                try
                {
                    using (Container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
                    {
                        IRequestContext _requestContext = Container.GetInstance<IRequestContext>();

                        SFtpSource newSource = (SFtpSource)_requestContext.GetById<DataSource>(source.Id);

                        newSource.HostFingerPrintKey = fingerprint;

                        _requestContext.SaveChanges();

                        Logger.Info($"Successfully added SFTP ssh fingerprint key");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed adding SFTP source ssh fingerprint key - Job:{job.Id} | DataSource:{source.Name} | DataSourceId:{source.Id} | SSHKey:{fingerprint}", ex);
                    throw;
                }   
            }

            using (Session session = new Session())
            {
                TransferOperationResult transferResult = null;
                DateTime transferStart = DateTime.Now;
                try
                {
                    //connect
                    Logger.Debug($"Opening SFTP session - Job:{job.Id} | DataSource:{source.Name} | DataSourceId:{source.Id}");
                    session.Open(_sessionOptions);

                    // Download file
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    Logger.Info($"Starting SFTP Transfer - Job:{job.Id} | DataSource:{source.Name} | DataSourceId:{source.Id}");
                    transferResult = session.GetFiles(job.RelativeUri.ToString(), targetFile, false, transferOptions);
                    Logger.Info($"Finished SFTP Transfer - Job:{job.Id} | DataSource:{source.Name} | DataSourceId:{source.Id}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"SFTP Job Initialization Failure - Job:{job.Id} | DatasetName:{job.DatasetConfig.ParentDataset.DatasetName} | DatasetId:{job.DatasetConfig.ParentDataset.DatasetId} | Schema:{job.DatasetConfig.Name} | SchemaId:{job.DatasetConfig.ConfigId}", ex);
                    throw;
                }

                try
                {
                    Logger.Info($"Checking for SFTP Transfer errors - Job:{job.Id} | DataSource:{source.Name} | DataSourceId:{source.Id}");
                    transferResult.Check();
                    Logger.Info($"SFTP Transfer Success - transferTime:{(DateTime.Now - transferStart).TotalSeconds} | Job:{job.Id} | Dataset:{job.DatasetConfig.ParentDataset.DatasetName} | DatasetId:{job.DatasetConfig.ParentDataset.DatasetId} | Schema:{job.DatasetConfig.Name} | SchemaId:{job.DatasetConfig.ConfigId}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"SFTP Job Transfer Failure - Job:{job.Id} | DatasetName:{job.DatasetConfig.ParentDataset.DatasetName} | DatasetId:{job.DatasetConfig.ParentDataset.DatasetId} | Schema:{job.DatasetConfig.Name} | SchemaId:{job.DatasetConfig.ConfigId}", ex);
                    throw;
                }
            }
        }
    }
}
