/******************************************************************************************
 * Javascript methods for the Home page
 ******************************************************************************************/

data.Home = {
    SentrySkipTotal: 0,
    AllSkipTotal: 10,
    AjaxStatus: true,

    Init: function () {
        if (data.Home.AjaxStatus)
        {
            data.Home.AjaxStatus = false;

            $.ajax({
                url: '/Home/GetFeed',
                dataType: 'html',
                success: function (html) {
                    $(".feedSpinner").hide();
                    $("#feed").append(html);
                    data.Home.SentrySkipTotal += 10;
                    data.Home.AjaxStatus = true;
                },
                error: function (e) {
                    data.Home.AjaxStatus = true;
                }
            });
        }

        $("body").tooltip({ selector: '[data-toggle=tooltip]' });
        $("#feed").bind('scroll', data.Home.ScrollBottom);
        //$("#sentryFeed").bind('scroll', data.Home.ScrollBottom);
        //$("#chbx").change(data.Home.ChangeFeeds);
    },

    ScrollBottom: function (e) {
        var elem = $(e.currentTarget);
        var startLoadHt = elem.outerHeight() + 400;
        //var isSentryFeed = $("#chbx").is(':checked');
        var isSentryFeed = false;

        if(data.Home.AllSkipTotal < 100 && data.Home.SentrySkipTotal < 100)
        {
            if (elem[0].scrollHeight - elem[0].scrollTop <= startLoadHt && data.Home.AjaxStatus)
            {
                data.Home.AjaxStatus = false;

                if (isSentryFeed)
                {
                    var skipThese = data.Home.SentrySkipTotal;
                
                    $.ajax({
                        url: '/Home/GetMoreSentryFeeds',
                        dataType: 'html',
                        data: { skip: skipThese },
                        success: function (html) {
                            $("#sentryFeed").append(html);
                            data.Home.SentrySkipTotal += 5;
                            data.Home.AjaxStatus = true;
                        },
                        error: function (e) {
                            data.Home.AjaxStatus = true;
                        }
                    });
                }
                else
                {
                    var skipThese = data.Home.AllSkipTotal;

                    $.ajax({
                        url: '/Home/GetMoreFeeds',
                        dataType: 'html',
                        data: { skip: skipThese },
                        success: function (html) {
                            $("#feed").append(html);
                            data.Home.AllSkipTotal += 5;
                            data.Home.AjaxStatus = true;
                        },
                        error: function (e) {
                            data.Home.AjaxStatus = true;
                        }
                    });
                }
            }
        }

    },

    ChangeFeeds: function () {
        $("#feed").toggleClass("hidden");
        $("#sentryFeed").toggleClass("hidden");
    }
}

