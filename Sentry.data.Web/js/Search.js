/******************************************************************************************
 * Javascript methods for the Search-related pages
 ******************************************************************************************/

data.Search = {

    Init: function () {
        /// <summary>
        /// Initialization function run at first load
        /// </summary>

        window.vm = new ListViewModel();
        ko.applyBindings(vm);

        //needed to set page number on initial page load
        // setPageNumber loop is needed as it is unknow when ko will load
        //   all pages for pagination.
        if (data.Search.GetParameterByName('page')) {
            localStorage.setItem("pageSelection", (data.Search.GetParameterByName('page')));
            data.Search.setPageNumber(localStorage.getItem("pageSelection"), true);
        }
        else {
            localStorage.setItem("pageSelection", 1);
        }

        if (window.location.href.match(/&ids=/)) {
            // do nothing
        }
        else {
            window.vm.clearFilters();
        }

        $(document).on("click", "[id^='filterType_']", function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var category = "#hide_" + id
            var icon = "#icon_" + id;

            $(category).slideToggle();
            $(icon).toggleClass("fa-chevron-down fa-chevron-up");
        });

        $(document).on("click", "[id^='filterMore_']", function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var show = "#hidden_" + id
            var icon = "#icon_" + id;
            var txt = "#txt_" + id;

            $(show).slideToggle();
            $(icon).toggleClass("fa-plus fa-minus");

            if ($(txt).text() === "Show Less") {
                $(txt).text("Show More");
            }
            else {
                $(txt).text("Show Less");
            }
        });

        $(document).on("click", ".btnFavoriteSearch", function (e) {
            e.preventDefault();
            var button = $(this);
            var id = button.attr("data");

            if (button.hasClass("fas")) {

                $.ajax({
                    url: '/Favorites/SetFavorite?datasetId=' + encodeURIComponent(id),
                    method: "GET",
                    dataType: 'json',
                    error: function () {
                        Sentry.ShowModalAlert("Failed to remove favorite.");
                    }
                });
            }
            else if (button.hasClass("far")) {

                $.ajax({
                    url: '/Favorites/SetFavorite?datasetId=' + encodeURIComponent(id),
                    method: "GET",
                    dataType: 'json',
                    error: function () {
                        Sentry.ShowModalAlert("Failed to set favorite.");
                    }
                });
            }
            e.stopImmediatePropagation();
            $(this).toggleClass("fas far");
            return false;
        });

        // mouseover effect to change the background color of the search results tile
        $(".ul-dataset-list-item").on("mouseenter", function () {
            $(this).css('background-color', $(this).css("border-color").slice(0, -1) + ', 0.1)');
        });

        // mouseout effect to change the background color of the search results tile back to white
        $(".ul-dataset-list-item").on("mouseleave", function () {
            $(this).css('background-color', 'white');
        });

    },

    GetDetailType: function () {
        // get the detail type from the URL
        return window.location.pathname.split("/")[2];
    },

    GetSearchType: function () {
        var regex = /\/Search\/(\w+)/,
            url = window.location.href;
        return regex.exec(url)[1];
    },

    GetParameterByName: function (name, url) {
        if (!url) url = window.location.href;

        name = name.replace(/[\[\]]/g, "\\$&");

        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);

        if (!results) return null;

        if (!results[2]) return '';

        return decodeURIComponent(results[2].replace(/\+/g, " "));
    },

    Filter: function (id, Title, Category) {

        this.id = id;
        this.Title = Title;
        this.Category = Category;

        this.Count = ko.computed(function () {

            var selectedFilters = [];//push them into a new array so we do not modify the original.
            ko.utils.arrayForEach(window.vm.SelectedFilters(), function (filter) { selectedFilters.push(filter); });

            //OR Group
            var selectedCategories = data.Search.GetSelectedFiltersFromGroup(window.vm.CategoryFilters, selectedFilters);
            var selectedBusinessUnits = data.Search.GetSelectedFiltersFromGroup(window.vm.BusinessUnitFilters, selectedFilters);
            var selectedFunctions = data.Search.GetSelectedFiltersFromGroup(window.vm.DatasetFunctionFilters, selectedFilters);
            var selectedExtensions = data.Search.GetSelectedFiltersFromGroup(window.vm.ExtensionFilters, selectedFilters);

            var items = [];

            if (Category === 'Category') {
                //filter datasets for the count
                var datasetsToLoop = filterForCount(selectedFilters, selectedCategories);
                items = ko.utils.arrayFilter(datasetsToLoop, function (dataset) {
                    if (dataset.Categories.includes(Title)) { return dataset; }
                });
            }
            else if (Category === 'Extension' || Category === "Report Type") {
                //filter datasets for the count
                var datasetsToLoop = filterForCount(selectedFilters, selectedExtensions);
                items = ko.utils.arrayFilter(datasetsToLoop, function (dataset) {
                    if (dataset.DistinctFileExtensions.includes(Title)) { return dataset; }
                });
            }
            else if (Category === 'Business Unit') {
                //filter datasets for the count
                var datasetsToLoop = filterForCount(selectedFilters, selectedBusinessUnits);
                items = ko.utils.arrayFilter(datasetsToLoop, function (dataset) {
                    if (dataset.BusinessUnits.includes(Title)) { return dataset; }
                });
            }
            else if (Category === 'Function') {
                //filter datasets for the count
                var datasetsToLoop = filterForCount(selectedFilters, selectedFunctions);
                items = ko.utils.arrayFilter(datasetsToLoop, function (dataset) {
                    if (dataset.DatasetFunctions.includes(Title)) { return dataset; }
                });
            }
            else {
                //This is going to be all teh Tags within different groups.  All of them are AND'd together and should filter off the searchResults.
                //filter datasets for the count
                var datasetsToLoop = window.vm.searchResults();
                // all AND groups here
                items = ko.utils.arrayFilter(datasetsToLoop, function (dataset) {
                    if (dataset.TagNames.includes(Title)) { return dataset; }
                });
            }

            return items.length;
        });

    },

    GetSelectedFiltersFromGroup: function (allFiltersFromGroup, mySelectedFilters) {

        if (allFiltersFromGroup().length === 0) {
            return allFiltersFromGroup();
        }

        var filters = ko.utils.arrayFilter(allFiltersFromGroup(), function (feature) {
            for (var i = 0; i < mySelectedFilters.length; i++) {
                if (feature.id === mySelectedFilters[i]) {
                    return true;
                }
            }
            return false;
        });

        return filters;

    },

    SortTheResults: function (sortVal, items, alphabetical, favorites, mostAccessed, recentlyAdded, recentlyUpdated) {

        switch (parseInt(sortVal)) {
            case alphabetical:
                return items.sort(function (left, right) {
                    return left.DatasetName === right.DatasetName ? 0 : (left.DatasetName < right.DatasetName ? -1 : 1);
                });

            case favorites:
                return items.sort(function (left, right) {
                    return right.IsFavorite - left.IsFavorite;
                });

            case mostAccessed:
                return items.sort(function (left, right) {
                    return right.PageViews - left.PageViews;
                });

            case recentlyAdded:
                return items.sort(function (left, right) {
                    return new Date(right.CreatedDtm) - new Date(left.CreatedDtm);
                });

            case recentlyUpdated:
                return items.sort(function (left, right) {
                    return new Date(right.ChangedDtm) - new Date(left.ChangedDtm);
                });

            default:
                return items;
        }
    },

    SetInitialDisplay: function () {

        $('#dataColumn').show();
        $('#filter-by-label').show();
        $('#filterColumn').show();
        $('.sentry-spinner-container').remove();
        $(".select2-container--default").css('width', '100%');

        // have the first filter section expanded by default
        $("#filterColumn .panel:first-child .filterViewIcon").removeClass("fa-chevron-down").addClass("fa-chevron-up");
        $("#filterColumn .panel:first-child .dataset-list-filter-category").show();

        // set the height of the filters <div> dynamically
        $("#search-pg-filters").height(
            $(window).height() - $(".sentry-navbar").height() - 30
        );

    },

    FilterCategory: function (Name, Filters, Sequence, ShowMore) {

        var self = this;

        self.Name = Name;
        self.Sequence = Sequence;
        self.ShowMore = ShowMore;

        self.Filters = ko.observableArray(Filters);

        self.HiddenId = 'filterMore_' + Name.replace(' ', '_');
        self.HiddenMore = 'hidden_filterMore_' + Name.replace(' ', '_');
        self.IconMore = 'icon_filterMore_' + Name.replace(' ', '_');
        self.TxtMore = 'txt_filterMore_' + Name.replace(' ', '_');

        self.HeadId = 'filterType_' + Name.replace(' ', '_');
        self.HeadIcon = 'icon_filterType_' + Name.replace(' ', '_');
        self.HeadHide = 'hide_filterType_' + Name.replace(' ', '_');

        self.OrderedFilters = ko.computed(function () {

            var list = ko.utils.arrayFilter(self.Filters(), function (prod) {
                return prod;
            });

            //Sort the list alphabetically and show them to the user.
            return list.sort(function (left, right) {
                return left.Title === right.Title ? 0 : (left.Title < right.Title ? -1 : 1)
            });
        });

    },

    setPageNumber: function (targetpage, initload) {
        //There is a delay in all pages showing up, therefore, we need a loop
        var pages = vm.searchResults.pageCount();

        //On initial load, wait 500 ms and then call setPageNumber function again
        if (pages < targetpage && initload === true) {
            //setTimeout(data.Search.setPageNumber, 500, targetpage);
            setTimeout(function() {
                data.Search.setPageNumber(targetpage, false);
            }, 500)
        }
        //On second call of function, set page number to 1 if targetpage is greater than pages.
        else if (pages < targetpage && initload === false) {
            vm.searchResults.pageNumber(1);
            localStorage.setItem("pageSelection", 1);
        }
        else {
            vm.searchResults.pageNumber(targetpage);
        }
    }
    
};