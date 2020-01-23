data.DataFlow = {
    DataFlow_DatasetSubmitInit: function () {

        SubmitFunciton = function () {

        }

    },

    DataFlowFormInit: function () {

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

            data.DataFlow.PopulateSchemas(datasetId, schemaSelectionDropDown);

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

    PopulateSchemas(datasetId, targetElement) {
        if (datasetId !== null) {
            var schemaId = targetElement.val();
            if (schemaId === null || schemaId === "0") {
                $.getJSON("/api/v2/metadata/dataset/" + datasetId + "/schema", function (result) {
                    var subItems;
                    subItems += "<option value='0'>Select Schema</option>";
                    $.each(result, function (index, item) {
                        subItems += "<option value='" + item.SchemaId + "'>" + item.Name + "</option>";
                    });

                    targetElement.html(subItems);
                    targetElement.val("0");
                });
            }
            if (schemaId !== null && schemaId !== "0") {
                $.getJSON("/api/v2/metadata/dataset/" + datasetId + "/schema", function (result) {
                    var subItems;                    
                    $.each(result, function (index, item) {
                        subItems += "<option value='" + item.SchemaId + "'>" + item.Name + "</option>";
                    });

                    targetElement.html(subItems);
                    targetElement.val(schemaId);
                });
            }
        }        
    }
}