function Dataset(id, name, data) {
    this.id = ko.observable(id);
    this.name = ko.observable(name);
    this.Configs = ko.observableArray($.map(data.Configs, function (item) { return new Config(id, item); }));
    this.DatasetCategory = ko.observable(data.datasetCategory);

    this.DatasetColor = ko.observable(data.datasetColor);

    this.BannerColor = ko.computed(function () {
        return 'categoryBanner-' + data.datasetColor;
    });

    this.BorderColor = ko.computed(function () {
        return 'borderSide_' + data.datasetColor;
    });

    this.url = ko.computed(function () {

        return 'Detail\\' + id;
    });

    this.htmlId = ko.computed(function () {

        return 'dataset' + id;
    });


}

function Config(id, data) {

    this.id = ko.observable(id);

    this.configName = ko.observable(data.configName);
    this.bucket = ko.observable(data.bucket);
    this.s3Key = ko.observable(data.s3Key);
    this.description = ko.observable(data.description);
    this.extensions = ko.observableArray(data.extensions);
    this.primaryFileId = ko.observable(data.primaryFileId);
    this.fileCount = ko.observable(data.fileCount);
    this.hasSchema = ko.observable(data.HasSchema);
    this.IsGeneric = ko.observable(data.IsGeneric);
    this.IsPowerUser = ko.observable(data.IsPowerUser);
    this.Schemas = ko.observableArray($.map(data.Schemas, function (item) { return new Schema(id, item); }));
    this.schemaOverride = ko.observable(false);
    this.hasQueryableSchema = ko.observable(data.HasQueryableSchema);

    this.tableName = ko.computed(function () {
        if (data.configName === 'Default') {
            return data.configName.replace(/ /g, "_") + '_' + id;
        } else {
            return data.configName.replace(/ /g, "_");
        }
    });

    this.ModalPopup = function () {
        console.log(data.primaryFileId)
        data.DatasetDetail.PreviewDatafileModal(data.primaryFileId);
    }

    this.hasFiles = ko.computed(function () {
        console.log(data.fileCount);

        if (data.fileCount > 0) {
            return true
        } else {
            return false;
        }
    });

    this.rowsClass = ko.computed(function () {

        if (data.fileCount > 0 && (!data.HasQueryableSchema || this.schemaOverride)) {
            var classes = {
                'configRow': true,
                'schemaRow': false
            };
            console.log("update classes: " + classes)
            return classes;
        }
        else if (data.fileCount > 0 && data.HasQueryableSchema) {
            var classes = {
                'configRow': false,
                'schemaRow': true
            };
            console.log("update classes: " + classes)
            return classes;
        }
        else {
            var classes = {
                'configRow': false,
                'schemaRow': false
            };
            console.log("update classes: " + classes)
            return classes;
        }
    });
}

function Schema(id, data) {
    this.id = ko.observable(id);
    this.schemaName = ko.observable(data.SchemaName);
    this.schemaDSC = ko.observable(data.SchemaDSC);
    this.schemaID = ko.observable(data.SchemaID);
    this.revisionID = ko.observable(data.RevisionID);
    this.hiveDatabase = ko.observable(data.HiveDatabase);
    this.hiveTable = ko.observable(data.HiveTable);
    this.hasTable = ko.observable(data.HasTable);
}

function ViewModel() {
    var self = this;

    self.Datasets = ko.observableArray();

    self.Columns = ko.observableArray();
    self.SelectedColumns = ko.observableArray();

    self.removeColumn = function (Name) {
        console.log(Name);
        self.SelectedColumns.remove(Name);
        self.SelectedColumns.notifySubscribers();
        $('#columnRenamerSelect').trigger('change');
    }

    self.schemaOverrideClick = function () {
        console.log("hit view model click event");
    }

    //$('.schemaOverrideCheckbox').onclick = function () {
        
    //    self.Datasets.notifySubscribers();
    //};

    $('#datasetList').on('select2:select', function (e) {
        $('#tablePanel').show();

        var data = e.params.data;

        var controllerURL = "/api/QueryTool/GetS3Key?datasetID=" + encodeURI(data.id);
        $.get(controllerURL, function (result) {

            console.log(result);
            var item = new Dataset(data.id, data.text, result);

            self.Datasets().push(item);
            console.log(self.Datasets()[0].Configs()[0]);
            self.Datasets.notifySubscribers();
            $('[data-toggle="tooltip"]').tooltip();

            updateTables();
        });
    });


    $('#datasetList').on('select2:unselect', function (e) {
        // Do something
        var data = e.params.data;

        self.Datasets.remove(function (item) { return  item.id() === data.id; });

        self.Datasets.notifySubscribers();
    });    
};

