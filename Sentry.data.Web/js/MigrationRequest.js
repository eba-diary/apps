data.MigrationRequest = {
    InitForDataset : function(datasetId) {
        $("#MigrationRequestModal").remove();
        var modal = Sentry.ShowModalWithSpinner("Dataset Migration Request");
        $(modal).attr("id", "MigrationRequestModal");

        getDatasetMigrationRequestUrl = "/Dataset/MigrationRequest?datasetId=" + encodeURI(datasetId);

        $.get(getDatasetMigrationRequestUrl, function (e) {
            modal.ReplaceModalBody(e);
            $('#MigrationRequestModal #SelectedSchema').attr('multiple', true);
            $("#MigrationRequestModal select").materialSelect();

            $("[id^='MigrationRequestSubmitButton']").off('click').on('click', function (e) {
                //alert('DatasetId' + $('#DatasetId').val())
                //alert('SchemaId' + $("#SelectedSchema").val())
                var request = data.MigrationRequest.CreateDatasetMigrationObject($('#DatasetId').val(), $('#TargetNamedEnvironment').val(), $('#SelectedSchema').val())
                alert(JSON.stringify(request));

                var postDatasetMigrationUrl = "/api/v20220609/metadata/MigrateDataset"

                //$.post(postDatasetMigrationUrl, request, function (migrationResultsModel) {
                //    console.log(JSON.stringify(migrationResultsModel));
                //    responseData = JSON.parse(migrationResultsModel);
                //    console.log(responseData)
                //})

                data.MigrationRequest.initResponseDataTableV2();
                $('#RequestMigrationFormSection').addClass('d-none');
                //$('#MigrationResponseBody').removeClass('d-none');
                $('#migration-response-modal-container').removeClass('d-none');
            })
        })
    }, 

    CreateDatasetMigrationObject: function (datasetId, targetNamedEnvironment, schemaIdList) {
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

    initResponseDataTable: function () {
        var groupColumn = 0;
        var responseTable = $('#migrationResponseTable').DataTable({
            ordering: false,
            searching: false,
            paging: true,
            columnDefs: [{ visible: false, targets: groupColumn }],
            drawCallback: function (settings) {
                var api = this.api();
                var rows = api.rows({ page: 'current' }).nodes();
                var last = null;

                api
                    .column(groupColumn, { page: 'current' })
                    .data()
                    .each(function (group, i) {
                        if (last !== group) {
                            $(rows)
                                .eq(i)
                                .before('<tr class="group"><td colspan="4">' + group + '</td></tr>');

                            last = group;
                        }
                    });
            }
        });

        responseTable.row.add(['ZZZ Test Data', 'Dataset Information', 'My Dataset', 'Not Migrated']).draw(false);
        responseTable.row.add(['CSV Data 02', 'Schema Information', 'CSV Data 02', 'Not Migrated']).draw(false);
        responseTable.row.add(['CSV Data 02', 'Column Metadata', 'CSV Data 02', 'Success']).draw(false);
        responseTable.row.add(['CSV Data 02', 'Producer DataFlow', 'ZZZTestData_CS', 'Not Migrated']).draw(false);
        responseTable.row.add(['JSON Data 02', 'Schema Information', 'JSON Data 02', 'Not Migrated']).draw(false);
        responseTable.row.add(['JSON Data 02', 'Column Metadata', 'JSON Data 02', 'Success']).draw(false);
        responseTable.row.add(['JSON Data 02', 'Producer DataFlow', 'ZZZTestData_JS', 'Not Migrated']).draw(false);
        responseTable.row.add(['XML Data 02', 'Schema Information', 'XML Data 01', 'Not Migrated']).draw(false);
        responseTable.row.add(['XML Data 02', 'Column Metadata', 'XML Data 01', 'Success']).draw(false);
        responseTable.row.add(['XML Data 02', 'Producer DataFlow', 'ZZZTestData_X', 'Not Migrated']).draw(false);
        responseTable.row.add(['CSV Data 03', 'Schema Information', 'CSV Data 03', 'Not Migrated']).draw(false);
        responseTable.row.add(['CSV Data 03', 'Column Metadata', 'CSV Data 03', 'Success']).draw(false);
        responseTable.row.add(['CSV Data 03', 'Producer DataFlow', 'ZZZTestData_', 'Not Migrated']).draw(false);

    },

    initResponseDataTableV2: function () {
        var groupColumn = 0;
        var responseTable = $('#migrationResponseTable_V2').DataTable({
            ordering: false,
            searching: false,
            paging: true,
            columnDefs: [{ visible: false, targets: groupColumn }],
            drawCallback: function (settings) {
                var api = this.api();
                var rows = api.rows({ page: 'current' }).nodes();
                var last = null;

                api
                    .column(groupColumn, { page: 'current' })
                    .data()
                    .each(function (group, i) {
                        if (last !== group) {
                            $(rows)
                                .eq(i)
                                .before('<tr class="group"><td colspan="4">' + group + '</td></tr>');

                            last = group;
                        }
                    });
            }
        });

        responseTable.row.add(['ZZZ Test Data', 'Dataset Information', 'My Dataset', 'Not Migrated']).draw(false);
        responseTable.row.add(['CSV Data 02', 'Schema Information', 'CSV Data 02', 'Not Migrated']).draw(false);
        responseTable.row.add(['CSV Data 02', 'Column Metadata', 'CSV Data 02', 'Success']).draw(false);
        responseTable.row.add(['CSV Data 02', 'Producer DataFlow', 'ZZZTestData_CS', 'Not Migrated']).draw(false);
        responseTable.row.add(['JSON Data 02', 'Schema Information', 'JSON Data 02', 'Not Migrated']).draw(false);
        responseTable.row.add(['JSON Data 02', 'Column Metadata', 'JSON Data 02', 'Success']).draw(false);
        responseTable.row.add(['JSON Data 02', 'Producer DataFlow', 'ZZZTestData_JS', 'Not Migrated']).draw(false);
        responseTable.row.add(['XML Data 02', 'Schema Information', 'XML Data 01', 'Not Migrated']).draw(false);
        responseTable.row.add(['XML Data 02', 'Column Metadata', 'XML Data 01', 'Success']).draw(false);
        responseTable.row.add(['XML Data 02', 'Producer DataFlow', 'ZZZTestData_X', 'Not Migrated']).draw(false);
        responseTable.row.add(['CSV Data 03', 'Schema Information', 'CSV Data 03', 'Not Migrated']).draw(false);
        responseTable.row.add(['CSV Data 03', 'Column Metadata', 'CSV Data 03', 'Success']).draw(false);
        responseTable.row.add(['CSV Data 03', 'Producer DataFlow', 'ZZZTestData_', 'Not Migrated']).draw(false);

    }
}