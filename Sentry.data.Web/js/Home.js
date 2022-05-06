/******************************************************************************************
 * Javascript methods for the Home page
 ******************************************************************************************/

data.Home = {
    SentrySkipTotal: 0,
    AllSkipTotal: 10,
    AjaxStatus: true,
    CLA2838_DSC_ANOUNCEMENTS: false,

    Init: function () {
        if (data.Home.AjaxStatus) {
            data.Home.AjaxStatus = false;

            //SHOW DSC ANNOUNCEMENTS BASED ON FEATURE FLAG DELETE THIS when feature is complete
            if (data.Home.CLA2838_DSC_ANOUNCEMENTS == true) {

                //init Subscription which basicaly sends up an event handler and modal when they click subscribe
                data.Subscription.init(3);

                //THIS AJAX CALL LOADS UP FIRST SET OF ITEMS IN FEED, REST ARE LOADED WHEN USER SCROLLS
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

            $.ajax({
                url: '/Home/GetFavorites',
                dataType: 'html',
                success: function (html) {
                    $(".favoriteSpinner").hide();
                    $("#favoritePanel").append(html);
                    //data.Home.SentrySkipTotal += 10;
                    data.Home.AjaxStatus = true;
                },
                error: function (e) {
                    data.Home.AjaxStatus = true;
                }
            });
        }

        $("body").tooltip({ selector: '[data-toggle=tooltip]' });
        $("#feed").bind('scroll', data.Home.ScrollBottom);

        $("#edit-favorites-icon").click(function () {
            window.location = '/Favorites/EditFavorites';
        });

        $(document).on("click", ".favorite-link", function (e) {
            e.preventDefault();
            var link = $(this);
            var type = link.attr("data-type");
            var dsId = link.attr("data-id");
            var eventType = "";
            var reason = "";
            var sendEvent = false;

            if (type.toUpperCase() === "DS") {
                eventType = "Viewed Dataset";
                reason = 'Viewed dataset from favorite link';
                sendEvent = true;
            }
            else if (type.toUpperCase() === "HTTPS") {
                eventType = 'Viewed Report';
                reason = 'Viewed report from favorite link';
                sendEvent = true;
            }

            if (sendEvent) {
                $.ajax({
                    url: '/Event/PublishSuccessEventByDatasetId?eventType=' + encodeURI(eventType) + '&reason=' + encodeURI(reason) + '&datasetId=' + encodeURI(dsId),
                    method: "GET",
                    dataType: 'json',
                    success: function (obj) {
                    }
                });
            }

            if (sendEvent || type.toUpperCase() === "WEB") {
                window.open(link.attr("href"));
            }

            return false;
        });

        $('body').on('DOMNodeInserted', function (e) {
            var target = e.target; //inserted element;

            if ($(target).hasClass('noFeedBox')) {
                $('.noFeedBox').remove();
            }
        });

    },

    //WHEN USER SCROLLS FEED THEN MORE ITEMS ARE LOADED
    ScrollBottom: function (e) {
        var elem = $(e.currentTarget);
        var startLoadHt = elem.outerHeight() + 400;
        var isSentryFeed = false;

        if (data.Home.AllSkipTotal < 100 && data.Home.SentrySkipTotal < 100) {
            if (elem[0].scrollHeight - elem[0].scrollTop <= startLoadHt && data.Home.AjaxStatus) {
                data.Home.AjaxStatus = false;
                var skipThese = data.Home.AllSkipTotal;

                $.ajax({
                    url: '/Home/GetMoreFeed',
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
    }
};