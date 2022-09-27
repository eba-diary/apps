﻿/******************************************************************************************
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
                    targets: [0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14],
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
        return "../../api/v2/metadata/dataset/" + datasetId + "/schema";
    },

    // creates schema dropdown for selected dataset
    GetSchemaDropdown: function (url) {
        $.ajax({
            type: "GET",
            url: url,
            success: function (data) {
                data.sort(function (a, b) {
                    if (a.Name < b.Name) {
                        return -1;
                    }
                    else if (a.Name > b.Name) {
                        return 1;
                    }
                    else {
                        return 0;
                    }
                });
                var scheamDropdown = '<option id="defaultSchemaSelection" selected value="-1">Please Select a Schema</option>';
                scheamDropdown += '<option value="-1">Please Select a Schema</option>';

                for (var d of data) {
                    scheamDropdown += '<option value="' + d.SchemaId + '">' + d.Name + '</option>';
                }

                $("#schemaDropdown").html(scheamDropdown);
                // proof of concept, alternate method of input validation for dropdown menues rather than current if(selected val!=-1) 
                $("#defaultSchemaSelection").prop("disabled", true);
                $("#schemaDropdown").materialSelect();
            }
        });
    },

    // creates dataset file dropdown for selected dataset
    GetDatasetFileDropdown: function (url, isBaseInit) {
        // Check if the dataset file drop down is being reinitialized on a dataset select dropwdown change
        var datasetFileDropdown = "";

        if (isBaseInit) {
            $("#datasetFileDropdown").materialSelect({ destroy: true });
            datasetFileDropdown = '<option id="defaultDatasetFileSelection" selected value="-1">Please Select a File</option>';

            $("#datasetFileDropdown").html(datasetFileDropdown);
            $("#defaultDatasetFileSelection").prop("disabled", true);
            $("#datasetFileDropdown").materialSelect();
        } else {
            $.ajax({
                type: "GET",
                url: url,
                success: function (data) {
                    $("#datasetFileDropdown").materialSelect({ destroy: true });
                    datasetFileDropdown = '<option id="defaultDatasetFileSelection" selected value="-1">Please Select a File</option>';

                    for (var d of data.Records) {
                        datasetFileDropdown += '<option value="' + d.DatasetFileId + '">' + d.FileName + '</option>';
                    }

                    $("#datasetFileDropdown").html(datasetFileDropdown);
                    $("#defaultDatasetFileSelection").prop("disabled", true);
                    $("#datasetFileDropdown").materialSelect();
                }
            });
        }
    },


    GetFileUrl: function (datasetId, schemaId) {
    // creates url for Ajax call to get data files
        return "../../api/v2/datafile/dataset/" + datasetId + "/schema/" + schemaId + "?pageNumber=1&pageSize=1000&sortDesc=true&fileStatusType=ActiveFiles";
    },

    GetFileDropdown: function (url) {
        $.ajax({
            type: "GET",
            url: url,
            dataType: "Json",
            dataSrc: "Records",
            success: function (data) {
                $("#fileDropdown").materialSelect({ destroy: true });
                var s = '<option value="-1"id="defaultFileSelection">All Files</option>';
                for (var d of data.Records) {
                    s+= '<option value="' + d.DatasetFileId + '">' + d.FileName + '</option>'
                }
                $("#fileDropdown").html(s);
                $("#fileDropdown").materialSelect();
            }
        })
    },

    //generates table with datafiles from selected dataset and schema
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
        return "../../api/v2/dataflow?schemaId=" + schemaId;
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
                    if (d.ActionName == "Raw Storage") {
                        s += '<option value="' + d.Id + '">' + d.ActionName + '</option>';
                    }
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

            // Checks if any acive dropdown items are not selected
            if ($(this).find(":selected").val() == null || $(this).find(":selected").val() == "" || $(this).find(":selected").val() == "-1") {
                searchStatus = true;

                // Break from each loop
                return false;
            } 

            console.log(searchStatus);
            console.log($(this).attr("id"));
        })

        $("#auditSearchButton").prop("disabled", searchStatus);
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
            var datasetId = $("#AllDatasets").find(":selected").val();
            var schemaId = $("#schemaDropdown").find(":selected").val();
            var auditId = $("#AllAuditTypes").find(":selected").val();

            let searchTypeId = $("#AllAuditSearchTypes").find(":selected").val();

            var searchParameter;

            // Post check is defaulted to true
            var postCheck = true;

            switch (searchTypeId) {
                case "0":
                    searchParameter = $("#datetime-picker").val();
                    postCheck = data.Admin.ReprocessJobDateRangeCheck(searchParameter, 720);
                    break;
                case "1":
                    searchParameter = $("#datasetFileDropdown").find(":selected").text();
                    break;
            }

            // Show spinner + Reprocess button
            $("#tab-spinner").show();
            $("#auditReprocessButton").show();

            if (postCheck) {
                $.ajax({
                    type: "POST",
                    url: "GetAuditTableResults",
                    contentType: "application/json",
                    data: JSON.stringify({ "datasetId": datasetId, "schemaId": schemaId, "auditId": auditId, "searchTypeId": searchTypeId, "searchParameter": searchParameter }),
                    success: function (result) {
                        $("#AuditResultTable").html(result);
                    },
                    error: function () {
                        data.Dataset.makeToast("error", "Selected Audit function has failed to run. Please try again.")
                    },
                    complete: function (msg) {
                        // Hide spinner
                        $("#tab-spinner").hide();
                    }
                })
            }
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

        admin.data.DataFileSelectAll();
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

            var timeCheck = data.Admin.ReprocessJobDateRangeCheck(selectedDate, 720);

            // Check if selected date is within a month (720hrs) of current date
            if (timeCheck) {
                $.ajax({
                    type: "GET",
                    url: "GetDeadJobs?selectedDate=" + encodeURIComponent(selectedDate),
                    dataType: "html",
                    success: function (msg) {
                        // Append table to parent div
                        $("#deadJobTable").html(msg);
                    },
                    error: function (req, status, error) {
                        alert("Error try again");
                    },
                    complete: function (msg) { 
                        // Hide spinner
                        $("#tab-spinner").hide();
                    }
                });
            }
        });
    },

    ReprocessJobDateRangeCheck: function (selectedDate, rangeMax) {
        // Calculate hours between current date and selected date
        var dateRangeToHours = Math.floor(Math.abs((new Date() - new Date(selectedDate)) / 36e5)) + 1;

        if (dateRangeToHours > rangeMax || dateRangeToHours < 0) {
            data.Dataset.makeToast("error", `Date selected must be within ${rangeMax/24} day(s) before current date`);
            return false;
        } 

        return true;
    },

    DataFileSelectAll: function () {
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
    },

    // group selected jobs by Dataset Name, Schema Name & DataFlowStepId and send them to be reprocessed
    ReprocessDeadJobs: function () {

        admin.data.DataFileSelectAll();

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

    // sets the dataset dropdown to scroll to top on click
    // this method should be considered to be refactored.
    DatasetDropdownScrollToTop: function () {
        $("input.select-dropdown.form-control").click(function () { $(this).next("ul.dropdown-content.select-dropdown").scrollTop(0) });
    },

    GetFlowEvents: function (data) {
        var s = '<table><tr><th>Event Metric ID</th><th>Flow Step Name</th><th>Execution Order</th><th>Status Code</th><th>Offset</th><th>Partition</th><th>Run Instance Guid</th><th>Event Contents</th></tr>';
        for (var flowEvent of data.FlowEvents) {
            s += '<tr><td>' + flowEvent.EventMetricId + '</td><td>' + flowEvent.DataFlowStepName + '</td><td>' + flowEvent.CurrentFlowStep + '/' + flowEvent.TotalFlowSteps + '</td><td>' + flowEvent.StatusCode + '</td><td>' + flowEvent.Offset + '</td><td>' + flowEvent.Partition + '</td><td>' + flowEvent.RunInstanceGuid + '</td><td><a href="#EventContentModal" id="EventContentLink" data-toggle="modal" data-eventContent=' + flowEvent.EventContents + '>View Contents</a></td></tr>';
        }
        s += '</table>';
        return s;
    },

    ActivateDeactivateSubmitButton: function () {
        if ($("#AllDatasets").find(":selected").val() != "" && $("#schemaDropdown").find(":selected").val() != "-1") {
            $("#submitButton").prop("disabled", false);
        }
        else {
            $("#submitButton").prop("disabled", true);
        }
    },

    // loads reprocessing page with event handlers
    ReprocessInit: function () {
        $("#AllDatasets").materialSelect();
        $("#schemaDropdown").materialSelect()
        $("#flowStepsDropdown").materialSelect();

        data.Admin.DatasetDropdownScrollToTop();

        $("#AllDatasets").change(function (event) {
            var datasetId = $("#AllDatasets").find(":selected").val();
            if (datasetId != "") {
                var url = data.Admin.GetSchemaUrl(datasetId);
                data.Admin.GetSchemaDropdown(url);

                data.Admin.DatasetDropdownScrollToTop();
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

                data.Admin.DatasetDropdownScrollToTop();
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
    //loads dataflow metric page events
    DataFlowMetricsInit: function () {
        var table;
        $("#AllDatasets").materialSelect();
        $("#schemaDropdown").materialSelect();
        $("#fileDropdown").materialSelect();

        data.Admin.DatasetDropdownScrollToTop();

        $("#AllDatasets").change(function (event) {
            var datasetId = $("#AllDatasets").find(":selected").val();
            if (datasetId != "") {
                var url = data.Admin.GetSchemaUrl(datasetId);
                data.Admin.GetSchemaDropdown(url);

                data.Admin.DatasetDropdownScrollToTop();
            }
            data.Admin.ActivateDeactivateSubmitButton();

        });
        $("#schemaDropdown").change(function (event) {
            var schemaId = $("#schemaDropdown").find(":selected").val();
            var datasetId = $("#AllDatasets").find(":selected").val();
            if (schemaId != -1 && datasetId != "") {
                var url = data.Admin.GetFileUrl(datasetId, schemaId);
                data.Admin.GetFileDropdown(url);

                data.Admin.DatasetDropdownScrollToTop();
            }
            data.Admin.ActivateDeactivateSubmitButton();
        });
        $("#EventContentModal").on("show.bs.modal", function (event) {
            var link = $(event.relatedTarget);
            var content = JSON.stringify(link.data("eventcontent"), null, "\t");
            var modalBody = "<textarea class='form-control' style = 'height: 500px'>" + content + "</textarea>"
            $("#EventDetails").html(modalBody);

        })
        $("#submitButton").click(function (event) {
            var dto = new Object();
            dto.DatasetFileId = $("#fileDropdown").find(":selected").val();
            dto.DatasetId = $("#AllDatasets").find(":selected").val();
            dto.SchemaId = $("#schemaDropdown").find(":selected").val();
            table = $('#metricGroupsTable').DataTable({
                destroy: true,
                ajax: {
                    type: "POST",
                    url: "/DataFlowMetric/PopulateTable",
                    data: dto,
                   // contentType: "application/json",
                    dataType: "json",
                    dataSrc: "",
                },
                columns: [
                    {
                        className: 'dt-control',
                        orderable: false,
                        data: null,
                        defaultContent: '<center><em id = "expand-collapse-icon" class = "fas fa-plus"></em></center>',
                    },
                    {
                        className: 'dt-control',
                        data: 'FileName'
                    },
                    {
                        className: 'dt-control',
                        type: 'date',
                        data: 'FirstEventTime',
                        render: function (data) {
                            return data ? moment(data).format("MM/DD/YYYY h:mm:ss a") : null;
                        }
                    },
                    {
                        className: 'dt-control',
                        type: 'date',
                        data: 'LastEventTime',
                        render: function (data) {
                            return data ? moment(data).format("MM/DD/YYYY h:mm:ss a") : null;
                        }
                    },
                    {
                        className: 'dt-control',
                        data: 'Duration'
                    },
                    {
                        className: 'dt-control',
                        data: null,
                        render: (d) => function (data, type, row) {
                            if (d.AllEventsPresent && d.AllEventsComplete) {
                                return '<center><em class="fas fa-check" style="color: green"></em></center>';
                            }
                            else if (!d.AllEventsPresent && d.AllEventsComplete) {
                                return '<center><em class="fas fa-clock"></em></center>';
                            }
                            else {
                                return '<center><em class="fas fa-times-circle" style="color: red"></em></center>';
                            }
                        }
                    }
                ],
            });
        })
        // Add event listener for opening and closing details
        $('#metricGroupsTable').on('click', 'td.dt-control', function () {
            var tr = $(this).closest('tr');
            var row = table.row(tr);
            var icon = $(this).closest('tr').find("#expand-collapse-icon");

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
                icon.addClass('fa-plus')
                icon.removeClass('fa-minus')
            } else {
                // Open this row
                row.child(data.Admin.GetFlowEvents(row.data())).show();
                tr.addClass('shown');
                icon.addClass('fa-minus')
                icon.removeClass('fa-plus')
            }
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
                url: `${actionUrl}`,
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

