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