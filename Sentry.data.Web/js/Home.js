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

    },

    ChangeFeeds: function () {
        $("#feed").toggleClass("hidden");
        $("#sentryFeed").toggleClass("hidden");
    },

    Refresh: function() {
        /// <summary>
        /// This will run for a periodic refresh
        /// </summary>
        //$.get('/Home/AssetOverview', function (html) {
        //    $("#overview-list").html(html);
        //});
    }

}

/// <summary>
/// This sets up the interval that invokes the data.Home.Refresh()
/// </summary>
$(document).ready(function (html) {
    window.setInterval(data.Home.Refresh, 300000); // refresh every five minutes
})


//FROM INIT FUNCTION
//###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
/// <summary>
/// Initialization function run at first load
/// </summary>
//Sentry.InjectSpinner($("#trending-carousel"), 340);
//Sentry.InjectSpinner($("#overview-list"), 300);
//Sentry.InjectSpinner($("#datafeed-list"), 500);

//$.get('/Home/AssetOverview', function (html) {
//    $("#overview-list").html(html);
//});
//$.get('/Home/HotTopicsFeed', function (html) {
//    $("#hot-topics-list").html(html);
//});
//$.get('/Home/NewsFeed', function (html) {
//    $("#news-feed-list").html(html);
//});

//###  END Sentry.Data  ### - Code above is Sentry.Data-specific
