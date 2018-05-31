/******************************************************************************************
 * Javascript methods for the Manage Asset Alert Page
 ******************************************************************************************/

data.ManageAssetAlert = {

    Init: function () {
        data.ManageAssetAlert.AssetNotificationTableInit();
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
                        { data: "IsActive", className: "isActive", render: function (data, type, row) { return (data == true) ? '<span class="glyphicon glyphicon-ok"> </span>' : '<span class="glyphicon glyphicon-remove"></span>'; } },
                        //{ data: "NotificationId", className: "notificationId" },
                        { data: "ParentDataAssetName", className: "parentDataAssetName" },
                        //{ data: "DisplayCreateUser.FullName", className: "displayCreateUser" },
                        { data: "StartTime", className: "startTime", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                        { data: "ExpirationTime", className: "expirationTime", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                        { data: "MessageSeverityTag", className: "messageSeverityTag" },
                        { data: "Message", className: "message" }
            ],
            order: [[2, 'desc'],[4, 'desc']]
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

        $("#Add_Notification").click(function () {
            data.ManageAssetAlert.CreateNotification();
        });
    },

    formatAssetNotificationTableDetails: function (d) {
        // `d` is the original data object for the row
        return '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">' +
            '<tr>' +
                '<td><b>Notification ID</b>:</td>' +
                '<td>' + d.NotificationId + '</td>' +
            '</tr>' +
            '<tr>' +
                '<td><b>Creator</b>: </td>' +
                '<td>' + d.DisplayCreateUser.FullName + '</td>' +
            '</tr>' +
        '</table>';
    }
}