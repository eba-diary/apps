data.DataFlow = {

    DataFlowFormInit: function () {
        $("#IsCompressedCheckbox").change(function () {
            if ($(this).is(":checked")) {
                $("#IsCompressed").val(true);
            }
            else {
                $("#IsCompressed").val(false);
            }
        });
    }
}