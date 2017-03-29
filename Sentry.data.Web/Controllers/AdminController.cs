using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data;
using Sentry.DataTables.QueryableAdapter;
using Sentry.DataTables.Mvc;
using Sentry.DataTables.Shared;
using DoddleReport.Web;
using DoddleReport;


namespace Sentry.data.Web.Controllers
{
    public class AdminController : BaseController
    {
        private IDataAssetContext _domainContext;
        private UserService _userService;

        public AdminController(IDataAssetContext context, UserService userService)
        {
            _domainContext = context;
            _userService = userService;
        }

        [HttpGet()]
        public ActionResult ManageUsers()
        {
            return View();
        }

        /// <summary>
        /// This is an example of a method that returns only a partial set of records for a DataTable grid.  You would use this 
        /// type of method when using server-side paging/sorting/filtering.  DataTables passes a specific data structure to the 
        /// server that tells what columns are being sorted, what filters should be applied, and what page size and page number
        /// should be used.  The DataTablesBinder knows how to transform this DataTables data structure into a .NET object, which
        /// can then be passed to the DataTablesQueryableAdapter.  The DataTablesQueryableAdapter can then take any
        /// IQueryable and dynamically apply the filtering, sorting, and paging that is being requested by DataTables.
        /// </summary>
        /// <param name="dtRequest"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public JsonResult GetUserInfoForGrid([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            // NOTE:  this is a really poor server-side implementation.  We are basically getting
            // ALL users from the database, running each through the _userService, before applying paging/sorting/filtering
            // in-memory.  A better implementation would do as much of the paging/sorting/filtering in the database
            // as possible.  A future update to this demo will show how to do this when some of the data for the grid
            // comes from outside sources, as is the case here.  If all the data for the grid comes from your database
            // through an IQueryable on your domain context, then it's super easy to implement everything in the database.
            IEnumerable<UserGridModel> users = _domainContext.Users.ToList().Select(((u) => _userService.GetByDomainUser(u))).
                Select(((u) => new UserGridModel() {
                    Id = u.DomainUser.Id,
                    AssociateId = u.DomainUser.AssociateId,
                    Name = u.DisplayName,
                    Ranking = u.DomainUser.Ranking }));

            DataTablesQueryableAdapter<UserGridModel> dtqa = new DataTablesQueryableAdapter<UserGridModel>(users.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This is an example of a method that returns ALL records for a DataTable grid.  You would use this type of 
        /// method when using client-side paging/sorting/filtering, since the grid needs all the records up-front.
        /// No filtering, sorting, or paging is applied server-side in this case.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public JsonResult GetAllUserInfoForGrid()
        {
            IEnumerable<UserGridModel> users = _domainContext.Users.ToList().Select(((u) => _userService.GetByDomainUser(u))).
                Select(((u) => new UserGridModel() {
                    Id = u.DomainUser.Id,
                    AssociateId = u.DomainUser.AssociateId,
                    Name = u.DisplayName,
                    Ranking = u.DomainUser.Ranking }));

            return Json(new { data = users }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// This is an example of a method that returns an Excel or other type of data export using Doddle Reports.
        /// Note that this method takes in the same DataTables request object as the server-side GetUserInfoForGrid method
        /// above.  This ensures that the same filtering and sorting that is being applied within the DataTables 
        /// (whether server-side or client-side) carries forward to the data export.  Note, however, that paging is ignored, 
        /// since we want to export ALL records, not just those records being displayed on the current page in DataTables.
        /// </summary>
        /// <param name="dtRequest"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public ReportResult UserExport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {

            IEnumerable<UserGridModel> users = _domainContext.Users.ToList().Select(((u) => _userService.GetByDomainUser(u))).
              Select(((u) => new UserGridModel() {
                  Id = u.DomainUser.Id,
                  AssociateId = u.DomainUser.AssociateId,
                  Name = u.DisplayName,
                  Ranking = u.DomainUser.Ranking }));


            DataTablesQueryableAdapter<UserGridModel> dtqa = new DataTablesQueryableAdapter<UserGridModel>(users.AsQueryable(), dtRequest);

            //The True parameter being passed to GetDataTablesResponse indicates that Paging should be ignored.
            System.Collections.IEnumerable filteredData = dtqa.GetDataTablesResponse(true).data;

            //At this point, filteredData has only the rows that are being filtered in the grid, sorted by
            //the columns as configured in the grid.  

            //Export using DoddleReports as normal...

            Report report = new Report(filteredData.ToReportSource());
            report.DataFields["Id"].Hidden = true;
            // If excel, we need to tell it to format the associate ID like a string, not a number
            if (Request.Url.AbsolutePath.EndsWith("xlsx", StringComparison.CurrentCultureIgnoreCase))
            {
                report.DataFields["AssociateId"].DataFormatString = "'{0}";
            }

            return new ReportResult(report);

        }

        [HttpPost()]
        public void DeleteUser(int id)
        {
            // TODO:  implement security check & referential integrity check
            _domainContext.RemoveById<DomainUser>(id);
            _domainContext.SaveChanges();
        }

        [HttpGet()]
        public ActionResult EditUser(int id)
        {
            IApplicationUser user = _userService.GetById(id);
            UserModel userModel = new UserModel(user);
            return View(userModel);
        }

        [HttpPost()]
        public ActionResult EditUser(UserModel userModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IApplicationUser user = _userService.GetById(userModel.Id);
                    UpdateUserFromModel(user.DomainUser, userModel);
                    _domainContext.SaveChanges();
                    return RedirectToAction("ManageUsers");
                }
                return View(userModel);
            }
            catch (Sentry.Core.ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(userModel);
            }
        }

        private void UpdateUserFromModel(DomainUser user, UserModel userModel)
        {
            user.Ranking = userModel.Ranking;
        }

        private DomainUser CreateUserFromModel(UserModel userModel)
        {
            DomainUser user = new DomainUser(userModel.AssociateId);
            user.Ranking = userModel.Ranking;
            return user;
        }

    }
}
