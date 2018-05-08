using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class S3Source : DataSource
    {
        public S3Source()
        {
            KeyCode = Guid.NewGuid().ToString().Substring(0, 13);

            //Default created and modified to same datetime value
            DateTime curDTM = DateTime.Now;
            Created = curDTM;
            Modified = curDTM;

            //Default to false, does not apply for S3 sources
            IsUserPassRequired = false;
            
        }

        public override Uri CalcRelativeUri(RetrieverJob Job)
        {
            throw new NotImplementedException();
        }

        public override string GetDropPrefix(RetrieverJob Job)
        {
            throw new NotImplementedException();
        }
    }
}
