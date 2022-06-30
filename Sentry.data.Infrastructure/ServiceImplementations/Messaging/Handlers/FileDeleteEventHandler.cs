using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.Messaging.Common;
using System;
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
            //LOOP THROUGH EACH FILE AND UPDATE STATUS
            foreach (DeleteFilesResponseSingleStatusModel response in responseModel.DeleteProcessStatusPerID)
            {
                if(response.DatasetFileIdDeleteStatus != null)
                {
                    Core.GlobalEnums.ObjectStatusEnum status = Core.GlobalEnums.ObjectStatusEnum.Deleted;

                    if (response.DatasetFileIdDeleteStatus.ToUpper() == GlobalConstants.DeleteFileResponseStatus.SUCCESS)
                    {
                        //FILE DELETED SUCCESSFULLY
                        _datafileService.UpdateObjectStatus(response.DatasetFileId, status);
                        Logger.Info($"filedeleteeventhandler processed {response.DatasetFileId} as {status.GetDescription()}");
                    }
                    else if (response.DatasetFileIdDeleteStatus.ToUpper() == GlobalConstants.DeleteFileResponseStatus.FAILURE)
                    {
                        //FILE DELETED FAILURE
                        status = Core.GlobalEnums.ObjectStatusEnum.Pending_Delete_Failure;
                        _datafileService.UpdateObjectStatus(response.DatasetFileId, status);
                        Logger.Info($"filedeleteeventhandler processed {response.DatasetFileId} as {status.GetDescription()}");
                    }
                    else
                    {
                        Logger.Info($"filedeleteeventhandler unable to process {response.DatasetFileId} because it does not understand {response.DatasetFileIdDeleteStatus}");
                    }
                }
                else
                {
                    Logger.Info($"filedeleteeventhandler unable to locate {nameof(response.DatasetFileIdDeleteStatus)} in message");
                }
            }
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
