data.MigrationRequest = {
    migrationRequestModal: {},

    InitForMigration: function (datasetId) {
        $("#MigrationRequestModal").remove();
        data.MigrationRequest.migrationRequestModal = Sentry.ShowModalWithSpinner("Migration Request");
        $(data.MigrationRequest.migrationRequestModal).attr("id", "MigrationRequestModal");

        let getMigrationRequestUrl = "/Migration/MigrationRequest?datasetId=" + encodeURI(datasetId);

        $.get(getMigrationRequestUrl, function (e) {
            data.MigrationRequest.migrationRequestModal.ReplaceModalBody(e);

            data.MigrationRequest.InitForDataset($(this).data("id"));
        });
    },

    InitForDataset: function (datasetid) {
        data.MigrationRequest.InitMigrationRequestForm();
        if ($('#QuartermasterManagedNamedEnvironments').val() === 'False') {
            $('#MigrationRequestModal #DatasetNamedEnvironment').attr('searchable', 'Search or add here...');
            $('#MigrationRequestModal #DatasetNamedEnvironment').attr('editable', true);
        }
        $("#MigrationFormSection select").materialSelect();

        $("[id^='MigrationRequestSubmitButton']").off('click').on('click', function (e) {

            let request = data.MigrationRequest.MapToDatasetMigrationRequestModel($('#DatasetId').val(), $('#DatasetNamedEnvironment').val(), $('#DatasetNamedEnvironmentType').val(), $('#SelectedSchema').val())
            let postDatasetMigrationUrl = "/api/" + data.GetApiVersion() + "/metadata/MigrateDataset";

            data.MigrationRequest.InitMigrationRequestSubmission(postDatasetMigrationUrl, request);
        });

        data.MigrationRequest.initNamedEnvironmentEvents();

        $('#SelectedSchemaApplyBtn').on("click", function () {
            $(document).click(); // This closes the MultiSelector
        });
    },

    InitForSchema: function (datasetId) {
        data.MigrationRequest.InitMigrationRequestForm();
        $('#MigrationRequestModal #TargetNamedEnvironment').attr('searchable', 'Search here...');
        $('#MigrationRequestModal #TargetNamedEnvironment').attr('editable', false);
        $('#MigrationRequestModal #SelectedSchema').attr('multiple', false);
        $("#MigrationFormSection select").materialSelect();
        $("[id^='MigrationRequestSubmitButton']").off('click').on('click', function (e) {

            let request = data.MigrationRequest.MapToSchemaMigrationRequestModel($('#DatasetId').val(), $('#DatasetNamedEnvironment').val(),  $('#SelectedSchema').val())
            let postDatasetMigrationUrl = "/api/" + data.GetApiVersion() + "/metadata/MigrateSchema";

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
        let requestErrorList = data.MigrationRequest.validateMigrationRequestForm();
        if (requestErrorList.length !== 0) {
            //Do not submit request if there are validation errors
            return;
        }

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
                let statusCode = errorResponse.status;
                let errorMessage = errorResponse.responseJSON.Message;

                if (statusCode === 400) {
                    let messageArray = JSON.parse(errorMessage);

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

    validateMigrationRequestForm: function () {
        let errorList = [];
        $.each(data.MigrationRequest.validateDatasetNamedEnvironmentType(), function (index, item) { errorList.push(item) });
        return errorList;
    },

    clearRequestFormValidationErrors: function () {
        data.MigrationRequest.clearDatasetNamedEnvironmentTypeValidationError();
    },

    validateDatasetNamedEnvironmentType: function () {
        const errorMessage = "Select Named Environment Type";
        let error = [];
        if ($('#DatasetNamedEnvironmentType.mdb-select').val() === "Select Type") {
            data.MigrationRequest.setDatasetNamedEnvironmentTypeValidationMessage(errorMessage);
            error.push(errorMessage);
        }
        return error;
    },

    clearDatasetNamedEnvironmentTypeValidationError: function () {
        data.MigrationRequest.setDatasetNamedEnvironmentTypeValidationMessage("");
    },

    setDatasetNamedEnvironmentTypeValidationMessage: function (message) {
        $('#DatasetNamedEnvironmentType.mdb-select').closest(".dropdown-container").find(".field-validation-valid").text(message);
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

    initNamedEnvironmentEvents() {
        //When the NamedEnvironment drop down changes (but only when it's rendered as a drop-down, reload the name environment type

        $("#DatasetNamedEnvironment.mdb-select").change(function () {
            data.MigrationRequest.populateNamedEnvironments();
        });
        if ($('#QuartermasterManagedNamedEnvironments').val() === 'False') {
            //This code is needed to trigger change event for an editable md-select dropdown when new item is added to list
            //https://mdbootstrap.com/support/jquery/material-select-editable-add-new-item-doesnt-fire-on-change-or-any-callback-i-can-find/
            $('#DatasetNamedEnvironmentPartial i.select-add-option').click(function () {
                $('#DatasetNamedEnvironmentPartial i.select-add-option').closest("div.select-wrapper").find("select").first().trigger("change");
            })

            $("#DatasetNamedEnvironmentType.mdb-select").change(function () {
                data.MigrationRequest.clearDatasetNamedEnvironmentTypeValidationError();
            })
        }
    },

    populateNamedEnvironments() {
        let assetKeyCode = $("div#RequestMigrationFormSection #SAIDAssetKeyCode").val();
        let selectedEnvironment = $("#DatasetNamedEnvironment").val();
        let datasetId = $("div#RequestMigrationFormSection #DatasetId").val();
        data.MigrationRequest.clearDatasetNamedEnvironmentTypeValidationError();

        $.get("/Migration/NamedEnvironment?assetKeyCode=" + assetKeyCode + "&namedEnvironment=" + selectedEnvironment + "&datasetId=" + datasetId, function (result) {
            $("#DatasetNamedEnvironment.mdb-select").materialSelect({ destroy: true });
            $("#DatasetNamedEnvironmentType.mdb-select").materialSelect({ destroy: true });

            $('#DatasetNamedEnvironmentPartial').html(result);
            if ($('#QuartermasterManagedNamedEnvironments').val() === 'False') {
                $('#MigrationRequestModal #DatasetNamedEnvironment').attr('searchable', 'Search or add here...');
                $('#MigrationRequestModal #DatasetNamedEnvironment').attr('editable', true);
            }

            if ($('#NewNonQManagedNamedEnvironment').val() === 'True') {
                data.MigrationRequest.setNamedEnvironmentTypeToDefaultSelection();
            }
            data.MigrationRequest.initNamedEnvironmentEvents();

            $("#DatasetNamedEnvironment.mdb-select").materialSelect();
            $("#DatasetNamedEnvironmentType.mdb-select").materialSelect();
        });
    },

    setNamedEnvironmentTypeToDefaultSelection() {
        let defaultValue = "Select Type";
        $.each($("#DatasetNamedEnvironmentType.mdb-select option"), function (index, item) {
            if (item.value === defaultValue) {
                item.selected = true;
            }
            else {
                item.selected = false;
            }
        });
    },

    AddErrorToValidationMessages: function (error) {
        let errorList = $('#RequestMigrationFormSection #ValidationMessages ul');
        errorList.append('<li>' + error + '</li>');
    },

    CreateErrorToastrMessage(message) {
        data.Dataset.makeToast("error", message);
    },

    MapToDatasetMigrationRequestModel: function (datasetId, targetNamedEnvironment, targetNamedEnvironmentType, schemaIdList) {
        /* Create json object for submission to MigrateDataset API endpoint */
        let requestObject = new Object();
        requestObject.SourceDatasetId = datasetId;
        requestObject.TargetDatasetNamedEnvironment = targetNamedEnvironment;
        requestObject.TargetDatasetNamedEnvironmentType = targetNamedEnvironmentType;
        let sourceSchemaRequests = [];
        if (schemaIdList != undefined) {
            for (let schemaId of schemaIdList) {
                let schemaRequestObject = data.MigrationRequest.MapToSchemaMigrationRequestModel(datasetId, targetNamedEnvironment, schemaId);
                sourceSchemaRequests.push(schemaRequestObject);
            }
            requestObject.SchemaMigrationRequests = sourceSchemaRequests;
        }
        return requestObject;
    },

    MapToSchemaMigrationRequestModel: function (datasetId, targetNamedEnvironment, schemaId) {
        /* Create json object for submission to MigrateSchema API endpoint 
             or SchemaMigrationRequests property submitted to MigrateDataset endpoint.*/
        let schemaRequestObject = new Object();
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