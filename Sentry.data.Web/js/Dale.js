﻿data.Dale =
{
    init: function ()
    {
        var daleResultsTable = $("#daleResultsTable").DataTable({

            //server side setup - leave here for potential future processing
            //autoWidth: true,
            //serverSide: true,
            //processing: true,
            //searching: true,
            //paging: true,
            //scroller: true,
            //deferRender: true,
            //pageLength: 50,
            //ajax: {
            //    url: "/Dale/GetSearchResultsServer/",
            //    type: "POST",
            //    data: function (d)
            //    {
            //        d.searchCriteria = $('#daleSearchCriteria').val();
            //        d.destination = data.Dale.getDaleDestiny(); 
            //    }
            //},


            //client side setup
            autoWidth: true,
            searching: true,
            pageLength: 50,

            ajax: {
                url: "/Dale/GetSearchResultsClient/",
                type: "GET",
                data: function (d)
                {
                    d.searchCriteria = $('#daleSearchCriteria').val();
                    d.destination = data.Dale.getDaleDestiny(); 
                }
            },

            columns: [
                { data: "Server", className: "Server" },
                { data: "Database", className: "Database" },
                { data: "Table", className: "Table" },
                { data: "Column", className: "ColumnMan"},
                { data: "ColumnType", className: "ColumnType", visible: false  },
                { data: "PrecisionLength", className: "PrecisionLength", visible: false  },
                { data: "ScaleLength", className: "ScaleLength", visible: false }
            ],

            order: [[1, 'desc'], [2, 'desc']]
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
                { type: "text" }
            ]
        });

        //Hide DataTable SearchBox (NOTE: jquery calls the search box what you named table and appends _filter)
        //to get column filtering requires the Searching=true to be set to true, if i set it to false then column filtering goes away so by default you can't have one without the other, so cheat the system here
        $('#daleResultsTable_filter').hide();

        //wire up column Picker for grid
        data.Dale.initDaleColumnPicker();

        //this reloads DataTable and does a refresh pulling criteria everytime
        $('#askDaleBtn').click(function () {
            data.Dale.disableDale();

            //call reload but use a callback function which actually gets executed when complete! otherwise long queries will show nothing in the grid
            daleResultsTable.ajax.reload(function() {
                data.Dale.enableDale();
            });
        });

        $('.input-group-addon').click(function (e) {

            data.Dale.disableDale();

            //call reload but use a callback function which actually gets executed when complete! otherwise long queries will show nothing in the grid
            daleResultsTable.ajax.reload(function () {
                data.Dale.enableDale();
            });
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
    enableDale: function () {

        //hide intitially since they could be doing a search after a previous search
        $('#daleContainer').show();
        $('#daleSearchCriteria').removeAttr('disabled');
        $('#DestinyTableRadio').removeAttr('disabled');
        $('#DestinyColumnRadio').removeAttr('disabled');
        $('#DestinyViewRadio').removeAttr('disabled');

        $('#daleSearchClick').show();
        $('#daleSearchClickSpinner').hide();
    },

    initDaleColumnPicker: function ()
    {
        var mySelect = document.getElementById('daleColumnPicker');

        mySelect.onchange = function () {
       
            var x = document.getElementById("daleColumnPicker").value;
            
            var table = $("#daleResultsTable").DataTable();
            
            if (table.column(x).visible() === true)
            {
                table.column(x).visible(false);
            }
            else
            {
                table.column(x).visible(true);
            }

            //
            document.getElementById("daleColumnPicker").value = "Column Selector";
        }
    }

};