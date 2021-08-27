data.DataFlow = {
    DataFlow_DatasetSubmitInit: function () {

        SubmitFunciton = function () {

        }

    },

    DataFlowFormInit: function () {

        data.DataFlow.InitIngestionType();
        //data.DataFlow.InitCompressionCheckbox();


        $("#PreprocessingOptions").select2({
            placeholder: "Select Options"
        });

        //init preprocessing panel
        if ($("#IsCompressed").val() === "true") {
            $(".compressionJobPanel").show();
        }
        else {
            $(".compressionJobPanel").hide();
        }

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

        //init preprocessing panel
        if ($("#IsPreProcessingRequired").val() === "true") {
            $(".preProcessingJobPanel").show();
        }
        else {
            $(".preProcessingJobPanel").hide();
        }
        //init preprocessing change eventS
        $("#IsPreProcessingRequired").change(function () {
            if ($(this).val() === "true") {
                $('.preProcessingJobPanel').show();
                //if ($('.compressionJobQuestion').length === 0) {
                //    $.get("/DataFlow/NewCompressionJob", function (e) {
                //        $("#compressionJobPanel").append(e);
                //    });
                //}
            }
            else {
                $('.preProcessingJobPanel').hide();
            }
        });

        $("#btnAddSchemaMap").on('click', function () {
            $.get("/DataFlow/NewSchemaMap", function (e) {
                $(e).insertBefore($("#btnAddSchemaMap"));
                data.DataFlow.InitSchemaMaps();               
            });
        });

        $(document).ready(function () {
            $('.selectpicker').selectpicker({
                liveSearch: false,
                showSubtext: true,
                size: '5',
                dropupAuto: false
            });

            $(".selectpicker-filtering").selectpicker({
                liveSearch: true,
                showSubtext: true,
                size: '5',
                dropupAuto: false
            });
        });
        
        data.DataFlow.InitSchemaMaps();

        data.Job.FormInit();
    },

    DataFlowDetailInit: function (dataflowId) {
        $('body').on('click', '.jobHeader', function () {
            if ($(this).children('.tracker-menu-icon').hasClass('glyphicon-menu-down')) {
                $(this).children('.tracker-menu-icon').switchClass('glyphicon-menu-down', 'glyphicon-menu-up');
            } else {
                $(this).children('.tracker-menu-icon').switchClass('glyphicon-menu-up', 'glyphicon-menu-down');
            }
            $(this).next('.jobContainer').toggle();

            if ($(this).next('.jobContainer:visible').length === 0) {
                // action when all are hidden
                $(this).css('border-radius', '5px 5px 5px 5px');
            } else {
                $(this).css('border-radius', '5px 5px 0px 0px');
            }
        });

        Sentry.InjectSpinner($('#targetSchemaSpinner'));

        var schemaMapUrl = "/DataFlow/_SchemaMapDetail/?dataflowId=" + encodeURI(dataflowId);
        $.get(schemaMapUrl, function (e) {
            $('#targetSchema').html(e);
        });

    },

    InitIngestionType() {
        var selection = $("[id$=IngestionTypeSelection]").val();

        if (selection === "2") {
            $('.namePanel').show();
            $('.compressionPanel').show();
            $('.schemaMapPanel').show();
            $('.preProcessingPanel').show();
            $('.formSubmitButtons').show();

            $('.retrieverPanel').show();
            $('.questionairePanel').show();
            data.Job.FormInit();
        }
        else if (selection === "1") {
            $('.namePanel').show();
            $('.retrieverPanel').hide();
            $('.compressionPanel').show();
            $('.schemaMapPanel').show();
            $('.preProcessingPanel').show();
            $('.formSubmitButtons').show();
        }

        if (selection === "0") {
            $('.namePanel').hide();
            $('.compressionPanel').hide();
            $('.schemaMapPanel').hide();
            $('.preProcessingPanel').hide();
            $('.formSubmitButtons').hide();
        }
        else {
            $('.namePanel').show();
            $('.compressionPanel').show();
            $('.schemaMapPanel').show();
            $('.preProcessingPanel').show();
            $('.formSubmitButtons').show();
        }            

        $("[id$=IngestionTypeSelection]").on('change', function () {
            var ingestionSelection = $(this).val();
            //if changing to Pull
            if (ingestionSelection === "2") {
                $('#retrieverPanelSpinner').css('float', 'left');
                Sentry.InjectSpinner($("#retrieverPanelSpinner"));
                $('.retrieverPanel').show();
            }
            //if changing to Push
            else {
                //Need to warn user potential loss of values
                //  for retriever job configuration if sourcetype has been selected.
                //If sourcetype has not been selected, then reset the RetrieverPanel as
                //  there are no values to loose.
                if ($('[id$=__SelectedSourceType]').val() == null) {
                    data.DataFlow.ResetRetrieverPanel();
                }
                else {
                    //need to manually pass buttons in order to specify callback for OK and Cancel options
                    Sentry.ShowModalCustom('Potential Loss of Configuration', 'You will loose configuration values within "Where Do You Want Us to Pull From" section if you continue', {
                        OK: {
                            label: "OK",
                            className: "btn-primary",
                            callback: function () { data.DataFlow.ResetRetrieverPanel(); }
                        },
                        Cancel: {
                            label: "Cancel",
                            className: "btn-link",
                            callback: function () { data.DataFlow.CancelIngestionSelection(ingestionSelection, $(this)); }
                        }
                    });
                };
            }
            $('.namePanel').show();
            $('.compressionPanel').show();
            $('.schemaMapPanel').show();
            $('.preProcessingPanel').show();
            $('.formSubmitButtons').show();
        });
    },

    CancelIngestionSelection: function (e, item) {
        //from https://stackoverflow.com/a/28324400
        var ingestionSelectBox = document.querySelector("[id$=IngestionTypeSelection]");
        switch (e) {
            case "1":
                ingestionSelectBox.value = "2";
                break;
            case "2":
                ingestionSelectBox.value = "1";
                break;
            default:
        }
        ingestionSelectBox.dispatchEvent(new Event('change'));
    },

    ResetRetrieverPanel: function () {
        $('.retrieverPanel').hide();
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
        $('.schemaSpinner').each(function (index) {
            var cur = $(this);
            cur.html('');
        });
        //data.DataFlow.InitSchemaMaps("0", null);
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
            }
        });
    },

    DatasetFileConfigFormCancelInit: function () {
        $('#DataFlowFormContainer').show();
        $('#DatasetFileConfigFormContainer').hide();
    },

    PopulateSchemas(datasetId, schemaId, targetElement) {
        var scmSpinner = $(targetElement).parent().parent().find('.schemaSpinner');
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

                scmSpinner.html('');
                targetElement.html(subItems);

                if (curVal === null || curVal === "0" || ((schemaId === undefined || schemaId === null) && curVal === "-1")) {
                    targetElement.val("0");
                }
                else if ((schemaId !== undefined && schemaId !== null) || curVal === "-1") {
                    targetElement.val(schemaId)
                }
                else {
                    $(targetElement).val(curVal);
                }
            });
        }
        else {
            var subItems;
            subItems += "<option value='0'>Select Dataset First</option>";
            scmSpinner.html('');
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
        


            $('[id$=__SelectedDataset]').change(function () {
                var curRow = $(this).parent().parent();
                var schemaSelectionDropDown = curRow.find("[id$=__SelectedSchema]");
                var datasetId = $(this).val();
                schemaSelectionDropDown.val("0");

                Sentry.InjectSpinner(curRow.find('.schemaSpinner'), 30);

                //if Create New Dataset Selected
                if (datasetId === "-1") {
                    $('#DataFlowFormContainer').hide();
                    data.DataFlow.RenderDatasetCreatePage();
                }
                else {
                    data.DataFlow.PopulateSchemas(datasetId, null, schemaSelectionDropDown);
                }

            });
    }
}