data.Subscription =
{
    init: function (eventTypeGroup)
    {
        //associate click event with SubscribeModal btn
        $("[id^='SubscribeModal']").click(function (e)
        {
            //data.Subscription.SubscribeModal($(this).data("eventTypeGroup"));
            data.Subscription.SubscribeModal(eventTypeGroup);
        });
    },

    //this function is executed when SubscribeModal BTN is clicked
    SubscribeModal: function (eventTypeGroup)
    {
        alert("1");

        

        //works
        
        //$.get("/Dataset/Subscribe/?id=195", function (e) {
        //    modal.ReplaceModalBody(e);
        //});

        //NOT WORK
        //var modal = Sentry.ShowModalWithSpinner("Subscribe");
        //$.get("/Notification/Subscribe/?eventTypeGroup=" + eventTypeGroup, this.displaySubscriptionModal);                //doesnt work
        //$.get("/Notification/GetNotifications/?businessAreaType=1", this.displaySubscriptionModal);                     //works

        
        $.get("/Dataset/Subscribe/?id=195", this.displaySubscriptionModal);                     

    },

    displaySubscriptionModal: function (e)
    {
       // alert("2");
        var modal = Sentry.ShowModalWithSpinner("Subscribe");
        modal.ReplaceModalBody(e);
    
    }
};   
