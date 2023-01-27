/******************************************************************************************
 * Javascript methods for the _Layout
 ******************************************************************************************/

data._Layout = {

    Init: function () {
        /// <summary>
        /// Page Initialization for the _Layout.vbhtml view
        /// </summary>

        // wire up search box
        $("#SearchForm").on("submit", function () {
            window.location.href = "/Search/" + $("#SearchText").val();
            return false;
        });

        // wire up restore user link
        $("#restoreUser").on("click", function () {
            data._Layout.restoreUser();
            return false;
        });

        $(document).on('select2:open', () => {
            document.querySelector('.select2-search__field').focus();
        });

        /* fix for pagination buttons holding onto focus style after click */
        $(document).on('mouseup', '.pagination .page-item .page-link', () => {
            document.activeElement.blur();
        })

        $("body").removeClass("prevent-animation");
    }
    , restoreUser: function () {
        $.post("/User/Restore").always(
                function () {
                    window.location.reload();
                });
    }
    //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
    , showRefreshDemoDataConfirmation: function () {
        /// <summary>
        /// Show the refresh demo data modal
        /// </summary>

        // Here's an example of displaying a modal with custom buttons
        Sentry.ShowModalCustom("Refresh Demo Data",
            "Delete all Sentry.Data data and refresh with defaults?",
            {
                Delete: {
                    label: "Delete",
                    className: "btn-danger",
                    callback:  data._Layout.confirmRefreshDemoData
                },
                Cancel: {
                    label: "Cancel",
                    className: "btn-link"
                }
        });
        return false;
    },

    confirmRefreshDemoData: function () {
        /// <summary>
        /// The user confirmed they want to refresh the demo data, so do the POST,
        /// and navigate back to the home page.  Returns false so that the modal
        /// will stay open while the AJAX request is in process.
        /// </summary>
        $.post("/DemoData/Refresh")
                    .fail(function () {
                        alert("An error occurred while refreshing the demo data.  Please try again.");
                    Sentry.HideAllModals();
                    })
                    .always(function () {
                        console.log("hi");
                        window.location = "/";
                    });
        return false;
    }
    //###  END Sentry.Data  ### - Code above is Sentry.Data-specific

}
