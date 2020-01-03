data.BusinessArea =
{
    init: function (businessAreaType)
    {
        data.Notification.displayNotifications(businessAreaType);

        data.Subscription.init("1");
    }
};   