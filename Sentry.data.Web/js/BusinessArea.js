data.BusinessArea =
{
    init: function (businessAreaType)
    {
        data.Notification.displayNotifications(businessAreaType);

        //1=DATASET 2=BUSINESSAREA
        data.Subscription.init(2);
    }
};   