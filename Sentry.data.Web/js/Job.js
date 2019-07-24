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
            $('.questionairePanel').hide();
            $('#btnCreateDataset').hide();
            $("#editDataSource").hide();
            var val = $('#SelectedSourceType :selected').val();

            $.getJSON("/Config/SourcesByType", { sourceType: val }, function (data) {
                var subItems = "";
                $.each(data, function (index, item) {
                    subItems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
                });

                $("#SelectedDataSource").html(subItems);
                $("#SelectedDataSource select").val("0");
            });

            data.Job.RequestMethodDropdownPopulate();

            data.Job.targetFileNameDescUpdate();
        });

        $('#SelectedRequestMethod').change(function () {
            data.Job.DisplayHttpPostPanel();
        });

        $("#SelectedDataSource").change(function () {
            var val = $(this).val();
            if (val != "0" && val != null) {
                $.ajax({
                    url: "/Config/SourceDetails",
                    dataType: 'json',
                    type: "GET",
                    data: { sourceId: $('#SelectedDataSource :selected').val() },
                    success: function (data) {
                        //data.Job.SetSecurityPanel(data.Secured, data.Permissions);
                        if (!data.Secured || (data.Secured && data.Permissions)) {
                            $('.securityPanel').hide();
                            $('.questionairePanel').show();
                            $('#btnCreateDataset').show();

                            $("#editDataSource").attr("href", "/Config/Source/Edit/" + val);
                            $("#editDataSource").show();

                            $.getJSON("/Config/DataSourceDescription", { sourceId: val }, function (data) {
                                $("#dataSourceDescription").text(data.Description);
                                $("#baseURLTextBox").val(data.BaseUri);
                                $("#baseURL").text(" The Base URL of the Data Source you picked is " + data.BaseUri + ".  What you type in the Relative URL will be appended to the end of this Base URL.");
                            });

                            $.getJSON("/Config/IsHttpSource/", { dataSourceId: val }, function (data) {
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
                        else {
                            $('.securityPanel').show();
                            $('.questionairePanel').hide();
                            $('#btnCreateDataset').hide();
                            $('#btnCreateDataset').hide();
                        }
                    }
                });
            }            
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

        $("[id^='RequestAccessButton']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Job.AccessRequest($('#SelectedDataSource :selected').val());
        });
    },

    AccessRequest: function (sourceId) {
        var modal = Sentry.ShowModalWithSpinner("Request Access");
        var createRequestUrl = "/Config/DataSourceAccessRequest/?dataSourceId=" + encodeURI(sourceId);

        $.get(createRequestUrl, function (e) {
            modal.ReplaceModalBody(e);

            $("input[type='checkbox']").change(function () {
                var selectedPermissions = [];
                $("input[type='checkbox']:checked").each(function () {
                    selectedPermissions.push($(this).data("code"));
                });
                $("#SelectedPermissions").val(selectedPermissions);
            });

            var isRealAdGroup = false;

            $("#AdGroupName").change(function () {
                $.ajax({
                    url: '/Dataset/CheckAdGroup?adGroup=' + encodeURIComponent($(this).val()),
                    method: "GET",
                    dataType: 'json',
                    success: function (data) {
                        if (data) {
                            $("#AccessRequesrErrorBox").hide();
                        } else {
                            $("#AccessRequesrErrorBox").html("<div>AD Group is not a vaild group</div>").show();
                        }
                        isRealAdGroup = data;
                    }
                });
            });

            $("[id^='SubmitAccessRequestButton']").off('click').on('click', function (e) {
                e.preventDefault();
                //check validation
                var errors = "";
                if ($("#AdGroupName").val() === undefined || $("#AdGroupName").val() === "") {
                    errors += "<div>AD Group is required</div>";
                }
                if (!isRealAdGroup) {
                    errors += "<div>AD Group is not a vaild group</div>";
                }
                if ($("#BusinessReason").val() === undefined || $("#BusinessReason").val() === "") {
                    errors += "<div>Business Reason is required</div>";
                }
                if ($("#SelectedApprover").val() === undefined || $("#SelectedApprover").val() === "") {
                    errors += "<div>Approver is required</div>";
                }
                if ($("#SelectedPermissions").val() === undefined || $("#SelectedPermissions").val() === "") {
                    errors += "<div>Permissions are required</div>";
                }

                if (errors === "") {
                    $("#accessRequestSpinner").css('float', 'left');
                    Sentry.InjectSpinner($("#accessRequestSpinner"), 30);
                    $.ajax({
                        type: 'POST',
                        data: $("#AccessRequestForm").serialize(),
                        url: '/Config/SubmitAccessRequest',
                        success: function (data) { modal.ReplaceModalBody(data); }
                    });
                } else {
                    $("#AccessRequesrErrorBox").html(errors).show();
                }
            });
        });
    },

    SetSecurityPanel: function (secured, hasPermission) {
        if (!secured || (secured && hasPermission)) {
            $('.securityPanel').hide();
            $('.questionairePanel').show();
        }
        else {
            $('.securityPanel').show();
            $('.questionairePanel').hide();
        }
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