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

                string uri = Configuration.Config.GetHostSetting("WebProxyUrl");

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

            //LAZER NOTE: none of this code is called if CLA2838_DSC_ANOUNCEMENTS is false
            //STEP #1 CREATE BLANK LIST OF DataFeedItems
            List<DataFeedItem> items = new List<DataFeedItem>();

            //STEP #2 CREATE DATASET DataFeedItems
            var dsEvents = Query<Event>().Where(x => x.Dataset != null && (x.EventType.Description == "Created Dataset")  && (x.TimeCreated >= DateTime.Now.AddDays(-30))).Take(50).OrderByDescending(o => o.TimeCreated);
            foreach (Event e in dsEvents)
            {
                Dataset ds = Query<Dataset>().Where(y => y.DatasetId == e.Dataset).FetchMany(x=> x.DatasetCategories).FirstOrDefault();

                if (ds != null)
                {
                    DataFeed feed = new DataFeed();
                    feed.Id = ds.DatasetId;
                    feed.Name = "Datasets";
                    feed.Url = "/Datasets/Detail/" + e.Dataset;
                    feed.Type = "Datasets";

                    DataFeedItem dfi = new DataFeedItem(
                    e.TimeCreated,
                    e.Dataset.ToString(),
                    ds.DatasetName + " - A New Dataset was Created in the " + ds.DatasetCategories.First().Name + " Category",
                    ds.DatasetName + " - A New Dataset was Created in the " + ds.DatasetCategories.First().Name + " Category",
                    feed 
                    );

                    items.Add(dfi);
                }                
            }

            //STEP #3  CREATE BI ITEMS
            var rptEvents = Query<Event>().Where(x => x.Dataset != null && (x.EventType.Description == "Created Report") && (x.TimeCreated >= DateTime.Now.AddDays(-30))).Take(50).OrderByDescending(o => o.TimeCreated);
            foreach (Event e in rptEvents)
            {
                Dataset ds = Query<Dataset>().FirstOrDefault(y => y.DatasetId == e.Dataset);

                if (ds != null)
                {
                    DataFeed feed = new DataFeed();
                    feed.Id = ds.DatasetId;
                    feed.Name = "Business Intelligence";
                    feed.Url = "/BusinessIntelligence/Detail/" + e.Dataset;
                    feed.Type = "Exhibits";

                    DataFeedItem dfi = new DataFeedItem(
                    e.TimeCreated,
                    e.Dataset.ToString(),
                    ds.DatasetName + " - A New Exhibit was Created",
                    ds.DatasetName + " - A New Exhibit was Created",
                    feed
                    );

                    items.Add(dfi);
                }
            }

            var notifications = Query<Notification>().Where(w => w.ExpirationTime >= DateTime.Now && (BusinessAreaType)w.ParentObject == BusinessAreaType.DSC);
            foreach (Notification n in notifications)
            {
                if (n != null)
                {
                    DataFeed feed = new DataFeed();
                    feed.Id = n.NotificationId;
                    feed.Name = "Notification";
                    feed.Category = (n.NotificationCategory != null) ? n.NotificationCategory.GetDescription() : feed.Name;
                    feed.Type = GlobalConstants.DataFeedType.Notifications;

                    DataFeedItem dfi = new DataFeedItem(
                        n.StartTime,                                                //PublishDate
                        n.NotificationId.ToString(),                                //Id
                        n.Title,                                                    //shortDesc
                        "",                                                         //longDesc
                        feed                                                        //DataFeed
                    );
                    items.Add(dfi);
                }
            }

            return items;
        }

        public IList<FavoriteItem> GetUserFavorites(string associateId)
        {
            List<FavoriteItem> items = new List<FavoriteItem>();
            List<Favorite> favsList = Query<Favorite>().Where(w => w.UserId == associateId).ToList();

            foreach (Favorite fav in favsList)
            {
                Dataset ds = Query<Dataset>().Where(w => w.DatasetId == fav.DatasetId && w.ObjectStatus == Core.GlobalEnums.ObjectStatusEnum.Active).FetchMany(w => w.DatasetFileConfigs).FirstOrDefault();

                if (ds != null)
                {
                    DataFeed df = null;
                    if (ds.DatasetType == GlobalConstants.DataEntityCodes.REPORT)
                    {
                        df = new DataFeed()
                        {
                            Id = ds.DatasetId,
                            Name = "Business Intelligence",
                            Url = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.Location)) ? ds.Metadata.ReportMetadata.Location : null,
                            UrlType = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.LocationType)) ? ds.Metadata.ReportMetadata.LocationType : null,
                            Type = "Exhibits"
                        };
                    }
                    else
                    {
                        df = new DataFeed()
                        {
                            Id = ds.DatasetId,
                            Name = "Datasets",
                            Url = "/Datasets/Detail/" + ds.DatasetId,
                            UrlType = ds.DatasetType,
                            Type = "Datasets"
                        };
                    }

                    items.Add(new FavoriteItem(
                        fav.FavoriteId,
                        fav.DatasetId.ToString(),
                        ds.DatasetName,
                        df,
                        fav.Sequence
                        )
                    );
                }
            }

            return items;
        }

        public FavoriteItem GetFavorite(int favoriteId)
        {
            Favorite fav = Query<Favorite>().Where(x => x.FavoriteId == favoriteId).SingleOrDefault();

            if (fav != null)
            {
                Dataset ds = Query<Dataset>().Where(w => w.DatasetId == fav.DatasetId).FetchMany(w => w.DatasetFileConfigs).FirstOrDefault();

                if (ds != null)
                {
                    DataFeed df = null;
                    if (ds.DatasetType != null && ds.DatasetType == GlobalConstants.DataEntityCodes.REPORT)
                    {
                        df = new DataFeed()
                        {
                            Id = ds.DatasetId,
                            Name = "Business Intelligence",
                            Url = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.Location)) ? ds.Metadata.ReportMetadata.Location : null,
                            UrlType = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.LocationType)) ? ds.Metadata.ReportMetadata.LocationType : null,
                            Type = "Exhibits"
                        };
                    }
                    else
                    {
                        df = new DataFeed()
                        {
                            Id = ds.DatasetId,
                            Name = "Datasets",
                            Url = "/Datasets/Detail/" + ds.DatasetId,
                            UrlType = ds.DatasetType,
                            Type = "Datasets"
                        };
                    }

                    return new FavoriteItem(
                        fav.FavoriteId,
                        fav.DatasetId.ToString(),
                        ds.DatasetName,
                        df,
                        fav.Sequence
                    );
                }

            }

            // last resort return null
            return null;
        }
    }
}