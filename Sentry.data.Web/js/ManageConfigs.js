/******************************************************************************************
 * Javascript methods for the Manage Configs Page
 ******************************************************************************************/

data.ManageConfigs = {

    Init: function () {

        data.ManageConfigs.DatasetFileConfigsTableInit();

    },
    
    EditConfigInit : function () {
            /// <summary>
            /// Initialize the EditConfigFile partial view
            /// </summary>
        $("#EditConfigForm").validateBootstrap(true);

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
                row.child(data.ManageConfigs.formatDatasetFileConfigDetails(row.data())).show();
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
                url: "/Dataset/GetDatasetFileConfigInfoForGrid/?Id=0",
                type: "POST"
            },
            columns: [
                        { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px" },
                        { data: "EditHref", className: "editConfig", width: "20px" },
                        { data: "ParentDatasetName", className: "parentDatasetName", width: "auto"},
                        { data: "ConfigFileName", className: "configFileName" },
                        { data: "SearchCriteria", className: "searchCriteria" },
                        { data: "TargetFileName", className: "targetFileName" },
                        { data: "IsRegexSearch", className: "isRegexSearch", render: function (data, type, row) { return (data == true) ? '<span class="icon-check"> </span>' : '<span class="icon-close"></span>'; } },
                        { data: "OverwriteDatasetFile", type: "date", className: "overwriteDatsetFile", render: function (data, type, row) { return (data == true) ? '<span class="icon-check"> </span>' : '<span class="icon-close"></span>'; } },
                        { data: "VersionsToKeep", type: "date", className: "versionToKeep" },
                        { data: "FileTypeId", type: "date", className: "fileTypeId" }
            ],
            select: {
                style: "os",
                selector: "td:first-child"
            },
            order: [2, 'asc']
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
                table.$('tr.active').removeClass('active');
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
                '<td><b>Drop Path</b>: </td>' +
                '<td>' + d.DropPath + '</td>' +
            '</tr>' +
        '</table>';
    },

    EditConfig: function (id) {
        var modal = Sentry.ShowModalWithSpinner("Edit Config File");
        var editConfigFileUrl = "/Dataset/GetEditConfigPartialView/?configId=" + id;

        $.get(editConfigFileUrl, function (e) {
            modal.ReplaceModalBody(e);
            data.ManageConfigs.EditConfigInit();
        })
    },

    UpdateSuccess: function (data) {
        if (Sentry.WasAjaxSuccessful(data)) {
            Sentry.HideAllModals();

            $('#datasetFileConfigsTable').DataTable().ajax.reload();

            //data.ManageConfigs.ReloadEditConfig();

            var modal = Sentry.ShowModalCustom(
                "Configuration Update",
                "Config File Updated Successfully",
                Sentry.ModalButtonsOK(function (result) { location.reload() })
            );
        }
        //else {

        //}
    },

    UpdateFailure: function () {
        Sentry.ShowModalAlert("Update")
        alert("Update Failed!");
        Sentry.HideAllModals();
    },

    ReloadEditConfig: function () {
        var table = $('#datasetFileConfigsTable').DataTable();
        table.ajax.reload();
    }
}