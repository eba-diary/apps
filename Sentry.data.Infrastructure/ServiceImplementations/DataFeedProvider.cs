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
        public DataFeedProvider(IStatelessSession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<DataFeedProvider>();
        }

        public IList<DataFeed> GetDataFeeds()
        {
            return Query<DataFeed>().Cacheable().ToList();
            //return Query<DataFeed>().ToList();
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

                return dataFeed.ToList();
            }
            catch (Exception e)
            {                
                Sentry.Common.Logging.Logger.Debug(e.Message);
                return dataFeed.ToList();
            }
        }

        //public IQueryable<DataFeedItem> HotTopicsFeed
        //{
        //    get
        //    {
        //        List<DataFeedItem> dataFeed = new List<DataFeedItem>();
        //        try
        //        {
        //            string url = "http://www.sas.com/content/sascom/en_us/resource-center/rss/_jcr_content/par/rssfeed_c3da.rss.xml";
        //            XmlReader reader = XmlReader.Create(url);
        //            SyndicationFeed feed = SyndicationFeed.Load(reader);
        //            reader.Close();
        //            foreach (SyndicationItem item in feed.Items)
        //            {
        //                dataFeed.Add(new DataFeedItem(
        //                    item.PublishDate.DateTime,
        //                    item.Id,
        //                    item.Title.Text,
        //                    item.Summary.Text));
        //            }
        //            return dataFeed.AsQueryable();
        //            //return DataFeedService.GetHotTopics().AsQueryable();
        //        }
        //        catch
        //        {
        //            return dataFeed.AsQueryable();
        //        }
        //    }
        //}

        //public IQueryable<DataFeedItem> NewsFeed
        //{
        //    get
        //    {
        //        List<DataFeedItem> dataFeed = new List<DataFeedItem>();
        //        try
        //        {
        //            string url = "https://community.tableau.com/groups/feeds/popularthreads?socialGroup=1061";
        //            XmlReader reader = XmlReader.Create(url);
        //            SyndicationFeed feed = SyndicationFeed.Load(reader);
        //            reader.Close();
        //            foreach (SyndicationItem item in feed.Items)
        //            {
        //                dataFeed.Add(new DataFeedItem(
        //                    item.PublishDate.DateTime,
        //                    item.Id,
        //                    item.Title.Text,
        //                    item.Summary.Text));
        //            }
        //            return dataFeed.AsQueryable();
        //        }
        //        catch
        //        {
        //            return dataFeed.AsQueryable();
        //        }
        //        //return DataFeedService.GetNewsFeed().AsQueryable();
        //    }
        //}
    }
}
