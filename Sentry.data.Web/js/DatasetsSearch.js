// @ts-check
data.DatasetsSearch = {

    searchableDatasets: null,
    keyupTimeout: null,

    init: function () {
        data.DatasetsSearch.initSearch(data.DatasetsSearch.getActivePage());
        data.DatasetsSearch.initEvents();
    },

    buildFilter: function () {
        //required for FilterSearch.js
        //filters are being calculated only on a text search
    },

    retrieveResultConfig: function () {
        //get sort, page size, layout
        var resultParameters = {
            PageSize: $("#tile-result-page-size").val(),
            SortBy: $("#tile-result-sort").val(),
            Layout: $("#tile-result-layout").val()
        }

        return JSON.stringify({ ResultParameters: resultParameters });
    },

    executeSearch: function () {
        data.DatasetsSearch.executeDatasetSearch(1, true);
    },

    initSearch: function (pageNumber) {
        $.get("/DatasetSearch/SearchableTiles/", function (tileModels) {
            data.DatasetsSearch.searchableDatasets = tileModels
            data.DatasetsSearch.executeDatasetSearch(pageNumber, true);
        });
    },

    executeSearchWithoutLoader: function (pageNumber, updateFilters) {
        data.FilterSearch.clearActiveSavedSearch();
        $("#tile-results").addClass("search-blur");
        data.DatasetsSearch.executeDatasetSearch(pageNumber, updateFilters);
    },

    executeDatasetSearch: function (pageNumber, updateFilters) {
        var lastPageNumber = 1;

        if ($("#tile-page-next").length) {
            lastPageNumber = parseInt($("#tile-page-next").prev().data("page"));
        }

        if (pageNumber > 0 && pageNumber <= lastPageNumber) {
            var request = data.DatasetsSearch.buildDatasetSearchRequest(pageNumber, updateFilters);

            //get tiles
            $.post("/DatasetSearch/TileResultsModel/", request, function (tileResultsModel) {

                //load result view
                $(".filter-search-results-container").load("/DatasetSearch/TileResults/", tileResultsModel, function () {
                    data.DatasetsSearch.initUI();
                    data.FilterSearch.completeSearch(tileResultsModel.TotalResults, request.PageSize, tileResultsModel.TotalResults);

                    if (updateFilters) {
                        data.FilterSearch.completeFilterRetrieval(tileResultsModel.FilterCategories);
                    }
                });
            });
        }
    },

    buildDatasetSearchRequest: function (pageNumber, updateFilters) {
        var request = data.FilterSearch.buildSearchRequest();
        request.SearchableTiles = data.DatasetsSearch.searchableDatasets;
        request.PageNumber = pageNumber;
        request.PageSize = $("#tile-result-page-size").val();
        request.SortBy = $("#tile-result-sort").val();
        request.Layout = $("#tile-result-layout").val();
        request.UpdateFilters = updateFilters;

        return request;
    },

    initUI: function () {
        $('#tile-result-page-size').materialSelect();
        $('#tile-result-sort').materialSelect();
        $('#tile-result-layout').materialSelect();
        $("#tile-results").removeClass("search-blur");
        $(".filter-search-categories-container").removeClass("search-blur");

        data.DatasetsSearch.setLayout();
    },

    initEvents: function () {
        //tile click event
        $(document).on("click", ".tile-result", function (e) {
            if ($(this).hasClass("tile-active")) {
                var datasetId = $(this).data("id");
                data.DatasetsSearch.setPreviousSearch();
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
            var previousPage = data.DatasetsSearch.getActivePage();
            previousPage--;
            data.DatasetsSearch.executeSearchWithoutLoader(previousPage);
        });

        $(document).on("click", "#tile-page-next", function () {
            var nextPage = data.DatasetsSearch.getActivePage();
            nextPage++;
            data.DatasetsSearch.executeSearchWithoutLoader(nextPage);
        });

        $(document).on("click", ".tile-page-number", function () {
            if (!$(this).hasClass("active")) {
                data.DatasetsSearch.executeSearchWithoutLoader(parseInt($(this).data("page")))
            }
        });

        //page size change event
        $(document).on("change", "#tile-result-page-size", () => data.DatasetsSearch.executeSearchWithoutLoader(1));

        //sort by change event
        $(document).on("change", "#tile-result-sort", () => data.DatasetsSearch.executeSearchWithoutLoader(1));

        //select category option
        $(document).off("change", ".filter-search-category-option-checkbox");
        $(document).on("change", ".filter-search-category-option-checkbox", function () {
            data.FilterSearch.handleCheckboxChange(this);
            data.DatasetsSearch.executeSearchWithoutLoader(1);
        });

        //filters cleared
        $(document).off("click", "#filter-search-clear");
        $(document).on("click", "#filter-search-clear", function () {
            data.FilterSearch.handleClearAll();
            data.DatasetsSearch.executeSearchWithoutLoader(1);
        });

        $(document).off("click", "[id^='clearOption_']");
        $(document).on("click", "[id^='clearOption_']", function () {
            data.FilterSearch.handleBadgeClear(this);
            data.DatasetsSearch.executeSearchWithoutLoader(1);
        });

        //text box type delay
        $(document).on("keyup", "#filter-search-text", function (e) {
            if (data.DatasetsSearch.keyupTimeout) {
                clearTimeout(data.DatasetsSearch.keyupTimeout);
            }

            data.DatasetsSearch.keyupTimeout = setTimeout(function () {
                $(".filter-search-categories-container").addClass("search-blur");
                data.DatasetsSearch.executeSearchWithoutLoader(1, true);
            }, 500);
        });
    },

    setLayout: function () {
        $("#tile-result-layout option").each(function () {
            $(".tile-result").removeClass(`tile-result-layout-${$(this).val()}`);
        });

        $(".tile-result").addClass(`tile-result-layout-${$("#tile-result-layout").val()}`)
    },

    getActivePage: function () {
        return parseInt($(".tile-page-number.active").data("page"));
    },

    setPreviousSearch: function () {
        //set localStorage items for searchText, filteredIds, pageSelection, sortByVal, itemsToShow
        localStorage.setItem("searchText", $.trim($("#filter-search-text").val()));
        localStorage.setItem("sortBy", $("#tile-result-sort").val());
        localStorage.setItem("pageNumber", data.DatasetsSearch.getActivePage());
        localStorage.setItem("pageSize", $("#tile-result-page-size").val());
        localStorage.setItem("layout", $("#tile-result-layout").val());

        var filters = [];
        $('.filter-search-category-option-checkbox:checkbox:checked').each(function () {
            filters.push(this.id);
        });
        localStorage.setItem("filters", JSON.stringify(filters));
    }
}