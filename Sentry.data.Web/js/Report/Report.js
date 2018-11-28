/******************************************************************************************
 * Javascript methods for the Asset-related pages
 ******************************************************************************************/

data.Report = {

    DatasetFilesTable: {},

    IndexInit: function () {
        /// <summary>
        /// Initialize the Index page for data assets (with the categories)
        /// </summary>
        $("[id^='category']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Report.DisplayCategory($(this).data("category"));
        });

        $("[id^='CreateDataset']").click(function (e) {
            e.preventDefault();
            data.Report.ViewUpload();
        });
    },

    ListInit: function () {
        /// <summary>
        /// Initialize the List results page for data assets
        /// </summary>

        //console.log(window.location.href);

        $("[id^='DownloadDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.DownloadDataset($(this).data("id"));
        });

        $("[id^='li-filter-disabled_']").hover(function (e) {
            $(this).children('span.filter-disabled-glyph').css({ 'display': 'inline-block' });
        }, function (e) {
            $(this).children('span.filter-disabled-glyph').css({ 'display': 'none' });
        });

        $("[id^='Pushtofilename_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.FileNameModal($(this).data("id"));
        });

        //Testing Functionality
        $("[id^='li-filter-disabled_Test_']").click(function (e) {
            e.preventDefault();
            data.Dataset.DisplayCategory_test($(this).data("filter-type"), $(this).data("value"));
        });

        $("[id^='Category_']").click(function (e) {
            var $btn = $("[id^=btnApply_Category]");
            $btn.removeAttr('hidden');
        });

        $("[id^='Frequency_']").click(function (e) {
            var $btn = $("[id^=btnApply_Frequency]");
            $btn.removeAttr('hidden');
        });

        $("[id^='Sentry Owner_']").click(function (e) {
            var $btn = $("[id^=btnApply_Sentry]");
            $btn.removeAttr('hidden');
        });

        $("[id^='PreviewData']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PreviewDataModal($(this).data("id"));
        });


        var stickySidebar = $('.outsideFilters').offset().top;
        var el = $('.menu');
        var bottom = el.position().top + el.outerHeight(true) + 10;

        $("[id^='filterType_']").click(function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var category = "#hide_" + id
            var icon = "#icon_" + id;

            $(category).slideToggle();
            $(icon).toggleClass("glyphicon-chevron-down glyphicon-chevron-up");
        });

        $("[id^='filterMore_']").click(function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var show = "#hidden_" + id
            var icon = "#icon_" + id;
            var txt = "#txt_" + id;

            $(show).slideToggle();
            $(icon).toggleClass("glyphicon-plus-sign glyphicon-minus-sign");

            if ($(txt).text() === "Show Less") {
                $(txt).text("Show More");
            }
            else {
                $(txt).text("Show Less");
            }
        });
    },

    DetailInit: function () {
        /// <summary>
        /// Initialize the dataset detail page for data assets
        /// </summary>
        $("[id^='EditDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.ViewEdit($(this).data("id"));
        });

        $("[id^='DownloadDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.DownloadDataset($(this).data("id"));
        });

        $("[id^='PushtoSAS_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PushToSAS($(this).data("id"));
        });

        $("[id^='Pushtofilename_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.FileNameModal($(this).data("id"));
        });

        $("[id^='PreviewData']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PreviewDataModal($(this).data("id"));
        });

        $(".DatasetFile_ID").click(function () {
            alert("DatasetFile_ID function")
        })

         $("#datasetFilesTable").DataTable({
            autoWidth: false,
            serverSide: true,
            processing: true,
            searching: false,
            paging: true,
            ajax: {
                url: "/Dataset/GetDatasetFileInfoForGrid/?Id=" + Id,
                type: "POST"
            },
            columns: [
                        {
                            data: "Id",
                            width: "20%",
                            type: "num",
                            className: "DatasetFile_ID"
                        },
                        { data: "Name", width: "50%", className: "Name" },
                        { data: "UploadUserName", className: "UploadUserName" }
                        //,{ data: "Id", render: data.Dataset.make_upload_links}
            ],
            order: [1, 'asc']
            //stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
        });

        $("#datasetFilesTable").dataTable().columnFilter({
            sPlaceHolder: "head:after",
            aoColumns: [
                    { type: "number-range" },
                    { type: "text" },
                    { type: "text" }
                    //, null
            ]
        });

        // DataTable
        var table = $('#datasetFilesTable').DataTable();

        // Apply the filter
        table.columns().every(function () {
            var column = this;

            $('input', this.footer()).on('keyup change', function () {
                column
                    .search(this.value)
                    .draw();
            });
        });

        $("#userTable_wrapper .dt-toolbar").html($("#userToolbar"));

        $("#exportToExcel").click(function () {
            alert("exportToExcel Function");
        });
    },

    make_upload_links: function (cellData, type, rowData) {
        return String.format($("#userTableRowIcons").html(), rowData.Id);
    },

    //showConfirmDeleteModal: function (cell) {
    //    alert("showConfirmDeleteModal");
    //    alert(cell);
    //},

    CreateInit: function () {
        /// <summary>
        /// Initialize the Create Dataset view
        /// </summary>

        //Set Secure HREmp service URL for associate picker
        $.assocSetup({ url: "https://hrempsecure.sentry.com/api/associates" });

        var picker = $("#OwnerID");

        picker.assocAutocomplete({
            associateSelected: function (associate) {
                $('#SentryOwnerName').val(associate.Id);
            }
        });

        //Initialize FileType description
        data.Report.setFileTypeInfo();
    },

    EditInit: function () {
        /// <summary>
        /// Initialize the Edit Dataset view
        /// </summary>

        var n1 = document.getElementById('SentryOwnerName');
        var n2 = document.getElementById('OwnerID');

        n2.value = n1.value;

        //Set Secure HREmp service URL for associate picker
        //var url = ConfigurationManager.AppSettings["HrApiUrl"] + "api/associates"
        //$.assocSetup({ url: System.Configuration.ConfigurationManager.AppSettings("HrApiUrl") })
        $.assocSetup({ url: "https://hrempsecurequal.sentry.com/api/associates" });

        var picker = $("#OwnerID");

        picker.assocAutocomplete({
            associateSelected: function (associate) {
                $('#SentryOwnerName').val(associate.Id);
            }
        });
    },

    PreviewInit: function () {
        $("[id^='CopyToClipboard']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.CopyToClipboard("PreviewText");
        });
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

    GetUserGuide: function (key) {
        /// <summary>
        /// Send temp URL (containing the dataset, from S3) to a new window
        /// This will initiate the download process
        ///
        var controllerURL = "/Dataset/GetUserGuide/?key=" + encodeURI("user-guides/" + key);
        $.get(controllerURL, function (result) {
            var jrUrl = result;
            window.open(jrUrl, "_blank");
        });
    },

    DownloadDatasetFile: function (id) {
        //alert(id);

        var data = data.Dataset.DatasetFilesTable.row($(id).closest("tr")).data();

        //alert(data.Id);
    },

    PushToSAS_Filename: function (id, filename, delimiter, guessingrows) {
        /// <summary>
        /// Download dataset from S3 and push to SAS file share
        /// </summary>
        var modal = Sentry.ShowModalWithSpinner("PushToMessage");
        var controllerURL = "/Dataset/PushToSAS/?id=" + encodeURI(id) + "&fileOverride=" + encodeURI(filename) + "&delimiter=" + encodeURI(delimiter) + "&guessingrows=" + encodeURI(guessingrows);
        $.post(controllerURL, function (result) {
            modal.ReplaceModalBody(result);
        });
    },

    PushToOverrideInit: function () {
        ///<summary>
        ///Initialize the PushToFilenameOverride partial view
        ///</summary>
        $("PushToForm").validateBootstrap(true);

        $("[id^='FilenameOverride']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PushToSAS_Filename($(this).data("id"), $("#FileNameOverride").val(), $("#Delimiter").val(), $("#GuessingRows").val());
        });
    },

    DisplayCategory: function (category) {
        /// <summary>
        /// Load a dataset Listing for a given category
        /// </summary>
        searchPhrase = data.Dataset.getParameterByName("searchPhrase");
        url = "/Search/Exhibits";
        if (searchPhrase !== null && searchPhrase !== "") {
            url = url + "?searchPhrase=" + encodeURI(searchPhrase);
        }
        if (category !== null && category !== "") {
            if (searchPhrase !== null && searchPhrase !== "") {
                url = url + "&category=" + encodeURI(category);
            } else {
                url = url + "?category=" + encodeURI(category);
            }
        }
        window.location = url;
    },

    DisplayCategory_test: function (type, value) {
        /// <summary>
        /// Load a dataset Listing for a given category
        /// </summary>
        searchPhrase = data.Dataset.getParameterByName("searchPhrase");
        url = "/Search/Datasets";
        if (searchPhrase !== null && searchPhrase !== "") {
            url = url + "?searchPhrase=" + encodeURI(searchPhrase);
        }
        if (value !== null && value !== "") {
            if (searchPhrase !== null && searchPhrase !== "") {
                url = url + "&filtertype=" + encodeURI(type) + "&filtervalue=" + encodeURI(value);
            } else {
                url = url + "?filtertype=" + encodeURI(type) + "&filtervalue=" + encodeURI(value);
            }
        }
        window.location = url;
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
        url = "/BusinessIntelligence/Create";
        window.location = url;
    },

    ViewCreateIndex: function () {
        /// <summary>
        /// Load the Edit Asset view
        /// </summary>
        url = "/Dataset/Create/?datasetId=0";
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
    
    startPushToSAS: function () {
        Sentry.HideAllModals();
        ProgressModalStatus();
    },

    ProgressModalStatus: function () {
        // --- progress bar stuff : start ---
        // Reference the auto-generated proxy for the hub.
        
        var progress = $.connection.progressHub;
        var connectionId

        // Create a function that the hub can call back to display messages.
        progress.client.AddProgress = function (message, percentage) {
            ProgressBarModal("show", message, "Progress: " + percentage);
            $('#ProgressMessage').width(percentage);
            if (percentage === "100%") {
                ProgressBarModal();
            }
        };

        connectionId = $.connection.hub.start()
            .done(function () {
                var connectionId = $.connection.hub.id;
                alert(".done: " + connectionId);
                console.log('Now connected, connection ID = ' + connectionId);
                return connectionId;
            })
            .fail(function () { console.log('Failed to connect') });
        
        alert("ProgressModalStatus: " + connectionId);

        return(connectionId)

        // --- progress bar stuff : end ---

    },

    FileNameModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("File Name Override");
        
        $.get("/Dataset/PushToFileNameOverride/" + id, function (result) {
            modal.ReplaceModalBody(result);
            modal.SetFocus("#FileNameOverride");
        });
    },

    PreviewDataModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Preview Data");

        $.get("/Dataset/PreviewData/" + id, function (result){
            modal.ReplaceModalBody(result);
        })
    },

    GenWeather: function () {
        $.ajax({
            url: "http://api.wunderground.com/api/219e771595b60ca7/geolookup/conditions/q/54481/54481.json",
            dataType: "jsonp",
            success: function (parsed_json) {
                var location = parsed_json['location']['city'];
                var temp_f = parsed_json['current_observation']['temp_f'];
                alert("Current temperature in " + location + " is: " + temp_f);
            }
        });

        $.get("/Dataset/GetWeatherData?zip=54481");
        
    },

    CopyToClipboard: function (containerid) {
        if (document.selection) {
            var range = document.body.createTextRange();
            range.moveToElementText(document.getElementById(containerid));
            range.select().createTextRange();
            document.execCommand("Copy");
            alert("Text Copied to Clipboard");

        } else if (window.getSelection) {
            var range = document.createRange();
            range.selectNode(document.getElementById(containerid));
            window.getSelection().addRange(range);
            document.execCommand("Copy");
            alert("Text Copied to Clipboard");
        }
    },

    setFileTypeInfo: function () {
        switch ($("#FileType").val()) {
            case "0":
                $('#fileTypeInfo').text('"Data Files" contain data pertaining to the dataset.');
                break;
            case "1":
                $('#fileTypeInfo').text('“Supplementary Files” offer additional information about the data files or dataset (i.e. vendor publishes a schema or descriptions in a ReadMe.txt file).  ' +
                    'Supplementary files are limited to viewing only capabilities.');
                break;
        }
    }

};

