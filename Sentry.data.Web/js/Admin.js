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
                    className: 'dropdown-control'
                },
                {
                    targets: [0, 1],
                    orderable: false
                }
            ],
            order: [],
            drawCallback: function () {
                $('#data-file-select-all').prop('checked', false);
                $('.select-all-target').prop('checked', false);
            }
        });

        // click event logic for table row + and - icons based on dropdown state
        $(".dropdown-control").click(function () {
            var targetId = $(this).parents("tr").data("file-id");
            var childRow = $(`#dropdown-toggle-${targetId}`);

            if (!$(this).parents("tr").hasClass("parent")) {
                childRow.removeClass("fa-plus");
                childRow.addClass("fa-minus");
            } else {
                childRow.addClass("fa-plus");
                childRow.removeClass("fa-minus");
            }
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

    RetrieveDeadSparkJobListButton: function () {

        // Get all dead spark jobs within chosen time span
        $("#timeCheck").click(function () {

            // Ensure table parent div is empty
            $("#deadJobTable").html("");

            // Show spinner 
            $("#tab-spinner").show();

            // Retrieve seleced date
            var selectedDate = $('#datetime-picker').val();

            // Calculate hours between current date and selected date
            var timeCheck = Math.floor(Math.abs((new Date() - new Date(selectedDate)) / 36e5)) + 1;

            // Check if selected date is within a month (720hrs) of current date
            if (timeCheck > 720 || timeCheck < 0) {
                data.Dataset.makeToast("error", `Date selected must be within a month of current date`);
            } else {
                $.ajax({
                    type: "GET",
                    url: "Admin/GetDeadJobs?selectedDate=" + encodeURIComponent(selectedDate),
                    dataType: "html",
                    success: function (msg) {
                        // Hide spinner
                        $("#tab-spinner").hide();

                        // Append table to parent div
                        $("#deadJobTable").html(msg);
                    },
                    error: function (req, status, error) {
                        alert("Error try again");
                    }
                });
            }
        });
    },

    // group selected jobs by Dataset Name, Schema Name & DataFlowStepId and send them to be reprocessed
    ReprocessDeadJobs: function () {

        // defines logic for select all checkbox 
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

        // submits selected jobs to be reprocessed
        $("#reprocessButton").click(function () {
            var files = [];

            // grabs the file & step id's of all checked rows
            $('.select-all-target:checked').each(function () {
                var obj = { fileId: $(this).data("file-id"), groupId: $(this).data("group-id") };
                files.push(obj);
            });

            // groups file id's by step id's and stores them in a JSON object
            var returnJson = files.reduce(function (rv, x) {
                (rv[x["groupId"]] = rv[x["groupId"]] || []).push(x["fileId"]);
                return rv;
            }, {});

            var tempArr = [];

            // loop through all json keys and POST them to the data file reprocess controller
            for (let x in returnJson) {

                tempArr = x.split("/");

                $.ajax({
                    type: "POST",
                    url: "../../api/v2/datafile/DataFile/Reprocess",
                    // Async has been set to false in order to await callback functions before the next iteration over the returnJson array
                    async: false,
                    contentType: "application/json",
                    data: JSON.stringify({ DataFlowStepId: tempArr[2], DatasetFileIds: returnJson[x] }),
                    success: function () {
                        data.Admin.makeToast("success", `Selected file(s) for DATASET: ${tempArr[0]} & SCHEMA: ${tempArr[1]} were posted for reprocessing.`);
                    },
                    error: function (msg) {
                        data.Admin.makeToast("error", `Selected file(s) for DATASET ("${tempArr[0]}") & SCHEMA ("${tempArr[1]}") could not be posted for reprocessing. Please try again`);
                    }
                });
            };
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
    },

    // makeToast config
    makeToast: function (severity, message) {

        if (severity === 'success') {
            toastr.options = {
                "closeButton": false,
                "debug": false,
                "newestOnTop": false,
                "progressBar": false,
                "positionClass": "toast-top-right",
                "preventDuplicates": false,
                "onclick": null,
                "showDuration": "300",
                "hideDuration": "1000",
                "timeOut": "5000",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut"
            };
        }
        else {
            toastr.options = {
                "closeButton": true,
                "debug": false,
                "newestOnTop": false,
                "progressBar": false,
                "positionClass": "toast-top-full-width",
                "preventDuplicates": false,
                "onclick": null,
                "showDuration": "300",
                "hideDuration": "1000",
                "timeOut": "0",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut"
            };
        }

        toastr[severity](message);
    }
}

