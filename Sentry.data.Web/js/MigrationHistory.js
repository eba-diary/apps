data.MigrationHistory = {

    Init: function (sourceDatasetId) {

        //SETUP ON CLICK MAGIC MODAL
        $(document).on('click', '#viewMigrationHistoryJSON', function (e) {
            data.MigrationHistory.MagicModalMigrationHistory($(this).data("id"));
        });

        //INIT Migration Details Partial View
        data.MigrationHistory.RefreshMigrationDetails(sourceDatasetId);

        //SETUP NAMED ENV FILTER
        $("#migration-history-named-env-filter").change(function (e) {
            data.MigrationHistory.RefreshMigrationDetails(sourceDatasetId);
        });
    },

    RefreshMigrationDetails(sourceDatasetId) {

        //SHOW SPINNER
        $("#migration-history-detail-spinner").show();

        //GRAB NAMED ENV FILTER
        let namedEnv = $("#migration-history-named-env-filter").val();

        //MAKE AJAX CALL TO REPLACE HTML IN PARTIAL VIEW BASED ON namedEnv FILTER
        $.ajax({
            type: "POST",
            url: '/Migration/Detail/' + sourceDatasetId + '/' + namedEnv,
            success: function (view) {
                $('#migration-history-detail-container').html(view);
                $("#migration-history-detail-spinner").hide();
            },
            failure: function () {
                data.Dataset.makeToast("error", "Failed to Retrieve Migration History.");
                $("#migration-history-detail-spinner").hide();
            },
            error: function () {
                data.Dataset.makeToast("error", "Failed to Retrieve Migration History.");
                $("#migration-history-detail-spinner").hide();
            }
        });
    },

     //GENERATE QUERY BASED ON WHERE THEY ARE IN ACCORDIAN     
    MagicModalMigrationHistory: function (migrationHistoryId) {

        $.ajax({
            type: "POST",
            url: "/Migration/MagicModalMigrationHistory",
            traditional: true,
            data: JSON.stringify({ migrationHistoryId: migrationHistoryId }),
            contentType: "application/json",
            success: function (r) {
                var modal = Sentry.ShowModalWithSpinner("Migration History");
                modal.ReplaceModalBody(data.MigrationHistory.CreateMagicModalHTML(r.migrationHistoryJson));
            },
            failure: function () {
                data.Dataset.makeToast("error", "Migration History Error.");
            },
            error: function () {
                data.Dataset.makeToast("error", "Migration History Error.");
            }
        });
        
    },

     //FORMAT HTML TO SEND TOO MODAL
    //NOTE: instead of returning an entire partial view from controller, i just pass the snow query and format HTML in here, it shaved seconds off
    CreateMagicModalHTML: function (migrationHistoryJson) {

        var queryView = "<div class='magicQueryView'> ";
        var queryMania = " <div id='magicQueryMania' style='max-height: 600px; white-space: pre-line; overflow-y: auto; overflow-x: auto; '> " + migrationHistoryJson + " </div >";
        var modalFooter = " <div class='modal-footer'>  <input id='magicCopyClipboard' type='button' class='btn btn-warning' value='Copy to Clipboard' onclick='data.MigrationHistory.CopyClipboardFromMagicModal()' />     </div >";
        queryView = queryView + queryMania + modalFooter + " </div>";
        return queryView;
    },

     //COPY TEXT IN MODAL TO CLIPBOARD
    //NOTE: the reason I need to copy from delroyQueryMania is because it has the white-space style which interprets newlines and therefore a copy gets me a nicely formatted line
    //without that extra tag the copy directly from the output of the controller method DelroyGenerateQuery2 has the newlines but doesn't actually show formatting
    CopyClipboardFromMagicModal: function () {

        var range = document.createRange();
        range.selectNode(document.getElementById("magicQueryMania"));
        window.getSelection().removeAllRanges(); // clear current selection
        window.getSelection().addRange(range); // to select text
        document.execCommand("copy");
        window.getSelection().removeAllRanges();// to deselect
    },
}