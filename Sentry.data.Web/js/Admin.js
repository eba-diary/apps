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
    //creates url for api call to get schema associated with selected dataset
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
    //creates url for API call to get data files
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
                        return '<input type="checkbox" id= checkbox' + d.DatasetFileId + ' class="form-check-input select-all-target" data-fileId = ' + d.DatasetFileId + '><label for=checkbox' + d.DatasetFileId + ' class="form-check-label"></label>';
                    }
                },
            ],
            searchable: true,
        });
    },

    GetFilesToReprocess: function () {
        var fileIds = [];
        $('.select-all-target:checkbox:checked').each(function () {
            fileIds.push($(this).data('fileid'));
        });
        return fileIds;
        //need to change fileid attribute into a data-fileid for consistency, can be called by .data() function in jquery then
    },
    //loads reprocessing page with relevant functions
    ReprocessInit: function () {
        $("#allDatasets").change(function (event) {
            var datasetId = $("#allDatasets").find(":selected").val();
            var url = data.Admin.GetSchemaUrl(datasetId);
            data.Admin.GetSchemaDropdown(url);
        });
        $("#schemaDropdown").change(function (event) {
            var schemaId = $("#schemaDropdown").find(":selected").val();
            var datasetId = $("#allDatasets").find(":selected").val();
            var url = data.Admin.GetFileUrl(datasetId, schemaId);
            data.Admin.PopulateTable(url);
        });
        $("#reprocessButton").click(function (event) {
            var files = data.Admin.GetFilesToReprocess();
            if (files.length > 100) {
                alert("Selected files exceed reprocessing limit of 100 files.");
            }
            else if (files.length == 0) {
                alert("You must select files before reprocessing!");
            }
            else {
                alert("Selected files (ID's: "+ files + ") submitted for reprocessing!")
            }
        });
        $("#flowStepsDropdown").change(function (event) {
            if ($("#flowStepsDropdown").find(":selected").val() != "-1") {
                $("#reprocessButton").prop("disabled", false);
            }
            else {
                $("#reprocessButton").prop("disabled", true);
            }
        });
        /*
         * Uncomment this block and and replace column header in _DataFileReprocessing.cshtml to activate select all functionality
        $("#selectAll").click(function (event) {
            var selectAllCheckbox = $(this);
            $(".select-all-target").each(function(){
                $(this).prop("checked", selectAllCheckbox.is(":checked"));
            });
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

