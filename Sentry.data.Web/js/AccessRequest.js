data.AccessRequest = {




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