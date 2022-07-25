data.DatasetsSearch = {
    init: function () {
        this.initUI();
        this.initEvents();
    },

    executeSearch: function () {
        this.executeSearch(1, true);
    },

    executeSearch: function (pageNumber, reloadFilters) {
        //build search request
        var request = data.FilterSearch.buildSearchRequest();
        request.PageNumber = pageNumber;
        request.PageSize = $("#tile-result-page-size").val();
        request.SortBy = $("#tile-result-sort").val();

        //get tiles
        $.post("/DatasetSearch/GetTileResultsModel/", request, function (tileResultsModel) {
            //load result view
            $(".filter-search-results-container").load("/DatasetSearch/TileResults/", tileResultsModel, function () {
                data.FilterSearch.completeSearch(tileResultsModel.TotalResults, request.PageSize, tileResultsModel.TotalResults);
            });

            if (reloadFilters) {
                data.FilterSearch.completeFilterRetrieval(tileResultsModel.FilterCategories);
            }
        });
    },

    buildFilter: function () {
        //executeSearch is handling both result and filter creation
    },

    retrieveResultConfig: function () {
        //no result config yet, might use for saving the sort by and page size
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
        //call executeSearch(1, false);

        //page size change
        //call executeSearch(1, false);

        //page change
        //call executeSearch($(".tile-page-item.active").data("page"), false);
    }
}