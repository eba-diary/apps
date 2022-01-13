data.DataInventory = {
    init: function () {

        var viewModel = {
            FilterCategories: this.initFilters()
        }

        ko.applyBindings(viewModel);

        this.initEvents();
        this.initDataTable();
        this.initActiveFilters();
        this.initUserInterface();
    },

    initFilters: function() {
        var sensitiveOptions = [];

        sensitiveOptions.push({
            id: 1,
            Title: "Sensitive",
            Count: 3,
            DefaultSelected: false
        });

        sensitiveOptions.push({
            id: 2,
            Title: "Public",
            Count: 10,
            DefaultSelected: false
        });

        var category = {
            Name: "Sensitivity",
            Sequence: 1,
            FilterCategoryOptions: sensitiveOptions,
            HiddenId: 'filterMore_Sensitivity',
            HiddenMore: 'hidden_filterMore_Sensitivity',
            IconMore: 'icon_filterMore_Sensitivity',
            TxtMore: 'txt_filterMore_Sensitivity',
            HeadId: 'filterType_Sensitivity',
            HeadIcon: 'icon_filterType_Sensitivity',
            HeadHide: 'hide_filterType_Sensitivity'
        };

        var environmentOptions = [];

        environmentOptions.push({
            id: 1,
            Title: "Prod",
            Count: 3,
            DefaultSelected: true
        });

        environmentOptions.push({
            id: 2,
            Title: "NonProd",
            Count: 10,
            DefaultSelected: false
        });

        var category2 = {
            Name: "Environment",
            Sequence: 2,
            FilterCategoryOptions: environmentOptions,
            HiddenId: 'filterMore_Environment',
            HiddenMore: 'hidden_filterMore_Environment',
            IconMore: 'icon_filterMore_Environment',
            TxtMore: 'txt_filterMore_Environment',
            HeadId: 'filterType_Environment',
            HeadIcon: 'icon_filterType_Environment',
            HeadHide: 'hide_filterType_Environment'
        };

        return [category, category2];
    },

    initEvents: function () {
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
    },

    initUserInterface: function () {
        //open all filters with a filter checked
        $('.filterChbx:checkbox:checked').closest('.filters-category').each(function () {
            $('#' + this.id.replace("hide", "icon")).removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
            $(this).show();
        });

        $("#di-result-spinner").hide();
        $("#daleContainer").show();
        $("#filterColumn").show();
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
                        { data: null, className: "IsSensitive", visible: false, render: (d) => data.DataInventory.getTableElementCheckbox(!obj.canDaleSensitiveEdit || (obj.canDaleSensitiveEdit && !obj.canDaleOwnerVerifiedEdit && d.IsOwnerVerified) || !obj.CLA3707_UsingSQLSource, d.IsSensitive) },
                        //OWNER VERIFIED CHECKBOX
                        { data: null, className: "IsOwnerVerified", visible: false, render: (d) => data.DataInventory.getTableElementCheckbox(!obj.canDaleOwnerVerifiedEdit, d.IsOwnerVerified) },
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
            disabled = 'disabled="disabled" ';
        }

        if (isChecked) {
            checked = 'checked="checked" ';
            cellValue = 'true';
        }

        return '<input type="checkbox" value="true" class="table-element-checkbox" ' + checked + disabled + '><label class="display-none">' + cellValue + '</label>';
    },

    initActiveFilters: function () {
        $("#filterSelector").select2({
            selectOnClose: false,
            closeOnSelect: false,
            templateSelection: formatTag,
            placeholder: "There are no active filters.  Pick an option to the left or click here to begin filtering.",
        });

        //This is fed into the Select2 Box to format tags.
        function formatTag(tag) {
            var cat = "";

            for (var i = 0; i < vm.FilterCategories.length; i++) {
                for (var j = 0; j < vm.FilterCategories[i].FilterCategoryOptions.length; j++) {
                    if (vm.FilterCategories[i].FilterCategoryOptions[j].id === tag.id) {
                        cat = vm.AllFilters()[i].Filters()[j].Category;
                        break;
                    }
                }
                if (cat !== "") {
                    break;
                }
            }

            var $state = $(
                '<span class="' + cat.split(" ").join("_").toLowerCase() + '_filter">' + cat + " : " + tag.text + '</span>'
            );

            return $state;
        };


        $('#filterSelector').on('select2:select', function (e) {

            var data = e.params.data;
            console.log(data);

            // push the selected filter Id to the observable array
            //window.vm.SelectedFilters.push(data.id);
        });

        $('#filterSelector').on('select2:unselect', function (e) {

            var data = e.params.data;
            console.log(data);

            // remove the selected filter Id from the observable array
            //window.vm.RemoveSelection(data.id);
        });
    }
}