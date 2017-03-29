using Sentry.Core;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class AssetController : BaseController
    {
        private IAssetDynamicDetailsProvider _dynamicDetailsProvider;
        private IDataAssetContext _dataAssetContext;
        private UserService _userService;

        public AssetController(IDataAssetContext context, UserService userService, IAssetDynamicDetailsProvider dynamicDetailsProvider)
        {
            _dataAssetContext = context;
            _userService = userService;
            _dynamicDetailsProvider = dynamicDetailsProvider;
        }

        // GET: Asset/Details/5
        [HttpGet()]
        public ActionResult Details(int id)
        {
            ViewAssetDetailsModel asset = new ViewAssetDetailsModel(_dataAssetContext.GetById<Asset>(id), _userService);
            return View(asset);
        }

        // GET: Asset/Create
        [HttpGet()]
        public ActionResult Create()
        {
            EditAssetModel asset = new EditAssetModel {  };
            asset.AllCategories = GetCategorySelectListAssets();
            return View(asset);
        }

        // POST: Asset/Create
        [HttpPost()]
        public ActionResult Create(EditAssetModel i)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DomainUser seller = SharedContext.CurrentUser.DomainUser;
                    Asset asset = CreateAssetFromModel(i);
                    _dataAssetContext.Add(asset);
                    _dataAssetContext.SaveChanges();
                    return RedirectToAction("Details", new {id = asset.Id});
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _dataAssetContext.Clear();
            }

            i.AllCategories = GetCategorySelectListAssets(); //re-populate the category list for re-display
            return View(i);
        }

        [HttpPost()]
        public void SubmitForApproval(int id)
        {
            Asset asset = _dataAssetContext.GetById<Asset>(id);
            _dataAssetContext.SaveChanges();
        }

        // GET: Asset/Edit/5
        [HttpGet()]
        public ActionResult Edit(int id)
        {
            EditAssetModel asset = new EditAssetModel(_dataAssetContext.GetById<Asset>(id));
            asset.AllCategories = GetCategorySelectListAssets();
            return View(asset);
        }

        // POST: Asset/Edit/5
        [HttpPost()]
        public ActionResult Edit(int id, EditAssetModel i)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Asset asset = _dataAssetContext.GetById<Asset>(id);
                    UpdateAssetFromModel(asset, i);
                    _dataAssetContext.SaveChanges();
                    return RedirectToAction("Details", new {id = id});
                }

                i.AllCategories = GetCategorySelectListAssets(); //re-populate the category list for re-display
                return View(i);
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _dataAssetContext.Clear();
                i.AllCategories = GetCategorySelectListAssets(); //re-populate the category list for re-display
                return View(i);
            }
        }

        private IEnumerable<SelectListItem> GetCategorySelectListAssets()
        {
            return _dataAssetContext.Categories.OrderByHierarchy().Select((c) => new SelectListItem { Text = c.FullName, Value = c.Id.ToString() });
        }

        protected override void AddCoreValidationExceptionsToModel(Sentry.Core.ValidationException ex)
        {
            foreach (ValidationResult vr in ex.ValidationResults.GetAll())
            {
                switch (vr.Id)
                {
                    case Asset.ValidationErrors.nameIsBlank:
                        ModelState.AddModelError("Name", vr.Description);
                        break;
                    case Asset.ValidationErrors.descriptionIsBlank:
                        ModelState.AddModelError("Description", vr.Description);
                        break;
                    default:
                        ModelState.AddModelError(string.Empty, vr.Description);
                        break;
                }
            }
        }

        private Asset CreateAssetFromModel(EditAssetModel assetModel)
        {
            DomainUser seller = SharedContext.CurrentUser.DomainUser;
            Asset i = new Asset(assetModel.Name, assetModel.Description);
            foreach (int c in assetModel.CategoryIDs)
            {
                i.AddCategory(_dataAssetContext.GetReferenceById<Category>((int)c));
            }
            return i;
        }

        private void UpdateAssetFromModel(Asset asset, EditAssetModel assetModel)
        {
            asset.Name = assetModel.Name;
            asset.Description = assetModel.Description;

            List<Category> catsToRemove = asset.Categories.Where((c) => !assetModel.CategoryIDs.Contains(c.Id)).ToList();
            catsToRemove.ForEach((c) => asset.RemoveCategory(c));

            List<Category> catsToAdd = assetModel.CategoryIDs.Select((c) => _dataAssetContext.GetReferenceById<Category>(c)).ToList();
            catsToAdd = catsToAdd.Where((c) => !asset.Categories.Contains(c)).ToList();
            catsToAdd.ForEach((c) => asset.AddCategory(c));

        }
    }
}
