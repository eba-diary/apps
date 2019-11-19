﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;
using Sentry.Common.Logging;
using Newtonsoft.Json;
using StructureMap;

namespace Sentry.data.Core
{
    public class HiveMetadataHandler : IMessageHandler<string>
    {
        #region Declarations
        private IDatasetContext _dsContext;
        private UserService _userService;
        private IEmailService _emailService;
        private ISchemaService _schemaService;
        #endregion

        #region Constructor
        public HiveMetadataHandler(IDatasetContext dsContext, UserService userService, IEmailService emailService, ISchemaService schemaService)
        {
            _dsContext = dsContext;
            _userService = userService;
            _emailService = emailService;
            _schemaService = schemaService;
        }
        #endregion

        void IMessageHandler<string>.Handle(string msg)
        {
            BaseEventMessage baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            FileSchema de = null;            
            try
            {
                switch (baseEvent.EventType.ToUpper())
                {
                    case "HIVE-TABLE-CREATE-COMPLETED":
                        HiveTableCreateModel hiveCreatedEvent = JsonConvert.DeserializeObject<HiveTableCreateModel>(msg);
                        Logger.Info("HiveMetadataHandler processing HIVE-TABLE-CREATE-COMPLETED message: " + JsonConvert.SerializeObject(hiveCreatedEvent));

                        switch (hiveCreatedEvent.HiveStatus.ToUpper())
                        {
                            case "CREATED":
                            case "EXISTED":
                                de = _dsContext.FileSchema.Where(w => w.SchemaId == hiveCreatedEvent.SchemaID).FirstOrDefault();
                                de.HiveTableStatus = HiveTableStatusEnum.Available.ToString();

                                if (de.IsInSAS)
                                {
                                    bool IsSuccessful = _schemaService.SasUpdateNotification(hiveCreatedEvent.SchemaID, hiveCreatedEvent.RevisionID);

                                    if (!IsSuccessful)
                                    {
                                        Logger.Error($"HiveMetadataHandler failed sending SAS email - revision:{hiveCreatedEvent.RevisionID}");
                                    }                                    
                                }
                                break;
                            case "FAILED":
                                de = _dsContext.GetById<FileSchema>(hiveCreatedEvent.SchemaID);
                                de.HiveTableStatus = HiveTableStatusEnum.RequestFailed.ToString();
                                break;
                            default:
                                de = _dsContext.GetById<FileSchema>(hiveCreatedEvent.SchemaID);
                                de.HiveTableStatus = HiveTableStatusEnum.Pending.ToString();
                                break;
                        }

                        _dsContext.Merge(de);
                        _dsContext.SaveChanges();
                        break;
                    case "HIVE-TABLE-CREATE-REQUESTED":
                        HiveTableCreateModel hiveReqeustedEvent = JsonConvert.DeserializeObject<HiveTableCreateModel>(msg);
                        Logger.Info($"HiveMetadataHandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(hiveReqeustedEvent)}");

                        de = _dsContext.GetById<FileSchema>(hiveReqeustedEvent.SchemaID);
                        de.HiveTableStatus = HiveTableStatusEnum.Requested.ToString();

                        _dsContext.Merge(de);
                        _dsContext.SaveChanges();
                        break;
                    case "HIVE-TABLE-DELETE-REQUESTED":
                        HiveTableDeleteModel deleteEvent = JsonConvert.DeserializeObject<HiveTableDeleteModel>(msg);
                        Logger.Info($"HiveMetadataHandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(deleteEvent)}");

                        de = _dsContext.GetById<FileSchema>(deleteEvent.SchemaID);
                        de.HiveTableStatus = HiveTableStatusEnum.DeleteRequested.ToString();
                        _dsContext.SaveChanges();
                        break;
                    case "HIVE-TABLE-DELETE-COMPLETED":
                        HiveTableDeleteModel deleteCompletedEvent = JsonConvert.DeserializeObject<HiveTableDeleteModel>(msg);
                        Logger.Info($"HiveMetadataHandler processing {baseEvent.EventType.ToUpper()} message: {JsonConvert.SerializeObject(deleteCompletedEvent)}");

                        de = _dsContext.GetById<FileSchema>(deleteCompletedEvent.SchemaID);

                        switch (deleteCompletedEvent.HiveStatus.ToUpper())
                        {
                            case "DELETED":
                                de.HiveTableStatus = HiveTableStatusEnum.Deleted.ToString();
                                break;
                            case "FAILED":
                                de.HiveTableStatus = HiveTableStatusEnum.DeleteFailed.ToString();
                                break;
                            default:
                                de.HiveTableStatus = HiveTableStatusEnum.Pending.ToString();
                                break;
                        }

                        _dsContext.SaveChanges();

                        break;
                    default:
                        Logger.Info($"HiveMetadataHandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"HiveMetadataHandler failed to process message: EventType:{baseEvent.EventType.ToUpper()} - Msg:({msg})", ex);
            }                        
        }

        bool IMessageHandler<string>.HandleComplete()
        {
            return true;
        }

        void IMessageHandler<string>.Init()
        {
            //do nothing
        }

        
    }
}
