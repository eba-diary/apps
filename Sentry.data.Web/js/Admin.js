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

    CompleteInit: function() {
        $("[id^='Complete_']").on("click", function () {
            data.Admin.CompleteAuction($(this).data("id"));
        });
    },
    Init: function () {
        //on selection of a dataset, load available schema
        $("#allDatasets").change(function (event) {
            var val = $("#allDatasets").find(":selected").val();
            var url = window.location.href;
            url = url.substring(0, url.length - 5);
            url = url + "api/v2/metadata/dataset/" + val + "/schema";
            console.log(url);
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
        });
        //on selection of a schema, fills in data table
        $("#SchemaDropdown").change(function (event) {
            var schemaId = $("#SchemaDropdown").find(":selected").val();
            var datasetId = $("#allDatasets").find(":selected").val();
            var url = window.location.href;
            url = url.substring(0, url.length - 5);
            url = url + "api/v2/datafile/dataset/" + datasetId + "/schema/" + schemaId + "?pageNumber=1&pageSize=1000";
            console.log(url);
            $("#results").DataTable({
                ajax: {
                    url: url,
                    dataSrc: "record",
                },
            });

        });
    
    }
}
