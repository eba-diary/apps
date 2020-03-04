data.BusinessArea =
{
    init: function (businessAreaType)
    {
        //data.Notification.displayNotifications(businessAreaType);

        //1=DATASET 2=BUSINESSAREA
        data.Subscription.init(2);
    },

    initLibertyBell: function ()
    {
        this.initLibertyBellPopover();
        this.initLibertyBellPopoverClick();
    },

    initLibertyBellPopover: function ()
    {
        $.ajax({
            url: "/BusinessArea/GetLibertyBellHtml",
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

                data.BusinessArea.showPopover(obj);
            },
            failure: function ()
            {
                alert('failure');
            },
            error: function (obj)
            {
                alert('error');
            }
        });

    },

    initLibertyBellPopoverClick: function ()
    {
        //associate click event with libertyBell so libertyBell is properly updated with latest notifications
        $("[id^='libertyBell']").click
        (function () {

                $.ajax({
                    url: "/BusinessArea/GetLibertyBellHtml",
                    method: "GET",
                    dataType: 'html',
                    success: function (obj) {
                        $(".liberty-bell").popover
                            (
                                {
                                    container: 'body',
                                    html: 'true',
                                    content: obj,
                                    template: '<div class="popover liberty-popover-medium"><div class="arrow"></div><div class="popover-inner"><h3 class="popover-title"></h3><div class="popover-content"><p></p></div></div></div>'
                                }
                            );
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

    showPopover: function (obj)
    {
        //$.ajax({
        //    url: "/BusinessArea/GetLibertyBellHtml",
        //    method: "GET",
        //    dataType: 'html',
        //    success: function (obj) {
        //        $(".liberty-bell").popover
        //            (
        //                {
        //                    container: 'body',
        //                    html: 'true',
        //                    content: obj,
        //                    template: '<div class="popover liberty-popover-medium"><div class="arrow"></div><div class="popover-inner"><h3 class="popover-title"></h3><div class="popover-content"><p></p></div></div></div>'
        //                }
        //            );

        //        data.BusinessArea.showPopover(obj);
        //    },
        //    failure: function () {
        //        alert('failure');
        //    },
        //    error: function (obj) {
        //        alert('error');
        //    }
        //});



        var objString = obj.toString();
        var index = objString.indexOf("#B3140B");
        if (index != 0)
            $(".liberty-bell").popover('show');

    }
};   