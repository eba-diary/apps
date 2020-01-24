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

        data.DataFlow.InitSchemaMap();

        

        //$("#SelectedSourceType").change(function () {
        //    data.Job.SetDataSourceSpecificPanels();
        //    data.Job.SetFtpPatternDefaults();
        //    $('.questionairePanel').hide();
        //    $(".editDataSourceLink").hide();
        //    $('#btnCreateDataset').hide();
        //    $('.dataSourceInfoPanel').hide();
        //    var val = $('#SelectedSourceType :selected').val();

        //    $.getJSON("/Config/SourcesByType", { sourceType: val }, function (data) {
        //        var subItems = "";
        //        $.each(data, function (index, item) {
        //            subItems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
        //        });

        //        $("#SelectedDataSource").html(subItems);
        //        $("#SelectedDataSource select").val("0");
        //    });

        //    data.Job.RequestMethodDropdownPopulate();

        //    data.Job.targetFileNameDescUpdate();
        //});
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

    InitSchemaMap() {
        $("#btnAddSchemaMap").on('click', function () {
            $.get("/DataFlow/NewSchemaMap", function (e) {
                $(e).insertBefore($("#btnAddSchemaMap"));
                $('[id$=__SelectedDataset]').change(function () {
                    var curRow = $(this).parent().parent();
                    var schemaSelectionDropDown = curRow.find("[id$=__SelectedSchema]");
                    var datasetId = $(this).val();

                    data.DataFlow.PopulateSchemas(datasetId, schemaSelectionDropDown);
                });
            });
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
                data.DataFlow.PopulateSchemas(datasetId, schemaSelectionDropDown);
            }

        });

        $('[id$=__SelectedDataset]').each(function (index) {
            var datasetId = $(this).val();
            var curRow = $(this).parent().parent();
            var schemaSelectionDropDown = curRow.find("[id$=__SelectedSchema]");

            data.DataFlow.PopulateSchemas(datasetId, schemaSelectionDropDown);

            //if (datasetId !== "0") {                
            //    var schemaId = schemaSelectionDropDown.val();
            //    if (schemaId === null) {
                    
            //    }
            //}
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

    DatasetFormCancelInit: function () {
        $('#DataFlowFormContainer').show();
        $('#DatasetFormContainer').hide();
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

                    //find dataset dropdown with -1 value and set back to 0
                    //$('[id$=__SelectedDataset]').each(function (index) {
                    //    var datasetId = $(this).val();
                    //    if (datasetId === "-1") {
                    //        $(this).val(0);
                    //    }
                    //});
                    data.DataFlow.RefreshDatasetLists(obj.dataset_id);
                }
                else {
                    $('#DatasetFormContent').replaceWith(obj);
                }
            },
            failure: function () {
            },
            error: function (obj) {
                alert('You messed up!')
                $('#DatasetFormContent').replaceWith(obj.responseText);
                var hrEnv = $('#HrempServiceEnv').val()
                var hrUrl = $('#HrempServiceUrl').val()
                //init the form passing the submit function specific for DataFlow page
                data.Dataset.FormInit(hrUrl, hrEnv, data.DataFlow.DatasetFormSubmitInit, data.DataFlow.DatasetFormCancelInit);
                alert('finished error catch')
            }
        });
    },

    PopulateSchemas(datasetId, targetElement) {
        if (datasetId !== null && datasetId !== "-1") {
            var schemaId = targetElement.val();
            $.getJSON("/api/v2/metadata/dataset/" + datasetId + "/schema", function (result) {
                var subItems;
                var groupName;
                var schemaCount = result.length;
                var sortedResults = data.DataFlow.sortJSON(result, 'Name', '123')

                // Add initial value
                subItems += "<option value='0'>Select Schema</option>";

                $.each(sortedResults, function (index, item) {
                    subItems += "<option value='" + item.SchemaId + "'>" + item.Name + "</option>";
                });

                targetElement.html(subItems);

                if (schemaId === null || schemaId === "0") {
                    targetElement.val("0");
                }
                if (schemaId !== null && schemaId !== "0") {
                    targetElement.val(schemaId);
                }
            });
        }
    },

    RefreshDatasetLists(datasetId) {
        $.getJSON("/api/v2/metadata/dataset", function (result) {
            var newSubItems;
            var groupName;
            var datasetCount = result.length;
            console.log(result);
            var sortedResult = data.DataFlow.sortJSON(result, 'Category', '123');
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
                if (curVal === "-1") {
                    cur.val(datasetId);
                    var curRow = cur.parent().parent();
                    data.DataFlow.PopulateSchemas(datasetId, curRow.find("[id$=__SelectedSchema]"));
                }
            });
        });
    },

    sortJSON(data, key, way) {
        return data.sort(function (a, b) {
            var x = a[key]; var y = b[key];
            if (way === '123') { return ((x < y) ? -1 : ((x > y) ? 1 : 0)); }
            if (way === '321') { return ((x > y) ? -1 : ((x < y) ? 1 : 0)); }
        });
    }
}