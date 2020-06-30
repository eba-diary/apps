data.Dale =
{
    //declare a property within the data.Dale that essentially represents a global var that all functions can use within data.Dale to set whether sensitive or not
    sensitive:false,

    init: function ()
    {
        
        localStorage.clear();                                                           // Clear all items

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

                { data: "IsSensitive", className: "IsSensitive" },
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
                    { extend: 'csv', text: 'Download' },

                    {
                        text: 'Select Page',
                        action: function (e, dt, node, config)
                        {
                            var rowsIndexes = table.rows({ page: 'current' }).indexes();
                            var rowsData = table.rows(rowsIndexes).data();
                            var rowData1 = rowsData[0];
                            rowData1.IsSensitive = !rowData1.IsSensitive;
                            var rowIndex1 = rowsIndexes[0];


                            table                                                                                               //remove dale-clicked class
                                .row(rowIndex1)
                                .data(rowData1)         //supply updated data for grid row
                                .draw()                 //redraw table because of changes
                                .rows(rowIndex1)        //identifies a given row to modify
                                .nodes()
                                .to$()
                                .addClass('dale-clicked');
                        }
                    }
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


        var table = $('#daleResultsTable').DataTable();

        //cell click event
        $('#daleResultsTable tbody').on('click', 'td', function () {
            
            var row = table.row(this).data();                                                                           //get whole row of data to use later
            var rowIndex = table.cell(this).index().row;                                                                //get rowIndex being updated
            var columnIndex = table.cell(this).index().columnVisible;                                                   //get columnIndex clicked
            var pageNumber = table.page();                                                                              //figure out page number


            if (columnIndex == 6)                                                                                       
            {
               
                row.IsSensitive = !row.IsSensitive;                                                                         //flip IsSensitive

                var sensitiveList = JSON.parse(localStorage.getItem("sensitiveList"));                                      //get stored object array
                var o = { "BaseColumnId": "IsSensitive" };                                                                  //create new obj based off current selection
                o.BaseColumnId = row.BaseColumnId;
                o.IsSensitive = row.IsSensitive;

                if (sensitiveList == null)
                {
                    sensitiveList = [];
                    sensitiveList[0] = o;
                }
                else
                {
                    var index = sensitiveList.findIndex(sensitive => sensitive.BaseColumnId === o.BaseColumnId);            //first make sure we remove if already exists in list
                    if (index >= 0)                                                                                         //check if already exists
                    {
                        sensitiveList.splice(index, 1);                                                                     //remove that index from array  
                        table                                                                                               //remove dale-clicked class
                            .row(rowIndex)
                            .data(row)              //supply updated data for grid row
                            .draw()                 //redraw table because of changes
                            .rows(rowIndex)         //identifies a given row to modify
                            .nodes()
                            .to$()
                            .removeClass('dale-clicked');
                    }
                    else
                    {
                        sensitiveList.push(o);                                                                              //add new item
                        table                                                                                               //add dale-clicked class 
                            .row(rowIndex)
                            .data(row)              //supply updated data for grid row
                            .draw()                 //redraw table because of changes
                            .rows(rowIndex)         //identifies a given row to modify
                            .nodes()
                            .to$()
                            .addClass('dale-clicked');
                    }
                    
                }
                localStorage.setItem("sensitiveList", JSON.stringify(sensitiveList));                                       //save array to storage
            }
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
        $('#daleSearchClick').hide();
        $('#daleSearchClickSpinner').show();
        $('#daleContainer').hide();

        $('#sensitiveSearchLink').addClass("dale-disable-stuff");
        $('#daleCriteriaContainer').addClass("dale-disable-stuff");
        $('#radioMadness').addClass("dale-disable-stuff");
    },

    //enable all controls user can hit during search
    enableDale: function ()
    {
        //everytime a new search happens, we want to redraw the grid so column contents is redrawn to fit properly
        $("#daleResultsTable").DataTable().columns.adjust().draw();

        $('#daleSearchClick').show();
        $('#daleSearchClickSpinner').hide();
        $('#daleContainer').show();

        $('#sensitiveSearchLink').removeClass("dale-disable-stuff");
        $('#daleCriteriaContainer').removeClass("dale-disable-stuff");
        $('#radioMadness').removeClass("dale-disable-stuff");
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