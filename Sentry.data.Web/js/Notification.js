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

    displayNotifications: function (businessAreaType)
    {
        data.Notification.libertyBellPopoverContent(businessAreaType);
        data.Notification.libertyBellPopoverOnClick(businessAreaType);
        data.Notification.showExpiredNotificationsBtnOnClick(businessAreaType);
        data.Notification.showActiveNotificationsBtnOnClick(businessAreaType);
    },

    //set content for popover and only show if necessary
    libertyBellPopoverContent: function (businessAreaType)
    {
        $.ajax({
            url: "/BusinessArea/GetLibertyBellHtml",
            data: { BusinessAreaType: businessAreaType, activeOnly: true }, 
            method: "GET",
            dataType: 'html',
            success: function (obj)
            {
                $(".liberty-bell").popover
                    (
                        {
                            container: 'body',
                            html: 'true',
                            content: obj,
                            template: '<div class="popover liberty-popover-medium"><div class="arrow"></div><div class="popover-inner"><h3 class="popover-title"></h3><div class="popover-content"><p></p></div></div></div>'
                        }
                    );

                //call to show or not show popover
                data.Notification.showPopover(businessAreaType, obj);      
            },
            failure: function () {
                alert('failure');
            },
            error: function (obj) {
                alert('error jive');
            }
        });
    },

    //associate click event with libertyBellPopover so popover is properly updated with latest notifications
    libertyBellPopoverOnClick: function (businessAreaType)
    {
        $("[id^='libertyBell']").click
        (   function ()
            {
                
                $.ajax({
                    url: "/BusinessArea/GetLibertyBellHtml",
                    data: { BusinessAreaType: businessAreaType, activeOnly: true }, 
                    method: "GET",
                    dataType: 'html',
                    success: function (obj)
                    {
                        $(".liberty-bell").data("bs.popover").options.content = obj;
                    },
                    failure: function () {
                        alert('failure');
                    },
                    error: function (obj) {
                        alert('error');
                    }
                });
                
            }
        );
    },


    //conditionally show popover
    showPopover: function (businessAreaType, obj) {
        $.ajax({
            url: "/Notification/GetNotifications/?businessAreaType=" + businessAreaType,
            method: "GET",
            dataType: 'json',
            success: function (obj) {

                //only show popover intitally if critical notifications exist
                if (obj.CriticalNotifications.length > 0)
                {
                    $(".liberty-bell").popover('show');
                }

            },
            failure: function () {
                alert('failure');
            },
            error: function (obj) {
                alert('error');
            }
        });
    },

    //click event that happens when they click the show expired btn
    showExpiredNotificationsBtnOnClick: function (businessAreaType)
    {
        $("body").on
            ("click", "#showExpiredNotificationBtn",   function () {

            $.ajax({
                url: "/BusinessArea/GetLibertyBellHtml",
                data: { BusinessAreaType: businessAreaType, activeOnly: true },
                method: "GET",
                dataType: 'html',
                success: function (obj)
                {

                    $(".liberty-bell").data("bs.popover").options.content = obj;
                    $(".liberty-bell").popover('show');
                    $(".showing-expired-notifications").removeClass("showing-expired-notifications").addClass("showing-active-notifications");
                },
                failure: function () {
                    alert('failure');
                },
                error: function (obj) {
                    alert('error');
                }
            });
            }
        );


    },

    //click event that happens when they click the show active btn
    showActiveNotificationsBtnOnClick: function (businessAreaType) {
        $("body").on
            ("click", "#showActiveNotificationBtn", function () {

                $.ajax({
                    url: "/BusinessArea/GetLibertyBellHtml",
                    data: { BusinessAreaType: businessAreaType, activeOnly: false },
                    method: "GET",
                    dataType: 'html',
                    success: function (obj) {

                        $(".liberty-bell").data("bs.popover").options.content = obj;
                        $(".liberty-bell").popover('show');
                        $(".showing-active-notifications").removeClass("showing-active-notifications").addClass("showing-expired-notifications");
                    },
                    failure: function () {
                        alert('failure');
                    },
                    error: function (obj) {
                        alert('error');
                    }
                });
            }
            );
    }
};