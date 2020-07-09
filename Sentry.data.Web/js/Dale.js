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
                { data: "Column", className: "ColumnMan" },

                //the key piece here is including a label with text to indicate whether the row is true or false so the filtering works
                //Since I did not want user to see label text and still have a filter.  My cheat to this was to style it with display:none while still keeping the filtering ability
                //later on when they check/uncheck the box my editRow() function will refresh the data associated with the grid which changes the label hidden text to the opposite so filtering can refresh
                {
                    data: null, className: "IsSensitive", render: function (data)
                    {
                        if (data.IsSensitive) {
                            return ' <input type="checkbox" name="vehicle3" value="true" checked> <label class="trains" style="display:none;" for="vehicle3">true</label><br><br>';
                        }
                        else {
                            return ' <input type="checkbox" name="vehicle3" value="false" > <label class="trains" style="display:none;" for="vehicle3">false</label><br><br>';
                        } 
                    }
                },

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

                    //create my own custom button, problem here is attr doesn't work with out version of buttons
                    //{
                    //   text: 'Save',
                    //    attr: {
                    //        id: 'SaveMeButton'
                    //    },
                    //    action: function (e, dt, node, config)
                    //    {
                    //        //data.Dale.editPage();
                    //    }
                    //}
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


        $('#daleResultsTable tbody').on('change', '.IsSensitive input', function (event)
        {
            var cellClicked = $(this).closest('td');
            var value = this.checked;                                                                                               //determine what the user checked

            var table = $('#daleResultsTable').DataTable();
            var rowIndex = table.cell(cellClicked).index().row;                                                                     //get rowIndex being updated
            var rowData = table.row(cellClicked).data();                                                                            //get whole row of data to use later
            var columnIndex = table.cell(cellClicked).index().columnVisible;                                                        //get columnIndex clicked

            data.Dale.editArray(rowIndex, rowData, columnIndex);

        });
    },

    //edit row (data and style) since user changed something
    editRow: function (edit, rowIndex, rowData) {
        var table = $('#daleResultsTable').DataTable();

        if (edit) {                                                                                                         
            table                                                                                               
                .row(rowIndex)
                .data(rowData)                  //supply updated data for grid row, NEED THIS FOR FILTERING!!!!!!! if we dont refresh rowData, then filtering will think its old values, even in the case of the checkboxes
                .draw()                         //redraw table because of changes
                .rows(rowIndex)                 //identifies a given row to modify
                .nodes()
                .to$()
                .addClass('dale-clicked');      //if editing then add dale-clicked class
        }
        else {                                                                                                             
            table                                                                                              
                .row(rowIndex)
                .data(rowData)                  //supply updated data for grid row, NEED THIS FOR FILTERING!!!!!!! if we dont refresh rowData, then filtering will think its old values, even in the case of the checkboxes
                .draw()                         //redraw table because of changes
                .rows(rowIndex)                 //identifies a given row to modify
                .nodes()
                .to$()
                .removeClass('dale-clicked');   //if not editing then remove dale-clicked class                                                        
        }
    },

    //edit array since user is changing data
    editArray: function (rowIndex, rowData, columnIndex) {

        //IsSensitive Cell Check
        if (columnIndex == 6) {

            var edit = true;

            rowData.IsSensitive = !rowData.IsSensitive;                                                                 //flip IsSensitive

            var sensitiveList = JSON.parse(localStorage.getItem("sensitiveList"));                                      //get stored object array
            var o = { "BaseColumnId": "IsSensitive" };                                                                  //create new obj based off current selection
            o.BaseColumnId = rowData.BaseColumnId;
            o.IsSensitive = rowData.IsSensitive;

            //first time create new array
            if (sensitiveList == null) {
                sensitiveList = [];
                sensitiveList[0] = o;
            }
            else {
                //check if item exists and remove or add new item to list
                var index = sensitiveList.findIndex(sensitive => sensitive.BaseColumnId === o.BaseColumnId);
                if (index >= 0)                                                                                         //check if already exists
                {
                    edit = false;
                    sensitiveList.splice(index, 1);                                                                     //remove that index from array  
                }
                else {
                    sensitiveList.push(o);                                                                              //add new item
                }

            }
            localStorage.setItem("sensitiveList", JSON.stringify(sensitiveList));                                       //save array to storage

            data.Dale.editRow(edit, rowIndex, rowData);
        }

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