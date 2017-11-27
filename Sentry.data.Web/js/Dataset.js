/******************************************************************************************
 * Javascript methods for the Asset-related pages
 ******************************************************************************************/

data.Dataset = {

    DatasetFilesTable: {},

    IndexInit: function () {
        /// <summary>
        /// Initialize the Index page for data assets (with the categories)
        /// </summary>
        $("[id^='category']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.DisplayCategory($(this).data("category"));
        });

        $("[id^='CreateDataset']").click(function (e) {
            e.preventDefault();
            data.Dataset.ViewUpload();
        });

        $("[id^='UploadDatafile']").click(function (e) {
            e.preventDefault();
            //data.Dataset.ViewCreateIndex();
            //data.DatasetDetail.ProgressModalStatus();
            data.DatasetDetail.UploadFileModal(0);
        });
    },

    //CreateInit: function () {
    //    $("#categoryList").change(function () {
    //        var cID = $(this).val();
    //        var controllerURL = "/Dataset/LoadDatasetList/?id=" + encodeURI(cID);
    //        $.get(controllerURL, function (result) {
    //            var select = $("#datasetList");
    //            select.empty();
    //            select.append($('<option/>', {
    //                value: 0,
    //                text: "Select Dataset"
    //            }));
    //            $.each(result, function (index, itemData) {
    //                select.append($('<option/>', {
    //                    value: itemData.Value,
    //                    text: itemData.Text
    //                }));
    //            });
    //        });
    //    })
    //},

    ListInit: function () {
        /// <summary>
        /// Initialize the List results page for data assets
        /// </summary>

        //$("[id^='datasetViewDetails_']").off('click').on('click', function (e) {
        //    e.preventDefault();
        //    data.Dataset.ViewDetails($(this).data("id"));
        //});

        console.log(window.location.href);

        $("[id^='DownloadDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.DownloadDataset($(this).data("id"));
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
            $(this).children('span.filter-disabled-glyph').css({ 'display': 'inline-block' });
        }, function (e) {
            $(this).children('span.filter-disabled-glyph').css({ 'display': 'none' });
        });

        //$("[id^='li-filter-disabled_']").click(function (e) {
        //    e.preventDefault();
        //    data.Dataset.DisplayCategory($(this).data("cat"));
        //});

        //$("[id^='li-filter-enabled_']").click(function (e) {
        //    e.preventDefault();
        //    data.Dataset.DisplayCategory(null);
        //});

        $("[id^='Pushtofilename_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.FileNameModal($(this).data("id"));
        });

        //$(document).on('show.bs.modal', '.modal', function (event) {
        //    var zIndex = 1040 + (10 * $('.modal:visible').length);
        //    $(this).css('z-index', zIndex);
        //    setTimeout(function () {
        //        $('.modal-backdrop').not('.modal-stack').css('z-index', zIndex - 1).addClass('modal-stack');
        //    }, 0);
        //});

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


        //$(function () {
        //    var $btn = $("[id^=btnApply_]");
        //    var $chk = $("[id^='frequency_']");
        //    // check on page load
        //    checkChecked($chk);
        //    $chk.click(function () {
        //        checkChecked($chk);
        //    });
        //    function checkChecked(chkBox) {
        //        if ($chk.is(':checked')) {
        //            $btn.removeAttr('hidden');
        //        }
        //        else {
        //            $btn.attr('hidden', 'hidden')
        //        }
        //    }
        //});

        var stickySidebar = $('.outsideFilters').offset().top;
        var el = $('.menu');
        var bottom = el.position().top + el.outerHeight(true) + 10;

        //$(window).scroll(function () {
        //    console.log(bottom + " : " + (stickySidebar - $(window).scrollTop()));

        //    if (bottom >= (stickySidebar - $(window).scrollTop())) {
        //        $('.outsideFilters').addClass('fixedScroll');
        //    }
        //    else {
        //        $('.outsideFilters').removeClass('fixedScroll');
        //    }
        //});

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

            if ($(txt).text() == "Show Less") {
                $(txt).text("Show More");
            }
            else {
                $(txt).text("Show Less");
            }
        });

        //var progress = $.connection.progressHub;

        //// Create a function that the hub can call back to display messages.
        //progress.client.AddProgress = function (message, percentage) {
        //    ProgressBarModal("show", message, "Progress: " + percentage);
        //    $('#ProgressMessage').width(percentage);
        //    if (percentage == "100%") {
        //        ProgressBarModal();
        //    }
        //};

        //$.connection.hub.start()
        //    .done(function () {
        //        var connectionId = $.connection.hub.id;
        //        console.log('Now connected, connection ID = ' + connectionId)
        //    })
        //    .fail(function () { console.log('Failed to connect') });

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


        //$(document).on('show.bs.modal', '.modal', function (event) {
        //    var zIndex = 1040 + (10 * $('.modal:visible').length);
        //    $(this).css('z-index', zIndex);
        //    setTimeout(function () {
        //        $('.modal-backdrop').not('.modal-stack').css('z-index', zIndex - 1).addClass('modal-stack');
        //    }, 0);
        //});

        //data.Dataset.DatasetFilesTable = $("#datasetFilesTable").DataTable({
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

        //$("#datasetFilesTable").dataTable().columns('.filter-row').every(function () {
        //    var that = this;

        //    // Create the select list and search operation
        //    var select = $('<select />')
        //        .appendTo(
        //            this.footer()
        //        )
        //        .on('change', function () {
        //            that
        //                .search($(this).val())
        //                .draw();
        //        });

        //    // Get the search data for the first column and add to the select list
        //    this
        //        .cache('search')
        //        .sort()
        //        .unique()
        //        .each(function (d) {
        //            select.append($('<option value="' + d + '">' + d + '</option>'));
        //        });
        //});

        //// Setup - add a text input to each footer cell
        //$('#datasetFilesTable tfoot th').each(function () {
        //    alert("This Function");
        //    var title = $('#datasetFilesTable thead th').eq($(this).index()).text();
        //    $(this).html('<input type="text" placeholder="Search ' + title + '" />');
        //});

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

    showConfirmDeleteModal: function (cell) {
        alert("showConfirmDeleteModal");
        alert(cell);
        //var data = data.Dataset.DatasetFilesTable.row($(cell).closest("tr")).data();

        //alert(data.Id);
        //alert(data.Name);
    },

    CreateInit: function () {
        /// <summary>
        /// Initialize the Create Dataset view
        /// </summary>

        //Set Secure HREmp service URL for associate picker
        $.assocSetup({ url: "https://hrempsecurequal.sentry.com/api/associates" });

        var picker = $("#OwnerID");

        picker.assocAutocomplete({
            associateSelected: function (associate) {
                $('#SentryOwnerName').val(associate.Id);
            }
        });
    },

    EditInit: function () {
        /// <summary>
        /// Initialize the Edit Dataset view
        /// </summary>
        //var x = 3;
        //$("#CategoryIDs").select2({
        //    placeholder: "Click here to choose one or more categories"
        //});

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

        //$("#OwnerID").val($("SentryOwnerName").val);
    },

    PreviewInit: function () {
        $("[id^='CopyToClipboard']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.CopyToClipboard("PreviewText");
        });
    },

    //ViewDetails: function (id) {
    //    /// <summary>
    //    /// Load the Dataset Details page
    //    /// </summary>
    //    var url = "/Dataset/Detail/?id=" + encodeURI(id);
    //    window.location = url;
    //},

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

    DownloadDatasetFile: function (id) {
        alert(id);

        var data = data.Dataset.DatasetFilesTable.row($(id).closest("tr")).data();

        alert(data.Id);
        /// <summary>
        /// Send temp URL (containing the dataset, from S3) to a new window
        /// This will initiate the download process
        /// </summary>

        //var controllerURL = "/Dataset/GetDatasetFileDownloadURL/?id=" + encodeURI(id);
        //$.get(controllerURL, function (result) {
        //    var jrUrl = result;
        //    window.open(jrUrl, "_blank");
        //});
    },

    PushToSAS_Filename: function (id, filename) {
        /// <summary>
        /// Download dataset from S3 and push to SAS file share
        /// </summary>
        var modal = Sentry.ShowModalWithSpinner("PushToMessage");
        var controllerURL = "/Dataset/PushToSAS/?id=" + encodeURI(id) + "&fileOverride=" + encodeURI(filename);
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
            data.Dataset.PushToSAS_Filename($(this).data("id"), $("#FileNameOverride").val());
        });
    },

    DisplayCategory: function (category) {
        /// <summary>
        /// Load a dataset Listing for a given category
        /// </summary>
        searchPhrase = data.Dataset.getParameterByName("searchPhrase");
        url = "/Dataset/List/Index";
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

    DisplayCategory_test: function (type, value) {
        /// <summary>
        /// Load a dataset Listing for a given category
        /// </summary>
        searchPhrase = data.Dataset.getParameterByName("searchPhrase");
        url = "/Dataset/List";
        if (searchPhrase != null && searchPhrase != "") {
            url = url + "?searchPhrase=" + encodeURI(searchPhrase);
        }
        if (value != null && value != "") {
            if (searchPhrase != null && searchPhrase != "") {
                url = url + "&filtertype=" + encodeURI(type) + "&filtervalue=" + encodeURI(value);
            } else {
                url = url + "?filtertype=" + encodeURI(type) + "&filtervalue=" + encodeURI(value);
            }
        }
        window.location = url;
    },

    //AjaxSuccess: function (data) {
    //    /// <summary>
    //    /// Event handlers for a successful AJAX post from the Add or
    //    /// Edit category modal dialogs
    //    /// </summary>
    //    /// <param name="data">Response from the Ajax post</param>
    //    /// <param name="parentCategory">The parent category ID, that we now need to reload</param>

    //    alert("AjaxSuccess Function");
    //    if (Sentry.WasAjaxSuccessful(data)) {
    //        alert(JSON.stringify(data));
    //        Sentry.HideAllModals();
    //    //Sentry.ShowModalAlert("File has been pushed to SAS Successfully.");
    //    //var controllerURL = "/Dataset/PushToSAS/?id=" + encodeURI(datasetID) + "&Override=" + encodeURI(filenameOverride);
    //    //data.Dataset.ProgressModalStatus();
    //    //$.post(controllerURL);
    //    //dataMySentryBayTestApp.Category.ReloadCategory(parentCategory);
    //    }
    //    else
    //    {
    //        var modal = Sentry.ShowModalAlert(data);
    //        alert(JSON.stringify(data));
    //    }
    //},

    //AjaxFailure: function () {
    //    /// <summary>
    //    /// Called when there was a non-200 response
    //    /// </summary>

    //    alert("AjaxFailure function");

    //    Sentry.ShowModalAlert("Error Processing your request, please try again!");
    //},

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
        url = "/Dataset/Create";
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
            if (percentage == "100%") {
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
        //(function ($) {
            $.ajax({
                url: "http://api.wunderground.com/api/219e771595b60ca7/geolookup/conditions/q/54481/54481.json",
                dataType: "jsonp",
                success: function (parsed_json) {
                    var location = parsed_json['location']['city'];
                    var temp_f = parsed_json['current_observation']['temp_f'];
                    alert("Current temperature in " + location + " is: " + temp_f);
                }
            });

            //$.get("/Dataset/GetWeather?" + "zip=" + encodeURI(54481));
            $.get("/Dataset/GetWeatherData?zip=54481");
        
        //});

        //$.get("/Dataset/GetWeather", function (result) {
        //    var jrUrl = result;
        //    window.open(jrUrl, "_blank");
        //});
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
    }

};

