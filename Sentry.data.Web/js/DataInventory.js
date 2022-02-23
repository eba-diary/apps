﻿data.DataInventory = {

    updateHash: {},

    init: function () {
        this.initToast();
        this.initDataInventory();
        this.buildFilter();
        this.initEvents();
    },

    executeSearch: function () {
        $("#di-result-table").DataTable().ajax.reload(function(json) {
            var tableInfo = $("#di-result-table").DataTable().page.info();
            data.FilterSearch.completeSearch(json.searchTotal, tableInfo.length, json.data.length);
        });
    },

    buildFilter: function () {
        $.post("/DataInventory/SearchFilters/", data.DataInventory.buildRequest(), (x) => data.FilterSearch.completeFilterRetrieval(x));
    },

    initDataInventory: function () {
        $.ajax({
            url: "/DataInventory/GetCanDaleSensitive/",
            method: "GET",
            dataType: 'json',
            success: function (obj) {
                data.DataInventory.initDataTable(obj);

                if (!obj.canDaleSensitiveView) {
                    toastr['error']("All results may not be displayed. " +
                        "Additional permission is needed to view columns marked " +
                        "as sensitive. Please click the Data Inventory info icon for more information.");
                }                
            }
        });
    },

    initToast: function () {
        toastr.options = {
            "closeButton": false,
            "debug": false,
            "newestOnTop": false,
            "progressBar": false,
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

    initDataTable: function (obj) {
        $("#di-result-table").DataTable({
            pageLength: 20,
            deferRender: true,
            ordering: false,
            ajax: {
                url: "/DataInventory/SearchResult/",
                type: "POST",
                data: data.DataInventory.buildRequest
            },
            columns: [
                {
                    data: null, className: "Asset", searchable: true, render: function (data) {
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
                { data: "Server", searchable: true, className: "Server" },
                { data: "Database", className: "Database" },
                { data: "Object", className: "Object" },
                { data: "ObjectType", className: "ObjectType" },
                { data: "Column", className: "ColumnMan" },
                //ISSENSITIVE CHECKBOX
                //the key piece here is including a label with text to indicate whether IsSensitive column is true or false so the filtering works
                //Since I did not want user to see label text and still have a filter.  My cheat to this was to style label with display:none while still keeping the filtering ability
                //later on when they check/uncheck the box my editRow() function will refresh the data associated with the grid which changes the label hidden text to the opposite so filtering can refresh
                { data: null, className: "IsSensitive", visible: false, render: (d) => data.DataInventory.getTableElementCheckbox(!obj.canDaleSensitiveEdit || (obj.canDaleSensitiveEdit && !obj.canDaleOwnerVerifiedEdit && d.IsOwnerVerified), d.IsSensitive, "sensitive_" + d.BaseColumnId) },
                //OWNER VERIFIED CHECKBOX
                { data: null, className: "IsOwnerVerified", visible: false, render: (d) => data.DataInventory.getTableElementCheckbox(!obj.canDaleOwnerVerifiedEdit, d.IsOwnerVerified, "owner_" + d.BaseColumnId) },
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
            dom: "<'row'" +
                "<'col-xs-6'l>" +
                "<'col-xs-6 text-right'B>>" +
                "<'row'<'col-xs-12'tr>>" +
                "<'row'<'col-xs-12 text-center'p>>",
            buttons: [{ extend: 'colvis', text: 'Columns' }, { text: 'Save', className: 'btn btn-primary display-none di-save', action: data.DataInventory.saveUpdates }],
            initComplete: function (settings, json) {
                data.FilterSearch.completeSearch(json.searchTotal, settings.oInit.pageLength, json.data.length);
            }
        });
    },

    initEvents: function () {

        //datatable page change
        $(document).on("page.dt", "#di-result-table", data.DataInventory.updatePageInfo);

        //datatable page length change
        $(document).on("length.dt", "#di-result-table", data.DataInventory.updatePageInfo);

        //sensitive or owner verified change
        $(document).on("change", ".table-element-checkbox", function (e) {

            var baseColumnId = parseInt(this.id.split("_")[1]);
            var change = {
                IsSensitive: $("#sensitive_" + baseColumnId).is(":checked"),
                IsOwnerVerified: $("#owner_" + baseColumnId).is(":checked")
            }

            var exists = data.DataInventory.updateHash[baseColumnId];

            if (exists) {
                if (exists.original.IsSensitive == change.IsSensitive && exists.original.IsOwnerVerified == change.IsOwnerVerified) {
                    delete data.DataInventory.updateHash[baseColumnId];
                }
                else {
                    exists.updated = change;
                }
            }
            else {
                data.DataInventory.updateHash[baseColumnId] = {
                    original: {
                        IsSensitive: this.id.includes("sensitive") ? !this.checked : $("#sensitive_" + baseColumnId).is(":checked"),
                        IsOwnerVerified: this.id.includes("owner") ? !this.checked : $("#owner_" + baseColumnId).is(":checked")
                    },
                    updated: change
                };
            }

            if (Object.keys(data.DataInventory.updateHash).length > 0) {
                $(".di-save").show();
            }
            else {
                $(".di-save").hide();
            }
        });
    },

    saveUpdates: function () {
        $(".di-update-overlay").show();

        var request = { models: [] };

        for (var [key, value] of Object.entries(data.DataInventory.updateHash)) {
            request.models.push({
                BaseColumnId: key,
                IsSensitive: value.updated.IsSensitive,
                IsOwnerVerified: value.updated.IsOwnerVerified
            });
        }

        $.post("/DataInventory/Update/", request, function (x) {
            if (x.success) {
                $(".di-save").hide();
                data.DataInventory.updateHash = {};
            }
            else {
                toastr['error']("Something went wrong trying to save changes. Please try again or reach out to DSCSupport@sentry.com.");
            }

            $(".di-update-overlay").hide();
        });
    },

    updatePageInfo: function () {
        var tableInfo = $("#di-result-table").DataTable().page.info();
        data.FilterSearch.setPageInfo(++tableInfo.start, tableInfo.end);
    },

    buildRequest: function () {
        return {
            SearchText: $.trim($("#filter-search-text").val()),
            FilterCategories: data.FilterSearch.getSelectedCategoryOptions()
        }
    },

    getTableElementCheckbox: function (isDisabled, isChecked, id) {
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

        return '<input type="checkbox" value="true" class="table-element-checkbox" id="' + id + '" ' + checked + disabled + '><label class="display-none">' + cellValue + '</label>';
    }
}