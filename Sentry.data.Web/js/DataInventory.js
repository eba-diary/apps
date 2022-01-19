data.DataInventory = {

    init: function () {
        this.initDataTable();
        this.initEvents();
    },

    initEvents: function () {
        $("#filter-search-text").keypress(function (e) {
            e.preventDefault();

            var keycode = (e.keyCode ? e.keyCode : e.which);

            if (keycode == '13') {
                data.DataInventory.executeSearch();
            }
        });

        $(".filter-search-start").on("click", function (e) {
            e.preventDefault();
            data.DataInventory.executeSearch();            
        });
    },

    executeSearch: function () {

        data.FilterSearch.startSearch();

        console.log('searching...');

        setTimeout(function () {
            console.log('complete');
            data.FilterSearch.completeSearch();
        }, 5000);
        //$("#di-result-table").DataTable().ajax.reload();
    },

    initDataTable: function () {
        $.ajax({
            url: "/DataInventory/GetCanDaleSensitive/",
            method: "GET",
            dataType: 'json',
            success: function (obj) {
                $("#di-result-table").DataTable({
                    pageLength: 20,
                    ordering: false,
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
                        { data: "Database", className: "Database" },
                        { data: "Object", className: "Object" },
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
                        { data: "ScanCategory", className: "ScanCategory", visible: false },
                        { data: "ScanType", className: "ScanType", visible: false }
                    ],
                    aLengthMenu: [10, 20, 50, 100, 500],
                    dom: "<'row'<'col-xs-12'i>>" +
                        "<'row'<'col-xs-6'l><'col-xs-6 text-right'B>>" +
                        "<'row'<'col-xs-12'tr>>" +
                        "<'row'<'col-xs-12 text-center'p>>",
                    buttons: [{ extend: 'colvis', text: 'Columns' }, { extend: 'csv', text: 'Download' }],
                    initComplete: data.FilterSearch.completeSearch
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