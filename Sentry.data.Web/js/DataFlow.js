data.DataFlow = {

    DataFlowFormInit: function () {
        console.log("hi");

        $("#IsCompressedCheckbox").change(function () {
            if ($(this).is(":checked")) {
                $("#IsCompressed").val(true);
            }
            else {
                $("#IsCompressed").val(false);
            }
        });

        $("#btnAddSchemaMap").on('click', function () {
            $.get("/DataFlow/NewSchemaMap", function (e) {
                $("#schemaMapPanel").append(e);
                $('[id$=__SelectedDataset]').change(function () {
                    var curRow = $(this).parent;
                    var schemaSelectionDropDown = curRow.find("[id$=__SelectedSchema]");
                    var val = $(this).val();
                    console.log(val);

                    $.getJSON("/api/v2/metadata/dataset/" + val + "/schema", function (data) {
                        var subItems = "";
                        $.each(data, function (index, item) {
                            subItems += "<option value='" + item.SchemaId + "'>" + item.Name + "</option>";
                        });

                        schemaSelectionDropDown.html(subItems);
                        schemaSelectionDropDown.val("0");
                    });
                });
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