ko.bindingHandlers.select2 = {
    init: function (el, valueAccessor, allBindingsAccessor, viewModel) {
        ko.utils.domNodeDisposal.addDisposeCallback(el, function () {
            $(el).select2('destroy');
        });

        var allBindings = allBindingsAccessor(),
            select2 = ko.utils.unwrapObservable(allBindings.select2);

        $(el).select2(select2);
    },
    update: function (el, valueAccessor, allBindingsAccessor, viewModel) {
        var allBindings = allBindingsAccessor();

        console.log(allBindings);

        if ("value" in allBindings) {
            if ((allBindings.select2.multiple || el.multiple) && allBindings.value().constructor !== Array) {
                $(el).val(allBindings.value().split(',')).trigger('change');
            }
            else {
                $(el).val(allBindings.value()).trigger('change');
            }
        } else if ("selectedOptions" in allBindings) {
            var converted = [];
            var textAccessor = function (value) { return value; };
            if ("optionsText" in allBindings) {
                textAccessor = function (value) {
                    var valueAccessor = function (item) { return item; }
                    if ("optionsValue" in allBindings) {
                        valueAccessor = function (item) { return item[allBindings.optionsValue]; }
                    }
                    var items = $.grep(allBindings.options(), function (e) { return valueAccessor(e) === value });
                    if (items.length === 0 || items.length > 1) {
                        return "UNKNOWN";
                    }
                    return items[0][allBindings.optionsText];
                }
            }
            $.each(allBindings.selectedOptions(), function (key, value) {
                converted.push({ id: value, text: textAccessor(value) });
            });
            $(el).select2("data", converted);
        }
        $(el).trigger("change");
    }
};
//control visibility, give element focus, and select the contents (in order)
ko.bindingHandlers.visibleAndSelect = {
    update: function (element, valueAccessor) {
        ko.bindingHandlers.visible.update(element, valueAccessor);
        if (valueAccessor()) {
            setTimeout(function () {
                $(element).find("input").focus().select();
            }, 0); //new tasks are not in DOM yet
        }
    }
};

