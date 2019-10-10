using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web.Controllers
{
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
    public class DataFlowController : Controller
    {
        private readonly IDataFlowService _dataFlowService;

        public DataFlowController(IDataFlowService dataFlowService)
        {
            _dataFlowService = dataFlowService;
        }
        // GET: DataFlow        
        public ActionResult Index()
        {
            List<DataFlowDto> dtoList = _dataFlowService.ListDataFlows();
            List<DataFlowModel> modelList = dtoList.ToModelList();
            return View(modelList);
        }

        public ActionResult Detail(int id)
        {
            DataFlowDetailDto dto = _dataFlowService.GetDataFlowDetailDto(id);
            DataFlowDetailModel model = new DataFlowDetailModel(dto);

            return View(model);
        } 
    }
}