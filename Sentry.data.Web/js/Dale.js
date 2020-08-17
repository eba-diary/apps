﻿data.Dale =
{

    sensitive: false,                                                                   //declare a property within the data.Dale that essentially represents a global var that all functions can use within data.Dale to set whether sensitive or not

    init: function ()
    {
        localStorage.clear();                                                           // Clear all items in our array

        //init GRID based on User Security
        $.ajax({                                                                        
            url: "/Dale/GetCanDaleSensitiveEdit/",
            method: "GET",
            dataType: 'json',
            success: function (obj) {
                data.Dale.dataTablCreate(obj);
            }
        });

        //SAVE BUTTON onCLICK
        $("#btnSaveMe").on('click', function () {

            var sensitiveList = JSON.parse(localStorage.getItem("sensitiveList"));                                      //get stored object array
            var verifiedList = JSON.parse(localStorage.getItem("verifiedList"));                                      //get stored object array

            sensitiveList = data.Dale.createMasterList(sensitiveList, verifiedList);

            //Send the JSON array to Controller using AJAX.
            $.ajax({
                type: "POST",
                url: "/Dale/UpdateIsSensitive",
                traditional: true,
                data: JSON.stringify(sensitiveList),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    if (r.success) {
                        data.Dale.resetAfterSave();
                        data.Dale.makeToast("success", "Success!  Changes Saved.");
                    }
                    else {
                        data.Dale.makeToast("error", "Failure!  Please try again.");
                    } 
                },
                failure: function () {
                     data.Dale.makeToast("error", "Failure!  Please try again.");
                },
                error: function () {
                    data.Dale.makeToast("error", "Failure!  Please try again.");
                }
            });
           
        });
    },

    createMasterList: function (sensitiveList, verifiedList) {

        //update SENSITIVE LIST FROM VERIFIED LIST
        var len = sensitiveList.length;
        for (let i = 0; i < len; i++) {

            var r = sensitiveList[i];
            var index = verifiedList.findIndex(item => item.BaseColumnId === r.BaseColumnId);

            if (index >= 0) {
                var o = verifiedList[index];
                r.IsUserVerified = o.IsUserVerified;

                sensitiveList[i] = r;
            }
        }

        //INSERT SENSITIVE LIST FROM VERIFIED LIST
        var len2 = verifiedList.length;
        for (let i = 0; i < len2; i++) {

            var r = verifiedList[i];
            var index = sensitiveList.findIndex(item => item.BaseColumnId === r.BaseColumnId);

            if (index < 0) {
                sensitiveList.push(r)
            }
        }

        return sensitiveList;
    },


    dataTablCreate: function (obj) {

        //init DataTable
        $("#daleResultsTable").DataTable({

            //client side setup
            pageLength: 100,

            ajax: {
                url: "/Dale/GetSearchResultsClient/",
                type: "GET",
                data: function (d) {
                    d.searchCriteria = $('#daleSearchCriteria').val();
                    d.destination = data.Dale.getDaleDestiny();
                    d.sensitive = data.Dale.sensitive;
                }
            },

            columns: [
                { data: null, className: "Asset", render: function (data) { return '<a target="_blank" rel="noopener noreferrer" href=https://said.sentry.com/ViewAsset.aspx?ID=' + data.Asset + '\>' + data.Asset + '</a>'; } },
                { data: "Server", className: "Server" },
                { data: "Database", className: "Database" },
                { data: "Object", className: "Object" },
                { data: "ObjectType", className: "ObjectType" },
                { data: "Column", className: "ColumnMan" },

                //the key piece here is including a label with text to indicate whether IsSensitive column is true or false so the filtering works
                //Since I did not want user to see label text and still have a filter.  My cheat to this was to style label with display:none while still keeping the filtering ability
                //later on when they check/uncheck the box my editRow() function will refresh the data associated with the grid which changes the label hidden text to the opposite so filtering can refresh
                {
                    data: null, className: "IsSensitive", /*visible: false,*/  render: function (d) {

                        //the below code is a way to not have to repeat the html checkbox creation below because it can be disabled or checked based on whether they can edit or if its IsSensitive
                        var disabled = '';
                        var checked = '';
                        var cellValue = 'false';

                        if (!obj.canDaleSensitiveEdit) {
                            disabled = ' disabled="disabled" ';
                        }

                        if (d.IsSensitive){
                            checked = ' checked="checked" ';
                            cellValue = 'true';
                        }

                        return ' <input type="checkbox" value="true"    style="margin:auto; width:100%; zoom:1.25;"  ' + checked + disabled + '  >   <label style="display:none;">' + cellValue + '</label>';  //styles center and make checkbox bigger
                    }
                },


                {
                    data: null, className: "IsUserVerified", /*visible: false,*/ render: function (d) {

                        //the below code is a way to not have to repeat the html checkbox creation below because it can be disabled or checked based on whether they can edit or if its IsSensitive
                        var disabled = '';
                        var checked = '';
                        var cellValue = 'false';

                        if (!obj.canDaleUserVerifiedEdit) {
                            disabled = ' disabled="disabled" ';
                        }

                        if (d.IsUserVerified) {
                            checked = ' checked="checked" ';
                            cellValue = 'true';
                        }

                        return ' <input type="checkbox" value="true"    style="margin:auto; width:100%; zoom:1.25;"  ' + checked + disabled + '  >   <label style="display:none;">' + cellValue + '</label>';  //styles center and make checkbox bigger
                    }
                },

                { data: "ProdType", className: "ProdType", visible: false },

                { data: "ColumnType", className: "ColumnType", visible: false },
                { data: "MaxLength", className: "MaxLength", visible: false },
                { data: "Precision", className: "Precision", visible: false },
                { data: "Scale", className: "Scale", visible: false },
                { data: "IsNullable", className: "IsNullable", visible: false },
                { data: "EffectiveDate", className: "EffectiveDate", visible: false }
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

        //setup onChange event to fire when a checkbox is changed.  This will update internal array for user to save later
        $('#daleResultsTable tbody').on('change', '.IsSensitive input', function () {                                                //filter down to '.IsSensitive' class and child of that which is 'input' which gets you too checkbox
            var cellClicked = $(this).closest('td');                                                                                //get which cell is clicked too use later to figure out rowIndex,rowData,columnIndex
            var columnValue = this.checked;                                                                                         //determine if checkbox is checked or not

            var table = $('#daleResultsTable').DataTable();
            var rowIndex = table.cell(cellClicked).index().row;                                                                     //get rowIndex being updated
            var rowData = table.row(cellClicked).data();                                                                            //get whole row of data to use later
            var columnIndex = table.cell(cellClicked).index().columnVisible;                                                        //get columnIndex clicked

            data.Dale.editArray(rowIndex, rowData, columnIndex, columnValue);
        });


        //setup onChange event to fire when a checkbox is changed.  This will update internal array for user to save later
        $('#daleResultsTable tbody').on('change', '.IsUserVerified input', function () {                                            //filter down to '.IsUserVerified' class and child of that which is 'input' which gets you too checkbox
            var cellClicked = $(this).closest('td');                                                                                //get which cell is clicked too use later to figure out rowIndex,rowData,columnIndex
            var columnValue = this.checked;                                                                                         //determine if checkbox is checked or not

            var table = $('#daleResultsTable').DataTable();
            var rowIndex = table.cell(cellClicked).index().row;                                                                     //get rowIndex being updated
            var rowData = table.row(cellClicked).data();                                                                            //get whole row of data to use later
            var columnIndex = table.cell(cellClicked).index().columnVisible;                                                        //get columnIndex clicked

            data.Dale.editArray(rowIndex, rowData, columnIndex, columnValue);
        });

    },

    //edit storage array since user is changing data
    editArray: function (rowIndex, rowData, columnIndex, columnValue) {

        var sensitiveList = JSON.parse(localStorage.getItem("sensitiveList"));                                      //get stored object array
        var verifiedList = JSON.parse(localStorage.getItem("verifiedList"));                                        //get stored object array
        
        //create new obj to represent all editable columns
        var o = new Object();                                                                                       
        o.BaseColumnId = rowData.BaseColumnId;
        o.IsSensitive = rowData.IsSensitive;
        o.IsUserVerified = rowData.IsUserVerified;

        //IsSensitive Cell Check
        if (columnIndex === 6) {

            rowData.IsSensitive = columnValue;                                                                          //flip IsSensitive for rowData
            o.IsSensitive = columnValue;                                                                                //flip IsSensitive for array storage

            sensitiveList = data.Dale.editArrayItem(o, sensitiveList);
            localStorage.setItem("sensitiveList", JSON.stringify(sensitiveList));                                       //save array to storage

        } //IsUserVerified
        else if (columnIndex === 7) {

            rowData.IsUserVerified = columnValue;                                                                       //flip IsSensitive for rowData
            o.IsUserVerified = columnValue;                                                                             //flip IsSensitive for array storage

            verifiedList = data.Dale.editArrayItem(o, verifiedList);
            localStorage.setItem("verifiedList", JSON.stringify(verifiedList));                                         //save array to storage
        }

        //init variables to use to know whether to mark row as edited and show save button
        var edit = false;
        var sensitiveIndexEDITED = -1;
        var verifiedIndexEDITED = -1;
        var sensitiveListLength = 0;
        var verifiedListLength = 0;

        //init 
        if (sensitiveList != null) {
            sensitiveIndexEDITED = sensitiveList.findIndex(sensitive => sensitive.BaseColumnId === o.BaseColumnId);
            sensitiveListLength = sensitiveList.length;
        }

        if (verifiedList != null) {
            verifiedIndexEDITED = verifiedList.findIndex(verified => verified.BaseColumnId === o.BaseColumnId);
            verifiedListLength = verifiedList.length;
        }

        //DETERMINE IF ROW SHOULD BE RED based on if we found that index in the array with changes
        if (sensitiveIndexEDITED >= 0 || verifiedIndexEDITED >= 0) {
            edit = true;
        }
        else {
            edit = false;
        }

        //DETERMINE WHETHER TO SHOW SAVE BUTTON based on if any of arrays have items
        if (sensitiveListLength > 0 || verifiedListLength > 0) {
            $('#btnSaveMe').show();
        }
        else {
       
            $('#btnSaveMe').hide();
        }

        //mark UI as edited or NOT
        data.Dale.editRow(edit, rowIndex, rowData);
    },

    //UPDATE ARRAY based on new data
    editArrayItem: function (o, list) {

        //first time create new array
        if (list == null) {
            list = [];
            list[0] = o;
        }
        else {
            //check if item exists and remove or add new item to list
            var index = list.findIndex(sensitive => sensitive.BaseColumnId === o.BaseColumnId);
            if (index >= 0) {
                list.splice(index, 1);                                                                     //remove that index from array  
            }
            else {
                list.push(o);                                                                              //add new item
            }
        }

        return list;
    },

    //edit row (data and style) since user changed something
    editRow: function (edit, rowIndex, rowData) {

        var table = $('#daleResultsTable').DataTable();

        //UPDATE INTERNAL DATATABLE DATA WHICH IS USED FOR FILTERING
        table
            .row(rowIndex)                  //pick which row to adjust
            .data(rowData);                 //supply updated data for grid row, NEED THIS FOR FILTERING!!!!!!! if we dont refresh rowData, then filtering will think its old values, even in the case of the checkboxes, what this update does is update the label associated with it

        //STYLE ROW BASED ON BEING EDITED OR NOT (add/remove class that makes it RED if they are changing)
        if (edit) {
            table
                .rows(rowIndex)                 //pick which row(s) to bring back
                .nodes()                        //grab all TR nodes (rows) under the .rows selector above
                .to$()                          //Convert to a jQuery object
                .addClass('dale-clicked');      //add dale-clicked class
        }
        else {
            table
                .rows(rowIndex)                 //pick which row(s) to bring back
                .nodes()                        //grab all TR nodes (rows) under the .rows selector above
                .to$()                          //Convert to a jQuery object
                .removeClass('dale-clicked');   //remove dale-clicked class                                                        
        }
    },

    //figure out which radio button was selected
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

        //MOUSE CLICK EVENT NEW SEARCH
        $('.input-group-addon').click(function (e) {
            data.Dale.sensitive = false;                                            //set sensitive property to true so grid does sensitive search back to controller
            data.Dale.disableDale();
            daleResultsTable.ajax.reload(function () { data.Dale.enableDale(); });  //call reload but use a callback function which actually gets executed when complete! otherwise long queries will show nothing in the grid
        });

        //ENTER BUTTON EVENT
        var input = document.getElementById("daleSearchCriteria");
        input.addEventListener("keyup", function (event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                data.Dale.disableDale();
                daleResultsTable.ajax.reload(function () { data.Dale.enableDale(); });  //call reload but use a callback function which actually gets executed when complete! otherwise long queries will show nothing in the grid
            }
        });

        //SENSITIVE SEARCH CLICK EVENT
        $("#sensitiveSearchLink").on('click', function () {

            data.Dale.sensitive = true;                                             //set sensitive property to true so grid does sensitive search back to controller
            $("#daleSearchCriteria").val("");
            data.Dale.disableDale();
            daleResultsTable.ajax.reload(function () { data.Dale.enableDale(); });  //call reload but use a callback function which actually gets executed when complete! otherwise long queries will show nothing in the grid
        });
    },

    resetAfterSave: function () {

        var table = $('#daleResultsTable').DataTable();
        var rowsIndexes = table.rows('.dale-clicked').indexes();

        var len = rowsIndexes.length;
        for (let i = 0; i < len; i++) {

            var ri = rowsIndexes[i];

            table
                .rows(ri)                       //pick which row(s) to bring back
                .nodes()                        //grab all TR nodes (rows) under the .rows selector above
                .to$()                          //Convert to a jQuery object
                .removeClass('dale-clicked');   //remove dale-clicked class                   
        }

        localStorage.clear();                   // Clear all items in our array
        $('#btnSaveMe').hide();                 //hide the save button again
    },

    makeToast: function (severity, message) {

        if (severity === 'success') {
            toastr.options = {
                "closeButton": false,
                "debug": false,
                "newestOnTop": false,
                "progressBar": false,
                "positionClass": "toast-top-right",
                "preventDuplicates": false,
                "onclick": null,
                "showDuration": "300",
                "hideDuration": "1000",
                "timeOut": "5000",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut"
            };
        }
        else {
            toastr.options = {
                "closeButton": false,
                "debug": false,
                "newestOnTop": false,
                "progressBar": false,
                "positionClass": "toast-top-right",
                "preventDuplicates": false,
                "onclick": null,
                "showDuration": "1000",
                "hideDuration": "1000",
                "timeOut": "5000",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut"
            };
        }
        
        toastr[severity](message);
    }
};