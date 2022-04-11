using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class UserFavoriteService : IUserFavoriteService
    {
        private readonly IUserFavoriteRepository _userFavoriteRepository;

        public UserFavoriteService(IUserFavoriteRepository userFavoriteRepository)
        {
            _userFavoriteRepository = userFavoriteRepository;
        }
        
        public IList<FavoriteItem> GetUserFavoriteItems(string associateId)
        {
            //get favorites from repository
            
            //get entity objects of favorites (where IFavorable comes in)

            //create the list of favorite items from entity objects (IFavorable.CreateFavoriteItem())
            
            //this method will also serve as backwards compatibility for existing favorite setup
            //look at DataFeedProvider.GetUserFavorites() for it is currently being done
            throw new NotImplementedException();
        }
    }
}
