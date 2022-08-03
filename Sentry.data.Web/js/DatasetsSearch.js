data.DatasetsSearch = {

    searchableDatasets: null,

    init: function () {
        data.DatasetsSearch.executeSearch();
        data.DatasetsSearch.initEvents();
    },

    buildFilter: function () {
        //required for FilterSearch.js
        //filters are being calculated only on a text search
    },

    retrieveResultConfig: function () {
        //required for FilterSearch.js
        //no result config yet, might use for saving the sort by and page size
    },

    executeSearch: function () {
        $.post("/DatasetSearch/SearchableDatasets/", data.FilterSearch.buildSearchRequest(), function (response) {
            data.DatasetsSearch.searchableDatasets = response
            data.DatasetsSearch.executeDatasetSearch(1);
            data.DatasetsSearch.getTileFilters();
        });
    },

    executeDatasetSearch: function (pageNumber) {
        var lastPageNumber = 1;

        var lastPage = $("#tile-page-next").prev();
        console.log(lastPage);
        console.log(lastPage.data("page"));

        if ($("#tile-page-next").length) {
            lastPageNumber = parseInt($("#tile-page-next").prev().data("page"));
        }

        if (pageNumber > 0 && pageNumber <= lastPageNumber) {
            var request = data.DatasetsSearch.buildDatasetSearchRequest(pageNumber);

            //get tiles
            $.post("/DatasetSearch/TileResultsModel/", request, function (tileResultsModel) {

                //load result view
                $(".filter-search-results-container").load("/DatasetSearch/TileResults/", tileResultsModel, function () {
                    data.DatasetsSearch.initUI();
                    data.FilterSearch.completeSearch(tileResultsModel.TotalResults, request.PageSize, tileResultsModel.TotalResults);
                });
            });
        }
    },

    getTileFilters: function () {
        $.post("/DatasetSearch/TileFilters/", data.DatasetsSearch.buildDatasetSearchRequest(1), (filters) => data.FilterSearch.completeFilterRetrieval(filters));
    },

    buildDatasetSearchRequest: function (pageNumber) {
        var request = data.FilterSearch.buildSearchRequest();
        request.SearchableTiles = data.DatasetsSearch.searchableDatasets;
        request.PageNumber = pageNumber;
        request.PageSize = $("#tile-result-page-size").val();
        request.SortBy = $("#tile-result-sort").val();
        request.Layout = $("#tile-result-layout").val();

        return request;
    },

    initUI: function () {
        $('#tile-result-page-size').materialSelect();
        $('#tile-result-sort').materialSelect();
        $('#tile-result-layout').materialSelect();

        data.DatasetsSearch.setLayout();
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
        $(document).on("change", "#tile-result-layout", data.DatasetsSearch.setLayout);

        //page change events
        $(document).on("click", "#tile-page-previous", function () {
            var previous = parseInt($(".tile-page-number.active").data("page"))--;
            data.DatasetsSearch.executeDatasetSearch(previous);
        });

        $(document).on("click", "#tile-page-next", function () {
            var next = parseInt($(".tile-page-number.active").data("page"))++;
            data.DatasetsSearch.executeDatasetSearch(next);
        });

        $(document).on("click", ".tile-page-number", function () {
            if (!$(this).hasClass("active")) {
                data.DatasetsSearch.executeDatasetSearch(parseInt($(this).data("page")))
            }
        });

        //page size change event
        $(document).on("change", "#tile-result-page-size", () => data.DatasetsSearch.executeDatasetSearch(1));

        //sort by change event
        $(document).on("change", "#tile-result-sort", () => data.DatasetsSearch.executeDatasetSearch(1));

        //select category option
        $(document).off("change", ".filter-search-category-option-checkbox");
        $(document).on("change", ".filter-search-category-option-checkbox", function () {
            data.FilterSearch.handleCheckboxChange(this);
            data.DatasetsSearch.executeDatasetSearch(1);
        });

        //filters cleared
        $(document).off("click", "#filter-search-clear");
        $(document).on("click", "#filter-search-clear", function () {
            data.FilterSearch.handleClearAll();
            data.DatasetsSearch.executeDatasetSearch(1);
        });

        $(document).off("click", "[id^='clearOption_']");
        $(document).on("click", "[id^='clearOption_']", function () {
            data.FilterSearch.handleBadgeClear($(this));
            data.DatasetsSearch.executeDatasetSearch(1);
        });
    },

    setLayout: function () {
        $("#tile-result-layout option").each(function () {
            $(".tile-result").removeClass(`tile-result-layout-${$(this).val()}`);
        });

        $(".tile-result").addClass(`tile-result-layout-${$("#tile-result-layout").val()}`)
    }
}