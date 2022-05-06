namespace Sentry.data.Core
{
    public interface IFavorable
    {
        void SetFavoriteItem(FavoriteItem favoriteItem);
        string GetFavoriteType();
        int GetFavoriteEntityId();
    }
}
