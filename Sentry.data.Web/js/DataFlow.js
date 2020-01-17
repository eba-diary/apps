data.DataFlow = {

    DataFlowFormInit: function () {

        //data.DataFlow.InitCompressionCheckbox();

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
                $('[id$=__SelectedDataset]').change(function () {
                    var curRow = $(this).parent().parent();
                    var schemaSelectionDropDown = curRow.find("[id$=__SelectedSchema]");
                    var val = $(this).val();

                    $.getJSON("/api/v2/metadata/dataset/" + val + "/schema", function (result) {
                        var subItems;
                        subItems += "<option value='0'>Select Schema</option>";
                        $.each(result, function (index, item) {
                            subItems += "<option value='" + item.SchemaId + "'>" + item.Name + "</option>";
                        });

                        schemaSelectionDropDown.html(subItems);
                        schemaSelectionDropDown.val("0");
                    });
                });
            });
        });

        $('[id$=__SelectedDataset]').change(function () {
            var curRow = $(this).parent().parent();
            var schemaSelectionDropDown = curRow.find("[id$=__SelectedSchema]");
            var val = $(this).val();
            console.log(val);

            $.getJSON("/api/v2/metadata/dataset/" + val + "/schema", function (result) {
                //var optgroup = $('<optgroup>');

                //var previousOpt = '';
                //$.each(result, function (index, inData) {

                //    if (inData. != previousOpt) {
                //        if (previousOpt != '') {
                //            schemaSelectionDropDown.append(optgroup);
                //        }
                //        optgroup = $('<optgroup>');
                //        optgroup.attr('label', inData.Group.Name);
                //        previousOpt = inData.Group.Name;
                //    }

                //    optgroup.append($('<option/>', {
                //        value: inData.Value,
                //        text: inData.Text
                //    }));
                //});

                //schemaSelectionDropDown.append(optgroup);
                var subItems;
                subItems += "<option value='0'>Select Schema</option>";
                $.each(result, function (index, item) {
                    subItems += "<option value='" + item.SchemaId + "'>" + item.Name + "</option>";
                });

                schemaSelectionDropDown.html(subItems);
                schemaSelectionDropDown.val("0");
            });
        });

        

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
    }
}