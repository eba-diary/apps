namespace Sentry.data.Core
{
    public static class DataSourceExtensions
    {
        public static AuthenticationTypeDto ToDto(this AuthenticationType entity)
        {
            return new AuthenticationTypeDto()
            {
                AuthID = entity.AuthID,
                AuthName = entity.AuthName
            };
        }
    }
}
