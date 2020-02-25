data.BusinessArea =
{
    init: function (businessAreaType)
    {
        data.Notification.displayNotifications(businessAreaType);

        //1=DATASET 2=BUSINESSAREA
        data.Subscription.init(2);
    },

    AlertNotificationHeroInit: function ()
    {

        $(".alert-notification-icon-hero").popover
        (
            {
                content: "BEWARE THE IDES OF MARCH ssssssssssss sssssssssssssssssssss ssssssssssssssssssssssss ssssssssssssssssss"
            }
        )
        

        //pass initilize hmtl tag




        //$(".alert-notification-hero").on("click", function (event)
        //{
        //    console.log("FIRE");
        //    $(".alert-notification-hero").popover('show');

        //    //$(".alert-panel-hero").show();
           
        //});

       
    }

};   