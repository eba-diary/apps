using Sentry.data.Core.Exceptions;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

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
        /// <summary>
        /// Performs the migration of a dataset between named environments associated with the dataset SAID asset code.
        /// <para>Optional: Schema, associated with source dataset, can be included in migration request.</para>
        /// </summary>
        /// <param name="migrationRequest"></param>
        /// <exception cref="DatasetUnauthorizedAccessException">Thrown when user not authorized to migrate dataset.</exception>
        /// <returns></returns>
        Task<DatasetMigrationRequestResponse> MigrateDataset(DatasetMigrationRequest migrationRequest);
        /// <summary>
        /// Performs the migration of a schema between named environemnts associated with parent dataset SAID asset code.
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        SchemaMigrationRequestResponse MigrateSchema(SchemaMigrationRequest request);
    }
}
