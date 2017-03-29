using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    //Since this controller is for admin purposes, you would put an authorize attribute on it
    //<AuthorizeApproveAssets>
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class CategoryController : BaseController
    {

        private IDataAssetContext _domainContext;
        public CategoryController(IDataAssetContext context)
        {
            _domainContext = context;
        }

        // GET: Category
        [HttpGet()]
        public ActionResult Index()
        {
            List<FullCategoryModel> cats = _domainContext.Categories.
                        WhereIsRoot().
                        Select(((c) => new FullCategoryModel(c, true))).
                        ToList();
            return View(cats);
        }

        [HttpGet()]
        public PartialViewResult TreeNode(int id)
        {
            FullCategoryModel model;
            if (id == 0)
            {
                List<FullCategoryModel> cats = _domainContext.Categories.WhereIsRoot().Select(((c) => new FullCategoryModel(c, true))).ToList();
                model = new FullCategoryModel {Id = 0,
                                                Name = string.Empty,
                                                SubCategories = cats
                                              };
            }
            else
            {
                Category cat = _domainContext.GetById<Category>(id);
                model = new FullCategoryModel(cat);
            }

            return PartialView("_CategoryTreeNode", model);
        }

        // GET: Category/Create
        [HttpGet()]
        public PartialViewResult Create(int? id)
        {
            EditCategoryModel model = new EditCategoryModel();
            if (id.HasValue)
            {
                Category parentCategory = _domainContext.GetById<Category>(id.Value);
                model.ParentCategoryId = parentCategory.Id;
                model.ParentCategoryName = parentCategory.Name;
            }
            return PartialView("_Create", model);
        }

        // POST: Category/Create
        [HttpPost()]
        public ActionResult Create(EditCategoryModel category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Category asset = CreateCategoryFromModel(category);
                    _domainContext.Add(asset);
                    _domainContext.SaveChanges();
                    return AjaxSuccessJson();
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                this.AddCoreValidationExceptionsToModel(ex);
                _domainContext.Clear();
            }


            return PartialView("_Create", category);
        }

        // GET: Category/Edit/5
        [HttpGet()]
        public ActionResult Edit(int id)
        {
            EditCategoryModel model = new EditCategoryModel(_domainContext.GetById<Category>(id));
            return PartialView("_Edit", model);
        }

        // POST: Category/Edit/5
        [HttpPost()]
        public ActionResult Edit(int id, EditCategoryModel category)
        {
            try
            {
                Category coreCategory = _domainContext.GetById<Category>(id);
                if (ModelState.IsValid)
                {
                    coreCategory.Name = category.Name;
                    _domainContext.SaveChanges();
                    return AjaxSuccessJson();
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                this.AddCoreValidationExceptionsToModel(ex);
                _domainContext.Clear();
            }
            return PartialView("_Edit", category);
        }

        // POST: Category/Delete/5
        [HttpPost()]
        public void Delete(int id)
        {
            //now delete the category
            _domainContext.RemoveById<Category>(id);
            _domainContext.SaveChanges();
        }

        private Category CreateCategoryFromModel(EditCategoryModel model)
        {
            Category parentCategory = null;
            if (model.ParentCategoryId.HasValue)
            {
                parentCategory = _domainContext.GetById<Category>(model.ParentCategoryId.Value);
            }
            Category category = new Category(model.Name, parentCategory);
            return category;
        }

    }
}
