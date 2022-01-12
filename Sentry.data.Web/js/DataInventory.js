data.DataInventory = {
    init: function () {

        var categoryOptions = [];

        categoryOptions.push({
            id: 1,
            Title: "Prod",
            Count: () => 3
        });

        categoryOptions.push({
            id: 2,
            Title: "NonProd",
            Count: () => 10
        });

        var category = {
            Name: "Sensitivity",
            Sequence: 1,
            ShowMore: true,
            Filters: categoryOptions,
            HiddenId: 'filterMore_Sensitivity',
            HiddenMore: 'hidden_filterMore_Sensitivity',
            IconMore: 'icon_filterMore_Sensitivity',
            TxtMore: 'txt_filterMore_Sensitivity',
            HeadId: 'filterType_Sensitivity',
            HeadIcon: 'icon_filterType_Sensitivity',
            HeadHide: 'hide_filterType_Sensitivity',
            OrderedFilters: () => categoryOptions
        };

        var category2 = {
            Name: "Environment",
            Sequence: 2,
            ShowMore: true,
            Filters: categoryOptions,
            HiddenId: 'filterMore_Environment',
            HiddenMore: 'hidden_filterMore_Environment',
            IconMore: 'icon_filterMore_Environment',
            TxtMore: 'txt_filterMore_Environment',
            HeadId: 'filterType_Environment',
            HeadIcon: 'icon_filterType_Environment',
            HeadHide: 'hide_filterType_Environment',
            OrderedFilters: () => categoryOptions
        };
        
        $('#dataColumn').show();
        $('#filter-by-label').show();
        $('#filterColumn').show();
        $(".select2-container--default").css('width', '100%');

        // have the first filter section expanded by default
        $("#filterColumn .panel:first-child .filterViewIcon").removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
        $("#filterColumn .panel:first-child .dataset-list-filter-category").show();

        // set the height of the filters <div> dynamically
        //$("#search-pg-filters").height(
        //    $(window).height() - $(".sentry-navbar").height() - 30
        //);

        window.vm = {
            AllFilters: [category, category2],
            SelectedFilters: []
        };

        ko.applyBindings(vm);

        $(document).on("click", "[id^='filterType_']", function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var category = "#hide_" + id
            var icon = "#icon_" + id;

            $(category).slideToggle();
            $(icon).toggleClass("glyphicon-chevron-down glyphicon-chevron-up");
        });

        $(document).on("click", "[id^='filterMore_']", function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var show = "#hidden_" + id
            var icon = "#icon_" + id;
            var txt = "#txt_" + id;

            $(show).slideToggle();
            $(icon).toggleClass("glyphicon-plus-sign glyphicon-minus-sign");

            if ($(txt).text() === "Show Less") {
                $(txt).text("Show More");
            }
            else {
                $(txt).text("Show Less");
            }
        });
    }
}