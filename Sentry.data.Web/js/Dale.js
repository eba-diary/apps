﻿data.Dale =
{
    //declare a property within the data.Dale that essentially represents a global var that all functions can use within data.Dale to set whether sensitive or not
    sensitive:false,

    init: function ()
    {
        var daleResultsTable = $("#daleResultsTable").DataTable({

            //client side setup
            searching: true,
            pageLength: 50,

            ajax: {
                url: "/Dale/GetSearchResultsClient/",
                type: "GET",
                data: function (d)
                {
                    d.searchCriteria = $('#daleSearchCriteria').val();
                    d.destination = data.Dale.getDaleDestiny(); 
                    d.sensitive = data.Dale.sensitive;
                }
            },

            columns: [
                { data: "Asset", className: "Asset", searchable: "false" },
                { data: "Server", className: "Server", searchable: "false" },
                { data: "Database", className: "Database", searchable: "false" },
                { data: "Object", className: "Object", searchable: "false" },
                { data: "ObjectType", className: "ObjectType",searchable: "true" },
                { data: "Column", className: "ColumnMan", searchable: "false" },

                { data: "Alias", className: "Alias", visible: false,searchable: "false" },
                { data: "ProdType", className: "ProdType", visible: false, searchable: "false" },

                { data: "ColumnType", className: "ColumnType", visible: false, searchable: "false", width: "100px" },
                { data: "MaxLength", className: "MaxLength", visible: false, searchable: "false", width: "75px" },
                { data: "Precision", className: "Precision", visible: false, searchable: "false", width: "100px" },
                { data: "Scale", className: "Scale", visible: false, searchable: "false", width: "75px"  },
                { data: "IsNullable", className: "IsNullable", visible: false, searchable: "false", width: "100px" },
                { data: "EffectiveDate", className: "EffectiveDate", visible: false, searchable: "false" }
            ],

            order: [2, 'desc'],

            //styles for columnVisibility to show
            dom: 'Bfrtip',
            buttons: ['colvis', 'csv']
        });

        //add a filter in each column
        $("#daleResultsTable").dataTable().columnFilter({
            sPlaceHolder: "head:after",
            aoColumns: [
                { type: "text" },
                { type: "text" },
                { type: "text" },
                { type: "text" },
                { type: "text" },
                { type: "text" },

                { type: "text" },
                { type: "text" },

                { type: "text" },
                { type: "text" },
                { type: "text" },
                { type: "text" },
                { type: "text" },
                { type: "text" }
            ]
        });


        //Hide DataTable SearchBox (NOTE: jquery calls the search box what you named table and appends _filter)
        //to get column filtering requires the Searching=true to be set to true, if i set it to false then column filtering goes away so by default you can't have one without the other, so cheat the system here
        $('#daleResultsTable_filter').hide();

        //setup click event for search  span for a new search
        $('.input-group-addon').click(function (e)
        {
            data.Dale.sensitive = false;
            data.Dale.disableDale();
            daleResultsTable.ajax.reload(function () { data.Dale.enableDale(); });  //call reload but use a callback function which actually gets executed when complete! otherwise long queries will show nothing in the grid
        });

        //setup search box enter event
        //add something around here to know if search is in progress
        var input = document.getElementById("daleSearchCriteria");
        input.addEventListener("keyup", function (event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                data.Dale.disableDale();
                daleResultsTable.ajax.reload(function () { data.Dale.enableDale(); });  //call reload but use a callback function which actually gets executed when complete! otherwise long queries will show nothing in the grid
            }
        });

        //setup click event for sensitive search
        $("#sensitiveSearch").on('click', function ()
        {
            data.Dale.sensitive = true;
            data.Dale.disableDale();
            daleResultsTable.ajax.reload(function () { data.Dale.enableDale(); });  //call reload but use a callback function which actually gets executed when complete! otherwise long queries will show nothing in the grid
            

        });

    },

    getDaleDestiny: function ()
    {
        var daleDestinyTableRadio = $('input[name="Destiny"]:checked').val();
        return daleDestinyTableRadio;
    },

    //disable all controls user can hit during search
    disableDale: function ()
    {
        //hide intitially since they could be doing a search after a previous search
        $('#daleContainer').hide();
        $('#daleSearchCriteria').attr('disabled', 'disabled');
        $('#DestinyTableRadio').attr('disabled', 'disabled');
        $('#DestinyColumnRadio').attr('disabled', 'disabled');
        $('#DestinyViewRadio').attr('disabled', 'disabled');

        $('#daleSearchClick').hide();
        $('#daleSearchClickSpinner').show();
    },

    //enable all controls user can hit during search
    enableDale: function ()
    {
        //hide intitially since they could be doing a search after a previous search
        $('#daleContainer').show();
        $('#daleSearchCriteria').removeAttr('disabled');
        $('#DestinyTableRadio').removeAttr('disabled');
        $('#DestinyColumnRadio').removeAttr('disabled');
        $('#DestinyViewRadio').removeAttr('disabled');

        $('#daleSearchClick').show();
        $('#daleSearchClickSpinner').hide();

    }
};