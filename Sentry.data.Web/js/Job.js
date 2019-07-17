data.Job = {

    FormInit: function () {

        if ($("#JobID").val() != "0" && $("#JobID").val() != undefined) {
            var val = $('#SelectedDataSource :selected').val();
            $.getJSON("/Config/IsHttpSource/", { dataSourceId: val }, function (data) {
                if (data) {
                    $('.httpSourcePanel').show();
                }
                else {
                    $('.httpSourcePanel').hide();
                }
            });

            data.Job.DisplayHttpPostPanel();
        };

        $("#SelectedSourceType").change(function () {
            var val = $('#SelectedSourceType :selected').val();

            $.getJSON("/Config/SourcesByType", { sourceType: val }, function (data) {
                var subItems = "";
                $.each(data, function (index, item) {
                    subItems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
                });

                if (data) {
                    $("#editDataSource").attr("href", "/Config/Source/Edit/" + data[0].Value);

                    $.getJSON("/Config/DataSourceDescription/", { sourceId: data[0].Value }, function (data) {
                        $("#dataSourceDescription").text(data.Description);
                        $("#baseURLTextBox").val(data.BaseUri);
                        $("#baseURL").text(" The Base URL of the Data Source you picked is " + data.BaseUri + ".  What you type in the Relative URL will be appended to the end of this Base URL.");
                    });
                    $("#editDataSource").show();

                    $.getJSON("/Config/IsHttpSource/", { dataSourceId: data[0].Value }, function (data) {
                        if (data) {
                            $('.httpSourcePanel').show();
                            $('.httpPostPanel').hide();
                        }
                        else {
                            $('.httpSourcePanel').hide();
                            $('.httpPostPanel').hide();
                        }
                    });
                }

                $("#SelectedDataSource").html(subItems);
            });

            data.Job.RequestMethodDropdownPopulate();

            data.Job.targetFileNameDescUpdate();
        });

        $('#SelectedRequestMethod').change(function () {
            data.Job.DisplayHttpPostPanel();
        });

        $("#SelectedDataSource").change(function () {
            var val = $(this).val();

            $("#editDataSource").attr("href", "/Config/Source/Edit/" + val);
            $("#editDataSource").show();

            $.getJSON("/Config/DataSourceDescription", { sourceId: val }, function (data) {
                $("#dataSourceDescription").text(data.Description);
                $("#baseURLTextBox").val(data.BaseUri);
                $("#baseURL").text(" The Base URL of the Data Source you picked is " + data.BaseUri + ".  What you type in the Relative URL will be appended to the end of this Base URL.");
            });
        });

        $('#jsonPreview').on('click', function () {
            try {
                var data = JSON.parse($('#HttpRequestBody').val());
                $('#json-viewer').jsonViewer(data);
                $('.jsonValidateResultsPanel').show();
            }
            catch (error) {
                alert("invalid json");
            }
        });
    },

    RequestMethodDropdownPopulate: function () {
        var val = $('#SelectedSourceType :selected').val();

        $.getJSON(encodeURI("/Config/RequestMethodByType/" + val), function (data) {
            var subItems = "";
            $.each(data, function (index, item) {
                subItems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
            });

            $('#SelectedRequestMethod').html(subItems);
        });

        data.Job.DisplayHttpPostPanel();
    },

    DisplayHttpPostPanel: function () {
        var val = $('#SelectedRequestMethod :selected').text();
        if (val.toUpperCase() === 'POST') {
            data.Job.RequestDataFormatDropdownPopulate();
            $('.httpPostPanel').show();
        }
        else {
            $('.httpPostPanel').hide();
        }
    },

    RequestDataFormatDropdownPopulate: function () {
        var val = $('#SelectedSourceType :selected').val();


        $.getJSON(encodeURI("/Config/RequestDataFormatByType/" + val), function (data) {
            var subItems = "";
            $.each(data, function (index, item) {
                subItems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
            });

            $('#SelectedRequestDataFormat').html(subItems);
        });
    },

    targetFileNameDescUpdate: function () {
        var val = $('#SelectedSourceType :selected').val();

        if (val == 'HTTPS') {
            $("#targetfilenamequestion").text("What should target file be named?");
            $("#targetfilenamedesc").text("Due to the type of data source, a target file name is required as we are receiving a message not retrieving specific file.");
            $('#targetfilenamelabel').removeClass("optional");
        }
        else {
            $("#targetfilenamequestion").text("Should file be renamed?");
            $("#targetfilenamedesc").text("The incoming file will be renamed to Target File Name supplied.  If left blank, original source file name will used when saved to data.sentry.com.");
            $('#targetfilenamelabel').addClass("optional");
        }
    }
}