function WriteSQLStatement() {

    var rawQuery = "'SELECT ";

    if ($('#distinctChk').is(':checked')) {
        rawQuery += "DISTINCT ";
    }

    rawQuery = AggColumns(rawQuery);
    rawQuery = SelectColumns(rawQuery);
    rawQuery = FromColumns(rawQuery);

    var rules = $('#builder-basic_group_0').children('dd.rules-group-body').children('ul.rules-list').children().length;
    var sql_raw;

    if (rules !== 0) {
        sql_raw = $('#builder-basic').queryBuilder('getSQL', false, true).sql.replace(/\'/g, "\\\\'").replace(/\n/g, " ");
    }

    if (rules !== 0) {
        rawQuery += " WHERE " + sql_raw;
    }

    rawQuery = GroupByColumns(rawQuery);
    rawQuery = OrderByColumns(rawQuery);

    rawQuery += "'";

    return rawQuery;
}


function AggColumns(rawQuery) {
    var renamed = $('#columnRenamerContainer').children('.rule-container').children().length / 3;
    var aggs = $('#aggregateContainer').children('.rule-container').children().length / 4;

    for (var i = 0; i < aggs; i++) {
        var aggMethod = $('#aggregateContainer').children('.rule-container').children()[(i * 4)].value;
        var aggCol = $('#aggregateContainer').children('.rule-container').children()[(i * 4) + 1].value;
        var aggRenamed = $('#aggregateContainer').children('.rule-container').children()[(i * 4) + 2].value;


        if (aggRenamed) {
            rawQuery += aggMethod + "(" + aggCol + ") AS " + aggRenamed + ", ";
        } else {
            rawQuery += aggMethod + "(" + aggCol + ")" + ", ";
        }

    }
    return rawQuery;
}

function SelectColumns(rawQuery) {

    var renamed = $('#columnRenamerContainer').children('.rule-container').children().length / 4;

    if (!$('#selectNoneChk').is(':checked')) {
        if (renamed === 0) {
            rawQuery += ' * ';
        } else {
            for (var j = 0; j < renamed; j++) {
                var col = $('#columnRenamerContainer').children('.rule-container').children()[(j * 4) + 1].value;
                var colRenamed = $('#columnRenamerContainer').children('.rule-container').children()[(j *4) + 2].value;

                //Column was selected and Renamed

                if (colRenamed) {
                    rawQuery += col + " AS " + colRenamed;
                } else {
                    rawQuery += col;
                }

                if (j !== renamed) {
                    rawQuery += ", ";
                }
            }
        }
    }

    if (rawQuery.endsWith(', ')) {
        rawQuery = rawQuery.substr(0, rawQuery.length - 2);
    }
    console.log(rawQuery);
    return rawQuery;
}

function FromColumns(rawQuery) {
    var joinRules = $('#joinContainer').children('.rule-container').children('.ruleController').length / 6;

    rawQuery += " FROM " + firstTableName;

    for (var i = 0; i < joinRules; i++) {

        var tableOne = $('#joinContainer').children('.rule-container').children('.ruleController')[(i * 6)].value;
        var joinType = $('#joinContainer').children('.rule-container').children('.ruleController')[(i * 6) + 1].value;
        var tableTwo = $('#joinContainer').children('.rule-container').children('.ruleController')[(i * 6) + 2].value;
        var tableOneCol = $('#joinContainer').children('.rule-container').children('.ruleController')[(i * 6) + 3].value;
        var operand = $('#joinContainer').children('.rule-container').children('.ruleController')[(i * 6) + 4].value;
        var tableTwoCol = $('#joinContainer').children('.rule-container').children('.ruleController')[(i * 6) + 5].value;

        var OnStatement = tableOneCol + ' ' + operand + ' ' + tableTwoCol;

        rawQuery += " " + joinType + " JOIN " + tableTwo + " ON " + OnStatement;
    }

    return rawQuery;
}

function GroupByColumns(rawQuery) {
    var groupBys = $('.groupBySelector').length;

    for (var i = 0; i < groupBys; i++) {
        if ($($('.groupBySelector')[i]).val() !== 0) {

            if (i === 0) {
                rawQuery += " GROUP BY ";
            }

            if (i === groupBys - 1) {
                rawQuery += $($('.groupBySelector')[i]).val();
            } else {
                rawQuery += $($('.groupBySelector')[i]).val() + ", ";
            }
        }

    }

    var havings = $("#havingContainer").children('.rule-container').children().length / 5;

    for (var j = 0; j < havings; j++) {
        var agg = $('#havingContainer').children('.rule-container').children()[(j * 5)].value;
        var col = $('#havingContainer').children('.rule-container').children()[(j * 5) + 1].value;
        var operand = $('#havingContainer').children('.rule-container').children()[(j * 5) + 2].value;
        var aggValue = $('#havingContainer').children('.rule-container').children()[(j * 5) + 3].value;

        if (j === 0) {
            rawQuery += " HAVING " + agg + "(" + col + ") " + operand + " " + aggValue;
        } else {
            rawQuery += " AND " + agg + "(" + col + ") " + operand + " " + aggValue;
        }

    }

    return rawQuery;
}

function OrderByColumns(rawQuery) {
    var orderBys = $('.orderBySelector').length;

    for (var i = 0; i < orderBys; i++) {
        if ($($('.orderBySelector')[i]).val()) {

            if (i === 0) {
                rawQuery += " ORDER BY ";
            }
            var option = '#option' + $('.orderBySelector')[i].id;
            if (i === orderBys - 1) {
                rawQuery += $($('.orderBySelector')[i]).find(":selected").val() + " " + $(option).find(":selected").val();
            } else {
                rawQuery += $($('.orderBySelector')[i]).find(":selected").val() + " " + $(option).find(":selected").val() + ", ";
            }
        }

    }
    return rawQuery;
}