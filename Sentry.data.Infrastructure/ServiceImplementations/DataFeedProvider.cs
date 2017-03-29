using System;
using System.Linq;
using NHibernate;
using Sentry.data.Core;
using Sentry.NHibernate;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Collections.Generic;

namespace Sentry.data.Infrastructure
{
    public class DataFeedProvider : NHReadableStatelessDomainContext, IDataFeedContext
    {
        public DataFeedProvider(IStatelessSession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<DataFeedProvider>();
        }

        public IQueryable<DataFeedItem> HotTopicsFeed
        {
            get
            {
                List<DataFeedItem> dataFeed = new List<DataFeedItem>();
                try
                {
                    string url = "http://www.sas.com/content/sascom/en_us/resource-center/rss/_jcr_content/par/rssfeed_c3da.rss.xml";
                    XmlReader reader = XmlReader.Create(url);
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    reader.Close();
                    foreach (SyndicationItem item in feed.Items)
                    {
                        dataFeed.Add(new DataFeedItem(
                            item.PublishDate.DateTime,
                            item.Id,
                            item.Title.Text,
                            item.Summary.Text));
                    }
                    return dataFeed.AsQueryable();
                    //return DataFeedService.GetHotTopics().AsQueryable();
                }
                catch
                {
                    return dataFeed.AsQueryable();
                }
            }
        }

        public IQueryable<DataFeedItem> NewsFeed
        {
            get
            {
                List<DataFeedItem> dataFeed = new List<DataFeedItem>();
                try
                {
                    string url = "https://community.tableau.com/groups/feeds/popularthreads?socialGroup=1061";
                    XmlReader reader = XmlReader.Create(url);
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    reader.Close();
                    foreach (SyndicationItem item in feed.Items)
                    {
                        dataFeed.Add(new DataFeedItem(
                            item.PublishDate.DateTime,
                            item.Id,
                            item.Title.Text,
                            item.Summary.Text));
                    }
                    return dataFeed.AsQueryable();
                }
                catch
                {
                    return dataFeed.AsQueryable();
                }
                //return DataFeedService.GetNewsFeed().AsQueryable();
            }
        }

    }
}
