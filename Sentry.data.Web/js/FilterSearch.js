data.FilterSearch = {

    lastSearchOptions: [],

    init: function () {
        this.initUserInterface();
        this.initEvents();
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
            hasSameValues = data.FilterSearch.lastSearchOptions.some(i => this.id === i);
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

    initUserInterface: function () {

        var selectedOptions = $('.filter-search-category-option-checkbox:checkbox:checked');

        data.FilterSearch.setLastSearchOptions(selectedOptions);

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

        $(".filter-search-container").show();
    },

    setLastSearchOptions: function (selectedOptions) {
        this.lastSearchOptions = selectedOptions.map(function (x) { return x.id });
    },

    startSearch: function () {
        $("#filter-search-text").prop("disabled", true);
        $(".filter-search-active-option-close").prop("disabled", true);
        $(".filter-search-category-option-checkbox").prop("disabled", true);

        $(".glyphicon-search").hide();
        $(".filter-search-results-container").hide();
        $("#filter-search-clear").hide();
        $("#filter-search-apply").hide();

        $(".fa-spin").show();
        $(".filter-search-sentry-spinner-container").show();
    },

    completeSearch: function () {
        $("#filter-search-text").prop("disabled", false);
        $(".filter-search-active-option-close").prop("disabled", false);
        $(".filter-search-category-option-checkbox").prop("disabled", false);

        $(".fa-spin").hide();
        $(".filter-search-sentry-spinner-container").hide();

        $(".glyphicon-search").show();
        $(".filter-search-results-container").slideDown();

        selectedOptions = $('.filter-search-category-option-checkbox:checkbox:checked');

        if (selectedOptions.length > 0) {
            data.FilterSearch.setLastSearchOptions(selectedOptions);
            data.FilterSearch.showBadgeContainer();
        }
    }
}