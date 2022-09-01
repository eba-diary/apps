data.DataFlow = {

    Orig_Dataset_Selection: 0,
    Orig_Schema_Selection: 0,

    IngestionType_TOPIC: "4",              //IngestionType_TOPIC matches public enum IngestionType

    DataFlowFormInit: function (datasetId, schemaId) {

        //initially hide or show TopicName
        data.DataFlow.HideOrShowTopic();

        data.DataFlow.Orig_Dataset_Selection = datasetId;
        data.DataFlow.Orig_Schema_Selection = schemaId;

        data.DataFlow.InitIngestionType();

        $("#PreprocessingOptions").materialSelect();

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

        //When the SAID asset changes, reload the named environments dropdown
        $("div#DataFlowFormContainer #SAIDAssetKeyCode").on('change', function () {
            Sentry.InjectSpinner($("div#DataFlowFormContainer #namedEnvironmentSpinner"), 30);
            data.DataFlow.populateNamedEnvironments();
        });

        //When the NamedEnvironment drop down changes (but only when it's rendered as a drop-down), reload the name environment type
        data.DataFlow.initNamedEnvironmentEvents();

        $(document).ready(function () {
            $("#SAIDAssetKeyCode").materialSelect();
            $("#IsCompressed").materialSelect();
            $("#IsPreProcessingRequired").materialSelect();
            $("#PreProcessingSelection").materialSelect();
            $("#NamedEnvironmentPartial select").materialSelect();
            $("#retrieverJobPanel select").materialSelect();
            $("#compressionJobQuestion select").materialSelect();
        });
        
        data.DataFlow.InitSchemaMaps(datasetId, schemaId);

        data.Job.FormInit();
    },

    DataFlowDetailInit: function (dataflowId) {
        let producerAssetGroupName = $("#producerAssetGroupName").text();

        $('body').on('click', '.jobHeader', function () {
            if ($(this).children('.tracker-menu-icon').hasClass('fa-chevron-down')) {
                $(this).children('.tracker-menu-icon').switchClass('fa-chevron-down', 'fa-chevron-up');
            } else {
                $(this).children('.tracker-menu-icon').switchClass('fa-chevron-up', 'fa-chevron-down');
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

        $("#RequestAccessManageEntitlement").text(producerAssetGroupName);

        $("#RequestAccessManageCopyBtn").click(function () {
            data.Dataset.copyTextToClipboard($("#RequestAccessManageEntitlement").text());
        });

    },

    InitIngestionType() {
        $("#IngestionTypeSelection").materialSelect();
        var selection = $("#IngestionTypeSelection").val();

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

        $("#IngestionTypeSelection").on('change', function () {
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
                if ($('[id$=__SelectedSourceType]')?.val() === undefined || $('[id$=__SelectedSourceType]')?.val() === null) {
                    data.DataFlow.ResetRetrieverPanel();
                }
                else {
                    //need to manually pass buttons in order to specify callback for OK and Cancel options
                    Sentry.ShowModalCustom('Potential Loss of Configuration', 'You will lose configuration values within "Where Do You Want Us to Pull From" section if you continue', {
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
                }
            }
            $('.namePanel').show();
            $('.compressionPanel').show();
            $('.schemaMapPanel').show();
            $('.preProcessingPanel').show();
            $('.formSubmitButtons').show();

            //DEPENDING ON IngestionTypeSelection SHOW OR HIDE TOPIC
            data.DataFlow.HideOrShowTopic();
        });
    },


    //function to HIDE OR SHOW TOPIC NAME for DSC PUSH DIGESTION types
    HideOrShowTopic: function () {

        var val = $("#IngestionTypeSelection").val();

        if (val == data.DataFlow.IngestionType_TOPIC) {
            $('#topicNameContainer').show();
        }
        else {
            $('#topicNameContainer').hide();
        }
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
    },

    DatasetFormSubmitInit: function () {
        $("#IsSecured").removeAttr("disabled");
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
                    $('#DatasetFormContainer').html(obj);
                }
            },
            error: function (obj) {
                $('#DatasetFormContainer').html(obj.responseText);
                var hrEnv = $('#HrempServiceEnv').val();
                var hrUrl = $('#HrempServiceUrl').val();
                if ($("#DatasetDesc").val()) {
                    $('label[for=DatasetDesc]').addClass("active");
                }
                if ($("#DatasetInformation").val()) {
                    $('label[for=DatasetInformation]').addClass("active");
                }
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
            error: function (obj) {
                $('#DatasetFileConfigFormContainer').html(obj.responseText);
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
        var createSchemaLink = targetElement.parent().parent().find('#CreateSchema');
        if (datasetId !== null && datasetId !== 0 && datasetId !== "0") {
            var curVal = schemaId;
            $.getJSON("/api/v2/metadata/dataset/" + String(datasetId) + "/schema", function (result) {
                var subItems;

                //Filter for only ACTIVE schema
                var filter = result.filter(function (item) {
                    return item.ObjectStatus === "ACTIVE";
                });

                filter = filter.filter(function (item) {
                    return item.SchemaId === data.DataFlow.Orig_Schema_Selection || item.HasDataFlow === false;
                });

                //Make list sorted by schema name
                var sortedResults = filter.sort(
                    firstBy("Name")
                );

                // Add initial value
                subItems += "<option value='0'>Select Schema</option>";

                // Add ACTIVE schema values
                $.each(sortedResults, function (index, item) {
                    subItems += "<option value='" + item.SchemaId + "'>" + item.Name + "</option>";
                });

                scmSpinner.html('');
                targetElement.html(subItems);

                if (curVal === null || curVal === "0") {
                    targetElement.val("0");
                }
                else {
                    $(targetElement).val(curVal);
                }

            });
            createSchemaLink.show();
        }
        else {
            var options;
            options += "<option value='0'>Select Dataset First</option>";
            scmSpinner.html('');
            targetElement.html(options);
            createSchemaLink.hide();
        }

        $('#CreateSchema').click(function () {
            $('#DataFlowFormContainer').hide();
            data.DataFlow.RenderSchemaCreatePage($(".dataset-select").val());
        });
    },

    InitSchemaMaps(datasetId, schemaId) {
        $('.datasetSpinner').each(function (index) {
            var cur = $(this);
            Sentry.InjectSpinner(cur, 30);
        });
        $('.schemaSpinner').each(function (index) {
            var cur = $(this);
            Sentry.InjectSpinner(cur, 30);
        });

        $.getJSON("/api/v2/metadata/dataset", function (result) {
            var newSubItems;
            var groupName;
            var datasetCount = result.length;
            var sortedResult = result.sort(
                firstBy("Category")
                    .thenBy("Name")
                    .thenBy("SAIDAssetKeyCode")
                    .thenBy("NamedEnvironment")
            );

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
                newSubItems += "<option value='" + item.Id + "'>" + item.Name + " (" + item.SAIDAssetKeyCode + " - " + item.NamedEnvironment + ")</option>";

                //close out group after last interation
                if (index === (datasetCount - 1)) {
                    newSubItems += "</optgroup>";
                }
            });

            $('[id$=__SelectedDataset]').each(function (index) {
                var cur = $(this);

                var curVal = datasetId;

                cur.html(newSubItems);

                var curRow = cur.parent().parent();
                if (curVal === null || curVal === undefined || isNaN(curVal)) {
                    $(this).val(0);
                    data.DataFlow.PopulateSchemas(0, schemaId, curRow.find("[id$=__SelectedSchema]"));
                }
                else {
                    cur.val(curVal);
                    data.DataFlow.PopulateSchemas(curVal, schemaId, curRow.find("[id$=__SelectedSchema]"));
                }

                $(".datasetSpinner").hide();
            });


            $('[id$=__SelectedDataset]').change(function () {
                var curRow = $(this).parent().parent();
                var schemaSelectionDropDown = curRow.find("[id$=__SelectedSchema]");
                var datasetChangeDatasetId = $(this).val();
                schemaSelectionDropDown.val("0");

                Sentry.InjectSpinner(curRow.find('.schemaSpinner'), 30);
                data.DataFlow.PopulateSchemas(datasetChangeDatasetId, null, schemaSelectionDropDown);
            });

            $('#CreateDataset').click(function () {
                $('#DataFlowFormContainer').hide();
                data.DataFlow.RenderDatasetCreatePage();
            });
        });
    },

    initNamedEnvironmentEvents() {
        //When the NamedEnvironment drop down changes (but only when it's rendered as a drop-down), reload the name environment type
        $("div#DataFlowFormContainer select#NamedEnvironment").change(function () {
            Sentry.InjectSpinner($("div#DataFlowFormContainer #namedEnvironmentTypeSpinner"), 30);
            data.DataFlow.populateNamedEnvironments();
        });
    },

    populateNamedEnvironments() {
        var assetKeyCode = $("div#DataFlowFormContainer #SAIDAssetKeyCode").val();
        var selectedEnvironment = $("div#DataFlowFormContainer #NamedEnvironment").val();
        $.get("/DataFlow/NamedEnvironment?assetKeyCode=" + assetKeyCode + "&namedEnvironment=" + selectedEnvironment, function (result) {
            $('div#DataFlowFormContainer #NamedEnvironmentPartial').html(result);
            data.DataFlow.initNamedEnvironmentEvents();
            $("#NamedEnvironmentPartial select").materialSelect();
        });
    },

    EditUrlRedirect(id) {
        window.open("/DataFlow/Edit/" + encodeURIComponent(id));
    },

    DetailUrlRedirect(id) {
        window.open("/DataFlow/" + encodeURIComponent(id) + "/Detail");
    }
}