data.MigrationRequest = {
    InitForDataset : function(datasetId) {
        $("#MigrationRequestModal").remove();
        var migrationRequestModal = Sentry.ShowModalWithSpinner("Dataset Migration Request");
        $(migrationRequestModal).attr("id", "MigrationRequestModal");

        getDatasetMigrationRequestUrl = "/Migration/DatasetMigrationRequest?datasetId=" + encodeURI(datasetId);

        $.get(getDatasetMigrationRequestUrl, function (e) {
            migrationRequestModal.ReplaceModalBody(e);
            $('#MigrationRequestModal #SelectedSchema').attr('multiple', true);
            $("#MigrationRequestModal select").materialSelect();

            $("[id^='MigrationRequestSubmitButton']").off('click').on('click', function (e) {
                $('#MigrationRequestSubmitButton').prop('disabled', true);
                $('#ValidationMessages').addClass('d-none');
                $('#RequestMigrationFormSection').addClass('d-none');
                $('#RequestMigrationSubmissionBody').removeClass('d-none');
                $('#MigrationModalSpinner').removeClass('d-none');

                var request = data.MigrationRequest.MapToDatasetMigrationRequestModel($('#DatasetId').val(), $('#TargetNamedEnvironment').val(), $('#SelectedSchema').val())
                console.log(JSON.stringify(request));

                var postDatasetMigrationUrl = "/api/v20220609/metadata/MigrateDataset"

                /*Send migration request to API*/
                $.post(postDatasetMigrationUrl, request, function (migrationResultsModel) {
                    console.log(migrationResultsModel);
                    migrationRequestModal.HideModal();
                    data.MigrationRequest.initMigrationResponseModal(migrationResultsModel);
                })
                    .fail(function (errorResponse) {
                        
                        console.log(errorResponse);
                        var statusCode = errorResponse.status;
                        var errorMessage = errorResponse.responseJSON.Message;

                        if (statusCode === 400) {                            
                            var messageArray = JSON.parse(errorMessage);

                            data.MigrationRequest.InitRequestValidationMessages(messageArray);                            
                            
                            $('#RequestMigrationSubmissionBody').addClass('d-none');
                            $('#RequestMigrationFormSection').removeClass('d-none');
                            $('#MigrationModalSpinner').addClass('d-none');
                            $('#MigrationRequestSubmitButton').prop('disabled', false);
                        }
                        else {
                            data.Dataset.makeToast("error", "Error Occurred, please contact DSC team if problem persists.");
                            $('#MigrationModalSpinner').addClass('d-none');
                            $('#RequestMigrationSubmissionBody').addClass('d-none');
                            $('#RequestMigrationFormSection').removeClass('d-none');
                            $('#MigrationRequestSubmitButton').prop('disabled', false);
                        }
                    })
            })
        })
    },

    InitRequestValidationMessages: function (errorArray) {
        //Clear existing validation errors
        $('#RequestMigrationFormSection #ValidationMessages ul').empty();
        errorArray.forEach(data.MigrationRequest.AddErrorToValidationMessages);
        $('#ValidationMessages').removeClass('d-none');
    },

    AddErrorToValidationMessages: function (error) {
        var errorList = $('#RequestMigrationFormSection #ValidationMessages ul');
        errorList.append('<li>' + error + '</li>');
    },

    CreateErrorToastrMessage(message) {
        data.Dataset.makeToast("error", message);
    },

    MapToDatasetMigrationRequestModel: function (datasetId, targetNamedEnvironment, schemaIdList) {
        var requestObject = new Object();
        requestObject.SourceDatasetId = datasetId;
        requestObject.TargetDatasetNamedEnvironment = targetNamedEnvironment;
        var sourceSchemaRequests = [];
        if (schemaIdList != undefined) {
            for (schemaId of schemaIdList) {
                var schemaRequestObject = new Object();
                schemaRequestObject.SourceSchemaId = schemaId;
                schemaRequestObject.TargetDatasetNamedEnvironment = targetNamedEnvironment;
                schemaRequestObject.TargetDataFlowNamedEnviornment = targetNamedEnvironment;
                sourceSchemaRequests.push(schemaRequestObject);

            }
            requestObject.SchemaMigrationRequests = sourceSchemaRequests;
        }
        return requestObject;
    },

    initMigrationResponseModal: function (response) {
        $('.migration-modal-container').load("/Migration/DatasetMigrationResponse", function () {
            data.MigrationRequest.initMigrationResponseDataTable(response);
            $('#migrationResponseModal').modal('show');
        });
    },

    initMigrationResponseDataTable: function (response) {
        var responseTable = $('#migrationResponseTable_V2').DataTable({
            ordering: false,
            searching: false,
            paging: false
        });

        if (response.hasOwnProperty('DatasetId')) {
            data.MigrationRequest.AddDatasetRowToMigrationResponseTable(responseTable, response);
        }

        if (response.hasOwnProperty('SchemaMigrationResponse')) {
            response.SchemaMigrationResponse.forEach(function (item, index) {
                data.MigrationRequest.AddSchemaRowToMigrationResponseTable(responseTable, item);
            });
        }
    },

    AddDatasetRowToMigrationResponseTable: function (responseTable, item) {
        responseTable.row.add(['Dataset', item.DatasetName, '<i class="fas fa-check-circle fa-2x" />']).draw(false);
    },

    AddSchemaRowToMigrationResponseTable: function (responseTable, item) {
        responseTable.row.add(['Schema', item.SchemaName, '<i class="fas fa-check-circle fa-2x" />']).draw(false);
    }
}