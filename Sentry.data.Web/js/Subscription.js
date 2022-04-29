data.Subscription =
{
    init: function (group)
    {
        //associate click event with SubscribeModal btn
        $("[id^='SubscribeModal']").click(function()
        {
            data.Subscription.SubscribeModal(group);
        });

        $("[id^='subscribe-Feed-icon']").click(function () {
            data.Subscription.SubscribeModal(group);
        });
    },

    //this function is executed when SubscribeModal BTN is clicked
    SubscribeModal: function (group)
    {
        var modal = Sentry.ShowModalWithSpinner("Subscribe");

        //for now only BUSINESSAREA EXISTS
        if (group === 2 || group === 3)
        {
            $.get("/Notification/SubscribeDisplay/?group=" + group, function (e)
            {
                modal.ReplaceModalBody(e);
                $("select").materialSelect();
            });
        }        
    }
};   
