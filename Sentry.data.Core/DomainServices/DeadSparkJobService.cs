using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DeadSparkJobService : IDeadSparkJobService
    {
        private readonly IDeadJobProvider _deadJobProvider;

        public DeadSparkJobService(IDeadJobProvider deadJobProvider)
        {
            _deadJobProvider = deadJobProvider;
        }  

        public List<DeadSparkJobDto> GetDeadSparkJobDtos(int timeCreated)
        {
            List<DeadSparkJobDto> deadSparkJobDtoList = _deadJobProvider.GetDeadSparkJobDtos(timeCreated);

            return deadSparkJobDtoList;
        }
    }
}