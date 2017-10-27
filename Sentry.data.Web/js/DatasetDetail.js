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

        data.DatasetDetail.DatasetFileConfigsTableInit(Id)

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

        if (id != 0) {
            $("[id^='UploadFile']").attr('data-id', id);
        }
        

        $("[id^='UploadFile']").off('click').on('click', function () {

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
            var controllerURL = "/Dataset/LoadDatasetList/?id=" + encodeURI(cID);
            $.get(controllerURL, function (result) {
                var select = $("#datasetList");
                select.empty();
                select.append($('<option/>', {
                    value: 0,
                    text: "Select Dataset"
                }));
                $.each(result, function (index, itemData) {
                    select.append($('<option/>', {
                        value: itemData.Value,
                        text: itemData.Text
                    }));
                });
            });
        });


        $("#datasetList").change(function () {
            var dID = $(this).val();
            $("[id^='UploadFile']").attr('data-id', dID);
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
        
        //data.Dataset.DatasetFilesTable = $("#datasetFilesTable").DataTable({
        $("#datasetFilesTable").DataTable({
            autoWidth: true,
            serverSide: true,
            processing: true,
            searching: false,
            paging: true,
            ajax: {
                url: "/Dataset/GetDatasetFileInfoForGrid/?Id=" + Id,
                type: "POST"
            },
            columns: [
                        { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px" },
                        { data: "ActionLinks", className: "downloadFile", width: "auto" },
                        { data: "NameHref", width: "40%", className: "Name" },
                        { data: "UploadUserName", className: "UploadUserName" },
                        { data: "CreateDTM", className: "createdtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null;}},
                        { data: "ModifiedDTM", type: "date", className: "modifieddtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } }
            ],
            order: [5, 'desc']
            //stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
        });

        $("#datasetFilesTable").dataTable().columnFilter({
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

        var DataFilesTable = $('#datasetFilesTable').dataTable();

        // DataTable
        var table = $('#datasetFilesTable').DataTable();


        $('#datasetFilesTable tbody').on('click', 'tr', function () {
            if ($(this).hasClass('active')) {
                $(this).removeClass('active');
            }
            else {
                //table.$('tr.active').removeClass('active');
                $(this).addClass('active');
            }
        });


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
    
    formatDatasetFileDetails: function (d) {
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