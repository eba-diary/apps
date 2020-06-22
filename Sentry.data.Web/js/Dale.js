data.Dale =
{
    //declare a property within the data.Dale that essentially represents a global var that all functions can use within data.Dale to set whether sensitive or not
    sensitive:false,

    init: function ()
    {
        //init DataTable
        $("#daleResultsTable").DataTable({

            //client side setup
            pageLength: 100,

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
                { data: null, className: "Asset", render: function (data) { return '<a target="_blank" rel="noopener noreferrer" href=https://said.sentry.com/ViewAsset.aspx?ID=' + data.Asset + '\>' + data.Asset + '</a>'; } },
                { data: "Server", className: "Server"},
                { data: "Database", className: "Database" },
                { data: "Object", className: "Object" },
                { data: "ObjectType", className: "ObjectType" },
                { data: "Column", className: "ColumnMan"},

                { data: "IsSensitive", className: "IsSensitive", visible: false},
                { data: "Alias", className: "Alias", visible: false },
                { data: "ProdType", className: "ProdType", visible: false },

                { data: "ColumnType", className: "ColumnType", visible: false},
                { data: "MaxLength", className: "MaxLength", visible: false},
                { data: "Precision", className: "Precision", visible: false},
                { data: "Scale", className: "Scale", visible: false},
                { data: "IsNullable", className: "IsNullable", visible: false},
                { data: "EffectiveDate", className: "EffectiveDate", visible: false}
            ],

            aLengthMenu: [
                [20, 100, 500],
                [20, 100, 500]
            ],

            order: [2, 'desc'],

            //style for columnVisibility and paging to show
            dom: 'Blrtip',

            //buttons to show and customize text for them
            buttons:
            [
                    { extend: 'colvis', text: 'Columns' },
                    { extend: 'csv', text: 'Download' }
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
                { type: "text" },
                { type: "text" },
                { type: "text" },
                { type: "text" },
                { type: "text" }
            ]
        });

        //setup all click events
        data.Dale.setupClickAttack();
    },

    getDaleDestiny: function ()
    {
        var daleDestinyTableRadio = $('input[name="Destiny"]:checked').val();
        return daleDestinyTableRadio;
    },

    //disable all controls user can hit during search
    disableDale: function ()
    {
        $('#daleSearchClick').hide();
        $('#daleSearchClickSpinner').show();
        $('#daleContainer').hide();

        $('#sensitiveSearchLink').addClass("dale-hide-stuff");
        $('#daleCriteriaContainer').addClass("dale-hide-stuff");
        $('#radioMadness').addClass("dale-hide-stuff");
    },

    //enable all controls user can hit during search
    enableDale: function ()
    {
        //everytime a new search happens, we want to redraw the grid so column contents is redrawn to fit properly
        $("#daleResultsTable").DataTable().columns.adjust().draw();

        $('#daleSearchClick').show();
        $('#daleSearchClickSpinner').hide();
        $('#daleContainer').show();

        $('#sensitiveSearchLink').removeClass("dale-hide-stuff");
        $('#daleCriteriaContainer').removeClass("dale-hide-stuff");
        $('#radioMadness').removeClass("dale-hide-stuff");
    },

    setupClickAttack: function ()
    {
        var daleResultsTable = $("#daleResultsTable").DataTable();

        //setup click event for search  span for a new search
        $('.input-group-addon').click(function (e) {
            data.Dale.sensitive = false;                                            //set sensitive property to true so grid does sensitive search back to controller
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
        $("#sensitiveSearchLink").on('click', function () {

            data.Dale.sensitive = true;                                             //set sensitive property to true so grid does sensitive search back to controller
            $("#daleSearchCriteria").val("");
            data.Dale.disableDale();
            daleResultsTable.ajax.reload(function () { data.Dale.enableDale(); });  //call reload but use a callback function which actually gets executed when complete! otherwise long queries will show nothing in the grid
        });
    }
};