using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.Messaging.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class FileDeleteEventHandler : IMessageHandler<string>
    {
        #region Declarations
        private readonly IEventService _eventService;
        private readonly IDatasetFileService _datafileService;
        private readonly IDataFeatures _dataFeatures;
        #endregion

        #region Constructor
        public FileDeleteEventHandler(IEventService eventService, IDatasetFileService dataFileService, IDataFeatures dataFeatures)
        {
            _eventService = eventService;
            _datafileService = dataFileService;
            _dataFeatures = dataFeatures;
        }
        #endregion

        void IMessageHandler<string>.Handle(string msg)
        {
            throw new NotImplementedException();
        }

        async Task IMessageHandler<string>.HandleAsync(string msg)
        {
            HandleLogic(msg);
        }

        public void HandleLogic(string msg)
        {
            Logger.Info($"Start method <filedeleteeventhandler-handle>");
            BaseEventMessage baseEvent = null;


            if (!_dataFeatures.CLA4049_ALLOW_S3_FILES_DELETE.GetValue())
            {
                return;
            }
            Logger.Info($"Method <filedeleteeventhandler-handle> Check feature flag {nameof(_dataFeatures.CLA4049_ALLOW_S3_FILES_DELETE)}");


            try
            {
                baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            }
            catch (Exception ex)
            {
                Logger.Error($"filedeleteeventhandler failed to convert incoming event", ex);
            }

            try
            {
                if(baseEvent != null && baseEvent.EventType != null)
                {
                    switch (baseEvent.EventType.ToUpper())
                    {
                        case "FILE_DELETE_RESPONSE":

                            DeleteFilesResponseModel responseModel = JsonConvert.DeserializeObject<DeleteFilesResponseModel>(msg);
                            Logger.Info($"filedeleteeventhandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(responseModel)}");

                            if (responseModel != null && responseModel.DeleteProcessStatusPerID != null && responseModel.DeleteProcessStatusPerID.Count > 0)
                            {
                                ProcessEachFile(responseModel);
                            }
                            else
                            {
                                Logger.Info($"filedeleteeventhandler not configured to process {nameof(responseModel.DeleteProcessStatusPerID)} need to check json format of event OR {nameof(responseModel.DeleteProcessStatusPerID)} is zero, skipping event.");
                            }
                            break;

                        default:
                            Logger.Info($"filedeleteeventhandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                            break;
                    }
                }
                else
                {
                    Logger.Error($"filedeleteeventhandler failed to parse baseEvent or EventType.  BaseEvent or baseEvent.EventType is null");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"filedeleteeventhandler failed to process message: Msg:({msg})", ex);
            }
            Logger.Info($"End method <filedeleteeventhandler-handle>");
        }


        private void ProcessEachFile(DeleteFilesResponseModel responseModel)
        {
            //GROUP INTO MULTIPLE GROUPS
            if (responseModel != null && responseModel.DeleteProcessStatusPerID != null)
            {
                List<int> successList  = new List<int>();
                List<int> failureList  = new List<int>();
                List<int> errorList = new List<int>();  

                foreach ( DeleteFilesResponseSingleStatusModel single in responseModel.DeleteProcessStatusPerID)
                {
                    string status = GetStatus(single);

                    if (status == GlobalConstants.DeleteFileResponseStatus.SUCCESS)
                    {
                        successList.Add(single.DatasetFileId);
                    }
                    else if (status == GlobalConstants.DeleteFileResponseStatus.FAILURE)
                    {
                        failureList.Add(single.DatasetFileId);
                    }
                    else
                    {
                        errorList.Add(single.DatasetFileId);
                    }
                }

                if(successList.Count > 0)
                {
                    _datafileService.UpdateObjectStatus(successList.ToArray(), Core.GlobalEnums.ObjectStatusEnum.Deleted);
                    Logger.Info($"filedeleteeventhandler will attempt to mark {successList.ToString()} as {Core.GlobalEnums.ObjectStatusEnum.Deleted.GetDescription()}");
                }

                if (failureList.Count > 0)
                {
                    _datafileService.UpdateObjectStatus(failureList.ToArray(), Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure);
                    Logger.Info($"filedeleteeventhandler will attempt to mark {failureList.ToString()} as {Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure.GetDescription()}");
                }

                if (errorList.Count > 0)
                {
                    Logger.Info($"filedeleteeventhandler unable to process {errorList.ToString()} due to unknown status");
                }
            }
            else
            {
                Logger.Info($"filedeleteeventhandler unable to locate {nameof(responseModel.DeleteProcessStatusPerID)} in message");
            }
        }

        private string GetStatus(DeleteFilesResponseSingleStatusModel single)
        {
            //DEFAULT TO ERROR
            string status = GlobalConstants.DeleteFileResponseStatus.ERROR;

            //IF THIS IS NULL, ERROR IMMEDIATEY
            if (single.DatasetFileIdDeleteStatus == null)
            {
                return status;
            }

            //SUCCESS = SUCCESS OR (FAILURE and NOTFOUND)
            if (   single.DatasetFileIdDeleteStatus.ToUpper() == GlobalConstants.DeleteFileResponseStatus.SUCCESS
                        ||
                        (
                            single.DatasetFileIdDeleteStatus.ToUpper() == GlobalConstants.DeleteFileResponseStatus.FAILURE
                             && single.DeletedFiles.Exists(w => (w.DeleteProcessStatus)?.ToUpper() == GlobalConstants.DeleteFileResponseStatus.NOTFOUND && (w.FileType)?.ToUpper() == GlobalConstants.DeleteFileResponseFileType.PARQUET)
                        )
            )
            {
                status = GlobalConstants.DeleteFileResponseStatus.SUCCESS;
            }
            //FAILURE
            else if (single.DatasetFileIdDeleteStatus.ToUpper() == GlobalConstants.DeleteFileResponseStatus.FAILURE)
            {
                status = GlobalConstants.DeleteFileResponseStatus.FAILURE;
            }
            
            return status;
        }

        bool IMessageHandler<string>.HandleComplete()
        {
            Logger.Info("FileDeleteEventhandlerComplete");
            return true;
        }

        void IMessageHandler<string>.Init()
        {
            Logger.Info("FileDeleteEventhandlerInitialized");
        }
    }
}
