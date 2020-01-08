data.Subscription =
{
    init: function (eventTypeGroup)
    {
        //associate click event with SubscribeModal btn
        $("[id^='SubscribeModal']").click(function (e)
        {
            data.Subscription.SubscribeModal(eventTypeGroup);
        });
    },

    //this function is executed when SubscribeModal BTN is clicked
    SubscribeModal: function (eventTypeGroup)
    {
        var modal = Sentry.ShowModalWithSpinner("Subscribe");
        $.get("/Notification/Subscribe/", function (e)
        {
            modal.ReplaceModalBody(e);
        });

    },

    
};   
