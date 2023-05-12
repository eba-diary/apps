using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using Sentry.Messaging.Common;
using System;
using System.Threading.Tasks;


namespace Sentry.data.Core
{
    public class S3EventHandler : BaseDomainService<S3EventHandler>, IMessageHandler<string>
    {
        #region Declarations
        private readonly IDataFlowProvider _dataFlowProvider;
        #endregion

        #region Constructor
        public S3EventHandler(IDataFlowProvider dataFlowProvider, DomainServiceCommonDependency<S3EventHandler> commonDependency) : base(commonDependency)
        {
            _dataFlowProvider = dataFlowProvider;
        }
        #endregion

        void IMessageHandler<string>.Handle(string msg)
        {
            throw new NotImplementedException();
        }

        async Task IMessageHandler<string>.HandleAsync(string msg)
        {
            BaseEventMessage baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            try
            {
                if (baseEvent.EventType.ToUpper() == "S3EVENT")
                {
                    S3Event s3Event = JsonConvert.DeserializeObject<S3Event>(msg);
                    _logger.LogInformation("s3eventhandler processing S3EVENT message: " + JsonConvert.SerializeObject(s3Event));

                    switch (s3Event.PayLoad.eventName.ToUpper())
                    {
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_PUT:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_POST:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_COPY:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_COMPLETEMULTIPARTUPLOAD:
                            _logger.LogInformation($"S3EventHandler processing AWS event - {JsonConvert.SerializeObject(s3Event)}");
                            await _dataFlowProvider.ExecuteDependenciesAsync(s3Event.PayLoad.s3.bucket.name, s3Event.PayLoad.s3.Object.key, s3Event.PayLoad).ConfigureAwait(false);
                            break;
                        default:
                            _logger.LogInformation($"s3eventhandler not configured to handle AWS event type ({s3Event.EventType}), skipping event.");
                            break;
                    }
                }
                else
                {
                    _logger.LogInformation($"s3eventhandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"s3eventhandler failed to process message: EventType:{baseEvent.EventType.ToUpper()} - Msg:({msg})");
            }
        }

        bool IMessageHandler<string>.HandleComplete()
        {
            _logger.LogInformation("S3EventHandlerComplete");
            return true;
        }

        void IMessageHandler<string>.Init()
        {
            _logger.LogInformation("S3EventHandlerInitialized");
        }
    }
}
