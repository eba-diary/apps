namespace Sentry.data.Core.Interfaces
{
    public interface IEntityService
    {
        /// <summary>
        /// Deletes object and all child objects.
        /// </summary>
        /// <param name="id">Id of object to be deleted</param>
        /// <param name="user">User performing the delete</param>
        /// <param name="logicalDelete">False = Sets objectstatus to Deleted.  True = Sets objectstatus to Pending_Delete if not already deleted. </param>
        /// <remarks>Method does not commit changes</remarks>
        /// <returns>True = Successful  
        /// False = Not successful.</returns>
        bool Delete(int id, IApplicationUser user, bool logicalDelete);
    }
}
