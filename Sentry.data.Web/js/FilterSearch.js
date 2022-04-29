data.FilterSearch = {

    lastSelectedOptionIds: [],

    executeSearch: function () {
        console.log('Must pass searchExecuter parameter to data.FilterSearch.init')
    },
    retrieveFilterOptions: function () {
        console.log('Must pass filterRetriever parameter to data.FilterSearch.init')
    },

    init: function (searchExecuter, filterRetriever) {
        this.executeSearch = searchExecuter;
        this.retrieveFilterOptions = filterRetriever;

        this.initEvents();

        $(".filter-search-container").show();
    },

    initEvents: function () {
        //open category options
        $(document).on("click", "[id^='categoryType_']", function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var category = "#hide_" + id
            var icon = "#icon_" + id;

            $(category).slideToggle();
            $(icon).toggleClass("fa-chevron-down fa-chevron-up");
        });

        //open additional hidden category options
        $(document).on("click", "[id^='categoryMore_']", function (e) {
            e.preventDefault();

            var show = "#hidden_" + $(this).attr("id");

            $(show).slideToggle();

            if ($(this).text() === "Show Less") {
                $(this).text("Show More");
            }
            else {
                $(this).text("Show Less");
            }
        });

        //clear single badge
        $(document).on("click", "[id^='clearOption_']", function (e) {
            e.preventDefault();

            //hide the badge container if no badges left
            data.FilterSearch.hideBadgeContainer(false);

            //uncheck the category option that was removed
            var optionId = $(this).attr("id").replace("clearOption_", "");
            data.FilterSearch.setOptionCheckbox(optionId, false);

            //hide the clicked badge
            $(this).addClass("display-none");

            data.FilterSearch.showHideApplyFilter();
        });

        //select category option
        $(document).on("change", ".filter-search-category-option-checkbox", function (e) {
            e.preventDefault();

            var id = this.id.replace('modal_', '');

            var badge = $("#clearOption_" + id);

            if (this.checked) {
                //making sure both modal and filter checkbox gets checked
                data.FilterSearch.setOptionCheckbox(id, true);

                badge.removeClass("display-none")
                data.FilterSearch.showBadgeContainer();
            }
            else {
                //making sure both modal and filter checkbox gets unchecked
                data.FilterSearch.setOptionCheckbox(id, false);

                data.FilterSearch.hideBadgeContainer(false);
                badge.addClass("display-none");
            }

            data.FilterSearch.showHideApplyFilter();
        });

        //clear all badges
        $(document).on("click", "#filter-search-clear", function (e) {
            e.preventDefault();

            data.FilterSearch.hideBadgeContainer(true);

            $("[id^='clearOption_']:visible").each(function () {
                $(this).addClass("display-none");
            })

            $('.filter-search-category-option-checkbox:checkbox:checked').each(function () {
                $(this).prop('checked', false);
            })

            data.FilterSearch.showHideApplyFilter();
        });

        //search when focus on search box and hit enter
        $(document).on("keypress", "#filter-search-text", function (e) {
            var keycode = (e.keyCode ? e.keyCode : e.which);

            if (keycode == '13') {
                data.FilterSearch.search();
            }
        });

        //search when apply filters
        $(document).on("click", ".filter-search-start", function (e) {
            e.preventDefault();
            data.FilterSearch.search();
        });
    },

    setOptionCheckbox: function (id, checked) {
        $("#" + id).prop('checked', checked);
        $("#modal_" + id).prop('checked', checked);
    },

    search: function () {
        data.FilterSearch.searchPrep();
        data.FilterSearch.filterRetrivalPrep();

        data.FilterSearch.executeSearch();
        data.FilterSearch.retrieveFilterOptions();
    },

    hideBadgeContainer: function (clearAll) {
        if ($("[id^='clearOption_']:visible").length === 1 || clearAll) {
            $(".filter-search-active-options-container").slideUp();
            $("#filter-search-clear").hide();
        }
    },

    showBadgeContainer: function () {
        $(".filter-search-active-options-container").slideDown();
        $("#filter-search-clear").show();
    },

    showHideApplyFilter: function () {
        //get all checked filters
        var selectedOptions = $('.filter-search-category-option-checkbox:checkbox:checked');

        var hasSameValues = true;
        //determine all selected filters were in the initial filter
        selectedOptions.each(function () {
            hasSameValues = data.FilterSearch.lastSelectedOptionIds.includes(this.id);
            return hasSameValues;
        });

        //hide apply button if filters are same as initial search, show if filters are different
        if (selectedOptions.length === data.FilterSearch.lastSelectedOptionIds.length && hasSameValues) {
            $(".filter-search-apply").hide();
        }
        else {
            $(".filter-search-apply").show();
        }
    },

    searchPrep: function () {
        $("#filter-search-text").prop("disabled", true);
        $(".filter-search-apply").prop("disabled", true);

        $(".modal").modal("hide");
        $(".icon-search").hide();
        $(".filter-search-results-container").hide();
        $(".filter-search-results-none").hide();
        $(".filter-search-result-count-container").hide();

        $(".filter-search-result-progress").show();
    },

    completeSearch: function (totalResultCount, pageSize, returnedResultCount) {
        $("#filter-search-text").prop("disabled", false);
        $(".filter-search-apply").prop("disabled", false);

        $(".filter-search-result-progress").hide();

        $(".icon-search").show();

        if (totalResultCount > 0) {
            $(".filter-search-results-container").slideDown();

            $("#filter-search-total").text(totalResultCount.toLocaleString("en-US"));
            $("#filter-search-returned-total").text(returnedResultCount.toLocaleString("en-US"));
            data.FilterSearch.setPageInfo(1, pageSize < returnedResultCount ? pageSize : returnedResultCount);
            $(".filter-search-result-count-container").slideDown();
        }
        else {
            $(".filter-search-results-none").show();
        }
    },

    filterRetrivalPrep: function () {
        $(".filter-search-categories-container").hide();
        $(".filter-search-categories-progress").show();
        $("#filter-search-clear").hide();
        $(".filter-search-apply").hide();
    },

    completeFilterRetrieval: function (filters) {

        var categories = { 'filterCategories': [] }

        if (filters && filters.length) {
            categories.filterCategories = filters;
        }
        else {
            //set the categories to previous available filters with the selected options at the time of searching
            categories.filterCategories = data.FilterSearch.getSelectedCategoryOptions()
        }

        $('.filter-search-show-all-container').load("/FilterSearch/FilterShowAll/", categories);
        $('.filter-search-categories-container').load("/FilterSearch/FilterCategories/", categories, function () {

            $(".filter-search-categories-progress").hide();
            $(".filter-search-categories-container").show();

            var selectedOptions = $('.filter-search-category-option-checkbox:checkbox:checked');

            data.FilterSearch.lastSelectedOptionIds = selectedOptions.map(function () { return this.id }).get();

            //open all filter categories with a selected option
            selectedOptions.closest('.filter-search-category-options').each(function () {
                $("#" + this.id.replace("hide", "icon")).removeClass("fa-chevron-down").addClass("fa-chevron-up");
                $(this).show();
            });

            //show selected option badges
            selectedOptions.each(function () {
                $('#clearOption_' + this.id).removeClass("display-none");
            });

            //show active options container if there are active options
            if (selectedOptions.length > 0) {
                data.FilterSearch.showBadgeContainer();
            }
        });
    },

    getSelectedCategoryOptions: function () {

        var categories = [];

        $('.filter-search-category-option-checkbox:checkbox:checked').each(function () {
            var parts = this.id.split('_');

            if (parts[0] != 'modal') {
                var option = {
                    OptionValue: $(this).attr('value'),
                    ParentCategoryName: parts[0].replace('-', ' '),
                    Selected: true
                };

                var exists = categories.find(x => x.CategoryName == option.ParentCategoryName);

                if (exists) {
                    exists.CategoryOptions.push(option)
                }
                else {
                    categories.push({
                        CategoryName: option.ParentCategoryName,
                        CategoryOptions: [option]
                    })
                }
            }
        });

        return categories;
    },

    setPageInfo: function (start, end) {
        $("#filter-search-page-size").text(start.toLocaleString("en-US") + ' - ' + end.toLocaleString("en-US"));
    }
}