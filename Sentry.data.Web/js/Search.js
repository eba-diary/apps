/******************************************************************************************
 * Javascript methods for the Search-related pages
 ******************************************************************************************/

data.Search = {

    Init: function () {
        /// <summary>
        /// Initialization function run at first load
        /// </summary>

        window.vm = new ListViewModel();
        ko.applyBindings(vm);

        if (window.location.href.match(/&ids=/)) {
            // do nothing
        }
        else {
            window.vm.clearFilters();
        }

        $(document).on("click", "[id^='filterType_']", function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var category = "#hide_" + id
            var icon = "#icon_" + id;

            $(category).slideToggle();
            $(icon).toggleClass("glyphicon-chevron-down glyphicon-chevron-up");
        });

        $(document).on("click", "[id^='filterMore_']", function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var show = "#hidden_" + id
            var icon = "#icon_" + id;
            var txt = "#txt_" + id;

            $(show).slideToggle();
            $(icon).toggleClass("glyphicon-plus-sign glyphicon-minus-sign");

            if ($(txt).text() == "Show Less") {
                $(txt).text("Show More");
            }
            else {
                $(txt).text("Show Less");
            }
        });

        $(document).on("click", ".btnFavorite", function (e) {
            e.preventDefault();

            var button = $(this);
            var id = button.attr("data");

            if (button.hasClass("glyphicon-star")) {

                $.ajax({
                    url: '/Favorites/SetFavorite?datasetId=' + encodeURIComponent(id),
                    method: "GET",
                    dataType: 'json',
                    success: function () {
                    },
                    error: function () {
                        Sentry.ShowModalAlert("Failed to remove favorite.");
                    }
                });
            }
            else if (button.hasClass("glyphicon-star-empty")) {

                $.ajax({
                    url: '/Favorites/SetFavorite?datasetId=' + encodeURIComponent(id),
                    method: "GET",
                    dataType: 'json',
                    success: function () {
                    },
                    error: function () {
                        Sentry.ShowModalAlert("Failed to set favorite.");
                    }
                });
            }

            $(this).toggleClass("glyphicon-star glyphicon-star-empty");

        });
        

        // remove the "container" class from the parent object so the fluid container takes affect
        $("#search-pg-wrapper").parent().removeClass("container");

        // mouseover effect to change the background color of the search results tile
        $(".ul-dataset-list-item").on("mouseenter", function () {
            $(this).css('background-color', $(this).css("border-color").slice(0, -1) + ', 0.1)');
        });

        // mouseout effect to change the background color of the search results tile back to white
        $(".ul-dataset-list-item").on("mouseleave", function () {
            $(this).css('background-color', 'white');
        });

    },

    GetDetailType: function () {
        // get the detail type from the URL
        return window.location.pathname.split("/")[2];
    },

    GetSearchType: function () {
        var regex = /\/Search\/(\w+)/,
            url = window.location.href;
        return regex.exec(url)[1];
    }
    
};