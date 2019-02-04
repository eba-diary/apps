using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Concurrent;

namespace Sentry.AsyncCommandProcessor
{
    public abstract class AsyncCommandProcessor
    {
        #region "declarations"
        private readonly ConcurrentQueue<ICommand> _queue = new ConcurrentQueue<ICommand>();
        private readonly BlockingCollection<ICommand> _queue2 = new BlockingCollection<ICommand>();
        private readonly int _waitTime;
        private readonly object _locker = new object();

        private bool _poll;
        #endregion

        #region "queue handlers"
        public void StartPolling()
        {
            _poll = true;            
            Task tsk = new Task(() => Poll());
            tsk.Start();              
        }

        public void QueueCommand(ICommand cmd)
        {
            this.CheckIsPolling();

            lock (_locker)
            {
                _queue.Enqueue(cmd);
                //Sentry.Common.Logging.Logger.Debug("Command queued. Queue Count: " + _queue.Count.ToString());
            }
        }

        public bool IsStarted()
        {
            return _poll;
        }

        public void Stop()
        {
            _poll = false;
        }

        public void RunOff()
        {
            _poll = false;
            ICommand cmd;
            while (_queue.TryDequeue(out cmd))
            {
                this.ProcessCommand(cmd);
            }
        }

        protected void Poll()
        {
            while (_poll)
            {
                ICommand cmd;
                int i = 0;
                while (_queue.TryDequeue(out cmd) && i < 5000)
                {
                    this.ProcessCommand(cmd);
                    i++;
                }

                System.Threading.Thread.Sleep(_waitTime);
            }
        }

        protected void CheckIsPolling()
        {
            if (!_poll) throw new InvalidOperationException("Command processor not started.  Call .Start() before using this action.");
        }
        #endregion

        #region "constructors"
        protected AsyncCommandProcessor() : this(100)
        {
            
        }

        protected AsyncCommandProcessor(int waitTime)
        {
            _waitTime = waitTime;
        }
        #endregion

        #region "must override" 
        protected abstract void ProcessCommand(ICommand cmd);
        #endregion
    }
}
