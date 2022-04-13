using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IFavorable
    {
        FavoriteItem CreateFavoriteItem(UserFavorite userFavorite);
    }
}
