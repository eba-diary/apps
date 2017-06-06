/******************************************************************************************
 * Javascript methods for the Asset-related pages
 ******************************************************************************************/

data.Dataset = {
    
    IndexInit: function () {
        /// <summary>
        /// Initialize the Index page for data assets (with the categories)
        /// </summary>
        $("[id^='category']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.DisplayCategory($(this).data("category"));
        });

        $("[id^='UploadDataset']").click(function (e) {
            e.preventDefault();
            data.Dataset.ViewUpload();
        });
    },

    ListInit: function () {
        /// <summary>
        /// Initialize the List results page for data assets
        /// </summary>
        $("[id^='datasetViewDetails_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.ViewDetails($(this).data("id"))
        });

        $("[id^='DownloadDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.DownloadDataset($(this).data("id"))
        });
/*
        $("[id^='datasetViewDetails_']").mouseenter(function (e) {
            e.preventDefault();
            $("[id=datasetViewDetailsHover_" + $(this).data("id") + "]").fadeIn();
        });

        $("[id^='datasetViewDetails_']").mouseleave(function (e) {
            e.preventDefault();
            $("[id=datasetViewDetailsHover_" + $(this).data("id") + "]").fadeOut(0);
        });
*/
        $("[id^='li-filter-disabled_']").hover(function (e) {
            $(this).children('span.filter-disabled-glyph').css({ 'display': 'inline-block' })
        }, function (e) {
            $(this).children('span.filter-disabled-glyph').css({ 'display': 'none' })
        });

        $("[id^='li-filter-disabled_']").click(function (e) {
            e.preventDefault();
            data.Dataset.DisplayCategory($(this).data("cat"));
        });

        $("[id^='li-filter-enabled_']").click(function (e) {
            e.preventDefault();
            data.Dataset.DisplayCategory(null);
        });
        
        $("[id^='Pushtofilename_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.FileNameModal($(this).data("id"))
        });

        $(document).on('show.bs.modal', '.modal', function (event) {
            var zIndex = 1040 + (10 * $('.modal:visible').length);
            $(this).css('z-index', zIndex);
            setTimeout(function () {
                $('.modal-backdrop').not('.modal-stack').css('z-index', zIndex - 1).addClass('modal-stack');
            }, 0);
        });

    },

    DetailInit: function () {
        /// <summary>
        /// Initialize the dataset detail page for data assets
        /// </summary>
        $("[id^='EditDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.ViewEdit($(this).data("id"))
        });

        $("[id^='DownloadDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.DownloadDataset($(this).data("id"))
        });

        $("[id^='PushtoSAS_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PushToSAS($(this).data("id"))
        });

        $("[id^='Pushtofilename_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.FileNameModal($(this).data("id"))
        });

        $(document).on('show.bs.modal', '.modal', function (event) {
            var zIndex = 1040 + (10 * $('.modal:visible').length);
            $(this).css('z-index', zIndex);
            setTimeout(function () {
                $('.modal-backdrop').not('.modal-stack').css('z-index', zIndex - 1).addClass('modal-stack');
            }, 0);
        });
    },

    UploadInit: function () {
        /// <summary>
        /// Initialize the Create Asset view
        /// </summary>
        //$("#CategoryIDs").select2({
        //    placeholder: "Click here to choose one or more categories"
        //});
    },

    EditInit: function () {
        /// <summary>
        /// Initialize the Edit Dataset view
        /// </summary>
        //var x = 3;
        //$("#CategoryIDs").select2({
        //    placeholder: "Click here to choose one or more categories"
        //});
    },

    ViewDetails: function (id) {
        /// <summary>
        /// Load the Dataset Details page
        /// </summary>
        var url = "/Dataset/Detail/?id=" + encodeURI(id);
        window.location = url;
    },

    DownloadDataset: function (id) {
        /// <summary>
        /// Send temp URL (containing the dataset, from S3) to a new window
        /// This will initiate the download process
        /// </summary>
        var controllerURL = "/Dataset/GetDownloadURL/?id=" + encodeURI(id);
        $.get(controllerURL, function (result) {
            var jrUrl = result;
            window.open(jrUrl, "_blank");
        });
    },

   PushToSAS_Filename: function (filename) {
        /// <summary>
        /// Download dataset from S3 and push to SAS file share
        /// </summary>
        var val = document.getElementById('FileNameOverride').value
        var controllerURL = "/Dataset/PushToSAS/?id=" + encodeURI(id) + "&filename=" + encodeURI(val);
        data.Dataset.ProgressModalStatus();
        $.post(controllerURL);
    },

    PushToOverrideInit: function() {
        ///<summary>
        ///Initialize the PushToFilenameOverride partial view
        ///</summary>
        $("PushToForm").validateBootstrap(true);

        //$("[id^='FilenameOverride']").off('click').on('click', function (e) {
        //    e.preventDefault();
        //    data.Dataset.PushToSAS_Filename($(this).data("filename"))
        //});
    },

    DisplayCategory: function (category) {
        /// <summary>
        /// Load a dataset Listing for a given category
        /// </summary>
        searchPhrase = data.Dataset.getParameterByName("searchPhrase")
        url = "/Dataset/List"
        if (searchPhrase != null && searchPhrase != "") {
            url = url + "?searchPhrase=" + encodeURI(searchPhrase);
        }
        if (category != null && category != "") {
            if (searchPhrase != null && searchPhrase != "") {
                url = url + "&category=" + encodeURI(category);
            } else {
                url = url + "?category=" + encodeURI(category);
            }
        }
        window.location = url;
    },

    AjaxSuccess: function () {
        /// <summary>
        /// Event handlers for a successful AJAX post from the Add or
        /// Edit category modal dialogs
        /// </summary>
        /// <param name="data">Response from the Ajax post</param>
        /// <param name="parentCategory">The parent category ID, that we now need to reload</param>
        //if (Sentry.WasAjaxSuccessful(data)) {
            Sentry.HideAllModals();
            //var controllerURL = "/Dataset/PushToSAS/?id=" + encodeURI(datasetID) + "&Override=" + encodeURI(filenameOverride);
            //data.Dataset.ProgressModalStatus();
            //$.post(controllerURL);
            //dataMySentryBayTestApp.Category.ReloadCategory(parentCategory);
        //}
    },

    AjaxFailure: function () {
        /// <summary>
        /// Called when there was a non-200 response
        /// </summary>
        Sentry.ShowModalAlert("An error occurred.  Please try again.");
    },

    ViewEdit: function (id) {
        /// <summary>
        /// Load the Edit Asset view
        /// </summary>
        url = "/Dataset/Edit/?" + "id=" + encodeURI(id);
        window.location = url;
    },

    ViewUpload: function () {
        /// <summary>
        /// Load the Edit Asset view
        /// </summary>
        url = "/Dataset/Upload";
        window.location = url;
    },

    getParameterByName: function (name, url) {
        if (!url) {
            url = window.location.href;
        }
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    },

    ProgressModalStatus: function () {
// --- progress bar stuff : start ---
        // Reference the auto-generated proxy for the hub.
        var progress = $.connection.progressHub;
        console.log(progress);

        // Create a function that the hub can call back to display messages.
        progress.client.AddProgress = function (message, percentage) {
            ProgressBarModal("show", message, "Progress: " + percentage);
            $('#ProgressMessage').width(percentage);
            if (percentage == "100%") {
                ProgressBarModal();
            }
        };

        $.connection.hub.start().done(function () {
            var connectionId = $.connection.hub.id;
            console.log(connectionId);
        });

        // --- progress bar stuff : end ---

    },

    FileNameModal: function (id) {
        var modal = Sentry.ShowModalWithSpinner("File Name Override");

        $.get("/Dataset/PushToFileNameOverride/" + id, function (result) {
            modal.ReplaceModalBody(result);
            modal.SetFocus("#FileNameOverride");
        });
    }
    
}

