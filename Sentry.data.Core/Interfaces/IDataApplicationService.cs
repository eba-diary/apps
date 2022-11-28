using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDataApplicationService
    {
        /// <summary>
        /// Delete dataset(s) and all associated children.
        /// </summary>
        /// <param name="deleteIdList">List of dataset id's</param>
        /// <param name="user">User issuing the delete</param>
        /// <param name="forceDelete">True = Will force objectstatus to deleted regardless of status.  False = Sets objectstatus to Pending_Delete if applicable</param>
        /// <returns>True = Delete was successfull and changed committed.  
        /// False = Delete failed and no changes committed.</returns>
        bool DeleteDataset(List<int> deleteIdList, IApplicationUser user, bool forceDelete = false);
        /// <summary>
        /// Delete datasetfileconfig(s) and all associated children
        /// </summary>
        /// <param name="deleteIdList">List of datasetfileconfig id(s)</param>
        /// <param name="user">User issuing the delete</param>
        /// <param name="forceDelete">True = Will force objectstatus to deleted regardless of status.  False = Sets objectstatus to Pending_Delete if applicable</param>
        /// <returns>True = Delete was successfull and changed committed.  
        /// False = Delete failed and no changes committed.</returns>
        bool DeleteDatasetFileConfig(List<int> deleteIdList, IApplicationUser user, bool forceDelete = false);
        /// <summary>
        /// Delete dataflow(s) and all associated children.
        /// </summary>
        /// <param name="deleteIdList">List of dataflow id(s)</param>
        /// <param name="user">User issuing the delete</param>
        /// <param name="forceDelete">True = Will force objectstatus to deleted regardless of status.  False = Sets objectstatus to Pending_Delete if applicable</param>
        /// <returns>True = Delete was successfull and changed committed.  
        /// False = Delete failed and no changes committed.</returns>
        bool DeleteDataflow(List<int> deleteIdList, IApplicationUser user, bool forceDelete = false);
        /// <summary>
        /// Each dataflow delete is sent to a Hangfire queue to be performed async.  When executed via Hangfire,
        ///   will delete dataflow(s)
        /// </summary>
        /// <param name="deleteIdList"></param>
        /// <param name="userId"></param>
        /// <param name="forceDelete">True = Will force objectstatus to deleted regardless of status.  False = Sets objectstatus to Pending_Delete if applicable</param>
        /// <returns>True = True if all dataflow deletes were successfully submitted to hangfire.</returns>
        bool DeleteDataFlow_Queue(List<int> deleteIdList, string userId, bool forceDelete = false);
    }
}
