/******************************************************************************************
 * Javascript methods for the Asset-related pages
 ******************************************************************************************/

data.DatasetDetail = {

    DatasetFilesTable: {},

    Init: function () {
        /// <summary>
        /// Initialize the dataset detail page for data assets
        /// </summary>
        $("[id^='EditDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.ViewEdit($(this).data("id"));
        });

        $("[id^='DownloadLatest']").off('click').on('click', function (e) {
            e.preventDefault();
            data.DatasetDetail.DownloadLatestDatasetFile($(this).data("id"));
        });

        $("[id^='PushtoSAS_']").off('click').on('click', function (e) {
            console.log(e);
            e.preventDefault();
            data.Dataset.PushToSAS($(this).data("id"));
        });

        $("[id^='Pushtofilename_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.FileNameModal($(this).data("id"));
        });

        $("[id^='PreviewLatestData']").off('click').on('click', function (e) {
            e.preventDefault();
            data.DatasetDetail.PreviewLatestDatafileModal($(this).data("id"));
        });

        $("[id^='UploadModal']").click(function (e) {
            e.preventDefault();
            //data.DatasetDetail.ProgressModalStatus();
            data.DatasetDetail.UploadFileModal($(this).data("id"));
        });

        $("[id^='detailSectionHeader_']").click(function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var category = "#hide_" + id
            var icon = "#icon_" + id;

            $(category).slideToggle();
            $(icon).toggleClass("glyphicon-chevron-down glyphicon-chevron-up");
        });


        data.DatasetDetail.DatasetFileTableInit(Id);

        data.DatasetDetail.DatasetFileConfigsTableInit(Id);

        localStorage.setItem("listOfFilesToBundle", JSON.stringify([]));

        $("#bundle_selected").click(function (e) {
            e.preventDefault();

            var datasetID = window.location.pathname.substr(window.location.pathname.lastIndexOf('/') + 1);
            var listOfFilesToBundle = JSON.parse(localStorage.getItem("listOfFilesToBundle"));

            for (i = 0; i < listOfFilesToBundle.length; i++) {
               // console.log(listOfFilesToBundle[i]);
            }
            console.log(listOfFilesToBundle);
            data.DatasetDetail.PushToBundler(datasetID, listOfFilesToBundle);
            //Send them to the bundler
        });

        $("#bundle_allFiltered").click(function (e) {
            e.preventDefault();

            var datasetID = window.location.pathname.substr(window.location.pathname.lastIndexOf('/') + 1);

            var params = Sentry.GetDataTableParamsForExport($('#datasetFilesTable').DataTable());

            var request = $.ajax({
                url: "/Dataset/GetDatasetFileInfoForGrid/?Id=" + datasetID + "&bundle=" + true,
                method: "POST",
                data:  params,
                dataType: 'json',
                success: function (obj) {
                    console.log('success');
                    console.log(obj);

                    var listOfFilesToBundle = [];

                    for (i = 0; i < obj.data.length; i++)
                    {
                        listOfFilesToBundle.push(obj.data[i].Id);
                    }
                    console.log(listOfFilesToBundle);
                    //Send them to the bundler
                    data.DatasetDetail.PushToBundler(datasetID, listOfFilesToBundle);
                },
                failure: function (obj) {
                    console.log('failed');
                    console.log(obj);
                },
                error: function (obj)
                {
                    console.log('error');
                    console.log(obj);
                }
            });        
        });
    },

    PushToBundler: function (dataSetID, listOfFilesToBundle) {
        for (i = 0; i < listOfFilesToBundle.length; i++) {
            console.log("Bundle " + i + " : " + listOfFilesToBundle[i]);
        }

        var modal = Sentry.ShowModalWithSpinner("Upload Results", {
            Confirm: {
                label: 'Confirm',
                className: 'btn-success'
            }
        });

        var request = $.ajax({
            url: "/Dataset/BundleFiles/?listOfIds=" + listOfFilesToBundle,
            method: "POST",
            success: function (obj) {
                modal.ReplaceModalBody(obj.Message);
            },
            failure: function (obj) {
                console.log(obj);
            },
            error: function (xhr, e) {
                console.log(e);
                console.log(xhr);
            }
        });        
    },

    VersionsModalInit: function (Id) {
        $('.modal-dialog').css('width','900px');
        //m.addClass('datasetversionmodal');

        $('#datasetFilesVersionsTable tbody').on('click', 'td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = table.row(tr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                row.child(data.DatasetDetail.formatDatasetFileVersionDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

        $("#datasetFilesVersionsTable").DataTable({
            autoWidth: true,
            serverSide: true,
            processing: true,
            searching: false,
            paging: true,
            ajax: {
                url: "/Dataset/GetVersionsOfDatasetFileForGrid/?Id=" + Id,
                type: "POST"
            },
            columns: [
                        { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px" },
                        { data: "ActionLinks", className: "downloadFile", width: "auto" },
                        //{ data: "PreviewHref", className: "previewFile", width: "20px" },
                        //{ data: "Id", width: "40px", type: "num", className: "datasetfileid" },
                        { data: "Name", width: "40%", className: "Name" },
                        { data: "ConfigFileName", className: "configFileName" },
                        { data: "UploadUserName", className: "UploadUserName" },
                        { data: "CreateDTM", className: "createdtm", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null;}},
                        { data: "ModifiedDTM", type: "date", className: "modifieddtm", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null;}}
            ],
            order: [6, 'desc']
            //stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
        });

        $("#datasetFilesVersionsTable").dataTable().columnFilter({
            sPlaceHolder: "head:after",
            aoColumns: [
                    null,
                    null,
                    null,
                    //{ type: "number-range" },
                    { type: "text" },
                    { type: "text" },
                    { type: "text" },
                    { type: "text"}
            ]
        });

        var DataFilesTable = $('#datasetFilesVersionsTable').dataTable();

        // DataTable
        var table = $('#datasetFilesVersionsTable').DataTable();

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

    UploadModalInit: function (id) {
        // Initialize SignalR Hub
        //data.DatasetDetail.ProgressModalStatus();

        $("#categoryList").append($("<option />").val(0).html("-- Select a Category --"));

        $("#categoryList").each(function (i) {
            $(this).val(i);
        })

        if (id == 0 || id == 1) {
            $("[id^='btnUploadFile']").attr('data-id', id);
            $("[id^='btnUploadFile']").prop("disabled", true);

            $("#btnCreateDatasetAtUpload").prop("disabled", true);
            $("#btnCreateDatasetAtUpload").hide();

            $("#DatasetFileUpload").prop("disabled", true);
            $("#DatasetFileUpload").parent().parent().hide();

            $("#datasetList").parent().parent().hide();
        }
        else {

            $("#btnCreateDatasetAtUpload").prop("disabled", true);
            $("#btnCreateDatasetAtUpload").hide();

            $("[id^='btnUploadFile']").attr('data-id', id);
            $("[id^='btnUploadFile']").prop("disabled", true);

        }

        $("[id^='btnUploadFile']").off('click').on('click', function () {

            var modal = Sentry.ShowModalWithSpinner("Upload Results", {
                    Confirm: {
                        label: 'Confirm',
                        className: 'btn-success'
                    }
            });


            // This approach is from the following site:
            // http://www.c-sharpcorner.com/UploadFile/manas1/upload-files-through-jquery-ajax-in-Asp-Net-mvc/
            if (window.FormData !== undefined) {
                var fileUpload = $("#DatasetFileUpload").get(0);
                var files = fileUpload.files;

                //Create FormData object
                var fileData = new FormData();

                fileData.append(files[0].name, files[0]);

                $.ajax({
                    url: '/Dataset/UploadDatafile/?id=' + encodeURI($(this).data('id')),
                    type: "Post",
                    contentType: false, // Not to set any content header
                    processData: false, // Not to process data
                    data: fileData,
                    success: function (result) {
                        modal.ReplaceModalBody(result);
                        //Sentry.ShowModalConfirmation(result);
                    },
                    error: function (err) {
                        modal.ReplaceModalBody(result);
                        //Sentry.ShowModalAlert(err.statusText);
                    }
                });
            } else {
                alert("FormData is not supported");
            }
        });

        // Dropdown selection drives populating secondary dropdown list
        // https://www.codeproject.com/Questions/696829/MVC-Dropdown-onchange-load-another-dropdown
        $("#categoryList").change(function () {
            var cID = $(this).val();

            if (cID > 0) {
                var controllerURL = "/Dataset/LoadDatasetList/?id=" + encodeURI(cID);
                $.get(controllerURL, function (result) {
                    var select = $("#datasetList");
                    select.empty();
                    select.append($('<option/>', {
                        value: 0,
                        text: "Select Dataset"
                    }));

                    select.append($('<option/>', {
                        value: 1,
                        text: "{Create New Dataset}"
                    }));

                    $.each(result, function (index, itemData) {
                        select.append($('<option/>', {
                            value: itemData.Value,
                            text: itemData.Text
                        }));
                    });
                });
            }

            //Simple Field Validation 
            //  Enables the button if both fields are picked.
            //  If Create New Dataset is Picked, a different button will be shown to the user.

            if (cID == 0) {
                $("#datasetList").parent().parent().hide();

                $("#DatasetFileUpload").prop("disabled", true);
                $("#DatasetFileUpload").parent().parent().hide();
            }
            else {
                $("#datasetList").parent().parent().show();

                var fileUpload = $("#DatasetFileUpload").get(0);
                var files = fileUpload.files;

                if (files.length > 0) {

                    if (cID != 0 && cID != 1 && files[0].name != null) {
                        $("[id^='btnUploadFile']").prop("disabled", false);
                    } else if (cID == 1) {
                        $("#btnCreateDatasetAtUpload").prop("disabled", false);
                        $("#btnCreateDatasetAtUpload").show();
                    }
                }
            }
        });

        $("#btnCreateDatasetAtUpload").click(function (e) {
            e.preventDefault();
            url = "/Dataset/Create";
            window.location = url;
        });


        $("#DatasetFileUpload").change(function () {
            var dID = $(this).val();
            $("[id^='btnUploadFile']").attr('data-id', dID);

            var fileUpload = $("#DatasetFileUpload").get(0);
            var files = fileUpload.files;

            if (files.length > 0) {

                if (dID != 0 && dID != 1 && files[0].name != null) {
                    $("[id^='btnUploadFile']").prop("disabled", false);
                }
            }   
        });


        $("#datasetList").change(function () {
            var dID = $(this).val();
            $("[id^='btnUploadFile']").attr('data-id', dID);

            if (dID == 0)
            {
                $("[id^='btnUploadFile']").prop("disabled", true);

                $("#btnCreateDatasetAtUpload").prop("disabled", true);
                $("#btnCreateDatasetAtUpload").hide();

                $("#DatasetFileUpload").prop("disabled", true);
                $("#DatasetFileUpload").parent().parent().hide();
            }
            else if (dID == 1)
            {
                $("#btnCreateDatasetAtUpload").prop("disabled", false);
                $("#btnCreateDatasetAtUpload").show();
                $("[id^='btnUploadFile']").prop("disabled", true);
                $("#DatasetFileUpload").prop("disabled", true);
                $("#DatasetFileUpload").parent().parent().hide();
            }
            else
            {
                $("#DatasetFileUpload").prop("disabled", false);
                $("#DatasetFileUpload").parent().parent().show();
                $("#btnCreateDatasetAtUpload").prop("disabled", true);
                $("#btnCreateDatasetAtUpload").hide();


                var fileUpload = $("#DatasetFileUpload").get(0);
                var files = fileUpload.files;

                if (files.length > 0) {

                    if (dID != 0 && dID != 1 && files[0].name != null) {
                        $("[id^='btnUploadFile']").prop("disabled", false);
                    }
                }
            }
        });

    },

    DownloadDatasetFile: function (id) {
        /// <summary>
        /// Send temp URL (containing the dataset, from S3) to a new window
        /// This will initiate the download process
        /// </summary>

        var controllerURL = "/Dataset/GetDatasetFileDownloadURL/?id=" + encodeURI(id);
        $.get(controllerURL, function (result) {
            var jrUrl = result;
            window.open(jrUrl, "_blank");
        });
    },

    PushToSAS: function (caller, id, filename) {
       // console.log($(caller).parent().next().text());
       // console.log(id);
       // console.log(filename);
        data.Dataset.FileNameModal(filename);
        data.Dataset.PushToSAS_Filename(filename, $(caller).parent().next().text());
    },

    DownloadLatestDatasetFile: function (id) {
        /// <summary>
        /// Send temp URL (containing the dataset, from S3) to a new window
        /// This will initiate the download process
        /// </summary>

        var getLatestURL = "/Dataset/GetLatestDatasetFileIdForDataset/?id=" + encodeURI(id);
        $.get(getLatestURL, function (e) {
            var controllerURL = "/Dataset/GetDatasetFileDownloadURL/?id=" + encodeURI(e);
            $.get(controllerURL, function (result) {
                var jrUrl = result;
                window.open(jrUrl, "_blank");
            });
        });
    },

    PreviewDatafileModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Preview Datafile");

        $.get("/Dataset/PreviewDatafile/" + id, function (result) {
            modal.ReplaceModalBody(result);
        });
    },

    GetDatasetFileVersions: function (id) {
        var modal = Sentry.ShowModalWithSpinner("Versions");   

        $.get("/Dataset/GetDatasetFileVersions/" + id, function (result) {
            modal.ReplaceModalBody(result);
            data.DatasetDetail.VersionsModalInit(id);
        })
    },

    PreviewLatestDatafileModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Preview Datafile");

        $.get("/Dataset/PreviewLatestDatafile/" + id, function (result) {
            modal.ReplaceModalBody(result);
        });
    },


    UploadFileModal: function (id) {
        /// <summary>
        /// Load Modal for Uploading new Datafile
        /// </summary>
        var modal = Sentry.ShowModalWithSpinner("UploadDataFile");
        var createDatafileUrl = "/Dataset/GetDatasetUploadPartialView/?datasetId=" + encodeURI(id);

        $.get(createDatafileUrl, function (e) {
            modal.ReplaceModalBody(e);
            data.DatasetDetail.UploadModalInit(id);
        });       
    },

    ProgressModalStatus: function () {
        // --- progress bar stuff : start ---
        // Reference the auto-generated proxy for the hub.

        var progress = $.connection.progressHub;

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
        });
        // --- progress bar stuff : end ---
    },

    DatasetFileTableInit: function(Id) {
        $('#datasetFilesTable tbody').on('click', 'td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = table.row(tr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                row.child(data.DatasetDetail.formatDatasetFileDetails(row.data())).show();
                tr.addClass('shown');
            }
        });
        
        data.Dataset.DatasetFilesTable = $("#datasetFilesTable").DataTable({
            //$("#datasetFilesTable").dataTable({
            width: "100%",
            serverSide: true,
            //responsive: true,
            processing: true,
            searching: true,
            paging: true,
            rowId: 'Id',
            ajax: {
                url: "/Dataset/GetDatasetFileInfoForGrid/?Id=" + Id + "&bundle=" + false,
                type: "POST"
            },
            iDisplayLength: 10,
            aLengthMenu: [
                [10, 25, 50, 100, 200, -1],
                [10, 25, 50, 100, 200, "All"]
            ],
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px", searchable: false },
                { data: "ActionLinks", className: "downloadFile", width: "100px", searchable: false },
                {
                    data: "Name", width: "40%", className: "Name", render: function (data, type, row)
                    {
                        return "<a href = \"#\" onclick=\"data.DatasetDetail.GetDatasetFileVersions(" + row.Id
                            + ")\" title=\"View File Versions\">" + row.Name
                            + "</a>"
                    }
                },
                { data: "UploadUserName", className: "UploadUserName" },
                { data: "CreateDTM", className: "createdtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ModifiedDTM", type: "date", className: "modifieddtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ConfigFileName", className: "ConfigFileName" }
            ],
            language: {
                search: "<div class='input-group'><span class='input-group-addon'><i class='glyphicon glyphicon-search'></i></span>_INPUT_</div>",
                searchPlaceholder: "Search",
                processing: ""
            },
            order: [5, 'desc'],
            stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
        });

        $("#datasetFilesTable").dataTable().columnFilter({
            sPlaceHolder: "head:after",
            aoColumns: [
                null,
                null,
                { type: "text" },
                { type: "text" },
                { type: "date-range" },
                { type: "date-range" },
                { type: "text" }
            ],
        });

        $("#bundledDatasetFilesTable").DataTable({
            //$("#datasetFilesTable").dataTable({
            autoWidth: true,
            serverSide: true,
            //responsive: true,
            processing: true,
            searching: true,
            paging: true,
            ajax: {
                url: "/Dataset/GetBundledFileInfoForGrid/?Id=" + Id,
                type: "POST"
            },
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px", searchable: false },
                { data: "ActionLinks", className: "downloadFile", width: "auto", searchable: false },
                {
                    data: "Name", width: "40%", className: "Name", render: function (data, type, row) {
                        return "<a href = \"#\" onclick=\"data.DatasetDetail.GetDatasetFileVersions(" + row.Id
                            + ")\" title=\"View File Versions\">" + row.Name
                            + "</a>"
                    }
                },
                { data: "UploadUserName", className: "UploadUserName" },
                { data: "CreateDTM", className: "createdtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ModifiedDTM", type: "date", className: "modifieddtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ConfigFileName", className: "ConfigFileName"}
            ],
            language: {
                search: "<div class='input-group'><span class='input-group-addon'><i class='glyphicon glyphicon-search'></i></span>_INPUT_</div>",
                searchPlaceholder: "Search",
                processing: ""
            },
            order: [5, 'desc'],
            stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
        });

        $("#bundledDatasetFilesTable").dataTable().columnFilter({
            sPlaceHolder: "head:after",
            aoColumns: [
                null,
                null,
                { type: "text" },
                { type: "text" },
                { type: "date-range" },
                { type: "date-range" },
                { type: "text"}
            ],
        });

        $(".dataTables_filter").parent().addClass("text-right");

        $(".dataTables_filter").parent().css("right", "3px");


        //var DataFilesTable = $('#datasetFilesTable').dataTable();

        // DataTable
        var table = $('#datasetFilesTable').DataTable();

        $('#datasetFilesTable tbody').on('click', 'tr', function () {
            if ($(this).hasClass('active')) {
                $(this).removeClass('active');

                var listOfFilesToBundle = JSON.parse(localStorage.getItem("listOfFilesToBundle"));

                if (listOfFilesToBundle != null) {
                    listOfFilesToBundle.splice(listOfFilesToBundle.indexOf(this.id), 1);
                }

                localStorage.setItem("listOfFilesToBundle", JSON.stringify(listOfFilesToBundle));

                $('#bundleCountSelected').html(parseInt($('#bundleCountSelected').html(), 10) - 1);
            }
            else {
                //table.$('tr.active').removeClass('active');
                $(this).addClass('active');

                var listOfFilesToBundle = JSON.parse(localStorage.getItem("listOfFilesToBundle"));

                if (listOfFilesToBundle != null) {
                    listOfFilesToBundle[listOfFilesToBundle.length] = this.id;
                }
                else {
                    var listOfFilesToBundle = [];
                    listOfFilesToBundle[0] = this.id;
                }

                localStorage.setItem("listOfFilesToBundle", JSON.stringify(listOfFilesToBundle));

                $('#bundleCountSelected').html(parseInt($('#bundleCountSelected').html(), 10) + 1);
            }
        });

        $('#datasetFilesTable').on('draw.dt', function () {
            $('#bundleCountFiltered').html(data.Dataset.DatasetFilesTable.page.info().recordsDisplay);
            $('#bundleCountSelected').html(0);
        });


        // Apply the filter
        //table.columns().every(function () {
        //    var column = this;

        //    $('input', this.footer()).on('keyup change', function () {
        //        column
        //            .search(this.value)
        //            .draw();
        //    });
        //});

        //$("#userTable_wrapper .dt-toolbar").html($("#userToolbar"));

        //$("#exportToExcel").click(function () {
        //    alert("exportToExcel Function");
        //});
    },
    
    formatDatasetFileDetails: function (d) {
        // `d` is the original data object for the row
        return '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">' +
            '<tr>' +
                '<td><b>File ID</b>: </td>' +
                '<td>' + d.Id + '</td>' +
            '</tr>' +
            '<tr>' +
                '<td><b>S3 Location</b>:</td>' +
                '<td>' + d.s3Key + '</td>' +
            '</tr>' +
            '<tr>' +
                '<td><b>Version ID</b>: </td>' +
                '<td>' + d.VersionId + '</td>' +
            '</tr>' +
            '<tr>' +
                '<td><b>ConfigFileDesc</b>: </td>' +
                '<td>' + d.ConfigFileDesc + '</td>' +
            '</tr>' +
        '</table>';
    },

    DatasetFileConfigsTableInit: function (Id) {
        $('#datasetFileConfigsTable tbody').on('click', 'td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = table.row(tr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                row.child(data.DatasetDetail.formatDatasetFileConfigDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

        $("#datasetFileConfigsTable").DataTable({
            autoWidth: true,
            serverSide: true,
            processing: true,
            searching: false,
            paging: true,
            ajax: {
                url: "/Dataset/GetDatasetFileConfigInfoForGrid/?Id=" + Id,
                type: "POST"
            },
            columns: [
                        { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px" },
                        { data: "EditHref", className: "editConfig", width: "20px" },
                        { data: "ConfigFileName", className: "configFileName" },
                        { data: "SearchCriteria", className: "searchCriteria"},
                        { data: "TargetFileName", className: "targetFileName" },
                        { data: "IsRegexSearch", className: "isRegexSearch", render: function (data, type, row) { return (data == true) ? '<span class="glyphicon glyphicon-ok"> </span>' : '<span class="glyphicon glyphicon-remove"></span>';} },
                        { data: "OverwriteDatasetFile", type: "date", className: "overwriteDatsetFile", render: function (data, type, row) { return (data == true) ? '<span class="glyphicon glyphicon-ok"> </span>' : '<span class="glyphicon glyphicon-remove"></span>'; } },
                        { data: "VersionsToKeep", type: "date", className: "versionToKeep", },
                        { data: "FileTypeId", type: "date", className: "fileTypeId", }
            ],
            order: [1, 'asc']
            //stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
        });

        //$("#datasetFileConfigsTable").dataTable().columnFilter({
        //    sPlaceHolder: "head:after",
        //    aoColumns: [
        //            null,
        //            null,
        //            null,
        //            { type: "number-range" },
        //            { type: "text" },
        //            { type: "text" },
        //            { type: "text" },
        //            { type: "text" }
        //    ]
        //});

        var DataFilesTable = $('#datasetFileConfigsTable').dataTable();

        // DataTable
        var table = $('#datasetFileConfigsTable').DataTable();


        $('#datasetFileConfigsTable tbody').on('click', 'tr', function () {
            if ($(this).hasClass('active')) {
                $(this).removeClass('active');
            }
            else {
                //table.$('tr.active').removeClass('active');
                $(this).addClass('active');
            }
        });


        //// Apply the filter
        //table.columns().every(function () {
        //    var column = this;

        //    $('input', this.footer()).on('keyup change', function () {
        //        column
        //            .search(this.value)
        //            .draw();
        //    });
        //});

        //$("#userTable_wrapper .dt-toolbar").html($("#userToolbar"));

        //$("#exportToExcel").click(function () {
        //    alert("exportToExcel Function");
        //});
    },

    formatDatasetFileConfigDetails: function (d) {
        // `d` is the original data object for the row
    return '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">' +
        '<tr>' +
            '<td><b>Description</b>:</td>' +
            '<td>' + d.ConfigFileDesc + '</td>' +
        '</tr>' +
        '<tr>' +
            '<td><b>Drop Location Type</b>: </td>' +
            '<td>' + d.DropLocationType + '</td>' +
        '</tr>' +
        '<tr>' +
            '<td><b>Drop Path</b>: </td>' +
            '<td>' + d.DropPath + '</td>' +
        '</tr>' +
    '</table>';
    },

    formatDatasetFileVersionDetails: function (d) {
        // `d` is the original data object for the row
        return '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">' +
            '<tr>' +
                '<td><b>S3 Location</b>:</td>' +
                '<td>' + d.s3Key + '</td>' +
            '</tr>' +
            '<tr>' +
                '<td><b>Version ID</b>: </td>' +
                '<td>' + d.VersionId + '</td>' +
            '</tr>' +
            '<tr>' +
                '<td><b>ConfigFileDesc</b>: </td>' +
                '<td>' + d.ConfigFileDesc + '</td>' +
            '</tr>' +
        '</table>';
    },
}