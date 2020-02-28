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
        ////associate click event with libertyBell
        //$("[id^='libertyBell']").click
        //(   function ()
        //    {
        //        data.BusinessArea.getLibertyBellHtml();
        //    }
        //);

        this.getLibertyBellHtml();
              
    },

    getLibertyBellHtml: function ()
    {
        $.get("/BusinessArea/GetLibertyBellHtml", this.displayLibertyBellPopover);      

    },


    //this function is executed when LibertyBell is clicked
    displayLibertyBellPopover: function (e)
    {
        $(".liberty-bell").popover
        (
            {
                //content: "BEWARE THE IDES OF MARCH ssssssssssss sssssssssssssssssssss ssssssssssssssssssssssss ssssssssssssssssss"
                container: 'body',
                html: 'true',
                content: e
                
            }
        )


        
    }

};   