data.MigrationRequest = {
    migrationRequestModal: {},

    InitForMigration: function (datasetId) {
        $("#MigrationRequestModal").remove();
        data.MigrationRequest.migrationRequestModal = Sentry.ShowModalWithSpinner("Migration Request");
        $(data.MigrationRequest.migrationRequestModal).attr("id", "MigrationRequestModal");

        var getMigrationRequestUrl = "/Migration/MigrationRequest?datasetId=" + encodeURI(datasetId);

        $.get(getMigrationRequestUrl, function (e) {
            data.MigrationRequest.migrationRequestModal.ReplaceModalBody(e);

            data.MigrationRequest.InitForDataset($(this).data("id"));
        });
    },

    InitForDataset: function (datasetid) {
        data.MigrationRequest.InitMigrationRequestForm();
        if ($('#QuartermasterManagedNamedEnvironments').val() === 'False') {
            $('#MigrationRequestModal #TargetNamedEnvironment').attr('searchable', 'Search or add here...');
            $('#MigrationRequestModal #TargetNamedEnvironment').attr('editable', true);
        }
        $("#MigrationFormSection select").materialSelect();

        $("[id^='MigrationRequestSubmitButton']").off('click').on('click', function (e) {

            var request = data.MigrationRequest.MapToDatasetMigrationRequestModel($('#DatasetId').val(), $('#TargetNamedEnvironment').val(), $('#SelectedSchema').val())
            var postDatasetMigrationUrl = "/api/v20220609/metadata/MigrateDataset";

            data.MigrationRequest.InitMigrationRequestSubmission(postDatasetMigrationUrl, request);
        });
    },

    InitForSchema: function (datasetId) {
        data.MigrationRequest.InitMigrationRequestForm();
        $('#MigrationRequestModal #TargetNamedEnvironment').attr('searchable', 'Search here...');
        $('#MigrationRequestModal #TargetNamedEnvironment').attr('editable', false);
        $('#MigrationRequestModal #SelectedSchema').attr('multiple', false);
        $("#MigrationFormSection select").materialSelect();
        $("[id^='MigrationRequestSubmitButton']").off('click').on('click', function (e) {

            var request = data.MigrationRequest.MapToSchemaMigrationRequestModel($('#DatasetId').val(), $('#TargetNamedEnvironment').val(), $('#SelectedSchema').val())
            var postDatasetMigrationUrl = "/api/v20220609/metadata/MigrateSchema";

            data.MigrationRequest.InitMigrationRequestSubmission(postDatasetMigrationUrl, request);
        });
    },

    InitMigrationRequestForm: function () {
        $('#MigrationRequestForSection').addClass('d-none');
        $('#RequestMigrationFormSection').removeClass('d-none');
        $('#MigrationFormSection').removeClass('d-none');
        $('#MigrationRequestSubmitButton').removeClass('d-none');
    },

    InitMigrationRequestSubmission: function (submissionApiUrl, request) {
        $('#MigrationRequestSubmitButton').prop('disabled', true);
        $('#ValidationMessages').addClass('d-none');
        $('#RequestMigrationFormSection').addClass('d-none');
        $('#RequestMigrationSubmissionBody').removeClass('d-none');
        $('#MigrationModalSpinner').removeClass('d-none');

        $.post(submissionApiUrl, request, function (migrationResultsModel) {
            console.log(migrationResultsModel);
            data.MigrationRequest.migrationRequestModal.HideModal();
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
    },

    initMigrationResponseModal: function (response) {
        $('.migration-modal-container').load("/Migration/MigrationResponse", function () {
            data.MigrationRequest.initMigrationResponseDataTable(response);
            $('#migrationResponseModal').modal('show');
        });
    },

    initMigrationResponseDataTable: function (response) {
        var responseTable = $('#MigrationResponseTable').DataTable({
            ordering: false,
            searching: false,
            paging: false
        });

        var hasDatasetMetadata = false;
        if (response.hasOwnProperty('DatasetId')) {
            hasDatasetMetadata = true;
            data.MigrationRequest.AddDatasetRowToMigrationResponseTable(responseTable, response);
        }

        if (response.hasOwnProperty('SchemaMigrationResponse')) {
            response.SchemaMigrationResponse.forEach(function (item, index) {
                data.MigrationRequest.AddSchemaRowToMigrationResponseTable(responseTable, item);
            });
        }

        if (hasDatasetMetadata === false) {
            data.MigrationRequest.AddSchemaRowToMigrationResponseTable(responseTable, response);
        }
    },

    InitRequestValidationMessages: function (errorArray) {
        /*Clear any potentail existing errors*/
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
        /* Create json object for submission to MigrateDataset API endpoint */
        var requestObject = new Object();
        requestObject.SourceDatasetId = datasetId;
        requestObject.TargetDatasetNamedEnvironment = targetNamedEnvironment;
        var sourceSchemaRequests = [];
        if (schemaIdList != undefined) {
            for (var schemaId of schemaIdList) {
                var schemaRequestObject = data.MigrationRequest.MapToSchemaMigrationRequestModel(datasetId, targetNamedEnvironment, schemaId);
                sourceSchemaRequests.push(schemaRequestObject);
            }
            requestObject.SchemaMigrationRequests = sourceSchemaRequests;
        }
        return requestObject;
    },

    MapToSchemaMigrationRequestModel: function (datasetId, targetNamedEnvironment, schemaId) {
        /* Create json object for submission to MigrateSchema API endpoint 
             or SchemaMigrationRequests property submitted to MigrateDataset endpoint.*/
        var schemaRequestObject = new Object();
        schemaRequestObject.SourceSchemaId = schemaId;
        schemaRequestObject.TargetDatasetId = datasetId;
        schemaRequestObject.TargetDatasetNamedEnvironment = targetNamedEnvironment;
        schemaRequestObject.TargetDataFlowNamedEnviornment = targetNamedEnvironment;

        return schemaRequestObject
    },

    AddDatasetRowToMigrationResponseTable: function (responseTable, item) {
        responseTable.row.add(['Dataset', item.DatasetName, '<i class="fas fa-check-circle fa-2x" />']).draw(false);
    },

    AddSchemaRowToMigrationResponseTable: function (responseTable, item) {
        responseTable.row.add(['Schema', item.SchemaName, '<i class="fas fa-check-circle fa-2x" />']).draw(false);
    }
}