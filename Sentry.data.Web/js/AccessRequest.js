data.AccessRequest = {


    InitForDataset: function (datasetId) {

        var modal = Sentry.ShowModalWithSpinner("Request Dataset Access");

        $("select").materialSelect();

        var createRequestUrl = "/Dataset/AccessRequest/?datasetId=" + encodeURI(datasetId);

        $.get(createRequestUrl, function (e) {
            modal.ReplaceModalBody(e);
            //auto check the preview 
            $("input[data-code='CanPreviewDataset']").prop('checked', true).attr('disabled', 'disabled');
            $("#SelectedPermissions").val($("input[type='checkbox']").first().data('code'));

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
                    url: '/Dataset/CheckAdGroup?adGroup=' + $(this).val(),
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
                if ($("#AdGroupName") !== undefined && $("#AdGroupName").val() === "") {
                    errors += "<div>AD Group is required</div>";
                }
                if ($("#AdGroupName") !== undefined && !isRealAdGroup) {
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
                        url: '/Dataset/SubmitAccessRequest',
                        success: function (data) { modal.ReplaceModalBody(data); }
                    });
                } else {
                    $("#AccessRequesrErrorBox").html(errors).show();
                }
            });
            data.Dataset.initRequestAccessWorkflow();
        });
       
    },

    InitForNotification: function () {

        var modal = Sentry.ShowModalWithSpinner("Request Notification Access");

        $("select").materialSelect();

        $.get('/Notification/AccessRequest', function (e) {
            modal.ReplaceModalBody(e);
            //auto check the preview 
            $("input[data-code='CanModifyNotification']").prop('checked', true).attr('disabled', 'disabled');
            $("#SelectedPermissions").val($("input[type='checkbox']").first().data('code'));

            //set up the change event to populate the approvers once dataAsset is selected.
            $("#SecurableObjectId").change(function () {
                $.ajax({
                    url: '/Notification/GetApproversByBusinessArea?businessAreaId=' + encodeURI($(this).val()),
                    method: "GET",
                    dataType: 'json',
                    success: function (data) {
                        var html = "";
                        for (var i = 0; i < data.length; i++) {
                            html += "<option value='" + data[i].Value + "'> " + data[i].Text + "</option>";
                        }
                        $("#SelectedApprover").html(html);
                    }
                });
            });

            $("[id^='SubmitAccessRequestButton']").off('click').on('click', function (e) {
                e.preventDefault();
                //check validation
                var errors = "";
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
                        url: '/Notification/SubmitAccessRequest',
                        success: function (data) {
                            modal.ReplaceModalBody(data);
                        }
                    });
                } else {
                    $("#AccessRequesrErrorBox").html(errors).show();
                }
            });
        });

    }

};