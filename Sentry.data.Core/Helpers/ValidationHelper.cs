using System.Text.RegularExpressions;

namespace Sentry.data.Core
{
    public static class ValidationHelper
    {
        public static bool IsDSCEmailValid(string email)
        {
            if (email != null)
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(email);
                if (!match.Success || !email.ToUpper().Contains("@SENTRY.COM") || email.Length > 256)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
