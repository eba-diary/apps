data.FilterSearch = {

    searchType: "",
    lastSelectedOptionIds: [],

    executeSearch: function () {
        console.log('Must pass searchExecuter parameter to data.FilterSearch.init')
    },
    retrieveFilterOptions: function () {
        console.log('Must pass filterRetriever parameter to data.FilterSearch.init')
    },

    init: function (searchExecuter, filterRetriever, searchTypeName) {
        this.initToast();
        
        this.executeSearch = searchExecuter;
        this.retrieveFilterOptions = filterRetriever;
        this.searchType = searchTypeName;

        this.initEvents();
        
        var urlParams = new URLSearchParams(window.location.search);
        this.loadSavedSearches(urlParams.get('savedSearch'));
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

                badge.removeClass("display-none");
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
            });
            
            $('.filter-search-category-option-checkbox:checkbox:checked').each(function () {
                $(this).prop('checked', false);
            });

            data.FilterSearch.clearActiveSavedSearch();
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

        $(document).on("click", ".filter-search-save", function (e) {
            $("#save-search-name").val('');
            $(".save-search-name-label").removeClass('active');
            $("#save-search-favorite").prop('checked', false);
        })

        //save search parameters
        $(document).on("submit", "#save-search", function (e) {
            e.preventDefault();
            
            $('#save-search').addClass('disabled');
            $('#cancel-save-search').addClass('display-none');
            $('.filter-search-save-search-modal-text').addClass('display-none');
            $('.filter-search-save-search-modal-spinner').removeClass('display-none');
            
            var request = data.FilterSearch.buildSearchRequest();
            request.SearchType = data.FilterSearch.searchType;
            request.SearchName = $("#save-search-name").val();
            request.AddToFavorites = $("#save-search-favorite").is(":checked");

            $.post("/FilterSearch/SaveSearch", request, (x) => data.FilterSearch.completeSaveSearch(x, request.SearchName));
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
            $("#filter-search-clear").addClass("display-none");
        }
    },

    showBadgeContainer: function () {
        $(".filter-search-active-options-container").slideDown();
        $("#filter-search-clear").removeClass("display-none");
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
            $(".filter-search-apply").addClass("display-none");
        }
        else {
            $(".filter-search-apply").removeClass("display-none");
        }
    },

    searchPrep: function () {
        $("#filter-search-text").prop("disabled", true);
        $(".filter-search-apply").prop("disabled", true);

        $(".modal").modal("hide");

        $(".filter-search-results-container").hide();

        $(".filter-search-save-search-container").addClass("display-none");        
        $(".filter-search-start").addClass("display-none");
        $(".filter-search-results-none").addClass("display-none");
        $(".filter-search-result-count-container").addClass("display-none");

        data.FilterSearch.clearActiveSavedSearch();

        $(".filter-search-result-progress").removeClass("display-none");
    },

    completeSearch: function (totalResultCount, pageSize, returnedResultCount) {
        $("#filter-search-text").prop("disabled", false);
        $(".filter-search-apply").prop("disabled", false);

        $(".filter-search-result-progress").addClass("display-none");

        $(".icon-search").removeClass("display-none");
        $(".filter-search-save-search-container").removeClass("display-none");

        if (totalResultCount > 0) {
            $(".filter-search-results-container").slideDown();

            $("#filter-search-total").text(totalResultCount.toLocaleString("en-US"));
            $("#filter-search-returned-total").text(returnedResultCount.toLocaleString("en-US"));
            data.FilterSearch.setPageInfo(1, pageSize < returnedResultCount ? pageSize : returnedResultCount);
            $(".filter-search-result-count-container").slideDown();
        }
        else {
            $(".filter-search-results-none").removeClass("display-none");
        }
    },

    filterRetrivalPrep: function () {
        $(".filter-search-categories-container").addClass("display-none");
        $(".filter-search-categories-progress").removeClass("display-none");
        $("#filter-search-clear").addClass("display-none");
        $(".filter-search-apply").addClass("display-none");
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

            $(".filter-search-categories-progress").addClass("display-none");
            $(".filter-search-categories-container").removeClass("display-none");

            var selectedOptions = $('.filter-search-category-option-checkbox:checkbox:checked');

            data.FilterSearch.lastSelectedOptionIds = selectedOptions.map(function () { return this.id }).get();

            //open all filter categories with a selected option
            selectedOptions.closest('.filter-search-category-options').each(function () {
                $("#" + this.id.replace("hide", "icon")).removeClass("fa-chevron-down").addClass("fa-chevron-up");
                $(this).removeClass("display-none");
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

    loadSavedSearches: function (activeSearchName) {
        var params = "searchType=" + data.FilterSearch.searchType;
        if (activeSearchName) {
            params += "&activeSearchName=" + encodeURIComponent(activeSearchName);
        }
        
        $('.filter-search-save-search-container').load("/FilterSearch/SavedSearches?" + params, data.FilterSearch.completeSaveSearchModal);
    },

    completeSaveSearch: function (result, searchName) {
        
        if (result.Result === "Failure") {
            data.FilterSearch.showToast("error", "There was an issue saving your search. Please try again or reach out to DSCSupport@sentry.com.")
        }
        else {
            data.FilterSearch.completeSaveSearchModal();
            data.FilterSearch.loadSavedSearches(searchName);

            if (result.Result === "New") {
                data.FilterSearch.showToast("success", "The search '" + searchName + "' has been saved.")
            }
            else if (result.Result === "Update") {
                data.FilterSearch.showToast("success", "The saved search '" + searchName + "' already existed and has been updated.")
            }
        }
    },

    completeSaveSearchModal: function () {
        $("#filter-search-save-modal").modal("hide");

        $('#save-search').removeClass('disabled');
        $('#cancel-save-search').removeClass('display-none');
        $('.filter-search-save-search-modal-text').removeClass('display-none');
        $('.filter-search-save-search-modal-spinner').addClass('display-none');
    },

    clearActiveSavedSearch: function () {
        $('.filter-search-saved-search-option').each(function () {
            $(this).removeClass('active');
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
    },

    buildSearchRequest: function () {
        return {
            SearchText: $.trim($("#filter-search-text").val()),
            FilterCategories: data.FilterSearch.getSelectedCategoryOptions()
        }
    },

    initToast: function () {
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "newestOnTop": false,
            "progressBar": true,
            "positionClass": "toast-top-right",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "1000",
            "hideDuration": "1000",
            "timeOut": "8000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };
    },

    showToast: function (level, message) {        
        toastr[level](message);
    }
}