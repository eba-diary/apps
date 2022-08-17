﻿// @ts-check
data.DatasetsSearch = {

    searchableDatasets: null,

    init: function () {
        data.DatasetsSearch.executeFullSearch(data.DatasetsSearch.getActivePage());
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
        data.DatasetsSearch.executeFullSearch(1);
    },

    executeFullSearch: function (pageNumber) {
        $.post("/DatasetSearch/SearchableDatasets/", data.FilterSearch.buildSearchRequest(), function (response) {
            data.DatasetsSearch.searchableDatasets = response
            data.DatasetsSearch.executeDatasetSearch(pageNumber);
            data.DatasetsSearch.getTileFilters();
        });
    },

    executeDatasetSearch: function (pageNumber) {
        var lastPageNumber = 1;

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
                    //write search event
                    data.DatasetsSearch.publishSearchEvent();
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
            data.DatasetsSearch.executeDatasetSearch(previousPage);
        });

        $(document).on("click", "#tile-page-next", function () {
            var nextPage = data.DatasetsSearch.getActivePage();
            nextPage++;
            data.DatasetsSearch.executeDatasetSearch(nextPage);
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
            data.FilterSearch.handleBadgeClear(this);
            data.DatasetsSearch.executeDatasetSearch(1);
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
    },

    publishSearchEvent: function () {
        $.ajax({
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            type: 'POST',
            url: '/Search/SearchEvent/' + self.SearchType() +
                '?categoryFilters=' + FilterTitlesToArray(data.Search.GetSelectedFiltersFromGroup(self.CategoryFilters, self.SelectedFilters())) +
                '&extensions=' + FilterTitlesToArray(data.Search.GetSelectedFiltersFromGroup(self.ExtensionFilters, self.SelectedFilters())) +
                '&businessUnits=' + FilterTitlesToArray(data.Search.GetSelectedFiltersFromGroup(self.BusinessUnitFilters, self.SelectedFilters())) +
                '&datasetFunctions=' + FilterTitlesToArray(data.Search.GetSelectedFiltersFromGroup(self.DatasetFunctionFilters, self.SelectedFilters())) +
                '&tags=' + FilterTitlesToArray(data.Search.GetSelectedFiltersFromGroup(self.TagFilters, self.SelectedFilters())) +
                '&searchTerm=' + window.vm.Query() +
                '&resultsReturned=' + queryResults.length
        });
    }
}