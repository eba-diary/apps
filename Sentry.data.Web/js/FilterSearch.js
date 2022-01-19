data.FilterSearch = {

    initialFilters: [],

    init: function (selectedFilters) {

        this.initialFilters = selectedFilters;

        this.initEvents();
        this.initDataTable();
        this.initUserInterface();
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

        //Apply filters (ie. Search)
        $(document).on("click", "#filter-search-apply", function (e) {
            e.preventDefault();

            console.log('submitted filters');
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
            hasSameValues = data.FilterSearch.initialFilters.some(i => this.id === i.OptionId);
            return hasSameValues;
        });

        //hide apply button if filters are same as initial search, show if filters are different
        if (selectedOptions.length === data.FilterSearch.initialFilters.length && hasSameValues) {
            $("#filter-search-apply").hide();
        }
        else {
            $("#filter-search-apply").show();
        }
    },

    initUserInterface: function () {

        var selectedOptions = $('.filter-search-category-option-checkbox:checkbox:checked');

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

        $("#daleContainer").show();
        $(".filter-search-container").show();
    },

    initDataTable: function () {
        $.ajax({
            url: "/Dale/GetCanDaleSensitive/",
            method: "GET",
            dataType: 'json',
            success: function (obj) {
                $("#daleResultsTable").DataTable({
                    //client side setup
                    pageLength: 25,
                    //ON table creation or refresh this AJAX code is called to fill grid
                    ajax: {
                        url: "/Dale/GetSearchResultsClient/",
                        type: "GET",
                        data: function (d) {
                            d.searchCriteria = ""
                            d.destination = "";
                            d.asset = "";
                            d.server = "";
                            d.database = "";
                            d.daleObject = "";
                            d.objectType = "";
                            d.column = "";
                            d.sourceType = "";
                            d.sensitive = true;
                        }
                    },
                    columns: [
                        {
                            data: null, className: "Asset", render: function (data) {
                                //the following render func is called for every single row column and passes in data as the specific value.  our func body below will insert our list of assets as links
                                if (data.AssetList != null) {

                                    var len = data.AssetList.length;
                                    var assetHtml = '';

                                    if (len > 0) {
                                        for (let i = 0; i < len; i++) {

                                            var ri = data.AssetList[i];
                                            var aTag = '<a target="_blank" rel="noopener noreferrer" href=https://said.sentry.com/ViewAsset.aspx?ID=' + ri + '\>' + ri + '</a>';
                                            assetHtml = assetHtml + ' ' + aTag;
                                        }
                                    }
                                }
                                return assetHtml;
                            }
                        },
                        { data: "Server", className: "Server" },
                        { data: "Database", className: "Database", width: "15%" },
                        { data: "Object", className: "Object", width: "15%" },
                        { data: "ObjectType", className: "ObjectType" },
                        { data: "Column", className: "ColumnMan" },
                        //ISSENSITIVE CHECKBOX
                        //the key piece here is including a label with text to indicate whether IsSensitive column is true or false so the filtering works
                        //Since I did not want user to see label text and still have a filter.  My cheat to this was to style label with display:none while still keeping the filtering ability
                        //later on when they check/uncheck the box my editRow() function will refresh the data associated with the grid which changes the label hidden text to the opposite so filtering can refresh
                        { data: null, className: "IsSensitive", visible: false, render: (d) => data.FilterSearch.getTableElementCheckbox(!obj.canDaleSensitiveEdit || (obj.canDaleSensitiveEdit && !obj.canDaleOwnerVerifiedEdit && d.IsOwnerVerified) || !obj.CLA3707_UsingSQLSource, d.IsSensitive) },
                        //OWNER VERIFIED CHECKBOX
                        { data: null, className: "IsOwnerVerified", visible: false, render: (d) => data.FilterSearch.getTableElementCheckbox(!obj.canDaleOwnerVerifiedEdit, d.IsOwnerVerified) },
                        { data: "ProdType", className: "ProdType", visible: false },
                        { data: "ColumnType", className: "ColumnType", visible: false },
                        { data: "MaxLength", className: "MaxLength", visible: false },
                        { data: "Precision", className: "Precision", visible: false },
                        { data: "Scale", className: "Scale", visible: false },
                        { data: "IsNullable", className: "IsNullable", visible: false },
                        { data: "EffectiveDate", className: "EffectiveDate", visible: false },
                        { data: "SourceType", className: "SourceType", visible: false },
                        { data: "ScanCategory", className: "ScanCategory", width: "5%", visible: false },
                        { data: "ScanType", className: "ScanType", width: "20%", visible: false }
                    ],
                    aLengthMenu: [10, 25, 100, 500],
                    order: [],
                    dom: "<'row'<'col-xs-12'i>>" +
                        "<'row'<'col-xs-6'l><'col-xs-6 text-right'B>>" +
                        "<'row'<'col-xs-12'tr>>" +
                        "<'row'<'col-xs-12 text-center'p>>",
                    buttons: [{ extend: 'colvis', text: 'Columns' }, { extend: 'csv', text: 'Download' }]
                });

                $("#daleResultsTable").dataTable().columnFilter({
                    sPlaceHolder: "head:after",
                    aoColumns: [
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" },
                        { type: "text" }
                    ]
                });
            }
        });        
    },

    getTableElementCheckbox: function (isDisabled, isChecked) {
        var disabled = '';
        var checked = '';
        var cellValue = 'false';

        if (isDisabled) {
            disabled = 'disabled ';
        }

        if (isChecked) {
            checked = 'checked ';
            cellValue = 'true';
        }

        return '<input type="checkbox" value="true" class="table-element-checkbox" ' + checked + disabled + '><label class="display-none">' + cellValue + '</label>';
    }
}