data.BusinessArea =
{
    init: function (businessAreaType)
    {
        data.Notification.displayNotifications(businessAreaType);

        //1=DATASET 2=BUSINESSAREA
        data.Subscription.init(2);
    },

    initLibertyBell: function ()
    {

        $(".liberty-bell").popover
        (
            {
                content: "BEWARE THE IDES OF MARCH ssssssssssss sssssssssssssssssssss ssssssssssssssssssssssss ssssssssssssssssss"
            }
        )
        

        //pass initilize hmtl tag

              
    }

};   