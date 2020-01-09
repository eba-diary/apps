data.Subscription =
{
    init: function (eventTypeGroup)
    {
        //associate click event with SubscribeModal btn
        $("[id^='SubscribeModal']").click(function()
        {
            data.Subscription.SubscribeModal(eventTypeGroup);
        });
    },

    //this function is executed when SubscribeModal BTN is clicked
    SubscribeModal: function (eventTypeGroup)
    {
        var modal = Sentry.ShowModalWithSpinner("Subscribe");

        //for now only BUSINESSAREA EXISTS
        if (eventTypeGroup === 'BUSINESSAREA')
        {
            $.get("/Notification/Subscribe/", function (e)
            {
                modal.ReplaceModalBody(e);
            });
        }
    }
};   
