/******************************************************************************************
 * Javascript methods for the Manage Asset Alert Page
 ******************************************************************************************/

data.Notification = {

    Init: function () {
        data.Notification.NotificationTableInit();

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
                row.child(data.Notification.formatAssetNotificationTableDetails(row.data())).show();
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
                { data: null, className: "editConfig", width: "20px", render: function (data) { return data.CanEdit ? '<a href=/Notification/ModifyNotification?notificationId=' + data.NotificationId + '\>Edit</a>' : ''; } },
                { data: "IsActive", className: "isActive", render: function (data) { return data === true ? 'Yes' : 'No'; } },
                { data: "ObjectName", className: "parentDataAssetName" },
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
        if (d.MessageSeverity === 1) {
            var htmlString = '<div id="preview-system-notification-wrapper">' +
                '<div class="critical-notification-container">' +
                '<div class="critical-notification-title">' + d.Title + '</div>' +
                '<div class="critical-notification-content-container">' +
                '<div class="critical-notification-date">' + moment(d.StartTime).format("M/D/YYYY") + '</div>' +
                '<div class="critical-notification-body">' + d.Message + d.MessageSeverity + '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
        }
        else {
            var htmlString = '<div id="preview-system-notification-wrapper">' +
                '<div id="standard-notification-carousel-preview" class="carousel slide" data-ride="carousel">' +
                '<div class="carousel-inner">' +
                '<div class="item active">' +
                '<div class="standard-notification-title">' + d.Title + '</div>' +
                '<div class="standard-notification-date">' + moment(d.StartTime).format("M/D/YYYY") + '</div>' +
                '<div class="standard-notification-body">' + d.Message + d.MessageSeverity + '</div>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
        }
        

        var div = document.createElement('div');
        div.innerHTML = htmlString.trim();

        return div.childNodes;

    },

    initNotifications: function ()
    {
        toastr.options = {
            "closeButton": false,
            "debug": false,
            "newestOnTop": false,
            "progressBar": false,
            "positionClass": "toast-top-right",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "0",
            "hideDuration": "0",
            "timeOut": "0",
            "extendedTimeOut": "0",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };
    },

    displayNotifications: function (businessAreaType)
    {
        this.initNotifications();
     
        $.get("/Notification/GetNotifications/?businessAreaType=" + businessAreaType, this.displayNotificationsPersonalLines);
        
    },

    displayNotificationsPersonalLines: function (e)
    {
        for (let i = 0; i < e.CriticalNotifications.length; i++)
        {
            toastr["error"](e.CriticalNotifications[i].Message, e.CriticalNotifications[i].Title);
        }

        for (let i = 0; i < e.StandardNotifications.length; i++)
        {
            if (e.StandardNotifications[i].MessageSeverity === "Warning")
            {
                toastr["warning"](e.StandardNotifications[i].Message, e.StandardNotifications[i].Title);
            }
        }

        for (let i = 0; i < e.StandardNotifications.length; i++)
        {
            if (e.StandardNotifications[i].MessageSeverity === "Info")
            {
                toastr["info"](e.StandardNotifications[i].Message, e.StandardNotifications[i].Title);
            }
        }
    }
};