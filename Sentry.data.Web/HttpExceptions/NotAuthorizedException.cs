using System;

namespace Sentry.data.Web
{
    public class NotAuthorizedException : System.Web.HttpException
    {
        public NotAuthorizedException(string message) : base(403, message)
        {
            //throw new NotImplementedException();
        }

        public NotAuthorizedException(string message, Exception inner) : base(403, message, inner)
        {
            //throw new NotImplementedException();
        }
    }
}
