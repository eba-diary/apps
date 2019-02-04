using Sentry.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.Messaging.Common
{
    public class MessageTransactionMongoConsumer : IMessageConsumer<MessageTransaction>
    {
        #region Declarations
        private readonly IQueryable<MessageTransaction> _queryable;
        private IEnumerable<MessageTransaction> _currentPage;
        private Guid? _lastId;
        private readonly int _pageSize;
        private int _pageCount = -1;
        private int _totalRecords;
        private int? _maxRecords;
        private bool _running;
        private bool _endReached;
        #endregion

        #region IMessageConsumer Implementation
        public event OnMessageReadyHandler<MessageTransaction> MessageReady;
        public event OnConsumerStoppedHandler ConsumerStopped;
        public event OnEndOfStreamHandler EndOfStream;
        public event OnSubscriptionReadyHandler SubscriptionReady;

        public void Open()
        {
            _running = true;

            while (HasNextPage() && _running)
            {
                Logger.Info("MessageTransaction Mongo Consumer: Page " + _pageCount.ToString() + " - Begin processing " + _currentPage.Count().ToString() + " messages");
                foreach (MessageTransaction msg in _currentPage)
                {
                    MessageReady(this, msg);
                }
                Logger.Info("MessageTransaction Mongo Consumer: Page " + _pageCount.ToString() + " Completed - " + _totalRecords.ToString() + " total messages processed");
            }

            _endReached = true;
        }

        public void RequestStop()
        {
            Logger.Info("MessageTransaction Mongo Consumer: Stop Requested - Consumer will finish current page.");
            _running = false;
        }

        public void Close()
        {
            if (!_endReached) Logger.Info("MessageTransaction Mongo Consumer: Closing - Waiting for current page to finish.");
            while (!_endReached)
            {
                System.Threading.Thread.Sleep(1000);
            }
            Logger.Info("MessageTransaction Mongo Consumer: Closing - Finished.");
        }
        #endregion

        #region Methods
        private bool HasNextPage()
        {
            return Next().Any();
        }

        private IEnumerable<MessageTransaction> Next()
        {
            IQueryable<MessageTransaction> docs;
            IEnumerable<MessageTransaction> docList = new List<MessageTransaction>();
            int takeSize = _pageSize;

            if (_maxRecords.HasValue)
            {
                if (_totalRecords >= _maxRecords.Value) takeSize = 0;
                else if (_totalRecords + _pageSize > _maxRecords.Value) takeSize = (_totalRecords + _pageSize) - _maxRecords.Value;
            }

            if (takeSize > 0)
            {
                if (_lastId != null) docs = _queryable.OrderBy(x => x.Id).Where(a => a.Id.CompareTo(_lastId) > 0).Take(takeSize);
                else  docs = _queryable.OrderBy(x => x.Id).Take(takeSize);

                docList = docs.ToList();

                if (docList.Any())
                {
                    _lastId = docList.Last().Id;
                    _pageCount++;
                    _currentPage = docList;
                    _totalRecords += docList.Count();
                }
                else EndOfStream(this);
            }

            return docList;
        }

        private void LastIdOfPage(int pageNumber)
        {
            if (pageNumber > 0)
            {
                _lastId = _queryable.OrderBy(x => x.Id).Take(pageNumber * _pageSize).Max(m => m.Id);
                _pageCount += pageNumber;
            }
        }

        private void SetMaxRecords(int pageNumber)
        {
            if (pageNumber > 0) _maxRecords = (pageNumber - _pageCount) * _pageSize;
        }
        #endregion

        #region Constructors
        public MessageTransactionMongoConsumer(IQueryable<MessageTransaction> queryable, int? pageSize, int? startPage, int? endPage)
        {
            _queryable = queryable;
            _pageSize = pageSize.HasValue ? pageSize.Value : 10000;
            if (startPage.HasValue) LastIdOfPage(startPage.Value);
            if (endPage.HasValue) SetMaxRecords(endPage.Value);
        }
        #endregion
    }
}
