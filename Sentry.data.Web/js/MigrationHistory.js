data.MigrationHistory = {
    
    Init: function () {

        $("[id^='viewMigrationHistoryJSON']").off('click').on('click', function (e) {
            data.MigrationHistory.MagicModalMigrationHistory($(this).data("id"));
        });
    },

     //GENERATE QUERY BASED ON WHERE THEY ARE IN SCHEMA     
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