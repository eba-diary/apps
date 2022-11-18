namespace Sentry.data.Core
{
    public static class DataSourceExtensions
    {
        public static DataSourceTypeDto ToDto(this DataSourceType entity)
        {
            return new DataSourceTypeDto()
            {
                Name = entity.Name,
                Description = entity.Description,
                DiscrimatorValue = entity.DiscrimatorValue
            };
        }

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
