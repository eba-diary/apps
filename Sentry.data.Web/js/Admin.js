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

    // load and initialize dead job data table
    DeadJobTableInit: function () {

        $('#deadJobs').DataTable({
            responsive: {
                details: {
                    type: 'column',
                    target: '.dropdown-control',
                    renderer: function (api, rowIdx, columns) {
                        var data = $.map(columns, function (col, i) {
                            return col.hidden ?
                                '<tr data-dt-row="' + col.rowIndex + '" data-dt-column="' + col.columnIndex + '">' +
                                '<td>' + col.title + ':' + '</td> ' +
                                '<td>' + col.data + '</td>' +
                                '</tr>' :
                                '';
                        }).join('');

                        return data ?
                            $('<table/>').append(data) :
                            false;
                    }
                }
            },
            columnDefs: [
                {
                    targets: [0, 2, 3, 4, 5, 6, 7, 8, 9],
                    className: 'dropdown-control dropdown-target'
                },
                {
                    targets: [0, 1],
                    orderable: false
                }
            ],
            order: [],
            drawCallback: function () {
                $('#data-file-select-all').prop('checked', false);
            }
        });

        // click event logic for table row + and - icons based on dropdown state
        $(".dropdown-target").click(function () {
            var targetId = $(this).parents("tr").data("file-id");

            if (!$(this).parents("tr").hasClass("parent")) {
                $(`#${targetId}`).removeClass("fa-plus");
                $(`#${targetId}`).addClass("fa-minus");
            } else {
                $(`#${targetId}`).addClass("fa-plus");
                $(`#${targetId}`).removeClass("fa-minus");
            }
        });
    },

    // group selected jobs by DataFlowStepId and send them to be reprocessed
    ReprocessDeadJobs: function () {
        $("#data-file-select-all").click(function (event) {
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
        });

        $("#reprocessButton").click(function () {
            var files = [];

            // grabs the file & step id's of all checked rows
            $('.select-all-target:checked').each(function () {
                var obj = { fileId: $(this).data("file-id"), stepId: $(this).data("step-id") };
                files.push(obj);
            });

            // groups file id's by step id's and stores them in a JSON object
            var returnJson = files.reduce(function (rv, x) {
                (rv[x["fileId"]] = rv[x["fileId"]] || []).push(x["stepId"]);
                return rv;
            }, {});

            var totalJobCount = 0;
            var successfulJobCount = 0;

            // loop through all json keys and POST them to the data file reprocess controller
            for (let x in returnJson) {
                $.ajax({
                    type: "POST",
                    url: "../../api/v2/datafile/DataFile/Reprocess",
                    contentType: "application/json",
                    data: JSON.stringify({ DataFlowStepId: x, DatasetFileIds: returnJson[x] }),
                    success: function () {
                        /*alert("success", "File Id(s) " + filesToReprocess + " posted for reprocessing at flow step " + flowStep + ".");*/
                        totalJobCount++;
                        successfulJobCount++;
                    },
                    error: function () {
                        totalJobCount++;

                        if (Object.keys(returnJson).length == totalJobCount) alert(`${successfulJobCount} out of ${totalJobCount} reprocessing jobs were successful`);
                    }
                });
            };
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
        console.log(url);
        return url;
    },

    // creates schema dropdown for selected dataset
    GetSchemaDropdown: function (url) {
        $.ajax({
            type: "GET",
            url: url,
            success: function (data) {
                $("#schemaDropdown").materialSelect({ destroy: true });
                var s = '<option value="-1">Please Select a Schema</option>';
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

    // creates url for Ajax call to get data files
    GetFileUrl: function (datasetId, schemaId) {
        var url = "../../api/v2/datafile/dataset/" + datasetId + "/schema/" + schemaId + "?pageNumber=1&pageSize=1000";
        console.log(url);
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
    }
}

