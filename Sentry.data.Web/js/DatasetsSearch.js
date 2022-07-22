data.DatasetsSearch = {
    init: function () {
        this.initUI();
        this.initEvents();
    },

    executeSearch: function () {
        data.FilterSearch.completeSearch(0, 0, 0);
        //use jquery load function
        //in callback, get the counts for result information from hidden elements
    },

    buildFilter: function () {

        //one option is to have the separate controller method to get the categories and run the group queries on the datasets to build the filters model
        //another option would be to on executeSearch, make ajax request to get the list of datasets that result from the query
        //then send that result back to another controller method that will load the partial view AND send the results to a controller method that will translate the results to the filters model
        //
        //Option 2 is probably more performant than running query twice
        //Can refactor the ResultView stuff again and probably take out the need for the Results() override
        //put ResultView back in FilterSearchModel and check if not null to render it
        data.FilterSearch.completeFilterRetrieval(null)
    },

    retrieveResultConfig: function () {

    },

    initUI: function () {
        $('#tile-result-page-size').materialSelect();
        $('#tile-result-sort').materialSelect();
    },

    initEvents: function () {
        //tile click event
        $(document).on("click", ".tile-result", function (e) {
            if ($(this).hasClass("tile-active")) {
                var datasetId = $(this).data("id");
                window.location.href = "/Dataset/Detail/" + encodeURIComponent(datasetId);
            }
        });

        //favorite click event
        $(document).on("click", ".tile-favorite", function (e) {
            e.stopPropagation();

            var element = $(this);
            var datasetId = element.data("id");

            $.ajax({
                url: '/Favorites/SetFavorite?datasetId=' + encodeURIComponent(datasetId),
                method: "GET",
                success: function () {
                    element.toggleClass("fas far");
                },
                error: function () {
                    data.Dataset.makeToast("error", "Failed to remove favorite.");
                }
            });

            //use this function once converted to UserFavorite
            //data.Favorites.toggleFavorite(this, "Dataset", function () { })
        });

        //sort by change


        //page size change

        //page change
    }
}