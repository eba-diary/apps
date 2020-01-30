data.DataFlow = {
    DataFlow_DatasetSubmitInit: function () {

        SubmitFunciton = function () {

        }

    },

    DataFlowFormInit: function () {

        data.DataFlow.InitIngestionType();
        //data.DataFlow.InitCompressionCheckbox();

        $("#IsCompressed").change(function () {
            if ($(this).val() === "true") {
                $('.compressionJobPanel').show();
                if ($('.compressionJobQuestion').length === 0) {
                    $.get("/DataFlow/NewCompressionJob", function (e) {
                        $("#compressionJobPanel").append(e);
                    });
                }
            }
            else {
                $('.compressionJobPanel').hide();
            }
        });

        $("#btnAddSchemaMap").on('click', function () {
            $.get("/DataFlow/NewSchemaMap", function (e) {
                $(e).insertBefore($("#btnAddSchemaMap"));
                data.DataFlow.InitSchemaMaps();
            });
        });

        data.DataFlow.InitSchemaMaps();
    },

    InitIngestionType() {
        var selection = $("[id$=IngestionType]").val();

        if (selection === "2") {
            $('.retrieverPanel').show();
            $('.compressionPanel').show();
            $('.schemaMapPanel').show();
            $('.formSubmitButtons').show();
        }
        else if (selection === "1") {
            $('.retrieverPanel').hide();
            $('.compressionPanel').show();
            $('.schemaMapPanel').show();
            $('.formSubmitButtons').show();
        }

        if (selection === "0") {
            $('.compressionPanel').hide();
            $('.schemaMapPanel').hide();
            $('.formSubmitButtons').hide();
        }
        else {
            $('.compressionPanel').show();
            $('.schemaMapPanel').show();
            $('.formSubmitButtons').show();
        }            

        $("[id$=IngestionType]").on('change', function () {
            if ($(this).val() === "2") {
                $('.retrieverPanel').show();
                Sentry.InjectSpinner($("#retrieverJobPanel"));
                $.get("/DataFlow/NewRetrieverJob", function (e) {
                    $("#retrieverJobPanel").replaceWith(e);
                    data.Job.FormInit();
                });
            }
            else {
                $('.retrieverPanel').hide();
            }
            $('.compressionPanel').show();
            $('.schemaMapPanel').show();
            $('.formSubmitButtons').show();
        });
    },
        
    RenderDatasetCreatePage() {
        $.get("/Dataset/_DatasetCreateEdit", function (result) {
            $('#DatasetFormContent').html(result);
            $('#DatasetFormContainer').show();
            var hrEnv = $('#HrempServiceEnv').val()
            var hrUrl = $('#HrempServiceUrl').val()
            data.Dataset.FormInit(hrUrl, hrEnv, data.DataFlow.DatasetFormSubmitInit, data.DataFlow.DatasetFormCancelInit);
        });
    },

    RenderSchemaCreatePage(datasetId) {
        $.get("/Config/_DatasetFileConfigCreate/" + encodeURIComponent(datasetId), function (result) {
            $('#DatasetFileConfigFormContent').html(result);
            $('#DatasetFileConfigFormContainer').show();
            data.Config.CreateInit(data.DataFlow.DatasetFileConfigFormSubmitInit, data.DataFlow.DatasetFileConfigFormCancelInit);
        });
    },

    DatasetFormCancelInit: function () {
        $('#DataFlowFormContainer').show();
        $('#DatasetFormContainer').hide();
        data.DataFlow.InitSchemaMaps("0", null);
    },

    DatasetFormSubmitInit: function () {
        $.ajax({
            url: "/Dataset/DatasetForm",
            method: "POST",
            data: $("#DatasetForm").serialize(),
            dataType: 'json',
            success: function (obj) {
                if (Sentry.WasAjaxSuccessful(obj)) {
                    //show dataflow form and hide dataset create form
                    $('#DataFlowFormContainer').show();
                    $('#DatasetFormContainer').hide();

                    data.DataFlow.InitSchemaMaps(obj.dataset_id, null);
                }
                else {
                    $('#DatasetFormContent').replaceWith(obj);
                }
            },
            failure: function () {
            },
            error: function (obj) {
                $('#DatasetFormContent').replaceWith(obj.responseText);
                var hrEnv = $('#HrempServiceEnv').val()
                var hrUrl = $('#HrempServiceUrl').val()
                //init the form passing the submit function specific for DataFlow page
                data.Dataset.FormInit(hrUrl, hrEnv, data.DataFlow.DatasetFormSubmitInit, data.DataFlow.DatasetFormCancelInit);
                alert('finished error catch')
            }
        });
    },

    DatasetFileConfigFormSubmitInit: function () {
        $.ajax({
            url: "/Config/DatasetFileConfigForm",
            method: "POST",
            data: $("#DatasetFileConfigForm").serialize(),
            dataType: 'json',
            success: function (obj) {
                if (Sentry.WasAjaxSuccessful(obj)) {
                    //show dataflow form and hide dataset create form
                    $('#DatasetFileConfigFormContainer').hide();
                    $('#DataFlowFormContainer').show();

                    data.DataFlow.InitSchemaMaps(obj.dataset_id, obj.schema_id);
                }
                else {
                    $('#DatasetFileConfigFormContent').replaceWith(obj);
                }
            },
            failure: function () {
            },
            error: function (obj) {
                $('#DatasetFileConfigFormContent').replaceWith(obj.responseText);
                //init the form passing the submit function specific for DataFlow page
                data.Config.CreateFormSubmitInit(data.DataFlow.DatasetFileConfigFormSubmitInit, data.DataFlow.DatasetFileConfigFormCancelInit);
                alert('finished error catch')
            }
        });
    },

    DatasetFileConfigFormCancelInit: function () {
        $('#DataFlowFormContainer').show();
        $('#DatasetFileConfigFormContainer').hide();
        data.DataFlow.InitSchemaMaps("0");
    },

    PopulateSchemas(datasetId, schemaId, targetElement) {
        if (datasetId !== null && datasetId !== "-1" && datasetId !== "0") {
            var curVal = targetElement.val();
            $.getJSON("/api/v2/metadata/dataset/" + datasetId + "/schema", function (result) {
                var subItems;
                var sortedResults = result.sort(
                    firstBy("Name")
                );

                // Add initial value
                subItems += "<option value='-1'>Create Schema</option>";
                subItems += "<option value='0'>Select Schema</option>";

                $.each(sortedResults, function (index, item) {
                    subItems += "<option value='" + item.SchemaId + "'>" + item.Name + "</option>";
                });

                targetElement.html(subItems);

                if (curVal === null || curVal === "0") {
                    targetElement.val("0");
                }
                else if (schemaId !== null || curVal === "-1") {
                    targetElement.val(schemaId)
                }
                else {
                    targetElement.val(curVal);
                }
            });
        }
        else {
            var subItems;
            subItems += "<option value='0'>Select Dataset First</option>";
            targetElement.html(subItems);
        }

        $('[id$=__SelectedSchema]').change(function () {
            var schemaId = $(this).val();
            var curRow = $(this).parent().parent();
            var datasetSelectionDropDown = curRow.find("[id$=__SelectedDataset]");
            var datasetId = datasetSelectionDropDown.val();

            //if Create New Dataset Selected
            if (schemaId === "-1") {
                $('#DataFlowFormContainer').hide();
                data.DataFlow.RenderSchemaCreatePage(datasetId);
            }
        });
    },

    InitSchemaMaps(datasetId, schemaId) {
        $.getJSON("/api/v2/metadata/dataset", function (result) {
            var newSubItems;
            var groupName;
            var datasetCount = result.length;
            var sortedResult = result.sort(
                firstBy("Category")
                    .thenBy("Name")
            );

            newSubItems += "<option value='-1'>Create Dataset</option>";
            newSubItems += "<option value='0'>Select Dataset</option>";

            $.each(sortedResult, function (index, item) {
                //initial pass inializes group and sets first group element
                if (groupName === null) {
                    newSubItems += "<optgroup label='" + item.Category + "'>";
                    groupName = item.Category;
                }

                //Close previous group and start new group if groupName changes
                if (groupName !== null && groupName !== item.Category) {
                    newSubItems += "</optgroup>";
                    newSubItems += "<optgroup label='" + item.Category + "'>";
                    groupName = item.Category;
                }

                //Add option item
                newSubItems += "<option value='" + item.Id + "'>" + item.Name + "</option>";

                //close out group after last interation
                if (index === (datasetCount - 1)) {
                    newSubItems += "</optgroup>";
                }
            });

            $('[id$=__SelectedDataset]').each(function (index) {
                var cur = $(this);
                var curVal = cur.val();
                cur.html(newSubItems);

                if (curVal === null || curVal === undefined) {
                    var curRow = cur.parent().parent();
                    $(this).val(0);
                    data.DataFlow.PopulateSchemas("0", schemaId, curRow.find("[id$=__SelectedSchema]"));
                }
                else if (curVal == "-1") {
                    var curRow = cur.parent().parent();
                    $(this).val(datasetId);
                    data.DataFlow.PopulateSchemas(datasetId, schemaId, curRow.find("[id$=__SelectedSchema]"));
                }
                else {
                    cur.val(curVal);
                    var curRow = cur.parent().parent();
                    data.DataFlow.PopulateSchemas(curVal, schemaId, curRow.find("[id$=__SelectedSchema]"));
                }
            });


            $('[id$=__SelectedDataset]').change(function () {
                var curRow = $(this).parent().parent();
                var schemaSelectionDropDown = curRow.find("[id$=__SelectedSchema]");
                var datasetId = $(this).val();

                //if Create New Dataset Selected
                if (datasetId === "-1") {
                    $('#DataFlowFormContainer').hide();
                    data.DataFlow.RenderDatasetCreatePage();
                }
                else {
                    data.DataFlow.PopulateSchemas(datasetId, null, schemaSelectionDropDown);
                }

            });

            $('[id$=_SelectedSchema]').change(function () {
                var schemaId = $(this).val();

                //If create new schema is selected
                if (schemaId === "-1") {
                    $('#DataFlowFormContainer').hide();
                    data.DataFlow.RenderSchemaCreatePage();
                }
            })
        });
    }
}