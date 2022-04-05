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

                //NEED THIS FOR mdb-select multi dropdown bootstrap 4 to show proper multi drop down html
                $('.buford-multi-select').materialSelect();
            });
        }        
    },

    /******************************************************************************************
    * SUBMIT OVERRIDE METHOD
    * Because upon save we have to extract/decode message from Quill editor for safe storage, this js function will override the normal MVC submit
    * we will grab the notification message from QUIL, encode it and use a bogus text area which is actually the model.title and model.message
    * when submit is triggered the ModifyNotification View will pick up the model.title and model.message which are the bogus encoded versions 
    * these bogus DIVS are what are SAVED TO MODEL
    ******************************************************************************************/
    submitChanges: function () {
        //INIT
        //var allVal = $("#buford").val();

        
        //var quillMessageEncoded = $("<div/>").text(message).html();     //encode message to safely pass and store
        //$('.quillMessageEncoded').val(quillMessageEncoded);             //set messageEncoded TextArea so normal MVC submit will use it

        //SUBMIT
        $('.subscribeHeroForm').submit();                          //call submit which triggers SubmitNotification controller method
    }






};   
