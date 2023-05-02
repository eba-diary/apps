﻿// @ts-check
data.GlobalDatasetSearch = {

    globalDatasets: null,
    inputTimeout: null,

    init: function () {
        data.GlobalDatasetSearch.executeSearchAndFilters(data.GlobalDatasetSearch.getActivePage());
        data.GlobalDatasetSearch.initEvents();
    },

    buildFilter: function () {
        //required for FilterSearch.js
        //filters are being calculated only on a text search
    },

    retrieveResultConfig: function () {
        //get sort, page size, layout
        let resultParameters = {
            PageSize: $("#tile-result-page-size").val(),
            SortBy: $("#tile-result-sort").val(),
            Layout: $("#tile-result-layout").val()
        }

        return JSON.stringify({ ResultParameters: resultParameters });
    },

    executeSearch: function () {
        data.GlobalDatasetSearch.executeSearchAndFilters(1);
    },

    getEndpoint: function () {
        return "/api/" + data.BetaApiVersion + "/globaldatasets/";
    },

    executeSearchAndFilters: function (pageNumber) {
        //gets new filters and search results
        $(".filter-search-categories-container").addClass("search-blur");

        let endpoint = data.GlobalDatasetSearch.getEndpoint() + "filters";
        let request = data.FilterSearch.buildSearchRequest();

        data.GlobalDatasetSearch.executeSearchWithRequest(request, pageNumber);
        $.post(endpoint, request, (x) => data.FilterSearch.completeFilterRetrieval(x.FilterCategories));
    },

    executeSearchOnly: function () {
        //only get new search results, keep current filters
        let request = data.FilterSearch.buildSearchRequest();
        data.GlobalDatasetSearch.executeSearchWithRequest(request, 1);
    },

    executeSearchWithRequest: function (request, pageNumber) {
        $("#tile-results").addClass("search-blur");

        let endpoint = data.GlobalDatasetSearch.getEndpoint() + "search";

        $.post(endpoint, request, function (response) {
            data.GlobalDatasetSearch.globalDatasets = response.GlobalDatasets;

            let totalResults = response.GlobalDatasets.length;
            let pageSize = parseInt($("#tile-result-page-size").val());

            data.GlobalDatasetSearch.updatePageOrganization(pageNumber);
            data.FilterSearch.completeSearch(totalResults, pageSize, totalResults);
        });
    },

    updatePageOrganizationFromEvent: function (pageNumber) {
        $("#tile-results").addClass("search-blur");
        data.FilterSearch.clearActiveSavedSearch();
        data.GlobalDatasetSearch.updatePageOrganization(pageNumber);
    },

    updatePageOrganization: function (pageNumber) {
        //update the view with current page of results
        let lastPageNumber = 1;

        if ($("#tile-page-next").length) {
            lastPageNumber = parseInt($("#tile-page-next").prev().data("page"));
        }

        if (pageNumber > 0 && pageNumber <= lastPageNumber) {
            let request = {
                GlobalDatasets: data.GlobalDatasetSearch.globalDatasets,
                PageNumber: pageNumber,
                PageSize: $("#tile-result-page-size").val(),
                SortBy: $("#tile-result-sort").val(),
                Layout: $("#tile-result-layout").val(),
            };

            $(".filter-search-results-container").load("/GlobalDatasetSearch/GlobalDatasetResults/", request, function () {
                let totalResults = data.GlobalDatasetSearch.globalDatasets.length;

                if (totalResults > 0 && pageNumber > 1) {
                    let pageSize = parseInt(request.PageSize);
                    let pageStart = ((pageNumber - 1) * pageSize) + 1;
                    let pageEnd = pageNumber * pageSize;
                    let resultCount = parseInt($("#result-count").val())

                    data.FilterSearch.setPageInfo(pageStart, resultCount < pageSize ? totalResults : pageEnd);
                }

                data.GlobalDatasetSearch.initUI()
            });
        }
    },

    initUI: function () {
        $('#tile-result-page-size').materialSelect();
        $('#tile-result-sort').materialSelect();
        $('#tile-result-layout').materialSelect();
        $("#tile-results").removeClass("search-blur");
        $(".filter-search-categories-container").removeClass("search-blur");

        data.GlobalDatasetSearch.setLayout();
    },

    initEvents: function () {
        $(document).on("click", ".tile-result", data.GlobalDatasetSearch.setPreviousSearch);

        //favorite click event
        $(document).on("click", ".card-favorite", function (e) {
            e.stopPropagation();
            e.preventDefault();

            let element = $(this);
            let datasetId = element.data("id");
            let iconElement = $(element[0].firstElementChild);

            $.ajax({
                url: '/Favorites/SetFavorite?datasetId=' + encodeURIComponent(datasetId) + '&removeForAllEnvironments=' + iconElement.hasClass("lt_gold"),
                method: "GET",
                success: function () {
                    iconElement.toggleClass("fas lt_gold far gray");

                    let updatedDatasetIndex = data.GlobalDatasetSearch.globalDatasets.findIndex(x => x.TargetDatasetId == datasetId);
                    data.GlobalDatasetSearch.globalDatasets[updatedDatasetIndex].IsFavorite = iconElement.hasClass("lt_gold");
                },
                error: function () {
                    data.Dataset.makeToast("error", "Failed to remove favorite.");
                }
            });
        });

        //sort by change event
        $(document).on("change", "#tile-result-layout", data.GlobalDatasetSearch.setLayout);

        //page change events
        $(document).on("click", "#tile-page-previous", function () {
            let previousPage = data.GlobalDatasetSearch.getActivePage();
            previousPage--;
            data.GlobalDatasetSearch.updatePageOrganizationFromEvent(previousPage);
        });

        $(document).on("click", "#tile-page-next", function () {
            let nextPage = data.GlobalDatasetSearch.getActivePage();
            nextPage++;
            data.GlobalDatasetSearch.updatePageOrganizationFromEvent(nextPage);
        });

        $(document).on("click", ".tile-page-number", function () {
            if (!$(this).hasClass("active")) {
                data.GlobalDatasetSearch.updatePageOrganizationFromEvent(parseInt($(this).data("page")));
            }
        });

        //page size change event
        $(document).on("change", "#tile-result-page-size", () => data.GlobalDatasetSearch.updatePageOrganizationFromEvent(1));

        //sort by change event
        $(document).on("change", "#tile-result-sort", () => data.GlobalDatasetSearch.updatePageOrganizationFromEvent(1));

        //filters cleared
        $(document).off("click", "#filter-search-clear");
        $(document).on("click", "#filter-search-clear", function () {
            data.FilterSearch.handleClearAll();
            data.GlobalDatasetSearch.executeSearchOnly();
        });

        //select category option
        $(document).off("change", ".filter-search-category-option-checkbox");
        $(document).on("change", ".filter-search-category-option-checkbox", function () {
            data.FilterSearch.handleCheckboxChange(this);
            data.FilterSearch.clearActiveSavedSearch();
            data.GlobalDatasetSearch.executeSearchOnly();
        });

        //remove filter option from badge
        $(document).off("click", ".chip .close");
        $(document).on("click", ".filter-search-chip-close", function () {
            data.FilterSearch.handleBadgeClear(this);
            data.FilterSearch.clearActiveSavedSearch();
            data.GlobalDatasetSearch.executeSearchOnly();
        });

        //text box type delay
        $(document).on("input", "#filter-search-text", function () {
            let searchText = $.trim($(this).val()).length
            if (searchText > 2 || searchText == 0) {
                data.FilterSearch.clearActiveSavedSearch();

                if (data.GlobalDatasetSearch.inputTimeout) {
                    clearTimeout(data.GlobalDatasetSearch.inputTimeout);
                }

                data.GlobalDatasetSearch.inputTimeout = setTimeout(function () {
                    if ($.trim($("#filter-search-text").val()).length > 0) {
                        $("#tile-result-sort").val('0');
                    }
                    else {
                        $("#tile-result-sort").val('1');
                    }
                    data.GlobalDatasetSearch.executeSearch();
                }, 500);
            }
        });
    },

    setLayout: function () {
        $("#tile-result-layout option").each(function () {
            $(".tile-result").removeClass(`card-result-layout-${$(this).val()}`);
        });

        $(".tile-result").addClass(`card-result-layout-${$("#tile-result-layout").val()}`)
    },

    getActivePage: function () {
        return parseInt($(".tile-page-number.active").data("page"));
    },

    setPreviousSearch: function () {
        //set localStorage items for searchText, filteredIds, pageSelection, sortByVal, itemsToShow
        localStorage.setItem("searchText", $.trim($("#filter-search-text").val()));
        localStorage.setItem("sortBy", $("#tile-result-sort").val());
        localStorage.setItem("pageNumber", data.GlobalDatasetSearch.getActivePage().toString());
        localStorage.setItem("pageSize", $("#tile-result-page-size").val());
        localStorage.setItem("layout", $("#tile-result-layout").val());

        let filters = [];
        $('.filter-search-category-option-checkbox:checkbox:checked').each(function () {
            filters.push(this.id);
        });
        localStorage.setItem("filters", JSON.stringify(filters));
    }
}