﻿/******************************************************************************************
 * Javascript methods for the Home page
 ******************************************************************************************/

data.Home = {
    SentrySkipTotal: 0,
    AllSkipTotal: 10,
    AjaxStatus: true,

    Init: function () {
        if (data.Home.AjaxStatus) {
            data.Home.AjaxStatus = false;

            //SHOW DSC ANNOUNCEMENTS BASED ON FEATURE FLAG
            //DELETE THIS OUTER AJAX Statement /Home/CLA2838_DSC_ANOUNCEMENTS when feature is complete
            //ONLY SAVE what is noted below
            //I didn't want to have the feed controller do any week until feature is fully on
            $.ajax({
                url: '/Home/CLA2838_DSC_ANOUNCEMENTS',
                method: "GET",
                dataType: 'json',
                success: function (obj) {

                    if (obj.CLA2838_DSC_ANOUNCEMENTS == true) {

                        //SAVE START
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
                        //SAVE END
                    }
                }
            });

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

            if (type.toUpperCase()  === "DS") {
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

                window.open(link.attr("href"));
            }

            return false;
        });

        ////Does not work for less then 100% magnifcation. DO NOT ZOOM OUT.  div.scrollTop is not happy with floating point pixels.
        //var div = $('div.feedBox');
        //var scroller = setInterval(function () {
        //    var pos = div.scrollTop();
        //    pos = Number((pos).toFixed(0));
        //    div.scrollTop(++pos);
        //}, 50);


        $('body').on('DOMNodeInserted', function (e) {
            var target = e.target; //inserted element;

            if ($(target).hasClass('noFeedBox')) {
                $('.noFeedBox').remove();
            }
        });

        $(document).on("mouseenter", ".feedItem", function () {
            // hover starts code here
            clearInterval(scroller);
        });

        $(document).on("mouseleave", ".feedItem", function () {
            // hover ends code here
            scroller = setInterval(function () {
                var pos = div.scrollTop();
                pos = Number((pos).toFixed(0));
                div.scrollTop(++pos);
            }, 50);
        });

    },

    ScrollBottom: function (e) {
        var elem = $(e.currentTarget);
        var startLoadHt = elem.outerHeight() + 400;
        //var isSentryFeed = $("#chbx").is(':checked');
        var isSentryFeed = false;

        if (data.Home.AllSkipTotal < 100 && data.Home.SentrySkipTotal < 100) {
            if (elem[0].scrollHeight - elem[0].scrollTop <= startLoadHt && data.Home.AjaxStatus) {
                data.Home.AjaxStatus = false;

                if (isSentryFeed) {
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
                else {
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
};