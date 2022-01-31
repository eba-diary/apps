data.FilterSearch = {

    lastSearchOptions: [],
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
            $(icon).toggleClass("glyphicon-chevron-down glyphicon-chevron-up");
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
            $("#" + optionId).prop('checked', false);

            //hide the clicked badge
            $(this).hide();

            data.FilterSearch.showHideApplyFilter();
        });

        //select category option
        $(document).on("change", ".filter-search-category-option-checkbox", function (e) {
            e.preventDefault();

            var badge = $("#clearOption_" + this.id);

            if (this.checked) {
                badge.show()
                data.FilterSearch.showBadgeContainer();
            }
            else {
                data.FilterSearch.hideBadgeContainer(false);
                badge.hide();
            }

            data.FilterSearch.showHideApplyFilter();
        });

        //clear all badges
        $(document).on("click", "#filter-search-clear", function (e) {
            e.preventDefault();

            data.FilterSearch.hideBadgeContainer(true);

            $("[id^='clearOption_']:visible").each(function () {
                $(this).hide();
            })

            $('.filter-search-category-option-checkbox:checkbox:checked').each(function () {
                $(this).prop('checked', false);
            })

            data.FilterSearch.showHideApplyFilter();
        });

        //search when focus on search box and hit enter
        $("#filter-search-text").on("keypress", function (e) {
            var keycode = (e.keyCode ? e.keyCode : e.which);
            var input = $(this).val();

            if (keycode == '13' && input && $.trim(input)) {
                data.FilterSearch.search();
            }
        });

        //search when apply filters
        $(".filter-search-start").on("click", function (e) {
            e.preventDefault();
            data.FilterSearch.search();
        });
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
            hasSameValues = data.FilterSearch.lastSearchOptions.includes(this.id);
            return hasSameValues;
        });

        //hide apply button if filters are same as initial search, show if filters are different
        if (selectedOptions.length === data.FilterSearch.lastSearchOptions.length && hasSameValues) {
            $("#filter-search-apply").hide();
        }
        else {
            $("#filter-search-apply").show();
        }
    },

    searchPrep: function () {
        $("#filter-search-text").prop("disabled", true);
        $("#filter-search-apply").prop("disabled", true);
        
        $(".glyphicon-search").hide();
        $(".filter-search-results-container").hide();
        $(".filter-search-results-none").hide();

        $(".fa-spin").show();
        $(".filter-search-result-sentry-spinner").show();
    },

    completeSearch: function (totalResults) {
        $("#filter-search-text").prop("disabled", false);
        $("#filter-search-apply").prop("disabled", false);

        $(".fa-spin").hide();
        $(".filter-search-result-sentry-spinner").hide();

        $(".glyphicon-search").show();

        if (totalResults > 0) {
            $(".filter-search-results-container").slideDown();
        }
        else {
            $(".filter-search-results-none").show();
        }
    },

    filterRetrivalPrep: function () {
        $(".filter-search-categories-container").hide();
        $(".filter-search-categories-sentry-spinner").show();
        $("#filter-search-clear").hide();
        $("#filter-search-apply").hide();
    },

    completeFilterRetrieval: function () {
        $(".filter-search-categories-sentry-spinner").hide();
        $(".filter-search-categories-container").show();

        var selectedOptions = $('.filter-search-category-option-checkbox:checkbox:checked');

        data.FilterSearch.lastSearchOptions = selectedOptions.map(function () { return this.id }).get();

        //open all filter categories with a selected option
        selectedOptions.closest('.filter-search-category-options').each(function () {
            $("#" + this.id.replace("hide", "icon")).removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
            $(this).show();
        });

        //show selected option badges
        selectedOptions.each(function () {
            $('#clearOption_' + this.id).show();
        });

        //show active options container if there are active options
        if (selectedOptions.length > 0) {
            data.FilterSearch.showBadgeContainer();
        }
    },

    getSelectedCategoryOptions: function () {

        var categories = [];

        $('.filter-search-category-option-checkbox:checkbox:checked').each(function () {
            var parts = this.id.split('_');

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
        });

        return categories;
    }
}