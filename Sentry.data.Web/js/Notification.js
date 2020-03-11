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
        data.Notification.initToast();
        data.Notification.libertyBellPopoverClickAttack();
        data.Notification.libertyBellSetPopoverContent(businessAreaType);
        data.Notification.libertyBellPopoverOnClick(businessAreaType);
        data.Notification.libertyBellExpiredBtnOnClick(businessAreaType);
        data.Notification.libertyBellActiveBtnOnClick(businessAreaType);
    },

    initToast: function ()
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

    makeToast: function (severity, message)
    {
        toastr[severity](message);
    },

    //this function takes care of popover default behavior oddities to allow popover to close outside of body and to NOT take 2 clicks to open or close
    libertyBellPopoverClickAttack: function ()
    {
        // hide any open popovers when the anywhere else besides popover is clicked
        $('body').on('click', function (e) {
            $('.liberty-bell').each(function () {
                if (!$(this).is(e.target) && $(this).has(e.target).length === 0 && $('.popover').has(e.target).length === 0) {
                    $(this).popover('hide');
                }
            });
        });

        //reset state of popover to click liberty bell once to re-open after close
        $('body').on('hidden.bs.popover', function (e) {
            $(e.target).data("bs.popover").inState = { click: false, hover: false, focus: false };
        });
    },

    //set content for popover and only show if necessary
    libertyBellSetPopoverContent: function (businessAreaType)
    {
        var errorMessage = 'Error getting Notifications on Page Load';

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
                data.Notification.showPopoverFirstTime(businessAreaType, obj);      
            },
            failure: function () {
                makeToast('error', errorMessage);
            },
            error: function () {
                makeToast('error', errorMessage);
            }
        });
    },


    updateBadgeContent: function (businessAreaType) {
        
        var errorMessage = 'Error updateBadgeContent';

        $.ajax({
            url: "/Notification/GetNotifications/?businessAreaType=" + businessAreaType,
            method: "GET",
            dataType: 'json',
            success: function (obj)
            {
                var badgeCount = obj.CriticalNotifications.length + obj.StandardNotifications.length;

                if (badgeCount == 0) {
                    $(".liberty-badge-red").removeClass("liberty-badge-red").addClass("liberty-badge-white");
                }
                else if (badgeCount > 0) {
                    $(".liberty-badge-white").removeClass("liberty-badge-white").addClass("liberty-badge-red");
                    $('.liberty-badge').html(badgeCount);
                }
            },
            failure: function () {
                makeToast('error', errorMessage);
            },
            error: function () {
                makeToast('error', errorMessage);
            }
        });
    },

    //associate click event with libertyBellPopover so popover is properly updated with latest notifications
    libertyBellPopoverOnClick: function (businessAreaType)
    {
        var errorMessage = 'Error refreshing Active Notifications after Bell Click';

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
                        $(".liberty-bell").popover('show');     //i had to include this for some reason to show refreshed popover, the cost is a flicker of reload
                        data.Notification.updateBadgeContent(businessAreaType);
                    },
                    failure: function () {
                        makeToast('error', errorMessage);
                    },
                    error: function () {
                        makeToast('error', errorMessage);
                    }
                });
            }
        );
    },

    //conditionally show popover
    showPopoverFirstTime: function (businessAreaType)
    {
        var errorMessage = 'Error determining if Critical Notifications exist';

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
                makeToast('error', errorMessage);
            },
            error: function () {
                makeToast('error', errorMessage);
            }
        });
    },

    //click event that happens when they click the show expired btn
    libertyBellExpiredBtnOnClick: function (businessAreaType)
    {
        var errorMessage = 'Error getting Active Notifications';

        $("body").on
            ("click", "#showExpiredNotificationBtn",
                function ()
                {
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
                            makeToast('error', errorMessage);
                        },
                        error: function () {
                            makeToast('error', errorMessage);
                        }
                    });
                }
        );
    },

    //click event that happens when they click the show active btn
    libertyBellActiveBtnOnClick: function (businessAreaType)
    {
        var errorMessage = 'Error getting Expired Notifications';

        $("body").on
            ("click", "#showActiveNotificationBtn",
                function ()
                {
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
                            makeToast('error', errorMessage);
                        },
                        error: function () {
                            makeToast('error', errorMessage);
                        }
                    });
                }
            );
    }
};