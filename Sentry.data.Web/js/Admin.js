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

    SideBarUtility: function ()
    {
        var currentHref = $(location).attr("href");

        // Gets title of current page
        var navID = /[^/]*$/.exec(currentHref)[0].toLowerCase();

        // Selects nav bar item that matches the current page title and sets the link to bold + blue font
        $("#sideNav").find(`[data-nav-id='${navID}']`).addClass("font-weight-bold text-primary");
        
    },

    // Reduce and group json fields by the specified key
    JsonReduce: function (jsonObject, groupKey, itemsKey) {    
        
        return jsonObject.reduce(function (returnedObject, iterationVariable) {

            // Checks if the current iteration variable object already exists inside of the returnedObject
            if (!returnedObject[iterationVariable[groupKey]]) {

                // If it does not, create a new new object inside of the returnedObject
                returnedObject[iterationVariable[groupKey]] = [];
            }

            // Push the current interation variable (which will only be the value defined by the items key passed in)
            // into the correlated location inside of the returnedObject.
            returnedObject[iterationVariable[groupKey]].push(iterationVariable[itemsKey]);

            // Returns the returnedObject for the next iteration
            return returnedObject;
        }, {});

    },

    // load and initialize dead job data table
    DeadJobTableInit: function (selectedDate) {
        data.Admin.DeadJobTable = $('#deadJobs').DataTable({
            ajax: {
                url: data.Admin.GetDeadJobResultsUrl(selectedDate),
                type: "POST"
            },
            searching: true,
            paging: true,
            orderCellsTop: true,
            iDisplayLength: 10,
            order: [4, 'desc'],
            aLengthMenu: [
                [10, 25, 50, 100, 200, 1000],
                [10, 25, 50, 100, 200, 1000]
            ],
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", searchable: false },
                { data: null, name: "jobSelect", className: "jobSelect text-center", render: (d) => data.Admin.renderDeadJobSelectOptions(d), searchable: false, orderable: false },
                { data: "SubmissionTime", type: "date", className: "submissionTime", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss a") : null; } },
                { data: "DatasetName", className: "datasetName" },
                { data: "ReprocessingRequired", className: "reprocessingRequired"},
                { data: "SchemaName", className: "sourceName" },
                { data: "SourceKey", className: "sourceKey" },
                { data: "FlowExecutionGuid", className: "flowexecutionGuid" }
            ],
            'rowCallback': function (row, data)
            {
                if (data.ReprocessingRequired)
                {
                    $('td:eq(4)', row).text("True").addClass('text-danger');
                } else {
                    $('td:eq(4)', row).text("False").addClass('text-success');
                }
            },
            drawCallback: function ()
            {
                $('#data-file-select-all').prop('checked', false);
                $('.select-all-target').prop('checked', false);
            }
        });

        // intialize filters

        yadcf.init(data.Admin.DeadJobTable,
            [
                {
                    column_number: 2,
                    filter_type: 'range_date',
                    datepicker_type: null, // disable Yadcf datepicker
                    moment_date_format: 'MM/DD/YYYY h:mm:ss a',
                    filter_reset_button_text: false,
                    filter_delay: 500
                }
            ],
            {
                filters_tr_index: 1
            }
        );

        data.Admin.deadJobTableInitEvents(data.Admin.DeadJobTable);
    },

    deadJobTableInitEvents: function (deadJobTable)
    {
        $('#deadJob-reprocessing-filter').materialSelect();

        $("#deadJob-reprocessing-filter").change(function ()
        {
            var table = $("#deadJobs").DataTable();

            var selectedValue = $(this).find(":selected").val();

            // Search for rows matching the current select value (corresponding reprocess required status)
            table.column(4).search(selectedValue).draw();
        });


        $(".yadcf-filter-range-date", deadJobTable.settings()[0].nTHead).pickadate({
            format: 'mm/dd/yyyy',
            formatSubmit: 'mm/dd/yyyy',
            onSet: function (context)
            {
                // Grab selected date values
                var startDate = $(".yadcf-filter-range-start").val();
                var endDate = $(".yadcf-filter-range-end").val();

                if (startDate != '' && endDate != '')
                {
                    var formattedStart = moment(startDate);
                    var formattedEnd = moment(endDate);

                    $.fn.dataTable.ext.search.pop();
                    deadJobTable.draw();


                    $.fn.dataTable.ext.search.push(
                        function (settings, data, dataIndex)
                        {
                            var rowDate = moment(data[2]);

                            return rowDate.isBetween(formattedStart, formattedEnd);
                        }
                    );

                    deadJobTable.draw();
                }
            }
        });

        $('#deadJobs tbody').on('click', 'td.details-control', function ()
        {
            var tr = $(this).closest('tr');
            var row = $('#deadJobs').DataTable().row(tr);

            if (row.child.isShown())
            {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else
            {
                // Open this row
                row.child(data.Admin.formatDeadJobDetails(row.data())).show();
                tr.addClass('shown');
            }
        });
    },

    // Renders checkboxes for dead job table
    renderDeadJobSelectOptions: function (d)
    {
        var checkboxId = 'dead-job-select-' + d.SubmissionID;

        return '<fieldset class="form-group mb-0 text-left">' +
            '<input type="checkbox" id="' + checkboxId + '" data-id="' + d.SubmissionID + '" class="form-check-input dead-job-select-checkbox select-all-target" >' +
            '<label for="' + checkboxId + '" class="form-check-label p-0"></label>' +
            '</fieldset >';
    },

    // Creates extension to the dead job table to allow for exapandable accordion
    formatDeadJobDetails: function (d)
    {
        // `d` is the original data object for the row
        var table = '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">';
        
        table +=
            '<tr>' +
            '<td><b>Dataset File ID</b>: </td>' +
            '<td>' + d.DatasetFileID + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>Livy Spark Ui Url</b>:</td>' +
            '<td>' + d.LivySparkUiUrl + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>Livy Driver Log Url</b>: </td>' +
            '<td>' + d.LivyDriverlogUrl + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>Livy App ID</b>: </td>' +
            '<td>' + d.LivyAppID + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>Batch ID</b>: </td>' +
            '<td>' + d.BatchID + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>Submission ID</b>: </td>' +
            '<td>' + d.SubmissionID + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>Run Instance Guid</b>: </td>' +
            '<td>' + d.RunInstanceGuid + '</td>' +
            '</tr>' +

            '</table>';

        return table;
    },

    // creates url for ajax call to get schema associated with selected dataset
    GetSchemaUrl: function (datasetId) {
        return "../../api/" + data.GetApiVersion() + "/metadata/dataset/" + datasetId + "/schema";
    },

    GetFileUrl: function (datasetId, schemaId) {
    // creates url for Ajax call to get data files
        return "../../api/" + data.GetApiVersion() + "/datafile/dataset/" + datasetId + "/schema/" + schemaId + "?pageNumber=1&pageSize=1000&sortDesc=true";
    },

    GetFlowStepUrl: function (schemaId) {
        return "../../api/" + data.GetApiVersion() + "/dataflow?schemaId=" + schemaId;
    },

    // creates url for ajax call to get all dead jobs succeeding the selected date
    GetDeadJobResultsUrl: function (selectedDate)
    {
        return "GetDeadJobsForGrid?selectedDate=" + encodeURIComponent(selectedDate);
    },

    /**
    * generates table with datafiles from selected dataset and schema
    * @param {String} url
    */
    DataFileReprocessTableInit: function (url) {
        // init table with DataTable data from the provided url
        $("#dataFileTableResults").DataTable({
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
            order: [4, 'desc'],
            drawCallback: function () {
                $(".select-all-target").click(function () {
                    data.Admin.ActivateDeactivateReprocessButton();
                });
            }
        });
    },

    ResetDataFileReprocessTable: function () {
        $("#dataFileTableResults").DataTable().clear().draw();
    },

    // creates schema dropdown for selected dataset
    GetSchemaDropdown: function (url) {
        $.ajax({
            type: "GET",
            url: url,
            success: function (schemaApiResponse)
            {
                // reduces api json response to only active schemas
                schemaApiResponse = schemaApiResponse.filter(function (jsonObject)
                {
                    return jsonObject.ObjectStatus === 'ACTIVE';
                });

                // sort schema results alphabetically
                schemaApiResponse.sort(function (a, b) {
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

                for (let schema of schemaApiResponse)
                {
                    scheamDropdown += '<option value="' + schema.SchemaId + '">' + schema.Name + '</option>';
                }


                $("#AllSchemas").materialSelect({ destroy: true });

                $("#AllSchemas").html(scheamDropdown);
                $("#defaultSchemaSelection").prop("disabled", true);

                $("#AllSchemas").materialSelect();
            },
            //upon error of a schema response, a blank drop down is createed, to ensure that a previous schema dropdown does not persist
            error: function (msg) {
                var scheamDropdown = '<option id="defaultSchemaSelection" selected value="-1">Please Select a Schema</option>';


                $("#schemaDropdown").materialSelect({ destroy: true });

                $("#schemaDropdown").html(scheamDropdown);
                $("#defaultSchemaSelection").prop("disabled", true);

                $("#schemaDropdown").materialSelect();

                data.Admin.DatasetDropdownScrollToTop();
            },
            complete: function () {
                data.Admin.DatasetDropdownScrollToTop();
                data.Admin.ActivateDeactivateAuditSearchButton();
            }
        });
    },

    GetFileDropdown: function (isApiInit, url) {

        // clear file dropdown menu
        $("#fileDropdown").empty();

        if (isApiInit) {
            $.ajax({
                type: "GET",
                url: url,
                dataType: "Json",
                dataSrc: "Records",
                success: function (fileApiResponse) {
                    let fileDropdown = '<option id="defaultFileSelection" selected value="-1" data-datasetIds="[-1]">Please Select a File</option>';
                    fileDropdown += '<option value="0" data-datasetIds="[-1]">All Files</option>';

                    // groups dataset file id's by shared file names and store them in a JSON object
                    var groupedFiles = data.Admin.JsonReduce(fileApiResponse.Records, "FileName", "DatasetFileId");

                    for (let file in groupedFiles) {
                        fileDropdown += '<option data-datasetIds=\'[' + groupedFiles[file] + ']\'>' + file + '</option>'
                    }


                    $("#fileDropdown").materialSelect({ destroy: true });

                    $("#fileDropdown").html(fileDropdown);
                    $("#defaultFileSelection").prop("disabled", true);

                    $("#fileDropdown").materialSelect();
                },
                complete: function () {
                    data.Admin.DatasetDropdownScrollToTop();
                }
            });
        } else { 
            let fileDropdown = '<option id="defaultFileSelection" selected value="-1" data-datasetIds="[-1]">No Files</option>';

            $("#fileDropdown").materialSelect({ destroy: true });

            $("#fileDropdown").html(fileDropdown);
            $("#defaultFileSelection").prop("disabled", true);

            $("#fileDropdown").materialSelect();
        }
    },

    // creates dropdown menu for flowsteps based on selected dataset and schema ***unfinished and unimplemented***
    GetFlowStepDropdown: function (url) {
        $.ajax({
            type: "GET",
            url: url,
            success: function (flowStepApiResponse) {
                var flowStepDropDown = '<option value="-1">Please Select a Flow Step</option>';
                for (let flowStep of flowStepApiResponse[0].steps) {
                    if (flowStep.ActionName == "Raw Storage") {
                        flowStepDropDown += '<option value="' + flowStep.Id + '">' + flowStep.ActionName + '</option>';
                    }
                }

                $("#flowStepsDropdown").materialSelect({ destroy: true });
                $("#flowStepsDropdown").html(flowStepDropDown);
                $("#flowStepsDropdown").materialSelect();
            },
            complete: function () {
                data.Admin.DatasetDropdownScrollToTop();
            }
        });
    },

    // creates dropdown menu for flowsteps based on selected dataset and schema ***unfinished and unimplemented***
    ResetFlowStepDropdown: function () {
        var flowStepDropDown = '<option value="-1">Please Select a Flow Step</option>';
        $("#flowStepsDropdown").materialSelect({ destroy: true });
        $("#flowStepsDropdown").html(flowStepDropDown);
        $("#flowStepsDropdown").materialSelect();
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
        var searchStatus = false;

        $("select.admin-audit-dropdown.active").each(function () {
            // Checks if any acive dropdown items are not selected
            if ($(this).find(":selected").val() == null || $(this).find(":selected").val() == "" || $(this).find(":selected").val() == "-1") {
                searchStatus = true;

                // Break from each loop
                return false;
            }
        })

        $("#auditSearchButton").prop("disabled", searchStatus);
    },

    RetrieveDeadSparkJobListButton: function () {

        // Get all dead spark jobs within chosen time span
        $("#timeCheck").click(function () {

            // Ensure table parent div is empty
            $("#deadJobTable").html("");

            // Show spinner 
            $("#tab-spinner").show();

            // Retrieve seleced date
            var formattedDateString = moment(new Date($('#datetime-picker').val())).format("YYYY-MM-DDThh:mm:ss");

            // Determine if the time is within 30 day (720 hrs)
            var timeCheck = data.Admin.ReprocessJobDateRangeCheck(formattedDateString, 720);

            // Check if selected date is within a month (720hrs) of current date
            if (timeCheck) {
                $.ajax({
                    type: "GET",
                    url: "DeadJobTable",
                    dataType: "html",
                    success: function (view) {
                        // Append table to parent div
                        $("#tab-spinner").hide();
                        $("#deadJobTable").html(view);
                    },
                    error: function (msg) {
                        alert(msg);
                    },
                    complete: function (msg) {
                        // Hide spinner
                        data.Admin.DeadJobTableInit(selectedDate);
                    }
                });
            }
        });
    },

    AuditInit: function () {
        $("#AllDatasets").materialSelect();
        $("#AllSchemas").materialSelect();
        $("#AllAuditTypes").materialSelect();
        $("#AllAuditSearchTypes").materialSelect();


        $("#AllDatasets").change(function (event) {
            $("#AuditResultTable").empty();

            var datasetId = $("#AllDatasets").find(":selected").val();
            if (datasetId != "") {
                var url = data.Admin.GetSchemaUrl(datasetId);
                data.Admin.GetSchemaDropdown(url);   
            }

            // Init an empty Dataset File Dropwdown on dataset selection change
            data.Admin.GetFileDropdown(false);
        });

        $("#AllSchemas").change(function (event) {
            $("#AuditResultTable").empty();

            var datasetId = $("#AllDatasets").find(":selected").val();
            var schemaId = $("#AllSchemas").find(":selected").val();
            if (datasetId != "" && schemaId != "") {
                var url = data.Admin.GetFileUrl(datasetId, schemaId);
                data.Admin.GetFileDropdown(true, url);
            }
        });

        // Activates the selected search type element
        $("#AllAuditSearchTypes").change(function () {
            var searchId = $(this).find(":selected").val();

            // Set's all search elements to default state
            var AuditSearchTypeSelection = $(".audit-search-selection");

            AuditSearchTypeSelection.removeClass("active");
            AuditSearchTypeSelection.addClass("hidden");

            $(`.audit-search-selection .admin-audit-dropdown`).removeClass("active");

            // Set's selected search element to active
            var AuditSelection = $(`#audit-selection-${searchId}`);

            AuditSelection.addClass("active")
            $(`#audit-selection-${searchId} .admin-audit-dropdown`).addClass("active");
            /*AuditSelection.childrem .admin-audit-dropdown`).addClass("active");*/
        });

        $("select.admin-audit-dropdown").change(function () {
            data.Admin.ActivateDeactivateAuditSearchButton();
        });

        $("#auditSearchButton").click(function () {
            var datasetId = $("#AllDatasets").find(":selected").val();
            var schemaId = $("#AllSchemas").find(":selected").val();
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
                    searchParameter = $("#fileDropdown").find(":selected").text();
                    break;
            }

            // Show spinner + Reprocess button
            $("#tab-spinner").show();

            if (postCheck) {
                $.ajax({
                    type: "POST",
                    url: "GetAuditTableResults",
                    contentType: "application/json",
                    data: JSON.stringify({ "datasetId": datasetId, "schemaId": schemaId, "auditId": auditId, "searchTypeId": searchTypeId, "searchParameter": searchParameter }),
                    success: function (result) {
                        if (result.failure) {
                            // throw toast error
                            data.Dataset.makeToast("error", result.message);

                            // ensure the audit results table is cleared from any potential previous searches
                            $("#AuditResultTable").empty();
                        } else {
                            $("#AuditResultTable").html(result);
                        }
                    },
                    error: function (msg) {
                        data.Dataset.makeToast("error", msg.responseText);
                        $("#tab-spinner").hide();
                    },
                    complete: function (msg) {
                        // Hide spinner
                        $("#tab-spinner").hide();
                    }
                })
            } else {
                $("#tab-spinner").hide();
            }
        });
    },

    TableDifferenceFilter: function (tableID, showDifference)
    {
        var table = $(tableID).DataTable();

        $.fn.dataTable.ext.search.pop();

        $.fn.dataTable.ext.search.push(
            function (settings, data, dataIndex)
            {
                return $(table.row(dataIndex).node()).data("record-difference") == showDifference;
            }
        );

        table.draw();
    },

    TableAllFilter: function (tableID)
    {
        var table = $(tableID).DataTable();

        $.fn.dataTable.ext.search.pop();
        table.draw();
    },

    RowCountCompareTableInit: function () {
        var table = $('#AuditTable').DataTable({
            columnDefs: [
                {
                    targets: [0],
                    orderable: true
                }
            ],
            dom: 'Bfrtip',
            buttons: [
                {
                    text: '<i class="fas fa-asterisk"></i> All',
                    action: function (e, dt, node, config)
                    {
                        data.Admin.TableAllFilter('#AuditTable');
                    },
                    className: 'btn-sm btn-outline-primary shadow-none',
                    init: function (api, node, config)
                    {
                        $(node).removeClass('btn-secondary')
                    }
                },
                {
                    text: '<i class="text-warning fas fa-not-equal"></i> Diffs',
                    action: function(e, dt, node, config)
                    {
                        data.Admin.TableDifferenceFilter('#AuditTable', "True");
                    },
                    className: 'btn-sm btn-outline-primary shadow-none',
                    init: function (api, node, config)
                    {
                        $(node).removeClass('btn-secondary')
                    }
                },
                {
                    text: '<i class="text-dark fas fa-equals"></i> Same',
                    action: function (e, dt, node, config)
                    {
                        data.Admin.TableDifferenceFilter('#AuditTable', "False");
                    },
                    className: 'btn-sm btn-outline-primary shadow-none',
                    init: function (api, node, config)
                    {
                        $(node).removeClass('btn-secondary')
                    }
                }
            ],
            select: true
        });

        // ensures that table is reset of all filters that might have been applied
        $.fn.dataTable.ext.search.pop();
        table.draw();

        // event for applying solid bg color for selected filter btns
        $(".dt-buttons .btn").click(function ()
        {
            // loops through all audit filtter buttons and reset them to default outline style
            $(".dt-buttons .btn").each(function ()
            {
                $(this).removeClass("btn-primary");
                $(this).removeClass("btn-outline-primary");
                $(this).addClass("btn-outline-primary");
            });

            $(this).removeClass("btn-outline-primary");
            $(this).removeClass("btn-primary");
            $(this).addClass("btn-primary");
        });
    },

    NonParquetFilesTableInit: function ()
    {
        $('#AuditTable').DataTable({
            columnDefs: [
                {
                    targets: [0],
                    orderable: true
                }
            ],
            drawCallback: function ()
            {
                $('#data-file-select-all').prop('checked', false);
                $('.select-all-target').prop('checked', false);
            }
        });

        data.Admin.DataFileSelectAll();
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

        data.Admin.DataFileSelectAll();

        // submits selected jobs to be reprocessed
        $("#reprocessButton").click(function () {
            var files = [];

            // grabs the file & step id's of all checked rows
            $('.select-all-target:checked').each(function () {
                var obj = { fileId: $(this).data("file-id"), groupId: $(this).data("group-id") };
                files.push(obj);
            });

            // groups file id's by shared step id's and store them in a JSON object
            var returnJson = data.Admin.JsonReduce(files, "groupId", "fileId");

            var tempArr = [];

            // loop through all json keys and POST them to the data file reprocess controller
            for (let x in returnJson) {

                tempArr = x.split("/");

                $.ajax({
                    type: "POST",
                    url: "../../api/" + data.GetApiVersion() + "/datafile/DataFile/Reprocess",
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
        for (let flowEvent of data.FlowEvents) {
            s += '<tr><td>' + flowEvent.EventMetricId + '</td><td>' + flowEvent.DataFlowStepName + '</td><td>' + flowEvent.CurrentFlowStep + '/' + flowEvent.TotalFlowSteps + '</td><td>' + flowEvent.StatusCode + '</td><td>' + flowEvent.Offset + '</td><td>' + flowEvent.Partition + '</td><td>' + flowEvent.RunInstanceGuid + '</td><td><a href="#EventContentModal" id="EventContentLink" data-toggle="modal" data-eventContent=' + flowEvent.EventContents + '>View Contents</a></td></tr>';
        }
        s += '</table>';
        return s;
    },

    ActivateDeactivateSubmitButton: function () {
        if ($("#AllDatasets").find(":selected").val() != "" && $("#AllSchemas").find(":selected").val() != "-1") {
            $("#submitButton").prop("disabled", false);
        }
        else {
            $("#submitButton").prop("disabled", true);
        }
    },

    // loads reprocessing page with event handlers
    ReprocessInit: function () {
        $("#AllDatasets").materialSelect();
        $("#AllSchemas").materialSelect()
        $("#flowStepsDropdown").materialSelect();

        data.Admin.DatasetDropdownScrollToTop();

        $("#AllDatasets").change(function (event) {
            var datasetId = $("#AllDatasets").find(":selected").val();
            if (datasetId != "") {

                // Init the schema dropdown
                var url = data.Admin.GetSchemaUrl(datasetId);
                data.Admin.GetSchemaDropdown(url);

                // Reset the table and flow step dropdown to an empty or default state
                data.Admin.ResetDataFileReprocessTable();
                data.Admin.ResetFlowStepDropdown();
                data.Admin.ActivateDeactivateReprocessButton();
            }

        });
        $("#AllSchemas").change(function (event) {
            var schemaId = $("#AllSchemas").find(":selected").val();
            var datasetId = $("#AllDatasets").find(":selected").val();
            if (schemaId != "" && datasetId != "") {
                var url = data.Admin.GetFileUrl(datasetId, schemaId);
                data.Admin.DataFileReprocessTableInit(url);
                url = data.Admin.GetFlowStepUrl(schemaId);
                data.Admin.GetFlowStepDropdown(url);
            }
        });

        // activate or deactivate button
        $("#flowStepsDropdown").change(function () {
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
                url: "../../api/" + data.GetApiVersion() + "/datafile/DataFile/Reprocess",
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

    //loads dataflow metric page events
    DataFlowMetricsInit: function () {
        var table;

        $("#AllDatasets").materialSelect();
        $("#AllSchemas").materialSelect();
        $("#fileDropdown").materialSelect();

        data.Admin.DatasetDropdownScrollToTop();

        $("#AllDatasets").change(function (event) {
            var datasetId = $("#AllDatasets").find(":selected").val();

            if (datasetId != "") {
                var url = data.Admin.GetSchemaUrl(datasetId);
                data.Admin.GetSchemaDropdown(url);
            }

            data.Admin.ActivateDeactivateSubmitButton();
        });

        $("#AllSchemas").change(function (event) {
            var schemaId = $("#AllSchemas").find(":selected").val();
            var datasetId = $("#AllDatasets").find(":selected").val();

            if (schemaId != -1 && datasetId != "") {
                var url = data.Admin.GetFileUrl(datasetId, schemaId);
                data.Admin.GetFileDropdown(true, url);
            }

            data.Admin.ActivateDeactivateSubmitButton();
        });

        $("#EventContentModal").on("show.bs.modal", function (event) {
            var link = $(event.relatedTarget);
            var content = JSON.stringify(link.data("eventcontent"), null, "\t");
            var modalBody = "<textarea class='form-control' style = 'height: 500px'>" + content + "</textarea>"

            $("#EventDetails").html(modalBody);
        });

        $("#submitButton").click(function (event) {
            var dto = new Object();
             
            dto.DatasetFileIds = $("#fileDropdown").find(":selected").data('datasetids');

            if (dto.DatasetFileIds == null || dto.DatasetFileIds == 0) dto.DatasetFileIds = -1;

            dto.DatasetId = $("#AllDatasets").find(":selected").val();
            dto.SchemaId = $("#AllSchemas").find(":selected").val();

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
                        className: 'metric-detail-control',
                        orderable: false,
                        data: null,
                        defaultContent: '<center><em id = "expand-collapse-icon" class = "fas fa-plus"></em></center>',
                    },
                    {
                        className: 'metric-detail-control',
                        data: 'FileName'
                    },
                    {
                        className: 'metric-detail-control',
                        type: 'date',
                        data: 'FirstEventTime',
                        render: function (data) {
                            return data ? moment(data).format("MM/DD/YYYY h:mm:ss a") : null;
                        }
                    },
                    {
                        className: 'metric-detail-control',
                        type: 'date',
                        data: 'LastEventTime',
                        render: function (data) {
                            return data ? moment(data).format("MM/DD/YYYY h:mm:ss a") : null;
                        }
                    },
                    {
                        className: 'metric-detail-control',
                        data: 'Duration'
                    },
                    {
                        className: 'metric-detail-control',
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
        });

        // Add event listener for opening and closing details
        $('#metricGroupsTable').on('click', 'td.metric-detail-control', function () {
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
    //support link init function
    SupportLinkInit: function () {
        $("#LinkSubmitButton").click(function () {
            $.ajax({
                type: "POST",
                url: "/Admin/AddSupportLink",
                datatype: "json",
                data: $("#SupportLinkForm").serialize(),
                success: function (data) {
                    window.location.href = data.redirectToUrl;
                }
            })
            $("#LinkName").val("");
            $("#LinkDescription").val("");
            $("#LinkUrl").val("");
            $("#AddLinkModal").modal("hide");
        })
        $("#ModalCloseButton").click(function () {
            $("#LinkName").val("");
            $("#LinkDescription").val("");
            $("#LinkUrl").val("");
            $("#AddLinkModal").modal("hide");
        })
        $("#link-results").on("click", ".delete-icon", function () {
            var icon = $(this);
            var model = new Object();
            model.SupportLinkId = icon.data("supportlinkid");
            $.ajax({
                type: "POST",
                url: "/Admin/RemoveSupportLink",
                data: model,
                success: function (data) {
                    window.location.href = data.redirectToUrl;
                }
            })
        })
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