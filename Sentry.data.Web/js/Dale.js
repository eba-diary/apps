data.Dale =
{
    init: function ()
    {
        var daleResultsTable = $("#daleResultsTable").DataTable({

            //server side setup
            //autoWidth: true,
            //serverSide: true,
            //processing: true,
            //searching: true,
            //paging: true,
            //scroller: true,
            //deferRender: true,
            //ajax: {
            //    url: "/Dale/GetSearchResults/",
            //    type: "POST",
            //    data: function (d)
            //    {
            //        d.searchCriteria = $('#daleSearchCriteria').val();
            //        d.destination = data.Dale.getDaleDestiny(); 
            //    }
            //},


            //client side setup
            autoWidth: true,
            serverSide: false,
            searching: true,
            paging: true,
            async: false,
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
                { data: "Column", className: "ColumnMan" },
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
                { type: "text" }
            ]
        });

        //Hide DataTable SearchBox (NOTE: jquery calls the search box what you named table and appends _filter)
        //to get column filtering requires the Searching=true to be set to true, if i set it to false then column filtering goes away so by default you can't have one without the other, so cheat the system here
        $('#daleResultsTable_filter').hide();

        //this reloads DataTable and does a refresh pulling criteria everytime
        $('#askDaleBtn').click(function ()
        {
            //hide intitially since they could be doing a search after a previous search
            $('#daleContainer').hide();

            //call reload but use a callback function which actually gets executed when complete! otherwise long queries will show nothing in the grid
            daleResultsTable.ajax.reload(function (json)
            {
                $('#daleContainer').show();
            });
        })
    },

    getDaleDestiny: function ()
    {
        var daleDestinyTableRadio = $('input[name="Destiny"]:checked').val();
        return daleDestinyTableRadio;
    },
};