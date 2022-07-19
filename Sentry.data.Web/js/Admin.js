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
                var s = '<option value="-1"  id = "defaultSchemaSelection">Please Select a Schema</option>';
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

    GetFileDropdown: function (url) {
        $.ajax({
            type: "GET",
            url: url,
            dataType: "Json",
            dataSrc: "Records",
            success: function (data) {
                console.log(data);
                var s = '<option value="-1"id="defaultFileSelection"> (Optional) Select a File</option>';
                for (var d of data.Records) {
                    s+= '<option value="' + d.DatasetFileId + '">' + d.FileName + '</option>'
                }
                $("#fileDropdown").html(s);
                $("#defaultFileSelection").prop("disabled", true);
                $("#fileDropdown").materialSelect("destroy");
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
    GetFlowEvents: function (data) {
        var s = '<table><tr><th>Event Metric ID</th><th>Flow Step Name</th><th>Execution Order</th><th>Status Code</th><th>Offset</th><th>Partiton</th></tr>';
        for (var flowEvent of data.FlowEvents) {
            s += '<a href="#EventContentModal" id="EventContentLink" data-toggle="modal" data-eventContent ="' + flowEvent.EventContents +  '"><tr><td>' + flowEvent.EventMetricId + '</td><td>' + flowEvent.DataFlowStepName + '</td><td>' + flowEvent.CurrentFlowStep + '/' + flowEvent.TotalFlowSteps + '</td><td>' + flowEvent.StatusCode + '</td><td>' + flowEvent.Offset + '</td><td>' + flowEvent.Partition + '</td></tr></a>'
        }
        s += '</table>';
        return s;
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
    //loads dataflow metric page events
    DataFlowMetricsInit: function () {
        $("#DatasetsList").materialSelect();
        $("#schemaDropdown").materialSelect();
        $("#fileDropdown").materialSelect();
        $("#DatasetsList").change(function (event) {
            var datasetId = $("#DatasetsList").find(":selected").val();
            if (datasetId != "") {
                var url = data.Admin.GetSchemaUrl(datasetId);
                data.Admin.GetSchemaDropdown(url);
            }
            if ($("#DatasetsList").find(":selected").val() != "" && $("#schemaDropdown").find(":selected").val() != "-1") {
                $("#submitButton").prop("disabled", false);
            }
            else {
                $("#submitButton").prop("disabled", true);
            }

        });
        $("#schemaDropdown").change(function (event) {
            var schemaId = $("#schemaDropdown").find(":selected").val();
            var datasetId = $("#DatasetsList").find(":selected").val();
            if (schemaId != -1 && datasetId != "") {
                var url = data.Admin.GetFileUrl(datasetId, schemaId);
                data.Admin.GetFileDropdown(url);
            }
            if ($("#DatasetsList").find(":selected").val != "" && $("#schemaDropdown").find(":selected").val != "-1") {
                $("#submitButton").prop("disabled", false);
            }
            else {
                $("#submitButton").prop("disabled", true);
            }
        });
        $("#EventContentModal").on("show.bs.modal", function (event) {
            var link = $(event.relatedTarget);
            var content = JSON.stringify(link.data("eventcontent"), null, "\t");
            var modalBody = "<textarea class='form-control' style = 'height: 500px'>" + content + "</textarea>"
            $("#EventDetails").html(modalBody);

        })
        $("#submitButton").click(function (event) {
            var dto = new Object();
            dto.FileToSearch = $("#fileDropdown").find(":selected").val();
            dto.DatasetToSearch = $("#DatasetsList").find(":selected").val();
            dto.SchemaToSearch = $("#schemaDropdown").find(":selected").val();
            var table = $('#metricGroupsTable').DataTable({
                destroy: true,
                ajax: {
                    type: "GET",
                    url: "/DataFlowMetric/PopulateTable",
                    data: JSON.stringify(dto),
                    contentType: "application/json",
                    dataType: "json",
                    dataSrc: "",
                   // success: function (data) {
                 //       console.log(data);
                 //   }
                },
                columns: [
                    {
                        className: 'dt-control',
                        orderable: false,
                        data: null,
                        defaultContent: '',
                    },
                    { data: 'FileName' },
                    { data: 'FirstEventTime' },
                    { data: 'LastEventTime' },
                    { data: 'Duration' },
                    {
                        data: null,
                        render: (d) => function (data, type, row) {
                            if (d.AllEventsPresent && d.AllEventsComplete) {
                                return '<em class="icon-checkmark" style="color: green"></em>';
                            }
                            else if (!d.AllEventsPresent && d.AllEventsComplete) {
                                return '<em class="icon-clock"></em>';
                            }
                            else {
                                return '<em class="icon-error-outline" style="color: red"></em>';
                            }
                        }
                    }
                ],
            });

            // Add event listener for opening and closing details
            $('#metricGroupsTable').on('click', 'td.dt-control', function () {
                var tr = $(this).closest('tr');
                var row = table.row(tr);

                if (row.child.isShown()) {
                    // This row is already open - close it
                    row.child.hide();
                    tr.removeClass('shown');
                } else {
                    // Open this row
                    row.child(data.Admin.GetFlowEvents(row.data())).show();
                    tr.addClass('shown');
                }
            });
            /*
            var url = $(this).data("url");
            $("#accordion-view-area").load(url);
            */
        })
    },
    FlowMetricAccordionInit: function () {
        $("#EventContentModal").on("show.bs.modal", function (event) {
            var link = $(event.relatedTarget);
            var content = JSON.stringify(link.data("eventcontent"), null, "\t");
            var modalBody = "<textarea class='form-control' style = 'height: 500px'>" + content + "</textarea>"
            $("#EventDetails").html(modalBody);

        })
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

