using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class UserFavoriteRepository : IUserFavoriteRepository
    {
        private readonly IDatasetContext _datasetContext;
        
        public UserFavoriteRepository(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public IList<UserFavorite> GetUserFavorites(string associateId)
        {
            //get user favorites by associate id from context
            throw new NotImplementedException();
        }
    }
}
