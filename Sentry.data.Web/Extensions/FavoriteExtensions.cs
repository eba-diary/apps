using System.Collections.Generic;
using System.Text;

namespace Sentry.data.Web
{
    public static class FavoriteExtensions
    {
        public static FavoritesDetailModel ToModel(this List<Core.FavoriteDto> Favorites)
        {
            FavoritesDetailModel model = new FavoritesDetailModel();
            List<FavoriteItemDetailModel> itemModelList = new List<FavoriteItemDetailModel>();
            StringBuilder emails = new StringBuilder();
            bool firstEmail = true;
            foreach (Core.FavoriteDto item in Favorites)
            {
                FavoriteItemDetailModel dmodel = item.ToModel();
                itemModelList.Add(dmodel);
                if (!firstEmail)
                {
                    emails.Append(";" + dmodel.Email);
                }
                else
                {
                    emails.Append(dmodel.Email);
                    firstEmail = false;
                }

            }
            model.Favorites = itemModelList;
            model.MailToAllLink = "mailto:" + emails.ToString();
            return model;
        }

        public static FavoriteItemDetailModel ToModel(this Core.FavoriteDto fav)
        {
            return new FavoriteItemDetailModel()
            {
                UserName = fav.UserDisplayName,
                Email = fav.UserEmail
            };
        }

        public static FavoriteItemModel ToModel(this Core.FavoriteItem core)
        {
            return new FavoriteItemModel()
            {
                Id = core.Id,
                FeedId = core.FeedId,
                FeedName = core.FeedName,
                FeedUrl = core.FeedUrl,
                FeedUrlType = core.FeedUrlType,
                Img = core.Img,
                Sequence = core.Sequence,
                Title = core.Title,
                Url = core.Url,
                IsLegacyFavorite = core.IsLegacyFavorite
            };
        }
    }
}