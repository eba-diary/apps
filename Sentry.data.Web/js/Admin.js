/******************************************************************************************
 * Javascript methods for the Admin-related pages
 ******************************************************************************************/

data.Admin = {
    // Approve.vbhtml
    ApproveInit: function () {
        $("[id^='Approve_']").on("click", function () {
            data.Admin.ApproveAsset($(this).data("id"));
        });
    },

    ApproveAsset: function (id) {
        $.post("/Admin/Approve/" + id, {}, function () {
            $("#approve-row-" + id).hide("slow");
        }).fail(function () {
            alert("An error occurred approving this asset.");
        });
    },

    // Complete.vbhtml

    CompleteInit: function () {
        $("[id^='Complete_']").on("click", function () {
            data.Admin.CompleteAuction($(this).data("id"));
        });
    },

    // creates url for ajax call to get schema associated with selected dataset
    GetSchemaUrl: function (datasetId) {
        var url = "../../api/v2/metadata/dataset/" + datasetId + "/schema";
        return url;
    },

    // creates schema dropdown for selected dataset
    GetSchemaDropdown: function (url) {
        $.ajax({
            type: "GET",
            url: url,
            success: function (data) {
                $("#schemaDropdown").materialSelect({ destroy: true });
                var s = '<option id="defaultSchemaSelection" selected value="-1">Please Select a Schema</option>';
                for (var d of data) {
                    s += '<option value="' + d.SchemaId + '">' + d.Name + '</option>';
                }
                $("#schemaDropdown").html(s);
                // proof of concept, alternate method of input validation for dropdown menues rather than current if(selected val!=-1) 
                $("#defaultSchemaSelection").prop("disabled", true);
                $("#schemaDropdown").materialSelect();
            }
        });
    },

    // creates dataset file dropdown for selected dataset
    GetDatasetFileDropdown: function (url, isBaseInit) {
        // Check if the dataset file drop down is being reinitialized on a dataset select dropwdown change
        if (isBaseInit) {
            $("#datasetFileDropdown").materialSelect({ destroy: true });
            var s = '<option id="defaultDatasetFileSelection" selected value="-1">Please Select a File</option>';

            $("#datasetFileDropdown").html(s);
            $("#defaultDatasetFileSelection").prop("disabled", true);
            $("#datasetFileDropdown").materialSelect();
        } else {
            $.ajax({
                type: "GET",
                url: url,
                success: function (data) {
                    $("#datasetFileDropdown").materialSelect({ destroy: true });
                    var s = '<option id="defaultDatasetFileSelection" selected value="-1">Please Select a File</option>';

                    for (var d of data.Records) {
                        s += '<option value="' + d.DatasetFileId + '">' + d.FileName + '</option>';
                    }

                    $("#datasetFileDropdown").html(s);
                    $("#defaultDatasetFileSelection").prop("disabled", true);
                    $("#datasetFileDropdown").materialSelect();
                }
            });
        }
    },

    // creates url for Ajax call to get data files
    GetFileUrl: function (datasetId, schemaId) {
        var url = "../../api/v2/datafile/dataset/" + datasetId + "/schema/" + schemaId + "?pageNumber=1&pageSize=1000";
        return url;
    },

    // generates table with datafiles from selected dataset and schema
    PopulateTable: function (url) {
        $("#results").DataTable({
            destroy: true,
            ajax: {
                url: url,
                dataSrc: "Records",
            },
            columns: [
                { data: "DatasetFileId" },
                { data: "FileName" },
                { data: "UploadUserName" },
                { data: "CreateDTM" },
                { data: "ModifiedDTM" },
                { data: "FileLocation" },
                {
                    data: null,
                    render: (d) => function (data, type, row) {
                        return '<center><input type="checkbox" id= checkbox' + d.DatasetFileId + ' class="form-check-input select-all-target" data-fileId = ' + d.DatasetFileId + '><label for=checkbox' + d.DatasetFileId + ' class="form-check-label"></label><center>';
                    }
                },
            ],
            searchable: true,
        });
    },

    GetFlowStepUrl: function (schemaId) {
        var url = "../../api/v2/dataflow?schemaId=" + schemaId;
        console.log(url);
      // need to see new api url structure before continuing
        return url;
    },
     
    // creates dropdown menu for flowsteps based on selected dataset and schema ***unfinished and unimplemented***
    GetFlowStepDropdown: function (url) {
        $.ajax({
            type: "GET",
            url: url,
            success: function (data) {
                $("#flowStepsDropdown").materialSelect({ destroy: true });
                var s = '<option value="-1">Please Select a Flow Step</option>';
                for (var d of data[0].steps) {
                    s += '<option value="' + d.Id + '">' + d.ActionName + '</option>';
                }
                $("#flowStepsDropdown").html(s);
                $("#flowStepsDropdown").materialSelect();
            },
        });
    },
    // activate or deactivate reprocess button based on input list of checked boxes
    ActivateDeactivateReprocessButton: function () {
        var checkedBoxes = $(".select-all-target:checkbox:checked");
        if (checkedBoxes.length > 0 && checkedBoxes.length <= 100 && $("#flowStepsDropdown").find(":selected").val() != "-1") {
            $("#reprocessButton").prop("disabled", false);
        }
        else {
            $("#reprocessButton").prop("disabled", true);
        }
    },

    // activate or deactivate reprocess button based on input list of checked boxes
    ActivateDeactivateAuditSearchButton: function () {
        console.log("calls")

        var searchStatus = false;

        $("select.admin-audit-dropdown.active").each(function () {
            console.log($(this).find(":selected").val());
            if ($(this).find(":selected").val() == null || $(this).find(":selected").val() == "" || $(this).find(":selected").val() == "-1") {
                searchStatus = true;
                return false;
            } 

            console.log(searchStatus);
            console.log($(this).attr("id"));
        })

        if (searchStatus) {
            $("#auditSearchButton").prop("disabled", searchStatus);
        } else {
            $("#auditSearchButton").prop("disabled", searchStatus);
        }
    },

    /*// activate or deactivate reprocess button based on input list of checked boxes
    ActivateDeactivateReprocessButton: function () {
        var checkedBoxes = $(".select-all-target:checkbox:checked");
        if (checkedBoxes.length > 0 && checkedBoxes.length <= 100) {
            $("#auditReprocessButton").prop("disabled", false);
        }
        else {
            $("#auditReprocessButton").prop("disabled", true);
        }
    },*/

    AuditReprocessButton: function () {
        // activate or deactivate button
        $("#AuditTable").on("click", ".generic-checkbox", function () {
            data.Admin.ActivateDeactivateReprocessButton();
        });

        $("#auditReprocessButton").click(function () {
            let flowStep = $("#AuditTable").data("schema-group");

            var filesToReprocess = [];

            $(".select-all-target:checkbox:checked").each(function () {
                filesToReprocess.push($(this).data("file-id"));
            });

            $.ajax({
                type: "POST",
                url: "../../api/v2/datafile/DataFile/Reprocess",
                contentType: "application/json",
                data: JSON.stringify({ DataFlowStepId: flowStep, DatasetFileIds: filesToReprocess }),
                success: function () {
                    data.Dataset.makeToast("success", "File Id(s) " + filesToReprocess + " posted for reprocessing at flow step " + flowStep + ".")
                },
                error: function () {
                    data.Dataset.makeToast("error", "Selected file(s) could not be posted for reprocessing. Please try again.")
                }

            })
        });
    },

    AuditInit: function () {
        $("#AllDatasets").materialSelect();
        $("#AllAuditTypes").materialSelect();
        $("#AllAuditSearchTypes").materialSelect();
        $("#schemaDropdown").materialSelect();
        $("#datasetFileDropdown").materialSelect();

        $("#AllDatasets").change(function (event) {
            var datasetId = $("#AllDatasets").find(":selected").val();
            if (datasetId != "") {
                var url = data.Admin.GetSchemaUrl(datasetId);
                data.Admin.GetSchemaDropdown(url);
            }

            // Init an empty Dataset File Dropwdown on dataset selection change
            data.Admin.GetDatasetFileDropdown("empty", true);
            
        });

        $("#schemaDropdown").change(function (event) {
            var datasetId = $("#AllDatasets").find(":selected").val();
            var schemaId = $("#schemaDropdown").find(":selected").val();
            if (datasetId != "" && schemaId != "") {
                var url = data.Admin.GetFileUrl(datasetId, schemaId);
                data.Admin.GetDatasetFileDropdown(url, false);
            }
        });

        // Activates the selected search type element
        $("#AllAuditSearchTypes").change(function () {
            console.log("search type defined")
            // Define elements
            var searchId = $(this).find(":selected").val();

            var currentAuditSelection = $(`#audit-selection-${searchId}`);

            var selection = $(".audit-search-selection");
            var dropdown = $(".admin-audit-dropdown");

            // Set's all search elements to hidden
            selection.removeClass("active");
            selection.addClass("hidden");

            // Set's selected search element to active
            currentAuditSelection.removeClass("hidden");
            $(`#audit-selection-${searchId} .admin-audit-dropdown`).removeClass("hidden");

            currentAuditSelection.addClass("active")
            $(`#audit-selection-${searchId} .admin-audit-dropdown`).addClass("active")
        });

        $("select.admin-audit-dropdown").change(function () {
            data.Admin.ActivateDeactivateAuditSearchButton();
        });

        $("#auditSearchButton").click(function () {
            var elementStatus = true;
            var elementState = true;

            $.ajax({
                type: "POST",
                url: "Admin/GetAuditTableResults",
                contentType: "application/json",
                data: JSON.stringify({ "schemaId": schemaId, "datasetId": datasetId, "auditId": auditId }),
                success: function (result) {
                    $("#AuditResultTable").html(result);
                },
                error: function () {
                    data.Dataset.makeToast("error", "Selected Audit function has failed to run. Please try again.")
                }
            }) 
        });
    },

    AuditTableInit: function () {
        $('#AuditTable').DataTable({
            columnDefs: [
                {
                    targets: [0],
                    orderable: false
                }
            ],
            drawCallback: function () {
                $('#data-file-select-all').prop('checked', false);
                $('.select-all-target').prop('checked', false);
            }
        });

        $("#data-file-select-all").click(function (event) {
            var selectAllCheckbox = $(this);
            if (selectAllCheckbox.is(":checked")) {
                $(".select-all-target").each(function () {
                    $(this).prop("checked", true);
                });
            }
            else {
                $(".select-all-target").each(function () {
                    $(this).prop("checked", false);
                });
            }
        });
    },
    // loads reprocessing page with event handlers
    ReprocessInit: function () {
        $("#AllDatasets").materialSelect();
        $("#schemaDropdown").materialSelect()
        $("#flowStepsDropdown").materialSelect();
        $("#AllDatasets").change(function (event) {
            var datasetId = $("#AllDatasets").find(":selected").val();
            if (datasetId != "") {
                var url = data.Admin.GetSchemaUrl(datasetId);
                data.Admin.GetSchemaDropdown(url);
            }

        });
        $("#schemaDropdown").change(function (event) {
            var schemaId = $("#schemaDropdown").find(":selected").val();
            var datasetId = $("#AllDatasets").find(":selected").val();
            if (schemaId != "" && datasetId != "") {
                var url = data.Admin.GetFileUrl(datasetId, schemaId);
                data.Admin.PopulateTable(url);
                url = data.Admin.GetFlowStepUrl(schemaId);
                data.Admin.GetFlowStepDropdown(url);
            }
        });
        // activate or deactivate button
        $("#results").on("click", ".select-all-target", function () {

            data.Admin.ActivateDeactivateReprocessButton();
        });
        // submit selected file list
        $("#reprocessButton").click(function (event) {
            var filesToReprocess = [];
            $(".select-all-target:checkbox:checked").each(function () {
                var checkbox = $(this);
                filesToReprocess.push(checkbox.data("fileid"));
            });
            var flowStep = $("#flowStepsDropdown").find(":selected").val();
            $.ajax({
                type: "POST",
                url: "../../api/v2/datafile/DataFile/Reprocess",
                contentType: "application/json",
                data: JSON.stringify({ DataFlowStepId: flowStep, DatasetFileIds: filesToReprocess }),
                success: function () {
                    data.Dataset.makeToast("success", "File Id(s) " + filesToReprocess + " posted for reprocessing at flow step " + flowStep + ".")
                },
                error: function () {
                    data.Dataset.makeToast("error", "Selected file(s) could not be posted for reprocessing. Please try again.")
                }
                
            })
        });
        // activate or deactivate button
        $("#flowStepsDropdown").change(function (event) {
            data.Admin.ActivateDeactivateReprocessButton();
        });
        
         // Uncomment this block and and replace final column header in _DataFileReprocessing.cshtml to activate select all functionality.
        /*
        $("#selectAll").click(function (event) {
            var selectAllCheckbox = $(this);
            if (selectAllCheckbox.is(":checked")) {
                $(".select-all-target").each(function () {
                    $(this).prop("checked", selectAllCheckbox.is(":checked"));
                });
            }
            else {
                $(".select-all-target").each(function () {
                    $(this).prop("checked", selectAllCheckbox.is(":checked"));
                });
            }

            console.log(filesToReprocess);
        });
        */
        
    },
  
    // Loads Admin jobs pages

    AdminPageInit: function () {
        $(".load-partial-view").click(function (event) {
            event.preventDefault();
            var url = $(this).data("url");
            $("#partial-view-test").load(url);
        });
    },

    SetupConnectorFilterTableInit: function () {
        $(document).ready(function () {
            $('#connector-status-table').DataTable({
                'columnDefs': [{
                    'targets': [2, 3], /* column index */
                    'orderable': false, /* true or false */
                }]
            });
        });
    },

    RetrieveModalDataInit: function () {
        $(".modalViewBtn").click(function () {
            var connectorName = $(this).data("name");
            var actionUrl = $(this).data("action");

            $("#modalViewLabel").text(`${connectorName} Info`);

            $.ajax({
                type: "POST",
                url: `Admin/${actionUrl}`,
                traditional: true,
                dataType: "json",
                data: { ConnectorId: connectorName },
                success: function (data) {
                    var json = JSON.stringify(data, null, "\t");

                    $(".modal-body").html(`<textarea style="resize:both;" rows="20" class="w-100" readonly=true>${json}</textarea>`);

                },
                failure: function (errMsg) {
                    alert(errMsg);
                }
            });
        });
    }
}

