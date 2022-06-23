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

    //creates url for ajax call to get schema associated with selected dataset
    GetSchemaUrl: function (datasetId) {
        var url = window.location.href;
        url = url.substring(0, url.length - 5);
        url = url + "api/v2/metadata/dataset/" + datasetId + "/schema";
        console.log(url);
        return url;
    },

    //creates schema dropdown for selected dataset
    GetSchemaDropdown: function (url) {
        $.ajax({
            type: "GET",
            url: url,
            data: "{}",
            success: function (data) {
                var s = '<option value>Please Select Schema</option>';
                for (var d of data) {
                    s += '<option value="' + d.SchemaId + '">' + d.Name + '</option>';
                }
                $("#schemaDropdown").html(s);
                $("#schemaDropdown").materialSelect('destroy');
            }
        });
    },

    //adds specified fileId to selectedFiles global var
    AddToSelectedFiles: function (fileList, idToAdd) {
        fileList.push(idToAdd)
    },

    //removes specified fileId from selectedFiles global var
    RemoveFromSelectedFiles: function (fileList, idToRemove) {
        fileList.splice(fileList.indexOf(idToRemove), 1);
    },

    //creates url for Ajax call to get data files
    GetFileUrl: function (datasetId, schemaId) {
        var url = window.location.href;
        url = url.substring(0, url.length - 5);
        url = url + "api/v2/datafile/dataset/" + datasetId + "/schema/" + schemaId + "?pageNumber=1&pageSize=1000";
        console.log(url);
        return url;
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

    //creates url for Ajax call to get flowsteps associated with selected schema
    GetFlowStepUrl: function (datasetId, schemaId) {
        var url = window.location.href;
        url = url.substring(0, url.length - 5);
      //need to see new api url structure before continuing
        return url;
    },

    //creates dropdown menu for flowsteps based on selected dataset and schema
    GetFlowStepDropdown: function (url) {
        $ajax({
            type: "GET",
            url: url,
            data: "{}",
            success: function (data) {
                var s = '<option value>Please Select Flow Step<option>';
                for (var d of data) {
                    //need to know what the API returns to finish this
                    s += '<option value="???"></option>';
                }
                $("#flowStepsDropdown").html(s);
                $("#flowStepsDropdown").materialSelect('destroy');
            },
        });
    },

    //loads reprocessing page with event handlers
    ReprocessInit: function () {
        $("#allDatasets").change(function (event) {
            var datasetId = $("#allDatasets").find(":selected").val();
            if (datasetId != "") {
                var url = data.Admin.GetSchemaUrl(datasetId);
                data.Admin.GetSchemaDropdown(url);
            }

        });
        $("#schemaDropdown").change(function (event) {
            var schemaId = $("#schemaDropdown").find(":selected").val();
            var datasetId = $("#allDatasets").find(":selected").val();
            if (schemaId != "" && datasetId != "") {
                var url = data.Admin.GetFileUrl(datasetId, schemaId);
                data.Admin.PopulateTable(url);
                filesToReprocess = [];
                // url = data.Admin.GetFlowStepUrl(datasetId, schemaId);
                 // data.Admin.getFlowStepDropdown(url);
            }
        });
        //add or remove from selected files list based on checkbox selection and input validation for reprocess
        $("#results").on("click",".select-all-target", function () {
            var checkbox = $(this);

            
            if (checkbox.is(":checked")) {
                data.Admin.AddToSelectedFiles(filesToReprocess, checkbox.data("fileid"));
            }
            else {
                data.Admin.RemoveFromSelectedFiles(filesToReprocess, checkbox.data("fileid"));
            }
            console.log(filesToReprocess);
            if ($("#flowStepsDropdown").find(":selected").val() != "-1" && filesToReprocess.length > 0 && filesToReprocess.length <= 100) {
                $("#reprocessButton").prop("disabled", false);
            }
            else {
                $("#reprocessButton").prop("disabled", true);
            }

        });
        //submit selected file list
        $("#reprocessButton").click(function (event) {
            alert("Selected files (ID's: " + filesToReprocess + ") submitted for reprocessing at flowstep " + $("#flowStepsDropdown").find(":selected").val());
        });
        //input validation for reprocess button
        $("#flowStepsDropdown").change(function (event) {
            if ($("#flowStepsDropdown").find(":selected").val() != "-1" && filesToReprocess.length > 0 && filesToReprocess.length <= 100) {
                $("#reprocessButton").prop("disabled", false);
            }
            else {
                $("#reprocessButton").prop("disabled", true);
            }
        });
        
         // Uncomment this block and and replace final column header in _DataFileReprocessing.cshtml to activate select all functionality.
        /*
        $("#selectAll").click(function (event) {
            var selectAllCheckbox = $(this);
            if (selectAllCheckbox.is(":checked")) {
                $(".select-all-target").each(function () {
                    $(this).prop("checked", selectAllCheckbox.is(":checked"));
                    data.Admin.AddToSelectedFiles(filesToReprocess, $(this).data("fileid"));
                });
            }
            else {
                $(".select-all-target").each(function () {
                    $(this).prop("checked", selectAllCheckbox.is(":checked"));
                    data.Admin.RemoveFromSelectedFiles(filesToReprocess, $(this).data("fileid"));
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

