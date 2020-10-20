﻿/******************************************************************************************
 * Javascript methods for the Manage Asset Alert Page
 ******************************************************************************************/

data.Notification = {

    Quill: null,                //declare as property for use later

    Init: function () {
        data.Notification.NotificationTableInit();

        $("[id^='RequestAccessButton']").off('click').on('click', function (e) {
            e.preventDefault();
            data.AccessRequest.InitForNotification();
        });

    },

    /******************************************************************************************
    * Init Quill RTF Editor and load with Notification Message
    * ManageNotification View has Quill RTF editor which requires JS to load it and extract it
    ******************************************************************************************/
    initQuill: function () {

        var toolbarOptions = [
            ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
            [{ 'header': 1 }, { 'header': 2 }],               // custom button values
            [{ 'list': 'ordered' }, { 'list': 'bullet' }],
            [{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
            [{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
            [{ 'direction': 'rtl' }],                         // text direction
            [{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
            [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
            [{ 'font': [] }],
            [{ 'align': [] }],
            ['link', 'image'],
            ['clean']                                         // remove formatting button
        ];

        data.Notification.Quill = new Quill('#editor',
        {
            modules: {
                toolbar: toolbarOptions
            },
            theme: 'snow'
        });


        //make ajax call to load notification message into Quill since we need to decode first
        $.ajax({
            url: "/Notification/GetQuillContents/",
            method: "GET",
            dataType: 'json',
            data: { notificationId: $('.notificationId').val() }, 
            success: function (obj) {

                var messageDecoded = $("<div/>").html(obj.data).text();         //decode message since it is stored encoded
                data.Notification.Quill.root.innerHTML = messageDecoded;        //set quill contents
            }
        });
    },

    /******************************************************************************************
    * Because upon save we have to extract/decode message from Quill editor for safe storage, this js function will override the normal MVC subit
    * we will grab the notification message, encode it and use a bogus text area which is actually the model.message
    * when submit is triggered the ModifyNotification View will pick up the bogus encoded version and save that 
    ******************************************************************************************/
    submitChanges: function ()
    {
        var message = data.Notification.Quill.root.innerHTML;           //get html of quill editor
        var messageEncoded = $("<div/>").text(message).html();          //encode message to safely pass and store
        $('.messageEncoded').val(messageEncoded);                       //set messageEncoded TextArea so normal MVC submit will use it

        $('.modifyNotificationForm').submit();                          //call submit which triggers SubmitNotification controller method
    },

    NotificationTableInit: function ()
    {
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
                { data: "Title", className: "title" },
                { data: "CreateUser", className: "displayCreateUser" },
                { data: "StartTime", className: "startTime", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ExpirationTime", className: "expirationTime", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "MessageSeverityDescription", className: "messageSeverityTag" },
                { data: null, className: "recycler", width: "20px", render: function (data) { return data.CanEdit ? '<a href=/Notification/ExpireNotification?notificationId=' + data.NotificationId + '\>Expire</a>' : ''; } }
            ],
            order: [[3, 'desc'], [6, 'desc']]
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

    formatAssetNotificationTableDetails: function (d)
    {
        var div = document.createElement('div');
        var messageDecoded = $("<div/>").html(d.Message).text();         //decode message since it is stored encoded
        div.innerHTML = messageDecoded.trim();

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