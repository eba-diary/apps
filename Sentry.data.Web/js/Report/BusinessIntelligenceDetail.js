/******************************************************************************************
 * Javascript methods for the Asset-related pages
 ******************************************************************************************/

data.BusinessIntelligenceDetail = {

    DatasetFilesTable: {},

    Init: function () {
        /// <summary>
        /// Initialize the dataset detail page for data assets
        /// </summary>

        var Id = $('#datasetConfigList').val();

        $("[id^='EditDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.BusinessIntelligenceDetail.ViewEdit($(this).data("id"));
        });

        $("[id^='SubscribeModal']").click(function (e) {
            e.preventDefault();

            data.BusinessIntelligenceDetail.SubscribeModal($(this).data("id"));
        });

        $('#deleteLink').click(function (e) {
            e.preventDefault();

            var modal = Sentry.ShowModalConfirmation("Delete Exhibit", function () { data.BusinessIntelligenceDetail.deleteDataset("BusinessIntelligence", window.location.pathname.split('/')[3]); });
            
            modal.ReplaceModalBody("This will <u>permanently</u> delete this Exhibit (<b>not the object which it references</b>). </br></br> Do you wish to continue?");

            modal.show();
        });

        $("[id^='detailSectionHeader_']").click(function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var category = "#hide_" + id;
            var icon = "#icon_" + id;

            $(category).slideToggle();
            $(icon).toggleClass("glyphicon-chevron-down glyphicon-chevron-up");
        });
    },

    PreviewDatafileModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Preview Datafile");

        $.get("/Dataset/PreviewDatafile/" + id, function (result) {
            modal.ReplaceModalBody(result);
        });
    },

    SubscribeModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Subscribe");
        var Url = "/Dataset/Subscribe/?id=" + encodeURI(id);

        $.get(Url, function (e) {
            modal.ReplaceModalBody(e);
        });      
    },

    deleteDataset: function (objectType, id) {

        var outUrl = "/" + objectType + "/Delete/" + encodeURI(id);
        var returnUrl = "/" + objectType + "/Index";

        var request = $.ajax({
            url: outUrl,
            method: "POST",
            dataType: 'json',
            success: function (obj) {
                if (obj.Success) {
                    var modal = Sentry.ShowModalConfirmation(
                        obj.Message, function () { window.location = returnUrl })
                }
                else {
                    var modal = Sentry.ShowModalAlert(
                        obj.Message, function () { window.location = returnUrl })
                }
            },
            failure: function (obj) {
                var modal = Sentry.ShowModalAlert(
                    obj.Message, function () { location.reload() })
            },
            error: function (obj) {
                var modal = Sentry.ShowModalAlert(
                    obj.Message, function () { location.reload() })
            }
        });

    },

    ViewEdit: function (id) {
        /// <summary>
        /// Load the Edit Asset view
        /// </summary>
        url = "/BusinessIntelligence/Edit/?" + "id=" + encodeURI(id);
        window.location = url;
    },

    ExternalFile: function (data) {

    }
}