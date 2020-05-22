data.Dale =
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
            //autoWidth: true,
            searching: true,
            pageLength: 50,
            //scrollX:true,

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
                { data: "Server", className: "Server", searchable: "false" },
                { data: "Database", className: "Database", searchable: "false" },
                { data: "Object", className: "Object", searchable: "false" },
                { data: "ObjectType", className: "ObjectType",searchable: "true" },
                { data: "Column", className: "ColumnMan", searchable: "false" },

                { data: "ColumnType", className: "ColumnType", visible: false, searchable: "false" },
                { data: "MaxLength", className: "MaxLength", visible: false, searchable: "false" },
                { data: "Precision", className: "Precision", visible: false, searchable: "false" },
                { data: "Scale", className: "Scale", visible: false, searchable: "false" },
                { data: "IsNullable", className: "IsNullable", visible: false, searchable: "false" },
                { data: "EffectiveDate", className: "EffectiveDate", visible: false, searchable: "false" }
            ],

            order: [4, 'desc'],

            margin: "0 auto",

            //styles for columnVisibility to show
            dom: 'Blrtip',
            buttons: [
                'colvis' 
            ]

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
                { type: "text" }
                
            ]
        });

        //Hide DataTable SearchBox (NOTE: jquery calls the search box what you named table and appends _filter)
        //to get column filtering requires the Searching=true to be set to true, if i set it to false then column filtering goes away so by default you can't have one without the other, so cheat the system here
        $('#daleResultsTable_filter').hide();

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
            daleResultsTable.ajax.reload(function ()
            {
                //daleResultsTable.fnFilter('View',3);
                //var jive = $('#daleResultsTable').dataTable();
                //var table = $('#daleResultsTable').DataTable();
                //table.search('Table').draw();
                //daleResultsTable.clear();
                //daleResultsTable.draw();
                //var table = $('#daleResultsTable').DataTable();
                //$('#column3_search').on('keyup', function () {
                //    table
                //        .columns(3)
                //        .search('Table')
                //        .draw();
                //});


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
    }
};