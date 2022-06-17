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
                for (var i = 0; i < data.length; i++) {
                    s += '<option value="' + data[i].SchemaId + '">' + data[i].Name + '</option>';
                }
                $("#SchemaDropdown").html(s);
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
                        return '<input type="checkbox" id= checkbox' + d.DatasetFileId + ' class="form-check-input" fileId = ' + d.DatasetFileId + '><label for=checkbox' + d.DatasetFileId + ' class="form-check-label"></label>';
                    }
                },
            ],
        });
    },

    GetFilesToReprocess: function () {
        var fileIds = [];
        $('.form-check-input:checkbox:checked').each(function () {
            fileIds.push($(this).attr("fileid"));
        });
        console.log(fileIds);
        //need to change fileid attribute into a data-fileid for consistency, can be called by .data() function in jquery then
    },
    //loads reprocessing page with relevant functions
    ReprocessInit: function () {
        $("#allDatasets").change(function (event) {
            var datasetId = $("#allDatasets").find(":selected").val();
            var url = data.Admin.GetSchemaUrl(datasetId);
            data.Admin.GetSchemaDropdown(url);
        });
        $("#SchemaDropdown").change(function (event) {
            var schemaId = $("#SchemaDropdown").find(":selected").val();
            var datasetId = $("#allDatasets").find(":selected").val();
            var url = data.Admin.GetFileUrl(datasetId, schemaId);
            data.Admin.PopulateTable(url);
        });
        $("#reprocessButton").click(function (event) {
            data.Admin.GetFilesToReprocess();
        });
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

