data.DatasetsSearch = {
    init: function () {
        this.executeSearch();
        this.initUI();
        this.initEvents();
    },

    executeSearch: function () {
        data.DatasetsSearch.executeDatasetSearch(1, true);
    },

    executeDatasetSearch: function (pageNumber, reloadFilters) {
        //build search request
        var lastPageNumber = 1;

        if ($("#tile-page-next").length) {
            lastPageNumber = parseInt($("#tile-page-next").prev().data("page"));
        }

        if (pageNumber > 0 && pageNumber <= lastPageNumber) {
            var request = data.FilterSearch.buildSearchRequest();
            request.PageNumber = pageNumber;
            request.PageSize = $("#tile-result-page-size").val();
            request.SortBy = $("#tile-result-sort").val();

            //get tiles
            $.post("/DatasetSearch/GetTileResultsModel/", request, function (tileResultsModel) {
                //load result view
                $(".filter-search-results-container").load("/DatasetSearch/TileResults/", tileResultsModel, function () {
                    data.DatasetsSearch.initUI();
                    data.FilterSearch.completeSearch(tileResultsModel.TotalResults, request.PageSize, tileResultsModel.TotalResults);
                });

                if (reloadFilters) {
                    data.FilterSearch.completeFilterRetrieval(tileResultsModel.FilterCategories);
                }
            });
        }
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
        $('#tile-result-layout').materialSelect();
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

        //sort by change event
        $(document).on("change", "#tile-result-sort", function (e) {
            console.log("sort click");
            data.DatasetsSearch.executeDatasetSearch(1, false);
        });

        //page size change event
        $(document).on("change", "#tile-result-page-size", function (e) {
            console.log("size click");
            data.DatasetsSearch.executeDatasetSearch(1, false);
        });

        //page change events
        $(document).on("click", "#tile-page-previous", function (e) {
            console.log("prev click");
            var previous = parseInt($(".tile-page-number.active").data("page"))--;
            data.DatasetsSearch.executeDatasetSearch(previous, false);
        });

        $(document).on("click", "#tile-page-next", function (e) {
            console.log("next click");
            var next = parseInt($(".tile-page-number.active").data("page"))++;
            data.DatasetsSearch.executeDatasetSearch(next, false);
        });

        $(document).on("click", ".tile-page-number", function (e) {
            if (!$(this).hasClass("active")) {
                console.log("number click");
                data.DatasetsSearch.executeDatasetSearch(parseInt($(this).data("page")), false)
            }
        });
    }
}