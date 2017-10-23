/******************************************************************************************
 * Javascript methods for the Manage Asset Alert Page
 ******************************************************************************************/

data.ManageAssetAlert = {

    Init: function () {
        data.ManageAssetAlert.AssetNotificationTableInit();
    },
    EditNotificationInit: function () {
        $("#EditAssetNotificationForm").validateBootstrap(true);
        $("#ExpirationTime").datetimepicker();
        //$("#ExpirationTime").datetimepicker();
        /// <summary>
        /// Initialize the Create Dataset view
        /// </summary>

        //Set Secure HREmp service URL for associate picker
        //$.assocSetup({ url: "https://hrempsecurequal.sentry.com/api/associates" });

        //var picker = $("#ExpireDTM");

        //picker.datetimepicker({ format: 'YYYY-MM-DD HH:mm:ss' });

        //picker.assocAutocomplete({
        //    associateSelected: function (associate) {
        //        $('#SentryOwnerName').val(associate.Id);
        //    },
        //    close: function () {
        //        picker.assocAutocomplete("clear");

        //    }
        //});
    },
    EditNotification: function (id) {
        var modal = Sentry.ShowModalWithSpinner("Edit Alert");
        var editConfigFileUrl = "/DataAsset/GetEditAssetNotificationPartialView/?notificationId=" + id;

        $.get(editConfigFileUrl, function (e) {
            modal.ReplaceModalBody(e);
            data.ManageAssetAlert.EditNotificationInit();
        })
    },
    AssetNotificationTableInit: function () {
        $('#assetnotificationTable tbody').on('click', 'td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = table.row(tr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                row.child(data.ManageAssetAlert.formatAssetNotificationTableDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

        $("#assetnotificationTable").DataTable({
            autoWidth: true,
            serverSide: true,
            processing: true,
            searching: false,
            paging: true,
            ajax: {
                url: "/DataAsset/GetAssetNotificationInfoForGrid/?Id=0",
                type: "POST"
            },
            columns: [
                        { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px" },
                        { data: "EditHref", className: "editConfig", width: "20px" },
                        //{ data: "NotificationId", className: "notificationId" },
                        //{ data: "DataAssetId", className: "dataAssetId" },
                        { data: "DisplayCreateUser.FullName", className: "displayCreateUser" },
                        { data: "StartTime", className: "startTime", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                        { data: "ExpirationTime", className: "expirationTime", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                        { data: "MessageSeverityTag", className: "messageSeverityTag" },
                        { data: "Message", className: "message" }
            ],
            order: [5, 'desc']
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

        var DataFilesTable = $('#assetnotificationTable').dataTable();

        // DataTable
        var table = $('#assetnotificationTable').DataTable();


        $('#assetnotificationTable tbody').on('click', 'tr', function () {
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

    formatAssetNotificationTableDetails: function (d) {
        // `d` is the original data object for the row
        return '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">' +
            '<tr>' +
                '<td><b>Notification ID</b>:</td>' +
                '<td>' + d.NotificationId + '</td>' +
            '</tr>' +
            '<tr>' +
                '<td><b>Data Asset Id</b>: </td>' +
                '<td>' + d.DataAssetId + '</td>' +
            '</tr>' +
        '</table>';
    }
}