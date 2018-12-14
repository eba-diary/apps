using System;
using System.Linq;
using NHibernate;
using Sentry.data.Core;
using Sentry.NHibernate;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Collections.Generic;
using NHibernate.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Configuration;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public class DataFeedProvider : NHReadableStatelessDomainContext, IDataFeedContext
    {
        private readonly IDatasetContext _datasetContext;


        public DataFeedProvider(IStatelessSession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<DataFeedProvider>();
        }

        public IList<DataFeed> GetDataFeeds()
        {
            return Query<DataFeed>().Cacheable().ToList();
        }

        public IList<DataFeedItem> GetAllFeedItems()
        {
            List<DataFeed> dataFeeds = GetDataFeeds().ToList();
            return GoGetItems(dataFeeds);
        }

        public IList<DataFeed> GetSentryDataFeeds()
        {
            return Query<DataFeed>().Where(w => w.Type == "TAB").Cacheable().ToList();
        }

        public IList<DataFeedItem> GetSentryFeedItems()
        {
            List<DataFeed> dataFeeds = GetSentryDataFeeds().ToList();
            return GoGetItems(dataFeeds);
        }

        public IList<DataFeedItem> GoGetItems(List<DataFeed> dataFeeds)
        {
            List<DataFeedItem> items = new List<DataFeedItem>();

            object sync = new object();

            Parallel.ForEach(dataFeeds, feed =>
            {
                List<DataFeedItem> list = GetFeedItems(feed).ToList();
                lock (sync)
                {
                    items.AddRange(list);
                }
            });

            items.AddRange(SentryEvents());

            return items.OrderByDescending(o => o.PublishDate).Take(100).ToList();
        }

        public IList<DataFeedItem> GetFeedItems(DataFeed feed)
        {
            List<DataFeedItem> dataFeed = new List<DataFeedItem>();
            try
            {
                Sentry.Common.Logging.Logger.Debug($"Feed Url: {feed.Url}");

                string uri = Configuration.Config.GetHostSetting("SentryWebProxyHost");

                //https://stackoverflow.com/questions/124932/xmldocument-loadurl-through-a-proxy
                WebProxy wp = new WebProxy(uri);
                wp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                WebClient wc = new WebClient();
                wc.Proxy = wp;

                MemoryStream ms = new MemoryStream(wc.DownloadData(feed.Url));
                XmlTextReader reader = new XmlTextReader(ms);
                
                SyndicationFeed sf = SyndicationFeed.Load(reader);
                reader.Close();
                foreach (SyndicationItem item in sf.Items)
                {
                    dataFeed.Add(new DataFeedItem(
                        item.PublishDate.DateTime,
                        item.Id,
                        item.Title.Text,
                        item.Summary.Text,
                        feed));
                }

                wc.Dispose();
                ms.Dispose();

                return dataFeed.ToList();
            }
            catch (Exception e)
            {                
                Sentry.Common.Logging.Logger.Debug(e.Message);
                return dataFeed.ToList();
            }
        }

        public IList<DataFeedItem> SentryEvents()
        {
            List<DataFeedItem> items = new List<DataFeedItem>();
            var events = Query<Event>().Where(x => x.Dataset != null && x.EventType.Description == "Created Dataset").OrderByDescending(x => x.TimeCreated).Take(50);

            var dsEvents = Query<Event>().Where(x => x.Dataset != null && (x.EventType.Description == "Created Dataset")).OrderByDescending(x => x.TimeCreated).Take(50);

            //|| x.EventType.Description == "Created Report"

            foreach (Event e in dsEvents)
            {
                Dataset ds = Query<Dataset>().FirstOrDefault(y => y.DatasetId == e.Dataset);

                if (ds != null)
                {
                    DataFeedItem dfi = new DataFeedItem(
                    e.TimeCreated,
                    e.Dataset.ToString(),
                    ds.DatasetName + " - A New Dataset was Created in the " + ds.Category + " Category",
                    ds.DatasetName + " - A New Dataset was Created in the " + ds.Category + " Category",
                    new DataFeed() { Name = "Datasets", Url = "/Datasets/Detail/" + e.Dataset, Type = "Datasets" }
                    );

                    items.Add(dfi);
                }                
            }

            var rptEvents = Query<Event>().Where(x => x.Dataset != null && (x.EventType.Description == "Created Report")).OrderByDescending(x => x.TimeCreated).Take(50);

            foreach (Event e in rptEvents)
            {
                Dataset ds = Query<Dataset>().FirstOrDefault(y => y.DatasetId == e.Dataset);
                
                if(ds != null)
                {
                    DataFeedItem dfi = new DataFeedItem(
                    e.TimeCreated,
                    e.Dataset.ToString(),
                    ds.DatasetName + " - A New Exhibit was Created",
                    ds.DatasetName + " - A New Exhibit was Created",
                    new DataFeed() { Name = "Business Intelligence", Url = "/BusinessIntelligence/Detail/" + e.Dataset, Type = "Exhibits" }
                    );

                    items.Add(dfi);
                }
            }
            return items;
        }
    }
}
