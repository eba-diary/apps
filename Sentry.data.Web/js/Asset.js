/******************************************************************************************
 * Javascript methods for the Asset-related pages
 ******************************************************************************************/

data.Asset = {

    
    DetailsInit: function (id) {
        /// <summary>
        /// Initialize the Details.vbhtml page
        /// </summary>
        /// <param name="id">The ID of the asset we're viewing</param>

        //If the SubmitForApproval section exists, then wire up the
        //Submit for Approval button
        if ($("#SubmitForApproval")) {
            $("#SubmitForApproval").on("click", function () {
                $.post("/Asset/SubmitForApproval/" + id, {}, function (data) {
                    $("#SubmitForApprovalAlert").hide("slow");
                    data.Asset.ReloadBidDetails(id);
                }).fail(function () {
                    alert("An error occurred submitting this asset for approval.");
                });
            });
        }

        //Show a modal dialog with a spinner in it, and then load
        //the body of the modal via AJAX
        $("#placebid").on("click", function () {
            var modal = Sentry.ShowModalWithSpinner("Edit Asset");
            $.get("/Asset/Edit/" + id, modal.ReplaceModalBody);
            });
    },

    CreateInit: function () {
        /// <summary>
        /// Initialize the Create Asset view
        /// </summary>
        $("#CategoryIDs").select2({
            placeholder: "Click here to choose one or more categories"
        });
    },

    EditInit: function () {
        /// <summary>
        /// Initialize the Edit Asset view
        /// </summary>
        data.Asset.CreateInit();
    }

}
