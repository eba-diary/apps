/******************************************************************************************
 * Javascript methods for the Manage Asset Alert Page
 ******************************************************************************************/

data.ManageNotification = {

    Init: function () {
        data.ManageNotification.NotificationTableInit();

        $("[id^='RequestAccessButton']").off('click').on('click', function (e) {
            e.preventDefault();
            data.AccessRequest.InitForNotification();
        });
    },


    NotificationTableInit: function () {
        $('#notificationTable tbody').on('click', 'td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = table.row(tr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                //ajax out to server to render the partial view and throw the html into the child row.
                row.child(data.ManageNotification.formatAssetNotificationTableDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

        $("#notificationTable").DataTable({
            autoWidth: true,
            serverSide: true,
            processing: false,
            searching: false,
            paging: true,
            ajax: {
                url: "/Notification/GetNotificationInfoForGrid/",
                type: "POST"
            },
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px" },
                { data: null, className: "editConfig", width: "20px", render: function (data) { return '<a href=\"/Notification/ModifyNotification?notificationId=' + data.NotificationId + '\">Edit</a>'; } },
                { data: "IsActive", className: "isActive", render: function (data) { return data === true ? 'Yes' : 'No'; } },
                { data: "DataAssetName", className: "parentDataAssetName" },
                { data: "CreateUser", className: "displayCreateUser" },
                { data: "StartTime", className: "startTime", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ExpirationTime", className: "expirationTime", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "MessageSeverityDescription", className: "messageSeverityTag" }
            ],
            order: [[2, 'desc'], [5, 'desc']]
        });


        // DataTable
        var table = $('#notificationTable').DataTable();

        $('#notificationTable tbody').on('click', 'tr', function () {
            if ($(this).hasClass('active')) {
                $(this).removeClass('active');
            }
            else {
                table.$('tr.active').removeClass('active');
                $(this).addClass('active');
            }
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
            '<td>' + d.CreateUser + '</td>' +
            '</tr>' +
            '</table>';
    }
};