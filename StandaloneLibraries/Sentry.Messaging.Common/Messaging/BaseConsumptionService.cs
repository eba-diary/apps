using Sentry.Common.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.Messaging.Common
{
    public abstract class BaseConsumptionService<msgT>
    {
        #region Declarations
        private readonly IMessageConsumer<msgT> _consumer;
        private readonly ConsumptionConfig _cfg;
        private bool _eos = false;
        private bool _cancel = false;
        #endregion

        #region Methods
        public void ConsumeMessages()
        {
            _consumer.EndOfStream += _consumer_EndOfStream;
            _consumer.MessageReady += _consumer_MessageReady;
            _consumer.ConsumerStopped += _consumer_ConsumerStopped;
            _consumer.SubscriptionReady += _consumer_SubscriptionReady;

            InitHandler();

            RunOpen();

            _consumer.RequestStop();
            _consumer.Close();

            CloseHandler();
        }

        private void RunOpen()
        {
            if (_cfg.ForceSingleThread)
            {
                Logger.Warn("Raw message consumer is running in single thread - this is intended for local debugging only.  Timing rules are not applied.  If this is not in 'Dev' then this consumer may run indefinitely unless it is manually closed.");
                _consumer.Open();
            }
            else
            {
                Task tsk = new Task(() => _consumer.Open());
                Stopwatch sw = new Stopwatch();

                //we run for a time limit (if specified) or End Of Stream
                tsk.Start();
                sw.Start();
                while (((_cfg.RunMinutes.HasValue && sw.Elapsed.TotalMinutes < _cfg.RunMinutes.Value) || !_cfg.RunMinutes.HasValue) && !_eos && !_cancel)
                {
                    //log every 5 minutes of the run
                    if (sw.ElapsedMilliseconds > 1000 && sw.ElapsedMilliseconds % 300000 == 0)
                    {
                        Logger.Info("Consumer has run for " + sw.Elapsed.TotalMinutes.ToString());
                    }

                    if (_cfg.UseKillFile && File.Exists(_cfg.KillFileLocation))
                    {
                        Logger.Info("Consumption has been stopped by Kill File.");
                        Stop();
                    }

                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        public void Stop()
        {
            _cancel = true;
        }
        #endregion

        #region Events
        private void _consumer_EndOfStream(object sender)
        {
            _eos = true;
        }
        private void _consumer_SubscriptionReady(object sender, bool ready)
        {
            //not sure why i need to do this but have to handle all events that get raised
        }

        private void _consumer_ConsumerStopped(object sender, bool success)
        {
            //not much to do here, but handle the event or a null reference will occur, this is for completeness
        }
        #endregion

        #region Must Override
        protected abstract void _consumer_MessageReady(object sender, msgT msg);
        protected abstract void InitHandler();
        protected abstract void CloseHandler();
        #endregion

        #region Constructors
        protected BaseConsumptionService(IMessageConsumer<msgT> consumer, ConsumptionConfig config)
        {
            _consumer = consumer;
            _cfg = config;
        }
        #endregion
    }

    public class ConsumptionConfig
    {
        public bool ForceSingleThread { get; set; }
        public bool UseKillFile { get; set; }
        public string KillFileLocation { get; set; }
        public double? RunMinutes { get; set; }

        public ConsumptionConfig() { }

        public ConsumptionConfig(bool singleThreaded, bool useKillFile, string killFileLocation, double? runMinutes)
        {
            ForceSingleThread = singleThreaded;
            UseKillFile = useKillFile;
            KillFileLocation = killFileLocation;
            RunMinutes = runMinutes;
        }
    }
}
