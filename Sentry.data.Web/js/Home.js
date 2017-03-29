/******************************************************************************************
 * Javascript methods for the Home page
 ******************************************************************************************/

data.Home = {

    Init: function () {
        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        /// <summary>
        /// Initialization function run at first load
        /// </summary>
        //Sentry.InjectSpinner($("#trending-carousel"), 340);
        //Sentry.InjectSpinner($("#overview-list"), 300);
        //Sentry.InjectSpinner($("#datafeed-list"), 500);

        $.get('/Home/AssetOverview', function (html) {
            $("#overview-list").html(html);
        });
        $.get('/Home/HotTopicsFeed', function (html) {
            $("#hot-topics-list").html(html);
        });
        $.get('/Home/NewsFeed', function (html) {
            $("#news-feed-list").html(html);
        });

        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    },

    Refresh: function() {
        /// <summary>
        /// This will run for a periodic refresh
        /// </summary>
        $.get('/Home/AssetOverview', function (html) {
            $("#overview-list").html(html);
        });
    }

}

/// <summary>
/// This sets up the interval that invokes the data.Home.Refresh()
/// </summary>
$(document).ready(function (html) {
    window.setInterval(data.Home.Refresh, 300000); // refresh every five minutes
})
