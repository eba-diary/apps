data.Dale =
{
    //declare a property within the data.Dale that essentially represents a global var that all functions can use within data.Dale to set whether sensitive or not
    sensitive:false,

    init: function ()
    {
        // Clear all items
        localStorage.clear();

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


        var table = $('#daleResultsTable').DataTable();

        //row click event
        $('#daleResultsTable tbody').on('click', 'tr', function ()
        {
            ////console.log(table.row(this).data());
            //var d = table.row(this).data();


            //alert(d.Column);
            //d.Column = 'reject';
            
            //table
            //    .row(this)
            //    .data(d)
            //    .draw();


            //alert('Clicked on cell in visible column: ' + table.cell(this).index().columnVisible);

        });



        //cell click event
        $('#daleResultsTable tbody').on('click', 'td', function () {
            //console.log(table.row(this).data());

            //alert(table.cell(this).index().columnVisible);
            var columnIndex = table.cell(this).index().columnVisible;


            var d = table.row(this).data();
            //var BaseColumnId = d.BaseColumnId;
            //var IsSensitive = d.IsSensitive;



            if (d.IsSensitive) {
                d.IsSensitive = false;
            }
            else {
                d.IsSensitive = true;
            }


            //get rowIndex being updated
            var rowIdx = table
                .cell(this)
                .index().row;

            table
                .row(this)
                .data(d)            //supply updated data for grid row
                .draw()             //redraw table because of changes
                .rows(rowIdx)       //identifies a given row to modify
                .nodes()
                .to$()
                .addClass('dale-clicked');

            
            var sensitiveList = JSON.parse(localStorage.getItem("sensitiveList"));
            var o = { "BaseColumnId": "IsSensitive" };
            o.BaseColumnId = d.BaseColumnId;
            o.IsSensitive = d.IsSensitive;

            if (sensitiveList == null)
            {
                sensitiveList = [];
                sensitiveList[0] = o;
            }
            else
            {
                //todo firgure out why its not adding!
                var index = sensitiveList.findIndex(sensitive => sensitive.BaseColumnId === d.BaseColumnId);       //first make sure we remove if already exists in list
                sensitiveList.splice(index, 1, o);                                                      //then remove that index from array and replace
            }
            localStorage.setItem("sensitiveList", JSON.stringify(sensitiveList));

            console.log(sensitiveList);
                
        });


        //$('#daleResultsTable tbody').on('click', 'td', function () {
        //    var tr = $(this).parent();
        //    var listOfFilesToBundle;

        //    if (!$(this).hasClass('details-control')) {

        //        if (!tr.hasClass('unUsable') && (tr.hasClass('even') || tr.hasClass('odd'))) {

        //            if (tr.hasClass('active'))
        //            {
        //                tr.removeClass('active');
        //                listOfFilesToBundle = JSON.parse(localStorage.getItem("SensitiveRows"));

        //                if ($(tr).prop('id')) {

        //                    if (listOfFilesToBundle !== null) {
        //                        listOfFilesToBundle.splice(listOfFilesToBundle.indexOf($(tr).prop('id')), 1);
        //                    }
        //                    localStorage.setItem("listOfFilesToBundle", JSON.stringify(listOfFilesToBundle));
        //                    $('#bundleCountSelected').html(parseInt($('#bundleCountSelected').html(), 10) - 1);
        //                }
        //            }
        //            else
        //            {
        //                tr.addClass('active');

        //                listOfFilesToBundle = JSON.parse(localStorage.getItem("listOfFilesToBundle"));
        //                if ($(tr).prop('id')) {

        //                    if (listOfFilesToBundle !== null) {
        //                        listOfFilesToBundle[listOfFilesToBundle.length] = $(tr).prop('id');
        //                    }
        //                    else {
        //                        listOfFilesToBundle = [];
        //                        listOfFilesToBundle[0] = $(tr).prop('id');
        //                    }

        //                    localStorage.setItem("listOfFilesToBundle", JSON.stringify(listOfFilesToBundle));
        //                    $('#bundleCountSelected').html(parseInt($('#bundleCountSelected').html(), 10) + 1);
        //                }
        //            }
        //        }
        //    }

        //    if (parseInt($('#bundleCountSelected').html(), 10) < 2) {
        //        $('#bundle_selected').attr("disabled", true);
        //    }
        //    else {
        //        $('#bundle_selected').attr("disabled", false);
        //    }
        //    if (parseInt($('#bundleCountFiltered').html(), 10) < 2) {
        //        $('#bundle_allFiltered').attr("disabled", true);
        //    }
        //    else {
        //        $('#bundle_allFiltered').attr("disabled", false);
        //    }
        //});













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