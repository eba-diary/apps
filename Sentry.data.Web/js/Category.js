/******************************************************************************************
 * Javascript methods for the Category-related pages
 ******************************************************************************************/

data.Category = {

    Init: function () {
        /// <summary>
        /// Category page initialization
        /// </summary>
        $("#AddRootCategory").on('click', function () { data.Category.AddCategory(); })
        data.Category.BindCategoryButtons();
    },

    BindCategoryButtons: function () {
        /// <summary>
        /// Un-binds, then re-binds the event handlers.  
        /// This is required since DOM elements get reloaded dynamically as you edit categories.
        /// </summary>
        $("[id^='AddSubcategory_']").off('click').on('click', function (e) {
            data.Category.AddCategory($(this).data("id"))
        });

        $("[id^='DeleteCategory_']").off('click').on('click', function (e) {
            data.Category.DeleteCategory($(this).data("id"), $(this).data("parent-id"))
        });

        $("[id^='EditCategory_']").off('click').on('click', function (e) {
            data.Category.EditCategory($(this).data("id"))
        });
    },

    AddCategory: function (parentCategory) {
        /// <summary>
        /// Event handler for the "Add Subcategory" button
        /// </summary>
        /// <param name="parentCategory">The parent category to add the subcategory to</param>
        var modal = Sentry.ShowModalWithSpinner("Add Category");        

        var url = "/Category/Create/";
        if (parentCategory) { url = url + parentCategory; }
        $.get(url, function (result) {
            modal.ReplaceModalBody(result);
            modal.SetFocus("#Name");
        });
    },

    EditCategory: function (id) {
        /// <summary>
        /// Event handler for the "Edit Category" button
        /// </summary>
        /// <param name="id">The category to edit</param>
        var modal = Sentry.ShowModalWithSpinner("Edit Category");
        $.get("/Category/Edit/" + id, function (result) {
            modal.ReplaceModalBody(result);
            modal.SetFocus("#Name");
        });
    },

    DeleteCategory: function (id, parentCategory) {
        /// <summary>
        /// Event handler for the "Delete Category" button
        /// </summary>
        /// <param name="id">The category to delete</param>
        /// <param name="parentCategory">The parent category ID, that we now need to reload</param>
        var url = "/Category/Delete/" + id;
        $.post(url, {}, function (result) {
            data.Category.ReloadCategory(parentCategory);
        });
    },

    CreateInit: function () {
        /// <summary>
        /// Initialize the Create partial view
        /// </summary>
        $("#CreateCategoryForm").validateBootstrap(true);
    },

    EditInit: function () {
        /// <summary>
        /// Initialize the Edit partial view
        /// </summary>
        $("#EditCategoryForm").validateBootstrap(true);
    },

    AjaxSuccess: function (data, parentCategory) {
        /// <summary>
        /// Event handlers for a successful AJAX post from the Add or
        /// Edit category modal dialogs
        /// </summary>
        /// <param name="data">Response from the Ajax post</param>
        /// <param name="parentCategory">The parent category ID, that we now need to reload</param>
        if (Sentry.WasAjaxSuccessful(data)) {
            Sentry.HideAllModals();
            data.Category.ReloadCategory(parentCategory);
        }
    },

    AjaxFailure: function () {
        /// <summary>
        /// Called when there was a non-200 response from saving the category
        /// </summary>
        Sentry.ShowModalAlert("An error occurred saving the category.  Please try again.");
    },

    ReloadCategory: function (id) {
        /// <summary>
        /// Reloads a certain category, and everything underneath it, on the page
        /// </summary>
        /// <param name="id">The category to reload</param>
        if (!$.isNumeric(id)) { id = 0; }
        var url = "/Category/TreeNode/" + id;
        $.get(url, function (result) {
            $("#category-" + id).replaceWith(result);
        }).always(function () {
            data.Category.BindCategoryButtons();
        });
    }
}
