using NHibernate;
using NHibernate.Linq;
using Sentry.data.Core;
using Sentry.NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public class DataFeedProvider : NHReadableStatelessDomainContext, IDataFeedContext
    {

        public DataFeedProvider(IStatelessSession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<DataFeedProvider>();
        }

        public IList<DataFeedItem> GetAllFeedItems()
        {
            List<DataFeed> dataFeeds = Query<DataFeed>().Cacheable().ToList();
            return GoGetItems(dataFeeds);
        }

        public IList<DataFeedItem> GetSentryFeedItems()
        {
            List<DataFeed> dataFeeds = Query<DataFeed>().Where(w => w.Type == "TAB").Cacheable().ToList();
            return GoGetItems(dataFeeds);
        }

        public IList<DataFeedItem> GoGetItems(List<DataFeed> dataFeeds)
        {
            List<DataFeedItem> items = new List<DataFeedItem>();
            items.AddRange(SentryEvents());
            return items.OrderByDescending(o => o.PublishDate).Take(100).ToList();
        }

        public IList<DataFeedItem> SentryEvents()
        {
            //NOTE: none of this code is called if CLA2838_DSC_ANOUNCEMENTS is false

            //STEP #1 CREATE BLANK LIST OF DataFeedItems
            List<DataFeedItem> items = new List<DataFeedItem>();

            //STEP #2 CREATE DATASET DataFeedItems
            var dsEvents = Query<Event>().Where(x => x.Dataset != null && (x.EventType.Description == "Created Dataset") && (x.TimeCreated >= DateTime.Now.AddDays(-30))).OrderByDescending(o => o.TimeCreated).Take(25);
            foreach (Event e in dsEvents)
            {
                Dataset ds = Query<Dataset>().Where(y => y.DatasetId == e.Dataset).FetchMany(x => x.DatasetCategories).FirstOrDefault();

                if (ds != null)
                {
                    DataFeed feed = new DataFeed();
                    feed.Id = ds.DatasetId;
                    feed.Name = GlobalConstants.DataFeedName.DATASET;
                    feed.Url = "/Datasets/Detail/" + e.Dataset;
                    feed.Type = GlobalConstants.DataFeedType.Datasets;

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
            var rptEvents = Query<Event>().Where(x => x.Dataset != null && (x.EventType.Description == "Created Report") && (x.TimeCreated >= DateTime.Now.AddDays(-30))).OrderByDescending(o => o.TimeCreated).Take(25);
            foreach (Event e in rptEvents)
            {
                Dataset ds = Query<Dataset>().FirstOrDefault(y => y.DatasetId == e.Dataset);

                if (ds != null)
                {
                    DataFeed feed = new DataFeed();
                    feed.Id = ds.DatasetId;
                    feed.Name = GlobalConstants.DataFeedName.BUSINESS_INTELLIGENCE;
                    feed.Url = "/BusinessIntelligence/Detail/" + e.Dataset;
                    feed.Type = GlobalConstants.DataFeedType.Exhibits;

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

            //STEP #4  CREATE NOTIFICATION ITEMS
            var notifications = Query<Notification>().Where(w => w.ExpirationTime >= DateTime.Now && (BusinessAreaType)w.ParentObject == BusinessAreaType.DSC).Take(50);
            foreach (Notification n in notifications)
            {
                if (n != null)
                {
                    DataFeed feed = new DataFeed();
                    feed.Id = n.NotificationId;
                    feed.Name = GlobalConstants.DataFeedName.NOTIFICATION;
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
                            Name = GlobalConstants.DataFeedName.BUSINESS_INTELLIGENCE,
                            Url = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.Location)) ? ds.Metadata.ReportMetadata.Location : null,
                            UrlType = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.LocationType)) ? ds.Metadata.ReportMetadata.LocationType : null,
                            Type = GlobalConstants.DataFeedType.Exhibits
                        };
                    }
                    else
                    {
                        df = new DataFeed()
                        {
                            Id = ds.DatasetId,
                            Name = GlobalConstants.DataFeedName.DATASET,
                            Url = "/Datasets/Detail/" + ds.DatasetId,
                            UrlType = ds.DatasetType,
                            Type = GlobalConstants.DataFeedType.Datasets
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
                            Name = GlobalConstants.DataFeedName.BUSINESS_INTELLIGENCE,
                            Url = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.Location)) ? ds.Metadata.ReportMetadata.Location : null,
                            UrlType = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.LocationType)) ? ds.Metadata.ReportMetadata.LocationType : null,
                            Type = GlobalConstants.DataFeedType.Exhibits
                        };
                    }
                    else
                    {
                        df = new DataFeed()
                        {
                            Id = ds.DatasetId,
                            Name = GlobalConstants.DataFeedName.DATASET,
                            Url = "/Datasets/Detail/" + ds.DatasetId,
                            UrlType = ds.DatasetType,
                            Type = GlobalConstants.DataFeedType.Datasets
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