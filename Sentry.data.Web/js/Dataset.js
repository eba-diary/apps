// ******************************************************************************************
// * Javascript methods for the Asset-related pages
// ******************************************************************************************

data.Dataset = {

    DatasetFilesTable: {},

    ViewModel: function () {
        var self = this;

        self.NoColumnsReturned = ko.observable();
        self.DataLoading = ko.observable(false);
        self.DataTableExists = ko.observable(false);
        self.RowCount = ko.observableArray();
        self.SchemaRows = ko.observableArray();
        self.DataLastUpdated = ko.observable();
        self.Description = ko.observable();
        self.LessDescription = ko.computed(function () {
            if (self.Description() != null && self.Description().length > 300) {

                var last = self.Description().substring(0, 300).lastIndexOf(" ");

                $('#schemaHasMoreDescription').show();
                return self.Description().substring(0, last);
            } else {
                $('#schemaHasMoreDescription').hide();
                return self.Description();
            }
        });
        self.MoreDescription = ko.computed(function () {
            if (self.Description() != null && self.Description().length > 300) {
                $('#schemaHasMoreDescription').show();
                var last = self.Description().substring(0, 300).lastIndexOf(" ");

                return " " + self.Description().substring(last);
            } else {
                $('#schemaHasMoreDescription').hide();
                return '';
            }
        });
        self.DFSDropLocation = ko.observable();
        self.SchemaId = null;
        self.S3DropLocation = ko.observable();
        self.OtherJobs = ko.observableArray();
        self.DataFlows = ko.observableArray();
        self.DFSCronJob = ko.observable();
        self.S3CronJob = ko.observable();
        self.CronJobs = ko.observableArray();
        self.Views = ko.observable();
        self.Downloads = ko.observable();
        self.FullyQualifiedSnowflakeViews = ko.observableArray();
        self.RenderDataFlows = ko.computed(function () {
            return !(self.DataFlows == undefined || self.DataFlows().length == 0);
        });
        self.ShowDataFileTable = ko.computed(function () {
            return self.DataLastUpdated() !== 'No Data Files Exist';
        });

        self.FooterRow = ko.computed(function () {
            return self.SchemaRows().length > 10;
        });

        self.MetadataLastDT = ko.computed(function () {
            if (self.SchemaRows().length >= 1) {
                var d = new Date("September 13, 1989 12:15:00");

                ko.utils.arrayForEach(self.SchemaRows(), function (feature) {
                    var f = new Date(feature.LastUpdated());
                    if (f > d) { d = f }
                });
                d = d.toLocaleString("en-us", { month: "long" }) + ' ' + d.getDate() + ', ' + d.getFullYear();
                $('#sessionSpinner').hide();
                return d;
            }
            else if (self.NoColumnsReturned()) {
                $('#sessionSpinner').hide();
                return null;
            }
            else {
                $('#sessionSpinner').show();
            }
        });

        self.AnyLastUpdated = ko.computed(function () {
            var d1 = Date.parse(self.DataLastUpdated());
            var d2 = Date.parse(self.MetadataLastDT());
            var date1;
            var date2;
            //Not dates for both metadata and data
            if (!isNaN(d1) && !isNaN(d2)) {

                date1 = new Date(d1);
                date2 = new Date(d2);
                $('#updatedSpinner').hide();
                $('#dataPreviewSection').show();
                if (date1 > date2) {
                    return date1.toLocaleString("en-us", { month: 'long' }) + ' ' + date1.getDate() + ', ' + date1.getFullYear();
                } else {
                    return date2.toLocaleString("en-us", { month: "long" }) + ' ' + date2.getDate() + ', ' + date2.getFullYear();
                }
            }
            //Only metadata date
            else if (!isNaN(d1) && isNaN(d2)) {
                $('#updatedSpinner').hide();
                $('#dataPreviewSection').show();
                date1 = new Date(d1);
                return date1.toLocaleString("en-us", { month: "long" }) + ' ' + date1.getDate() + ', ' + date1.getFullYear();
            }
            //Only data date
            else if (isNaN(d1) && !isNaN(d2)) {
                $('#updatedSpinner').hide();
                $('#dataPreviewSection').hide();
                date2 = new Date(d2);
                return date2.toLocaleString("en-us", { month: "long" }) + ' ' + date2.getDate() + ', ' + date2.getFullYear();
            }
            //Are both dates not populated
            else if (self.DataLastUpdated() === 'No Data Files Exist' && self.MetadataLastDT() === null) {
                $('#updatedSpinner').hide();
                $('#dataPreviewSection').hide();
                return $('#datasetInfoLastUpdated').text();
            }
            return self;
        });


    },

    //****************************************************************************************************
    //DELROY FUNCTIONS
    //****************************************************************************************************
    delroyFieldArray: [],
    delroySnowflakeViewsArray: [],
    delroyStructTrackerArray: [],
    firstTime: true,

    delroyInit: function () {

        data.Dataset.delroyTableCreate();
        data.Dataset.delroySetupClickAttack();
        data.Dataset.delroyStructTrackerArray = [];
    },


    //RELOAD EVERYTHING:  clean datatable, breadcrumbs and reload with schema selected
    delroyReloadEverything: function (datasetId, schemaId, snowflakeViews) {

        $('#delroySpinner').show();
        $('#delroyTable').DataTable().clear();
        $('#delroyTable').DataTable().draw();
        $('#delroyBreadcrumb').empty();
        data.Dataset.delroyFieldArray = [];
        data.Dataset.delroyStructTrackerArray = [];
        data.Dataset.delroySnowflakeViewsArray = snowflakeViews;

        var schemaURL = "/api/v2/metadata/dataset/" + datasetId + "/schema/" + schemaId + "/revision/latest/fields";
        $.get(schemaURL, function (result) {

            data.Dataset.delroyAddFieldArray(result.Fields);
            data.Dataset.delroyAddBreadCrumb(data.Dataset.delroyCreateBogusField("Home"), 0);
            data.Dataset.delroyGridRefresh();
            $('#delroySpinner').hide();

        }).fail(function (result) {
            if (result.status === 404) {
                $('#delroySpinner').hide();
                data.Dataset.delroyAddBreadCrumb(data.Dataset.delroyCreateBogusField("No Columns Exist"), -1);       //PASS -1 which indicates this is a FAKE breadcrumb
                data.Dataset.makeToast("success", "No columns Exist.");
            }
        });
    },


    //CREATE BOGUS FIELD OBJ FOR HOME OR NO COLUMNS EXIST
    delroyCreateBogusField: function (name) {

        //create new obj to represent a BOGUS field
        var o = new Object();
        o.Name = name;

        return o;
    },


    //INIT DATATABLE
    delroyTableCreate: function () {

        //init DataTable,
        var delroyTable = $("#delroyTable").DataTable({
            orderCellsTop: true,

            //client side setup
            pageLength: 100,

            //Code if i wanted the grid to reload and make AJAX call, but i reload manually without AJAX call in delroyGridRefresh
            //ajax: {
            //    url: "/api/v2/metadata/dataset/230/schema/4039/revision/latest/fields",
            //    type: "GET",
            //    dataSrc: "Fields"       //this is a trick too specify a property within the obj coming back from URL call, in this scenario i need to sepcify WHERE array of columns exists to load
            //},

            columns: [
                {
                    data: "Name", className: "Name",
                    render: function (d, type, row, meta) {
                        var color = $('#delroyBreadcrumb').data('page-color');
                        var innerLink = "<a  class='" + color + "' style='cursor:pointer' > " + d + "</a >";                        //want cursor too turn pointer
                        var parent = "<em class='far fa-folder-open " + color + "' > " + innerLink + "</em>";

                        if (row.Fields != null) {
                            return parent;
                        }
                        else {
                            return d;
                        }
                    }
                },
                { data: "Description", className: "Description" },
                { data: "FieldType", className: "FieldType" },
                { data: "IsArray", className: "IsArray" },
                { data: "Length", className: "Length", visible: false, render: function (d, type, row, meta) { return data.Dataset.delroyFillGridLength(d, row); } },
                { data: "Precision", className: "Precision", visible: false, render: function (d, type, row, meta) { return data.Dataset.delroyFillGridPrecisionScale(d, row); } },
                { data: "Scale", className: "Scale", visible: false, render: function (d, type, row, meta) { return data.Dataset.delroyFillGridPrecisionScale(d, row); } }
            ],

            aLengthMenu: [
                [20, 100, 500],
                [20, 100, 500]
            ],

            order: [0, 'desc'],

            //style for columnVisibility and paging to show
            //dom: 'B',

            dom: '<"d-inline-block mt-4"l><"float-right d-inline-block"B>t<"d-inline-block"i><"float-right d-inline-block"p>',
            buttons: {
                dom: {
                    container: {
                        className: 'dt-buttons btn-group'
                    },
                    button: {
                        className: 'btn btn-primary p-2'
                    },
                    collection: {
                        //tag: 'div',
                        className: 'dt-button-collection dropdown-menu',
                        button: {
                            //tag: 'a',
                            className: 'dropdown-item'
                        }
                    }
                },
                buttons: [
                    {
                        extend: 'colvis',
                        //text: "<i class='icon-onecol_twocol'></i>"
                    },
                    {
                        text: 'Snowflake Query',
                        action: function () {
                            data.Dataset.delroyQueryGenerator();
                        }
                    }
                ]
            }
        });
        if ($("#delroyTable_length").length > 0) {
            yadcf.init(delroyTable, [
                { column_number: 0, filter_type: 'text', style_class: 'form-control', filter_reset_button_text: false, filter_delay: 500 },
                { column_number: 1, filter_type: 'text', style_class: 'form-control', filter_reset_button_text: false, filter_delay: 500 },
                { column_number: 2, filter_type: 'text', style_class: 'form-control', filter_reset_button_text: false, filter_delay: 500 },
                { column_number: 3, filter_type: 'text', style_class: 'form-control', filter_reset_button_text: false, filter_delay: 500 },
                { column_number: 4, filter_type: 'text', style_class: 'form-control', filter_reset_button_text: false, filter_delay: 500 },
                { column_number: 5, filter_type: 'text', style_class: 'form-control', filter_reset_button_text: false, filter_delay: 500 },
                { column_number: 6, filter_type: 'text', style_class: 'form-control', filter_reset_button_text: false, filter_delay: 500 },
            ],
                {
                    filters_tr_index: 1
                }
            );
        }
    },



    //GRID CLICK EVENTS
    delroySetupClickAttack: function () {

        //DELROY GRID CLICK
        $('#delroyTable tbody').on('click', 'tr', function () {                         //anything clicked in delroyTable tbody, filter down to capture 'tr' class clicks only

            var table = $('#delroyTable').DataTable();
            var field = table.row(this).data();

            // click reload the grid if children exist
            if (field.Fields != null) {
                data.Dataset.delroyAddFieldArray(field);
                data.Dataset.delroyAddBreadCrumb(field, data.Dataset.delroyFieldArray.length - 1);
                data.Dataset.delroyGridRefresh();
            }
        });


        //BREADCRUMBS NAV CLICK EVENT
        $("#delroyBreadcrumb").on('click', 'li', function () {
            var myIndex = this.id;

            //only allow breadcrumbs use if have a valid Id)
            if (myIndex >= 0) {
                data.Dataset.delroyRefreshFieldArrayFromIndex(myIndex);
                data.Dataset.delroyRefreshBreadCrumbsFromIndex(myIndex);
                data.Dataset.delroyGridRefresh();
            }

        });


    },


    //ADD NEW FIELD TOO ARRAY
    delroyAddFieldArray: function (field) {

        data.Dataset.delroyFieldArray.push(field);
    },


    //GET LATEST FIELD FROM ARRAY
    delroyGetLatestField: function () {

        var index = data.Dataset.delroyFieldArray.length - 1;
        var field = data.Dataset.delroyFieldArray[index];
        //NOTE: this is a strange concept:  but a field is either a ROOT level one (index 0) or a CHILD level (index > 0)
        //All levels > (index 0) you need too specify the Fields array which is a child property whereas the root level(index 0) the array is at that level
        //also check if field is NULL, because in that case return a null field else returning field.Fields will cause an error
        if (index == 0 || field == null) {
            return field;                //index=0 means ROOT level where array of fields is actually on that level
        }
        else {
            return field.Fields;        //child of ROOT so specify array as a property of the field
        }
    },


    //CLEAR AND RELOAD DATATABLE WITH LATEST ELEMENT IN FIELD ARRAY
    delroyGridRefresh: function () {
        var table = $('#delroyTable').DataTable();
        var field = data.Dataset.delroyGetLatestField();

        table.clear();
        if (field) {
            table.rows.add(field);       //when clicking on an item, we need to specify Fields property since all chidren of ROOT use this
        }
        table.draw();

        //dont do anything first time since firstTime is on whole page refresh and we don't want to mess with scroll position
        if (data.Dataset.firstTime) {
            data.Dataset.firstTime = false;
        }
        else {
            //change scroll position to top of columns detail section upon grid refresh for consistency (anytime they click breadcrumb or click a struct)
            //this is needed because of grid expanding and shrinking when structs are selected and the scrolling un changed goes sometimes to bottom of grid
            var elmnt = document.getElementById("schemaSection");
            if (elmnt != undefined) {
                elmnt.scrollIntoView();
                window.scrollBy(0, -100); //adjust scrolling because scrollIntoView() is off by about 100px which is a known issue in various browsers
            }
        }
    },


    //ADD NEW BREADCRUMB TOO LIST
    delroyAddBreadCrumb: function (field, index) {

        //add breadcrumb to UI
        var color = $('#delroyBreadcrumb').data('page-color');
        var h = "<li class='breadcrumb-item' id='" + index.toString() + "' ><a  class='" + color + "' style='cursor:pointer' >" + field.Name + "</a></li>";
        $('#delroyBreadcrumb').append(h);

        //add struct too tracker to hold if a query needs to be generated
        if (field.Name !== "Home" && index > 0) {
            data.Dataset.delroyStructTrackerArray.push(field);
        }
    },


    //REFRESH BREAD CRUMBS:  Clear all breadcrumbs and refresh up too one passed in
    delroyRefreshBreadCrumbsFromIndex: function (lastIndexKeep) {
        //STEP 1: empty all breadcrumbs
        $('#delroyBreadcrumb').empty();

        //STEP 2: empty breadcrumb Tracker which feeds Query Generator
        var deleteStartIndex = parseInt(0);
        data.Dataset.delroyStructTrackerArray.splice(deleteStartIndex);

        //STEP 3: add in all breadcrumbs from start until the one they clicked on
        for (let i = 0; i < data.Dataset.delroyFieldArray.length; i++) {

            var field = {};
            if (i > 0) {
                field = data.Dataset.delroyFieldArray[i];
            }
            else {
                field = data.Dataset.delroyCreateBogusField("Home");
            }

            data.Dataset.delroyAddBreadCrumb(field, i);
        }
    },


    //REFRESH FIELD ARRAY : If user clicks a breadcrumb we need to refresh the array to ONLY contain up through last index too keep
    delroyRefreshFieldArrayFromIndex: function (lastIndexKeep) {

        //Refresh array to reflect current breadcrumb selected
        var deleteStartIndex = parseInt(lastIndexKeep, 10) + 1;     //use parseInt to ensure we are dealing with an integer data type
        data.Dataset.delroyFieldArray.splice(deleteStartIndex);     //splice deletes array from index specified essentially right half deleted
    },


    //LENGTH COLUMN LOGIC:  ONLY RETURN DATA IF VALID
    //Reason we do this is so for Length Column in Grid, zero won't show up because if its zero, we don't want to see it
    delroyFillGridLength: function (d, row) {
        if (d) {
            return d;
        }
        else {
            return ' ';
        }
    },

    //PRECISION AND SCALE LOGIC:  ONLY RETURN DATA IF VALID AND DECIMAL
    //Reason we do this is so for Prec/Scale is so they only show up for DECIMAL datatypes since thats only datatype to have prec/scale
    delroyFillGridPrecisionScale: function (d, row) {
        if (d != null && row.FieldType == 'DECIMAL') {
            return d;
        }
        else {
            return ' ';
        }
    },


    //GENERATE QUERY BASED ON WHERE THEY ARE IN SCHEMA     
    delroyQueryGenerator: function () {

        $('#delroySpinner').show();

        var field = data.Dataset.delroyGetLatestField();

        //if field is null, means no columns to generate a query so hide spinner and create a toastr
        if (field == null) {
            data.Dataset.makeToast("warning", "No columns Exist, Query not Generated.  Please select a schema with columns to see a query.");
            $('#delroySpinner').hide();
        }
        else {
            //pass the current field array and necessary info to controller and get back the snowflake query
            $.ajax({
                type: "POST",
                url: "/Dataset/DelroyGenerateQuery",
                traditional: true,
                data: JSON.stringify({ models: field, snowflakeViews: data.Dataset.delroySnowflakeViewsArray, structTracker: data.Dataset.delroyStructTrackerArray }),
                contentType: "application/json",
                success: function (r) {
                    var modal = Sentry.ShowModalWithSpinner("Snowflake Query");
                    modal.ReplaceModalBody(data.Dataset.delroyCreateModalHTML(r.snowQuery));
                    $('#delroySpinner').hide();
                },
                failure: function () {
                    data.Dataset.makeToast("error", "Error creating Snowflake Query.");
                    $('#delroySpinner').hide();
                },
                error: function () {
                    data.Dataset.makeToast("error", "Error creating Snowflake Query.");
                    $('#delroySpinner').hide();
                }
            });
        }
    },

    //FORMAT HTML TO SEND TOO MODAL
    //NOTE: instead of returning an entire partial view from controller, i just pass the snow query and format HTML in here, it shaved seconds off
    delroyCreateModalHTML: function (snowQuery) {

        var delroyQueryView = "<div class='delroyQueryView'> ";
        var delroyQueryMania = " <div id='delroyQueryMania' style='max-height: 600px; white-space: pre-line; overflow-y: auto; overflow-x: auto; '> " + snowQuery + " </div >";
        var delroyModalFooter = " <div class='modal-footer'>  <input id='delroyCopyClipboard' type='button' class='btn btn-warning' value='Copy to Clipboard' onclick='data.Dataset.delroyCopyClipboard()' />     </div >";
        delroyQueryView = delroyQueryView + delroyQueryMania + delroyModalFooter + " </div>";
        return delroyQueryView;
    },


    //COPY TEXT IN MODAL TO CLIPBOARD
    //NOTE: the reason I need to copy from delroyQueryMania is because it has the white-space style which interprets newlines and therefore a copy gets me a nicely formatted line
    //without that extra tag the copy directly from the output of the controller method DelroyGenerateQuery2 has the newlines but doesn't actually show formatting
    delroyCopyClipboard: function () {

        var range = document.createRange();
        range.selectNode(document.getElementById("delroyQueryMania"));
        window.getSelection().removeAllRanges(); // clear current selection
        window.getSelection().addRange(range); // to select text
        document.execCommand("copy");
        window.getSelection().removeAllRanges();// to deselect
    },

    //****************************************************************************************************
    //END DELROY FUNCTIONS
    //****************************************************************************************************



    UpdateMetadata: function () {

        if (!isNaN(getUrlParameter('configID'))) {
            $('#datasetConfigList').val(getUrlParameter('configID')).trigger('change');
        }

        if ($('#datasetConfigList').val() === undefined) { return; }

        var metadataURL = "/api/v1/metadata/datasets/" + $('#datasetConfigList').val();
        $.get(metadataURL, function (result) {
            //Set schema metadata
            self.vm.Downloads(result.Downloads);
            self.vm.Views(result.Views);
            self.vm.Description(result.Description);
            self.vm.FullyQualifiedSnowflakeViews.removeAll();
            $.each(result.SnowflakeViews, function (i, val) {
                self.vm.FullyQualifiedSnowflakeViews.push(val);
            });


            //Populate legacy retriever jobs
            self.vm.OtherJobs.removeAll();
            $.each(result.OtherJobs, function (i, val) {
                var item = new data.Dataset.DropLocation(val);

                self.vm.OtherJobs().push(item);
            });
            self.vm.OtherJobs.notifySubscribers();

            //Populate associated DataFlows
            self.vm.DataFlows.removeAll();
            $.each(result.DataFlows, function (i, val) {
                var item = new data.Dataset.DataFlow(val);

                //only add if data flow is active
                if (val.ObjectStatus === 1) self.vm.DataFlows().push(item);
            });
            self.vm.DataFlows.notifySubscribers();
            self.vm.SchemaId = result.SchemaId;
            //Determine last
            var d = new Date(result.DataLastUpdated);
            if (d < new Date('1990-01-01')) {
                self.vm.DataLastUpdated('No Data Files Exist');
            } else {
                d = d.toLocaleString("en-us", { month: "long" }) + ' ' + d.getDate() + ', ' + d.getFullYear();
                self.vm.DataLastUpdated(d);
            }
            $('#dataLastUpdatedSpinner').hide();

            data.Dataset.UpdateColumnSchema();
            data.Dataset.delroyReloadEverything(result.DatasetId, result.SchemaId, result.SnowflakeViews);

            data.Dataset.tryUpdateSchemaSearchTab();
        });
    },

    UpdateColumnSchema: function () {
        if ($('#datasetConfigList').val() === undefined) { return; }

        var schemaURL = "/api/v1/metadata/datasets/" + $('#datasetConfigList').val() + "/schemas/0/columns";

        self.vm.DataLoading(true);
        $('#dataSection').hide();

        //If no data files exist, 1.) do not query table, 2.) do not show Data Preview section
        if (!self.vm.ShowDataFileTable()) {
            data.Dataset.showDataPreviewError();
        }
        else
        {
            this.renderDataPreview();
        }

        $.get(schemaURL, function (result) {
            if (result.RowCount) {
                self.vm.RowCount(result.RowCount.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ","));
            } else {
                self.vm.RowCount(0);
            }

            if (result.rows.length == 0) {
                self.vm.NoColumnsReturned(true);
                $('#schemaHR').hide();
            }
            else {
                self.vm.NoColumnsReturned(false);
                $('#schemaHR').show();

                $.each(result.rows, function (i, val) {
                    var item = new data.Dataset.SchemaRow(val);

                    self.vm.SchemaRows().push(item);
                });
                self.vm.SchemaRows.notifySubscribers();
            }

        }).fail(function () {
            self.vm.RowCount(0);
            self.vm.SchemaRows([]);

            self.vm.NoColumnsReturned(true);
            $('#schemaHR').hide();
        });
    },

    renderDataPreview: function () {
        if ($("#datasetRowTable_filter").length > 0) {
            $("#datasetRowTable").DataTable().destroy();
        };
        $("#tab-spinner").show();


        $.ajax({
            type: "GET",
            url: "/api/v2/querytool/dataset/" + location.pathname.split('/')[3] + "/config/" + $('#datasetConfigList').val() + "/SampleRecords",
            data: {
                rows: 20
            },
            dataType: "json",
            success: function (msg) {
                if (msg !== undefined) {
                    var obj = JSON.parse(msg);
                    if (obj.length > 0) {
                        //pass data returned from AJAX call to render Data Table
                        data.Dataset.renderTable_v2(obj, false);

                        $('#dataSection').show();
                        self.vm.DataLoading(false);
                        self.vm.DataTableExists(true);

                        //this is a trick to know if datatable has been generated before we modify it
                        if ($("#datasetRowTable_filter").length > 0) {
                            $("#datasetRowTable").dataTable().fnAdjustColumnSizing();
                        }
                    }
                    else {
                        data.Dataset.showDataPreviewError();
                        $('#dataSection').hide();
                    }
                }
            },
            error: function (error) {
                //Schema NotFound response
                if (error.status == 404 && error.responseText == "Schema not found") {
                    $('#dataSection').hide();
                }

                data.Dataset.showDataPreviewError();
            },
            complete: function () {
                $("#tab-spinner").hide();
            }
        }).fail(function () {
            data.Dataset.showDataPreviewError();
        });

    },

    //DATA PREVIEW DATA TABLE SETUP
    renderTable_v2: function (input, push) {

        //Determine Column metadata
        var parsedColumns = [];

        //Remove existing column metadata
        $('#tableColumnIDs').empty();
        $('#tableFirstRow').empty();
        $('#datasetRowTable tbody').empty();

        //SETUP ALL COLUMNS for DATA PREVIEW
        Object.keys(input[0]).forEach(function (key) {

            //load up ARRAY with each column
            //CUSTOM RENDER will wrap column data in DIV which adds a scroll bar if needed since STRUCT columns are HUGE
            parsedColumns.push({
                'title': key.trim(), 'width': "auto", render: function (d) {
                    return "<div style='max-height:100px; overflow-y: auto;'>" + d + "</div>";
                }
            });

            $('#tableColumnIDs').append("<th>" + key.trim() + "</th>");
            $('#tableFirstRow').append("<td></td>");
        });

        var parsedRows = [];
        //Data for Each Row in the Result Set
        input.forEach(function (row) {
            var parsedCells = [];

            for (var key of Object.keys(row)) {
                parsedCells.push(row[key]);
            }

            if (parsedCells.length >= 1) {
                parsedRows.push(parsedCells);
            }
        });
        if ($("#datasetRowTable_filter").length > 0) {
            $("#datasetRowTable").DataTable().destroy();
        }
        if (!push) {
            $('#datasetRowTable').DataTable({
                "scrollX": true,
                "autoWidth": true,
                "scrollY": "1200px",                //set max height at 1200
                "scrollCollapse": true,             //if less then 1200, shrink so no white space
                data: parsedRows,
                columns: parsedColumns,
            });
        } else {
            $('#datasetRowTable').DataTable({ "scrollX": true }).rows.add(parsedRows).draw();
            $($('#datasetRowTable_info').children()[2]).text($('#rowCount').text());
        }

        $(".dataTables_filter").parent().addClass("text-right");
    },

    SchemaRow: function (data) {
        this.Name = ko.observable(data.Name);
        this.Description = ko.observable(data.Description);
        this.Type = ko.observable(data.Type);
        this.Precision = ko.observable(data.Precision);
        this.Scale = ko.observable(data.Scale);
        this.LastUpdated = ko.observable(data.LastUpdated);
    },

    DropLocation: function (data) {
        this.Name = ko.observable(data.Name);
        this.Location = ko.observable(data.Location);
        this.JobId = ko.observable(data.JobId);
        this.IsEnabled = ko.observable(data.IsEnabled);
    },

    DataFlow: function (dataInput) {
        var self = this;
        self.Name = ko.observable(dataInput.Name);
        self.Id = ko.observable(dataInput.Id);
        self.DetailUrl = ko.observable(dataInput.DetailUrl);
        self.Jobs = ko.observableArray();
        $.each(dataInput.RetrieverJobs, function (i, val) {
            var item = new data.Dataset.DropLocation(val);

            self.Jobs().push(item);
        });
        self.DataFlowDetailRedirect = function () {
            data.DataFlow.DetailUrlRedirect(this.Id());
        }
        self.DataFlowEditRedirect = function () {
            data.DataFlow.EditUrlRedirect(this.Id());
        }
        self.RenderJobs = ko.pureComputed(function () {
            return ko.unwrap(self.Jobs) ? "RenderJobs" : "noRenderJobs";
        });
    },





    IndexInit: function () {
        /// Initialize the Index page for data assets (with the categories)

        $(".sentry-app-version").hide();

        $("[id^='CreateDataset']").click(function (e) {
            e.preventDefault();
            window.location = "/Dataset/Create";
        });

        $("[id^='CreateDataflow']").click(function (e) {
            e.preventDefault();
            window.location = "/Dataflow/Create";
        });

        $("[id^='UploadDatafile']").click(function (e) {
            e.preventDefault();
            data.Dataset.UploadFileModal(0);
        });

        $('.tile').click(function (e) {
            var storedNames = JSON.parse(localStorage.getItem("filteredIds"));

            if (storedNames !== null) {
                for (i = 0; i < storedNames.length; i++) {
                    $('#' + storedNames[i]).prop('checked', false);
                }
                localStorage.removeItem("filteredIds");
            }

            localStorage.removeItem("searchText");
        });

        $('#DatasetSearch').submit(function (e) {
            localStorage.removeItem("filteredIds");

            localStorage.removeItem("searchText");

            localStorage.setItem("searchText", $('#SearchText')[0].value);
        });

        $('.input-group-prepend').click(function (e) {
            $('#DatasetSearch').submit();
        });

    },

    FormSubmitInit: function () {
        $.ajax({
            url: "/Dataset/DatasetForm",
            method: "POST",
            data: $("#DatasetForm").serialize(),
            dataType: 'json',
            success: function (obj) {
                if (Sentry.WasAjaxSuccessful(obj)) {
                    Sentry.HideAllModals();
                    //redirect to dataset detail page
                    window.location.href = "/Dataset/Detail/" + obj.dataset_id;
                }
                else {
                    $('#DatasetFormContent').html(obj);
                }
            },
            failure: function () {
                alert('An error occured submiting your request.  Please try again.');
            },
            error: function (obj) {
                $('#DatasetFormContent').html(obj.responseText);
                var hrEnv = $('#HrempServiceEnv').val();
                var hrUrl = $('#HrempServiceUrl').val();

                data.Dataset.FormInit(hrUrl, hrEnv, data.Dataset.FormSubmitInit);
            }
        });
    },

    FormCancelInit: function (e) {
        e.preventDefault();
        window.location = data.Dataset.CancelLink($(this).data("id"));
    },

    //INIT _DatasetCreateEdit.cshtml
    FormInit: function (hrEmpUrl, hrEmpEnv, PageSubmitFunction, PageCancelFunction) {

        //_DatasetCreateEdit Submit Button click 
        $('#SubmitDatasetForm').click(function (e) {

            //disable submit button so they cannot click more than once
            $('#SubmitDatasetForm').addClass("dataset-disable-stuff");
        });


        //CONFIGURE SAID ASSET PICKER on _DatasetCreateEdit.cshtml TO INCLUDE a filter box that comes up
        $(document).ready(function () {
            $("#DatasetCategoryIds").materialSelect();
            $("#saidAsset").materialSelect();
            $("#OriginationID").materialSelect();
            $("#DataClassification").materialSelect();
            $("#NamedEnvironmentPartial select").materialSelect();
            $("#DatasetScopeTypeId").materialSelect();
            $("#FileExtensionId").materialSelect();
        });

        $("#DatasetName").on('keyup', function () {
            let datasetNameWoSpecialChars = $("#DatasetName")[0].value.replace(/[^0-9a-zA-Z]/g, "");
            if (datasetNameWoSpecialChars.length <= 12) {
                $("#ShortName")[0].value = datasetNameWoSpecialChars;
                $("label[for=ShortName]").addClass("active");
            }
        })

        //saidAsset onChange needs to update #PrimaryOwnerName and #PrimaryOwnerId based on saidAsset picked
        $("#saidAsset").change(function () {
            //Load the named environments for the selected asset
            Sentry.InjectSpinner($("div#DatasetFormContent #namedEnvironmentSpinner"), 30);
            data.Dataset.populateNamedEnvironments();
        });

        //When the NamedEnvironment drop down changes (but only when it's rendered as a drop-down), reload the name environment type
        data.Dataset.initNamedEnvironmentEvents();

        //SubmitDatasetForm
        $("[id='SubmitDatasetForm']").click(PageSubmitFunction);

        if ($("#DatasetId").val() !== undefined && $("#DatasetId").val() > 0) {
            $("#DatasetScopeTypeId").attr('readonly', 'readonly');
        }

        //Set Secure HREmp service URL for associate picker
        $.assocSetup({ url: hrEmpUrl });
        var permissionFilter = "DatasetModify,DatasetManagement," + hrEmpEnv;

        $("#PrimaryContactName").assocAutocomplete({
            associateSelected: function (associate) {
                $('#PrimaryContactId').val(associate.Id);
            },
            filterPermission: permissionFilter,
            minLength: 0,
            maxResults: 10
        });

        $(".associatePicker label").addClass("active");
        $(".twitter-typeahead").addClass("w-100");

        if ($("#DatasetDesc").val()) {
            $('label[for=DatasetDesc]').addClass("active");
        }
        if ($("#DatasetInformation").val()) {
            $('label[for=DatasetInformation]').addClass("active");
        }

        $("#DataClassification").change(function () {
            switch ($("#DataClassification").val()) {
                case "1":
                    $('#dataClassInfo').text('“Restricted” information is proprietary and has significant business value for Sentry. ' +
                        'Unauthorized disclosure or dissemination could result in severe damage to Sentry.  Examples of restricted data include secret contracts or trade secrets.  ' +
                        'This information must be limited to only the few associates that require access to it.  If it is shared, accessed, or altered without the permission ' +
                        'of the Information Owner, Information Security must be notified immediately.  Designating information as Restricted involves significant ' +
                        'costs to Sentry.  For this reason, Information Owners making classification decisions must balance the damage that could result from ' +
                        'unauthorized access to or disclosure of the information against the cost of additional hardware, software or services required to protect it.');
                    $("#IsSecured").parents('.md-form').hide();
                    break;
                case "2":
                    $('#dataClassInfo').text('“Highly Sensitive” information is highly confidential, typically includes personally ' +
                        'identifiable information, and is intended for limited, specific use by a workgroup, ' +
                        'department, or group of individuals with a legitimate need to know. Disclosure or ' +
                        'dissemination of this information could result in significant damage to Sentry. ' +
                        'Examples of highly sensitive data include medical records, financial account or ' +
                        'bank account numbers, credit card numbers, individuals’ government-issued ' +
                        'identification numbers (for example driver’s license numbers, social security ' +
                        'numbers), and user passwords. This information must be limited to need to know ' +
                        'access. If it is shared, accessed, or altered without the permission of the ' +
                        'Information Owner, Information Security must be notified immediately.');
                    $("#IsSecured").parents('.md-form').hide();
                    break;
                case "3":
                    $('#dataClassInfo').text('“Internal Use Only” information can be disclosed or disseminated to Sentry ' +
                        'associates, but will only be shared with other individuals or organizations when a ' +
                        'non - disclosure agreement is in place and management has approved for legitimate ' +
                        'business reasons.  Examples include items such as email correspondence, internal ' +
                        'documentation that is available to all associates.');
                    $("#IsSecured").parents('.md-form').show();
                    break;
                case "4":
                    $('#dataClassInfo').text('“Public” information can be disclosed or disseminated without any restrictions on ' +
                        'content, audience, or time of publication.  Examples are datasets that were generated by the Federal or State Governments like the Federal Motor Carrier Safety Administration or NOAA Weather Data.  ' +
                        'These datasets can be freely shared throughout Sentry.');
                    $("#IsSecured").parents('.md-form').hide();
                    break;
            }
        }).change();

        $("[id^='CancelButton']").off('click').on('click', PageCancelFunction);

        $("#DatasetFileUpload").change(function () {

            //Hide Configuration List
            $('#configList').parent().parent().show();
            $("#configDescription").show();

            var fileUpload = $("#DatasetFileUpload").get(0);
            var files = fileUpload.files;

            if (files.length > 0) {
                getConfigs();
            }
            else {
                //Hide Configuration List
                $('#configList').parent().parent().hide();
                $("#configDescription").hide();
            }
        });

        data.Config.SetFileExtensionProperites($('#FileExtensionId option:selected').text(), $('#DatasetId').val() !== "0");

        $("#FileExtensionId").change(function () {
            data.Config.SetFileExtensionProperites($('#FileExtensionId option:selected').text(), $('#DatasetId').val() !== "0");
        });

        data.Config.DatasetScopeTypeInit($("#DatasetScopeTypeId"));
    },

    DetailInit: function (datasetDetailModel) {

        this.delroyInit();

        $("[id^='EditDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            window.location = "/Dataset/Edit/" + encodeURI($(this).data("id"));
        });

        $("[id^='Pushtofilename_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.FileNameModal($(this).data("id"));
        });

        $("[id^='UploadModal']").click(function (e) {
            e.preventDefault();
            data.Dataset.UploadFileModal($(this).data("id"));
        });

        $("[id^='RequestAccessButton']").off('click').on('click', function (e) {
            e.preventDefault();
            data.AccessRequest.InitForDataset($(this).data("id"));
        });

        $("[id^='DownloadLatest']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.DownloadLatestDatasetFile($(this).data("id"));
        });

        $("[id^='PushtoSAS_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PushToSAS($(this).data("id"));
        });

        $("[id^='Pushtofilename_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.FileNameModal($(this).data("id"));
        });

        $("[id^='SubscribeModal']").click(function (e) {
            e.preventDefault();
            data.Dataset.SubscribeModal($(this).data("id"));
        });


        $("[id^='detailSectionHeader_']").click(function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var category = "#hide_" + id;
            var icon = "#icon_" + id;

            $(category).slideToggle();
            $(icon).toggleClass("fa-chevron-down fa-chevron-up");
        });

        var Id = $('#datasetConfigList').val();

        localStorage.setItem("listOfFilesToBundle", JSON.stringify([]));

        $("#bundle_selected").click(function (e) {
            e.preventDefault();

            var datasetID = window.location.pathname.substr(window.location.pathname.lastIndexOf('/') + 1);
            var listOfFilesToBundle = JSON.parse(localStorage.getItem("listOfFilesToBundle"));

            var configName = $('#' + listOfFilesToBundle[0]).children('td.ConfigFileName').text();
            var multipleConfigs = false;

            for (var i = 0; i < listOfFilesToBundle.length; i++) {
                if ($('#' + listOfFilesToBundle[i]).children('td.ConfigFileName').text() !== configName) {
                    multipleConfigs = true;
                    break;
                }
            }

            if (multipleConfigs) {
                Sentry.ShowModalAlert(
                    "There are multiple configuration files associated with the set of files you want to bundle.  Please only select file from a single configuration."
                );
            }
            else {
                data.Dataset.PushToBundler(datasetID, listOfFilesToBundle);
            }
        });

        $("#bundle_allFiltered").click(function (e) {
            e.preventDefault();

            var datasetID = window.location.pathname.substr(window.location.pathname.lastIndexOf('/') + 1);
            var params = Sentry.GetDataTableParamsForExport($('#datasetFilesTable').DataTable());

            $.ajax({
                url: "/Dataset/GetDatasetFileInfoForGrid/?Id=" + Id,
                method: "POST",
                data: params,
                dataType: 'json',
                success: function (obj) {
                    var listOfFilesToBundle = [];
                    var configName = obj.data[0].ConfigFileName;
                    var multipleConfigs = false;

                    for (i = 0; i < obj.data.length; i++) {
                        listOfFilesToBundle.push(obj.data[i].Id);

                        if (obj.data[i].ConfigFileName !== configName) {
                            multipleConfigs = true;
                        }
                    }
                    if (multipleConfigs) {
                        Sentry.ShowModalAlert("There are multiple configuration files associated with the set of files you want to bundle.  Please only select file from a single configuration.");
                    }
                    else {
                        data.Dataset.PushToBundler(datasetID, listOfFilesToBundle);
                    }
                },
                failure: function () { },
                error: function () { }
            });
        });

        $('body').on('click', '.jobstatus', function () {
            var controllerurl = "";
            if ($(this).hasClass('jobstatus_enabled')) {
                controllerurl = "/Dataset/DisableRetrieverJob/?id=";
            }
            else {
                controllerurl = "/Dataset/EnableRetrieverJob/?id=";
            }

            $.ajax({
                url: controllerurl + $(this).attr('id'),
                method: "POST",
                dataType: 'json',
                success: function (obj) {
                    Sentry.ShowModalConfirmation(obj.Message, function () { location.reload(); });
                },
                failure: function (obj) {
                    Sentry.ShowModalAlert(obj.Message, function () { location.reload(); });
                },
                error: function (obj) {
                    Sentry.ShowModalAlert(obj.Message, function () { location.reload(); });
                }
            });
        });


        data.Dataset.SetReturntoSearchUrl();

        $('#datasetConfigList').select2({ width: '85%' });

        $('body').on('click', '.on-demand-run', function () {
            $.ajax({
                url: "/Dataset/RunRetrieverJob/?id=" + $(this).attr('id'),
                method: "POST",
                dataType: 'json',
                success: function (obj) {
                    Sentry.ShowModalConfirmation(obj.Message, function () { });
                },
                failure: function (obj) {
                    Sentry.ShowModalAlert(obj.Message, function () { });
                },
                error: function (obj) {
                    Sentry.ShowModalAlert(obj.Message, function () { });
                }
            });
        });

        $(document).on("click", "[id^='btnFavorite']", function (e) {
            e.preventDefault();
            var icon = $(this).children();
            $.ajax({
                url: '/Favorites/SetFavorite?datasetId=' + encodeURIComponent($(this).data("id")),
                method: "GET",
                dataType: 'json',
                success: function () { icon.toggleClass("fas far"); },
                error: function () { Sentry.ShowModalAlert("Failed to toggle favorite."); }
            });
        });



        $('body').on('click', '#btnDeleteDataset', function () {
            var dataset = $(this);
            var UserAccessMessage;

            if (dataset.attr("data-IsSecured") === "False") {
                UserAccessMessage = "This is a public dataset, all users have access.";
            }
            else {
                UserAccessMessage = "There are " + dataset.attr("data-grpCnt") + " active directory groups with access to this dataset.";
            }

            var ConfirmMessage = "<p>Are you sure?</p><p><h3><b><font color=\"red\">THIS IS NOT A REVERSIBLE ACTION!</font></b></h3></p> </br> <p>" + UserAccessMessage + "</p>  Deleting the dataset will remove all associated schemas, data files, hive consumption layers, and metadata.  If at a later point " +
                "this is needed, dataset and schema(s) will need to be recreated along with all files resent from source."


            Sentry.ShowModalCustom("Delete Dataset", ConfirmMessage,
                '<button type="button" id="datasetDeleteBtn" data-dismiss="modal" class="btn btn-danger waves-effect waves-light">Delete</button>'
            );
            $("#datasetDeleteBtn").click(function () {
                $.ajax({
                    url: "/Dataset/" + dataset.attr("data-id") + "/Delete",
                    method: "DELETE",
                    dataType: 'json',
                    success: function (obj) {
                        Sentry.ShowModalAlert(obj.Message, function () {
                            window.location = "/Dataset";
                        })
                    },
                    failure: function (obj) {
                        Sentry.ShowModalAlert(
                            obj.Message, function () { })
                    },
                    error: function (obj) {
                        Sentry.ShowModalAlert(
                            obj.Message, function () { })
                    }
                });
            });
        });

        $('body').on('click', "#btnSyncConsumptionLayers", function () {
            var syncBtn = $(this);
            var datasetId = syncBtn.attr("data-id");

            var warningMsg = `<p><b><h3><font color=\"red\">WARNING</font color></h3></b></p><p>Performing this action will re-generate all hive consumption layer tables and views for associated schemas.</p>
            <p>In addition, this will generate notification to SAS Administration to refresh associated metadata.  Depending on schema change, this
            may break SAS processes referencing these libraries.</p>`;

            Sentry.ShowModalConfirmation(warningMsg, function () {
                $.ajax({
                    url: "/api/v2/metadata/dataset/" + datasetId + "/schema/0/syncconsumptionlayer",
                    method: "POST",
                    dataType: 'json',
                    success: function (obj) {
                        Sentry.ShowModalAlert(obj, function () { });
                    },
                    failure: function () {
                        Sentry.ShowModalAlert("Failed to submit request", function () { });
                    },
                    error: function (obj) {
                        var msg;
                        if (obj.status === 400) {
                            msg = obj.responseJSON.Message;
                        }
                        else {
                            msg = "Failed to submit request";
                        };
                        Sentry.ShowModalAlert(msg, function () { });
                    }
                });
            });
        });

        //*****************************************************************************************************
        //CONFIG DROP DOWN CHANGE
        //*****************************************************************************************************
        $('#datasetConfigList').on('select2:select', function (e) {
            $("#tab-spinner").show();
            var url = new URL(window.location.href);
            url.searchParams.set('configID', $('#datasetConfigList').val());
            window.history.pushState({}, '', url);

            Id = $('#datasetConfigList').val();
            self.vm.NoColumnsReturned(false);
            $('#schemaHR').show();
            self.vm.SchemaRows.removeAll();

            if ($("#datasetFilesTable_filter").length > 0) {
                var fileInfoURL = "/Dataset/GetDatasetFileInfoForGrid/?Id=" + $('#datasetConfigList').val();
                $('#datasetFilesTable').DataTable().ajax.url(fileInfoURL).load();
            }

            if (!$("#DataPreviewNoRows").hasClass("d-none"))
            { 
                $("#DataPreviewNoRows").addClass("d-none"); //Hide no rows returned div if it is shown, preview code should show it again if necessary
            }

            data.Dataset.UpdateMetadata();

            $("#schemaSearchInput").val("")

            $("#tab-spinner").hide();

        });
        Id = $('#datasetConfigList').val();
        //on initial load, try pulling the Id from the URL first. 

        Id = $('#datasetConfigList').val();
        //on initial load, try pulling the Id from the URL first.

        $('#dataLastUpdatedSpinner').show();
        //data.Dataset.UpdateMetadata();
        var url = new URL(window.location.href);

        Id = url.searchParams.get('configID')
        if (Id == undefined) {
            Id = $('#datasetConfigList').val();
        }

        data.Dataset.DatasetFileTableInit(Id);
        data.Dataset.DatasetBundingFileTableInit(Id);

        //Hook up handlers for tabbed sections

        $('#detailTabSchemaColumns').click(function (e) {
            e.preventDefault();
            var id = $('#RequestAccessButton').attr("data-id");
            $("#schemaSearchInput").val("")

            var url = new URL(window.location.href);
            url.searchParams.set('tab', 'SchemaColumns');
            window.history.pushState({}, '', url);

            if ($('#tabSchemaColumns').is(':empty')) {
                $("#tab-spinner").show();
                $.ajax({
                    type: "POST",
                    url: '/Dataset/DetailTab/' + id + '/' + 'SchemaColumns',
                    data: datasetDetailModel,
                    success: function (view) {
                        $('#tabSchemaColumns').html(view);
                        data.Dataset.delroyInit();
                        data.Dataset.UpdateMetadata();
                        $('#delroySpinner').hide();
                        $("#tab-spinner").hide();
                    }
                });
            }
            else {
                $.ajax({
                    url: '/Dataset/DetailTab/' + id + '/' + 'SchemaColumns/LogView',
                });
            }
        });

        $('#detailTabSchemaAbout').click(function (e) {
            e.preventDefault();

            var url = new URL(window.location.href);
            url.searchParams.set('tab', 'SchemaAbout');
            window.history.pushState({}, '', url);

            var id = $('#RequestAccessButton').attr("data-id");
            $("#schemaSearchInput").val("")

            if ($('#tabSchemaAbout').is(':empty')) {
                $("#tab-spinner").show();
                $.ajax({
                    type: "POST",
                    url: '/Dataset/DetailTab/' + id + '/' + 'SchemaAbout',
                    data: datasetDetailModel,
                    success: function (view) {
                        $('#tabSchemaAbout').html(view);
                        ko.applyBindings(self.vm, $("#tabSchemaAbout")[0]);
                        data.Dataset.UpdateMetadata();
                        $("#tab-spinner").hide();
                    }
                });
            }
            else {
                $.ajax({
                    url: '/Dataset/DetailTab/' + id + '/' + 'SchemaAbout/LogView',
                });
            }
        });

        $('#detailTabDataPreview').click(function (e) {
            e.preventDefault();

            var url = new URL(window.location.href);
            url.searchParams.set('tab', 'DataPreview');
            window.history.pushState({}, '', url);

            var id = $('#RequestAccessButton').attr("data-id");
            $("#schemaSearchInput").val("")

            if ($('#tabDataPreview').is(':empty')) {
                $("#tab-spinner").show();
                $.ajax({
                    type: "POST",
                    url: '/Dataset/DetailTab/' + id + '/' + 'DataPreview',
                    data: datasetDetailModel,
                    success: function (view) {
                        $('#tabDataPreview').html(view);
                        if (self.vm.ShowDataFileTable()) {
                            data.Dataset.renderDataPreview();
                        }
                        $("#tab-spinner").hide();
                    }
                });
            }
            else {
                $.ajax({
                    url: '/Dataset/DetailTab/' + id + '/' + 'DataPreview/LogView',
                });
            }
        });

        $('#detailTabDataFiles').click(function (e) {
            e.preventDefault();
            var id = $('#RequestAccessButton').attr("data-id");
            $("#schemaSearchInput").val("")

            var url = new URL(window.location.href);
            url.searchParams.set('tab', 'DataFiles');
            window.history.pushState({}, '', url);

            if ($('#tabDataFiles').is(':empty')) {
                $("#tab-spinner").show();
                $.ajax({
                    type: "POST",
                    url: '/Dataset/DetailTab/' + id + '/' + 'DataFiles',
                    data: datasetDetailModel,
                    success: function (view) {
                        $('#tabDataFiles').html(view);
                        if (self.vm.ShowDataFileTable()) {
                            var configId = $('#datasetConfigList').val();
                            data.Dataset.DatasetFileTableInit(configId);
                            data.Dataset.DatasetBundingFileTableInit(configId);
                        }
                        $("#tab-spinner").hide();
                    }
                });
            }
            else {
                $.ajax({
                    url: '/Dataset/DetailTab/' + id + '/' + 'DataFiles/LogView',
                });
            }
        });

        $('#detailTabSchemaSearch').click(function (e) {
            e.preventDefault();
            var id = $('#RequestAccessButton').attr("data-id");

            var url = new URL(window.location.href);
            url.searchParams.set('tab', 'SchemaSearch');
            window.history.pushState({}, '', url);

            if ($('#tabSchemaSearch').is(':empty')) {
                $("#tab-spinner").show();
                $.ajax({
                    type: "POST",
                    url: '/Dataset/DetailTab/' + id + '/' + 'SchemaSearch',
                    data: datasetDetailModel,
                    success: function (view) {
                        $('#tabSchemaSearch').html(view);
                        var metadataURL = "/api/v2/metadata/datasets/" + $('#datasetConfigList').val();

                        $.get(metadataURL, function (result) {
                            self.vm.SchemaId = result.SchemaId;
                            data.Dataset.InitSchemaSearchTab();
                        });

                        $("#tab-spinner").hide();
                    }
                });
            }
            else {
                $.ajax({
                    url: '/Dataset/DetailTab/' + id + '/' + 'SchemaSearch/LogView',
                });
            }
        });

        var url = new URL(window.location.href);
        var tab = url.searchParams.get('tab');
        if (tab == undefined) {
            tab = 'SchemaAbout';
        }
        $("#detailTab" + tab).trigger('click');

    },

    CancelLink: function (id) {
        if (id === undefined || id === 0) {
            return "/Dataset/Index";
        } else {
            return "/Dataset/Detail/" + encodeURIComponent(id);
        }
    },

    PushToSAS_Filename: function (id, filename, delimiter, guessingrows) {
        /// Download dataset from S3 and push to SAS file share
        var modal = Sentry.ShowModalWithSpinner("PushToMessage");
        var controllerURL = "/Dataset/PushToSAS/?id=" + encodeURI(id) + "&fileOverride=" + encodeURI(filename) + "&delimiter=" + encodeURI(delimiter) + "&guessingrows=" + encodeURI(guessingrows);
        $.post(controllerURL, function (result) {
            modal.ReplaceModalBody(result);
        });
    },

    PushToBundleInit: function () {
        //Initialize the PushToFilenameOverride partial view
        $("[id^='BundleFilename']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PushToSAS_Filename($(this).data("id"), $("#BundleFilename").val());
        });
    },

    PushToOverrideInit: function () {
        ///Initialize the PushToFilenameOverride partial view
        $("PushToForm").validateBootstrap(true);

        $("[id^='FilenameOverride']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PushToSAS_Filename($(this).data("id"), $("#FileNameOverride").val(), $("#Delimiter").val(), $("#GuessingRows").val());
        });
    },

    startPushToSAS: function () {
        Sentry.HideAllModals();
        ProgressModalStatus();
    },

    ProgressModalStatus: function () {
        // Reference the auto-generated proxy for the hub.
        var progress = $.connection.progressHub;
        // Create a function that the hub can call back to display messages.
        progress.client.AddProgress = function (message, percentage) {
            ProgressBarModal("show", message, "Progress: " + percentage);
            $('#ProgressMessage').width(percentage);
            if (percentage === "100%") {
                ProgressBarModal();
            }
        };

        connectionId = $.connection.hub.start()
            .done(function () {
                var connectionId = $.connection.hub.id;
            });
    },

    DownloadLatestDatasetFile: function (id) {
        /// Send temp URL (containing the dataset, from S3) to a new window
        /// This will initiate the download process

        var getLatestURL = "/Dataset/GetLatestDatasetFileIdForDataset/?id=" + encodeURI(id);
        $.get(getLatestURL, function (e) {
            var controllerURL = "/Dataset/GetDatasetFileDownloadURL/?id=" + encodeURI(e);
            $.get(controllerURL, function (result) {
                window.open(result, "_blank");
            })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    Sentry.ShowModalCustom("Error", jqXHR.responseJSON.message, {
                        Cancel:
                        {
                            label: 'Ok',
                            className: 'btn-Ok'
                        }
                    });
                });
        });
    },

    SubscribeModal: function (id) {
        var modal = Sentry.ShowModalWithSpinner("Subscribe");
        $.get("/Dataset/Subscribe/?id=" + encodeURI(id), function (e) {
            modal.ReplaceModalBody(e);
        });
    },

    FileNameModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("File Name Override");

        $.get("/Dataset/PushToFileNameOverride/" + id, function (result) {
            modal.ReplaceModalBody(result);
            modal.SetFocus("#FileNameOverride");
        });
    },

    DownloadDatasetFile: function (id) {
        /// <summary>
        /// Send temp URL (containing the dataset, from S3) to a new window
        /// This will initiate the download process
        /// </summary>

        var controllerURL = "/Dataset/GetDatasetFileDownloadURL/?id=" + encodeURI(id);
        $.get(controllerURL, function (result) {

            if (result.message && result.message.startsWith('Encountered Error Retrieving File')) {
                Sentry.ShowModalCustom("Error", result.message, {
                    Cancel:
                    {
                        label: 'Ok',
                        className: 'btn-Ok'
                    }
                });
            } else {
                window.open(result, "_blank");
            }
        })
            .fail(function (jqXHR, textStatus, errorThrown) {
                Sentry.ShowModalCustom("Error", jqXHR.responseJSON.message, {
                    Cancel:
                    {
                        label: 'Ok',
                        className: 'btn-Ok'
                    }
                });
            });
    },

    UploadFileModal: function (id) {
        /// <summary>
        /// Load Modal for Uploading new Datafile
        /// </summary>
        var modal = Sentry.ShowModalWithSpinner("Upload Data File");
        //var createDatafileUrl = "/Dataset/GetDatasetUploadPartialView/?datasetId=" + encodeURI(id);
        var selectedConfig = $('#datasetConfigList').find(":selected").val();
        var createDatafileUrl = "/Dataset/Upload/" + encodeURI(id) + "/Config/" + encodeURI(selectedConfig);

        $.get(createDatafileUrl, function (e) {
            modal.ReplaceModalBody(e);
            data.Dataset.UploadModalInit(id);
        });
    },

    EditDataFileInformation: function (id) {
        var modal = Sentry.ShowModalWithSpinner("Edit Data File");

        $.get("/Dataset/EditDatasetFile/" + id, function (result) {
            modal.ReplaceModalBody(result);
        });
    },

    UploadModalInit: function (id) {
        var configs;
        var idFromSelectList = $('#datasetConfigList').find(":selected").val();
        var dID;

        if (idFromSelectList === undefined) {
            dId = id;
        } else {
            dId = idFromSelectList;
        }

        $('#btnUploadFile').prop("disabled", true);

        $("[id^='btnUploadFile']").off('click').on('click', function () {
            $('#btnUploadFile').closest('.bootbox').hide();
            $('.modal-backdrop').remove();

            var modal = Sentry.ShowModalWithSpinner("Upload Results", {
                Confirm: {
                    label: 'Confirm',
                    className: 'btn-success'
                },
                Cancel:
                {
                    label: 'Cancel',
                    className: 'btn-cancel'
                }
            });

            // This approach is from the following site:
            // http://www.c-sharpcorner.com/UploadFile/manas1/upload-files-through-jquery-ajax-in-Asp-Net-mvc/
            if (window.FormData !== undefined) {
                var fileUpload = $("#DatasetFileUpload").get(0);
                var files = fileUpload.files;

                //Create FormData object
                var fileData = new FormData();

                fileData.append(files[0].name, files[0]);
                var datasetID = $('#DatasetId').val();
                var configID = $("#configList").val();

                var token = $('input[name="__RequestVerificationToken"]').val();

                var xhr = new XMLHttpRequest();

                (xhr.upload || xhr).addEventListener('progress', function (e) {
                    var done = e.position || e.loaded;
                    var total = e.totalSize || e.total;

                    $('#percentTotal').text(Math.round(done / total * 100) + '%');
                    $('#progressKB').text('(' + Math.round(done / 1024) + ' KB / ' + Math.round(total / 1024) + ' KB)');
                    $('#progressBar').width(Math.round(done / total * 100) + '%');

                    $('.btn-success').prop("disabled", true);
                });
                xhr.addEventListener('load', function (e) {
                    $('.modal-footer button').prop("disabled", false);
                    modal.ReplaceModalBody(e.currentTarget.response.replace(/"/g, ''));
                });

                $('.btn-cancel')[0].addEventListener('click', function () { xhr.abort(); }, false);
                $('.bootbox-close-button').hide();
                var url = '/Dataset/UploadDatafile/?id=' + encodeURI(datasetID) + "&configId=" + encodeURI(configID);
                xhr.open('post', url, true);
                xhr.setRequestHeader('__RequestVerificationToken', token);
                xhr.send(fileData);
            } else {
                alert("FormData is not supported");
            }
        });

        $("#DatasetFileUpload").change(function () {

            var fileUpload = $("#DatasetFileUpload").get(0);
            var files = fileUpload.files;
            if (files[0].size / 1000000 > 100) {
                message = 'The file you are attempting to upload to is too large to upload through the browser.  ' +
                    'Please use a drop location to upload this file.' +
                    '<br/><br/>' +
                    'If you don\'t have access to this drop location please contact <a href="mailto:DSCSupport@sentry.com"> Data.sentry.com Administration </a> for further assistance.'

                largeFileModel = Sentry.ShowModalCustom("File Too Large", message, Sentry.ModalButtonsOK())
                //largeFileModel = Sentry.ShowModalAlert(message);
                largeFileModel.show();
            }
            else {
                $("#btnUploadFile").prop("disabled", false);
                $('#btnUploadFile').show();
            }
        });
    },

    DatasetFileTableInit: function (Id) {

        data.Dataset.DatasetFilesTable = $("#datasetFilesTable").DataTable({
            orderCellsTop: true,
            width: "100%",
            serverSide: true,
            //responsive: true,
            processing: true,
            searching: true,
            paging: true,
            destroy: true,
            rowId: 'Id',
            ajax: {
                url: "/Dataset/GetDatasetFileInfoForGrid/?Id=" + Id,
                type: "POST"
            },
            iDisplayLength: 10,
            aLengthMenu: [
                [10, 25, 50, 100, 200, -1],
                [10, 25, 50, 100, 200, "All"]
            ],
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px", searchable: false },
                { data: "ActionLinks", className: "downloadFile", width: "100px", searchable: false, orderable: false },
                { data: "Name", className: "Name" },
                { data: "UploadUserName", className: "UploadUserName" },
                { data: "CreateDtm", className: "createdtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss a") : null; } },
                { data: "ModifiedDtm", type: "date", className: "modifieddtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss a") : null; } },
                { data: "ConfigFileName", className: "ConfigFileName" }
            ],
            language: {
                processing: ""
            },
            order: [5, 'desc'],
            stateSave: true,
            "createdRow": function (row, data, dataIndex) { }
        });

        if ($("#datasetFilesTable_filter").length > 0) {
            yadcf.init(data.Dataset.DatasetFilesTable, [
                { column_number: 2, filter_type: 'text', style_class: 'form-control', filter_reset_button_text: false, filter_delay: 500 },
                { column_number: 3, filter_type: 'text', style_class: 'form-control', filter_reset_button_text: false, filter_delay: 500 },
               // { column_number: 4, filter_type: 'range_date', datepicker_type: null, filter_reset_button_text: false, filter_delay: 500 },  SEE CLA-4004
               // { column_number: 5, filter_type: 'range_date', datepicker_type: null, filter_reset_button_text: false, filter_delay: 500 },
                { column_number: 6, filter_type: 'text', style_class: 'form-control', filter_reset_button_text: false, filter_delay: 500 },
            ],
                {
                    filters_tr_index: 1
                }
            );
        }

        $(".dataTables_filter").parent().addClass("text-right");
        $(".dataTables_filter").parent().css("right", "3px");

        var table = $('#datasetFilesTable').DataTable();

        $('#datasetFilesTable tbody').on('click', 'td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = table.row(tr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                row.child(data.Dataset.formatDatasetFileDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

        $('#datasetFilesTable tbody').on('click', 'td', function () {
            var tr = $(this).parent();
            var listOfFilesToBundle;
            if (!$(this).hasClass('details-control')) {
                if (!tr.hasClass('unUsable') && (tr.hasClass('even') || tr.hasClass('odd'))) {
                    if (tr.hasClass('active')) {
                        tr.removeClass('active');
                        listOfFilesToBundle = JSON.parse(localStorage.getItem("listOfFilesToBundle"));

                        if ($(tr).prop('id')) {

                            if (listOfFilesToBundle !== null) {
                                listOfFilesToBundle.splice(listOfFilesToBundle.indexOf($(tr).prop('id')), 1);
                            }
                            localStorage.setItem("listOfFilesToBundle", JSON.stringify(listOfFilesToBundle));
                            $('#bundleCountSelected').html(parseInt($('#bundleCountSelected').html(), 10) - 1);
                        }
                    }
                    else {
                        tr.addClass('active');

                        listOfFilesToBundle = JSON.parse(localStorage.getItem("listOfFilesToBundle"));
                        if ($(tr).prop('id')) {

                            if (listOfFilesToBundle !== null) {
                                listOfFilesToBundle[listOfFilesToBundle.length] = $(tr).prop('id');
                            }
                            else {
                                listOfFilesToBundle = [];
                                listOfFilesToBundle[0] = $(tr).prop('id');
                            }

                            localStorage.setItem("listOfFilesToBundle", JSON.stringify(listOfFilesToBundle));
                            $('#bundleCountSelected').html(parseInt($('#bundleCountSelected').html(), 10) + 1);
                        }
                    }
                }
            }

            if (parseInt($('#bundleCountSelected').html(), 10) < 2) {
                $('#bundle_selected').attr("disabled", true);
            }
            else {
                $('#bundle_selected').attr("disabled", false);
            }
            if (parseInt($('#bundleCountFiltered').html(), 10) < 2) {
                $('#bundle_allFiltered').attr("disabled", true);
            }
            else {
                $('#bundle_allFiltered').attr("disabled", false);
            }
        });

        $('#datasetFilesTable').on('draw.dt', function () {
            if ($('#datasetFilesTable >tbody >tr').length >= 1 && $($('#datasetFilesTable >tbody >tr>td')[0]).hasClass('dataTables_empty') === false) {
                $("#UploadModal").css({ "animation": "none" });
                $('#alertInfoBanner').hide();
            } else {
                $("#UploadModal").css({ "animation": "blink 2s ease-in infinite" });
                $('#alertInfoBanner').show();
            }

            $('#bundleCountFiltered').html(data.Dataset.DatasetFilesTable.page.info().recordsDisplay);
            $('#bundleCountSelected').html(0);
            localStorage.setItem("listOfFilesToBundle", JSON.stringify([]));

            if (data.Dataset.DatasetFilesTable.page.info().recordsDisplay < 2) {
                $('#bundle_allFiltered').attr("disabled", true);
            }
            else {
                $('#bundle_allFiltered').attr("disabled", false);
            }
            if (parseInt($('#bundleCountSelected').html(), 10) < 2) {
                $('#bundle_selected').attr("disabled", true);
            }
            else {
                $('#bundle_selected').attr("disabled", false);
            }
        });
    },

    formatDatasetFileDetails: function (d) {
        // `d` is the original data object for the row
        var table = '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">';
        if (d.Information !== null) {
            table +=
                '<tr>' +
                '<td><b>Information</b>: </td>' +
                '<td>' + d.Information + '</td>' +
                '</tr>';
        }
        table +=
            '<tr>' +
            '<td><b>File ID</b>: </td>' +
            '<td>' + d.Id + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>S3 Location</b>:</td>' +
            '<td>' + d.S3Key + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>Version ID</b>: </td>' +
            '<td>' + d.VersionId + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>ConfigFileDesc</b>: </td>' +
            '<td>' + d.ConfigFileDesc + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>FlowExecutionGuid</b>: </td>' +
            '<td>' + d.FlowExecutionGuid + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>RunInstanceGuid</b>: </td>' +
            '<td>' + d.RunInstanceGuid + '</td>' +
            '</tr>' +
            '</table>';

        return table;
    },

    DatasetBundingFileTableInit: function (Id) {
        $("#bundledDatasetFilesTable").DataTable({
            width: "100%",
            serverSide: true,
            //responsive: true,
            processing: true,
            searching: true,
            paging: true,
            rowId: 'Id',
            iDisplayLength: 10,
            aLengthMenu: [
                [10, 25, 50, 100, 200, -1],
                [10, 25, 50, 100, 200, "All"]
            ],
            ajax: {
                url: "/Dataset/GetBundledFileInfoForGrid/?Id=" + Id,
                type: "POST"
            },
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px", searchable: false },
                { data: "ActionLinks", className: "downloadFile", width: "100px", searchable: false, orderable: false },
                { data: "Name", width: "40%", className: "Name" },
                { data: "UploadUserName", className: "UploadUserName" },
                { data: "CreateDTM", className: "createdtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ModifiedDTM", type: "date", className: "modifieddtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ConfigFileName", className: "ConfigFileName" }
            ],
            language: {
                search: "<div class='input-group'><span class='input-group-addon'><em class='icon-search'></em></span>_INPUT_</div>",
                searchPlaceholder: "Search",
                processing: ""
            },
            order: [5, 'desc'],
            stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
            "createdRow": function (row, data, dataIndex) { }
        });

        var values = [true, false];

        /* TODO REPLACE WITH YADCF

$("#bundledDatasetFilesTable").dataTable().columnFilter({
    sPlaceHolder: "head:after",
    aoColumns: [
        null,
        null,
        { type: "text" },
        { type: "text" },
        { type: "date-range" },
        { type: "date-range" },
        { type: "text" }
    ]
});
*/

        $('#bundledDatasetFilesTable').on('draw.dt', function () {
            if ($("#bundledDatasetFilesTable").DataTable().page.info().recordsTotal !== 0) {
                $("#detailSectionHeader_BundledFiles").show();
                $("#hide_detailSectionHeader_BundledFiles").show();
            }
        });

        var table = $('#bundledDatasetFilesTable').DataTable();

        $('#bundledDatasetFilesTable tbody').on('click', 'td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = table.row(tr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                row.child(data.Dataset.formatDatasetBundlingFileDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

    },

    formatDatasetBundlingFileDetails: function (d) {
        // `d` is the original data object for the row
        var table = '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">';
        if (d.Information !== null) {
            table +=
                '<tr>' +
                '<td><b>Information</b>: </td>' +
                '<td>' + d.Information + '</td>' +
                '</tr>';
        }
        table +=
            '<tr>' +
            '<td><b>File ID</b>: </td>' +
            '<td>' + d.Id + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>S3 Location</b>:</td>' +
            '<td>' + d.S3Key + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>Version ID</b>: </td>' +
            '<td>' + d.VersionId + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>ConfigFileDesc</b>: </td>' +
            '<td>' + d.ConfigFileDesc + '</td>' +
            '</tr>' +
            '</table>';

        return table;
    },

    formatDatasetFileConfigDetails: function (d) {
        // `d` is the original data object for the row
        return '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">' +
            '<tr>' +
            '<td><b>Description</b>:</td>' +
            '<td>' + d.ConfigFileDesc + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>Drop Path</b>: </td>' +
            '<td>' + d.DropPath + '</td>' +
            '</tr>' +
            '</table>';
    },

    PushToBundler: function (dataSetID, listOfFilesToBundle) {

        function DoWork(dataSetID, listOfFilesToBundle, newName) {
            $.ajax({
                url: "/Dataset/BundleFiles/?listOfIds=" + encodeURI(listOfFilesToBundle) + "&newName=" + encodeURI(newName) + "&datasetID=" + encodeURI(dataSetID),
                method: "POST",
                success: function (obj) {
                    Sentry.ShowModalCustom("Upload:", obj.Message, { Confirm: { label: "Confirm", class: "btn-success" } });
                }
            });
        }

        Sentry.ShowModalCustom(
            "Upload Results:",
            "<div> Please supply the new name of your bundled file: (Please Do NOT include the file extension)</div><hr/><div><input id='inputNewName' placeholder='New Name: '/></div>",
            Sentry.ModalButtonsOKCancel(function (result) { DoWork(dataSetID, listOfFilesToBundle, $('#inputNewName').val()); })
        );

        $('.btn-primary').prop("disabled", true);
        $('#inputNewName').keyup(function (e) {
            e.preventDefault();
            if ($('#inputNewName').val() !== "" && $('#inputNewName').val() !== undefined && $('#inputNewName').val() !== null) {
                $('.btn-primary').prop("disabled", false);
            }
            else {
                $('.btn-primary').prop("disabled", true);
            }
        });
    },

    GetDatasetFileVersions: function (id) {
        var modal = Sentry.ShowModalWithSpinner("Versions");

        $.get("/Dataset/GetDatasetFileVersions/" + id, function (result) {
            modal.ReplaceModalBody(result);
            data.Dataset.VersionsModalInit(id);
        });
    },

    VersionsModalInit: function (Id) {
        $('.modal-dialog').css('width', '900px');

        $("#datasetFilesVersionsTable").DataTable({
            autoWidth: true,
            serverSide: true,
            processing: true,
            searching: false,
            paging: true,
            ajax: {
                url: "/Dataset/GetVersionsOfDatasetFileForGrid/?Id=" + Id,
                type: "POST"
            },
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px" },
                { data: "ActionLinks", className: "downloadFile", width: "auto" },
                { data: "Name", width: "40%", className: "Name" },
                { data: "ConfigFileName", className: "configFileName" },
                { data: "UploadUserName", className: "UploadUserName" },
                { data: "CreateDTM", className: "createdtm", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ModifiedDTM", type: "date", className: "modifieddtm", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } }
            ],
            order: [6, 'desc'],
            "createdRow": function (row, data, dataIndex) { }
        });

        // DataTable
        var table = $('#datasetFilesVersionsTable').DataTable();

        $('#datasetFilesVersionsTable tbody').on('click', 'td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = table.row(tr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                row.child(data.Dataset.formatDatasetFileVersionDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

        // Apply the filter
        table.columns().every(function () {
            var column = this;

            $('input', this.footer()).on('keyup change', function () {
                column
                    .search(this.value)
                    .draw();
            });
        });

        $("#userTable_wrapper .dt-toolbar").html($("#userToolbar"));
    },

    formatDatasetFileVersionDetails: function (d) {
        // `d` is the original data object for the row
        var table = '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">';

        if (d.Information !== null) {
            table +=
                '<tr>' +
                '<td><b>Information</b>: </td>' +
                '<td>' + d.Information + '</td>' +
                '</tr>';
        }

        table +=
            '<tr>' +
            '<td><b>File ID</b>: </td>' +
            '<td>' + d.Id + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>S3 Location</b>:</td>' +
            '<td>' + d.S3Key + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>Version ID</b>: </td>' +
            '<td>' + d.VersionId + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>ConfigFileDesc</b>: </td>' +
            '<td>' + d.ConfigFileDesc + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>FlowExecutionGuid</b>: </td>' +
            '<td>' + d.FlowExecutionGuid + '</td>' +
            '</tr>' +
            '<tr>' +
            '<td><b>RunInstanceGuid</b>: </td>' +
            '<td>' + d.RunInstanceGuid + '</td>' +
            '</tr>' +
            '</table>';

        return table;
    },

    SetReturntoSearchUrl: function () {
        var returnUrl = "/Search/Datasets";
        var returnLink = $('#linkReturnToDatasetList');
        var firstParam = true;

        //---is this neede?
        if (localStorage.getItem("searchText") !== null) {
            var text = { searchPhrase: localStorage.getItem("searchText") };

            if (firstParam) { returnUrl += "?"; firstParam = false; } else { returnUrl += "&"; }

            returnUrl += $.param(text);

        }

        if (localStorage.getItem("filteredIds") !== null) {
            storedNames = JSON.parse(localStorage.getItem("filteredIds"));

            if (firstParam) { returnUrl += "?"; firstParam = false; } else { returnUrl += "&"; }

            returnUrl += "ids=";

            for (i = 0; i < storedNames.length; i++) {
                returnUrl += storedNames[i] + ',';
            }
            returnUrl = returnUrl.replace(/,\s*$/, "");
        }

        if (localStorage.getItem("pageSelection") !== null) {

            if (firstParam) { returnUrl += "?"; firstParam = false; } else { returnUrl += "&"; }

            returnUrl += "page=" + localStorage.getItem("pageSelection");
        }

        if (localStorage.getItem("sortByVal") !== null) {
            if (firstParam) { returnUrl += "?"; firstParam = false; } else { returnUrl += "&"; }

            returnUrl += "sort=" + localStorage.getItem("sortByVal");
        }

        if (localStorage.getItem("itemsToShow") !== null) {
            if (firstParam) { returnUrl += "?"; firstParam = false; } else { returnUrl += "&"; }

            returnUrl += "itemsToShow=" + localStorage.getItem("itemsToShow");
        }

        returnLink.attr('href', returnUrl);
    },

    initNamedEnvironmentEvents() {
        //When the NamedEnvironment drop down changes (but only when it's rendered as a drop-down), reload the name environment type
        $("div#DatasetFormContent select#NamedEnvironment").change(function () {
            Sentry.InjectSpinner($("div#DatasetFormContent #namedEnvironmentTypeSpinner"), 30);
            data.Dataset.populateNamedEnvironments();
        });
    },

    populateNamedEnvironments() {
        var assetKeyCode = $("div#DatasetFormContent #saidAsset").val();
        var selectedEnvironment = $("div#DatasetFormContent #NamedEnvironment").val();
        $.get("/Dataset/NamedEnvironment?assetKeyCode=" + assetKeyCode + "&namedEnvironment=" + selectedEnvironment, function (result) {
            $('div#DatasetFormContent #NamedEnvironmentPartial').html(result);
            data.Dataset.initNamedEnvironmentEvents();
            $("#NamedEnvironmentPartial select").materialSelect();
        });
    },

    InitSchemaSearchTab() {
        var datasetId = $('#RequestAccessButton').attr("data-id");

        $("#schemaSearchTable").DataTable({
            "ajax": {
                "url": "/Dataset/Detail/" + datasetId + "/SchemaSearch/" + self.vm.SchemaId,
                "type": "POST",
                "dataSrc": "",
                "data": function (d) {
                    var searchObject = {};
                    searchObject.search = $("#schemaSearchInput").val();
                    return $.extend(d, searchObject)
                }
            },
            "processing": true,
            "language": {
                "processing": "Searching..."
            },
            "sort": false,
            "columns": [
                { "data": "Name" },
                { "data": "Description" },
                { "data": "DotNamePath" }
            ],
            "dom": 'lrt<"dataset-detail-datatable-information"i><"dataset-detail-datatable-pagination"p>'
        });

        //search button on change query elastic
        $("#schemaSearchInput").change(function () {
            data.Dataset.UpdateSchemaSearchTab();
        });
    },

    UpdateSchemaSearchTab() {
        var schemaSearchTable = $("#schemaSearchTable").DataTable();
        var datasetId = $('#RequestAccessButton').attr("data-id");
        schemaSearchTable.ajax.url("/Dataset/Detail/" + datasetId + "/SchemaSearch/" + self.vm.SchemaId).load();
    },

    tryUpdateSchemaSearchTab() {
        if ($('#schemaSearchTable_wrapper').length) {
            data.Dataset.UpdateSchemaSearchTab();
        }
    },

    managePermissionsInit() {

        $("#Inheritance_SelectedApprover").materialSelect();
        $("#inheritanceSwitch label").click(function () {
            $("#inheritanceModal").modal('show');
        });
        data.Dataset.permissionInheritanceSwitchInit();
        //Event to refresh inheritance switch on modal close
        $("#inheritanceModal").on('hide.bs.modal', function () {
            $.ajax({
                type: "GET",
                url: '/Dataset/Detail/' + $("#DatasetHeader").attr("value") + '/Permissions/GetLatestInheritanceTicket',
                success: function (result) {
                    $("#inheritanceSwitch").attr("value", result.TicketStatus);
                    data.Dataset.permissionInheritanceSwitchInit(result);
                }
            });
        });
        $("#inheritanceModalSubmit").click(function () {
            if (data.Dataset.validateInheritanceModal()) {
                $.ajax({
                    type: 'POST',
                    data: $("#InheritanceRequestForm").serialize(),
                    url: '/Dataset/SubmitInheritanceRequest',
                    success: function (data) {
                        //handle result data
                        $("#inheritanceModal").modal('hide');
                    }
                });
            }
            else {
                $("#inheritanceValidationMessage").removeClass("d-none");
            }
        });
        $("#RequestAccessButton").off('click').on('click', function (e) {
            e.preventDefault();
            data.AccessRequest.InitForDataset($(this).data("id"));
        });
    },

    permissionInheritanceSwitchInit(result) {
        var inheritance = $("#inheritanceSwitch").attr("value");
        switch (inheritance) {
            case "ACTIVE":
                $('#inheritanceSwitchInput').prop('checked', true);
                $("#addRemoveInheritanceMessage").text("Request Remove Inheritance");
                $("#Inheritance_IsAddingPermission").val(false);
                break;
            case "PENDING":
                $("#inheritanceSwitch").html('<p>Inheritance change pending. See ticket ' + result.TicketId + '.</p>');
                break
            default: //we treat default the same as "DISABLED"
                $("#addRemoveInheritanceMessage").text("Request Add Inheritance");
                $('#inheritanceSwitchInput').prop('checked', false);
                $("#Inheritance_IsAddingPermission").val(true);
        }
    },

    validateInheritanceModal() {
        return ($("#Inheritance_BusinessReason").val() != '' && $("#Inheritance_SelectedApprover").val != '')
    },

    initRequestAccessWorkflow() {
        data.Dataset.addRequestAccessBreadcrumb("Access To", "#RequestAccessToSection")
        $("#RequestAccessToDatasetBtn").click(function (e) {
            var datasetName = $("#RequestAccessDatasetName").text();
            $("#RequestAccess_Scope").val('0')
            data.Dataset.editActiveRequestAccessBreadcrumb(datasetName);
            $("#RequestAccessConsumeEntitlement").text("Placeholder Entitlement");
            data.Dataset.onAccessToSelection(e);
        });
        $("#RequestAccessToAssetBtn").click(function (e) {
            $("#RequestAccess_Scope").val('1')
            data.Dataset.editActiveRequestAccessBreadcrumb(event.target.value);
            $("#RequestAccessConsumeEntitlement").text("Placeholder Entitlement");
            data.Dataset.onAccessToSelection(e);
        });
        $("#RequestAccessTypeConsumeBtn").click(function (e) {
            data.Dataset.editActiveRequestAccessBreadcrumb("Consume");
            data.Dataset.requestAccessCleanActiveBreadcrumb();
            data.Dataset.addRequestAccessBreadcrumb("Consume Type", "#RequestAccessConsumerTypeSection");
            $("#RequestAccessTypeSection").addClass("d-none");
            $("#RequestAccessConsumerTypeSection").removeClass("d-none");
        });
        $("#RequestAccessTypeManageBtn").click(function (e) {
            data.Dataset.editActiveRequestAccessBreadcrumb("Manage");
            data.Dataset.requestAccessCleanActiveBreadcrumb();
            data.Dataset.addRequestAccessBreadcrumb("Manage Request", "#RequestAccessManageTypeSection");
            $("#RequestAccessManageEntitlement").text("Placeholder Entitlement");
            $("#RequestAccessTypeSection").addClass("d-none");
            $("#RequestAccessManageTypeSection").removeClass("d-none");
        });
        $("#RequestAccessConsumeSnowflakeBtn").click(function (e) {
            data.Dataset.editActiveRequestAccessBreadcrumb("Snowflake Account");
            data.Dataset.requestAccessCleanActiveBreadcrumb();
            data.Dataset.addRequestAccessBreadcrumb("Create Request", "#RequestAccessFormSection");
            $("#RequestAccessConsumerTypeSection").addClass("d-none");
            $("#RequestAccessFormSection").removeClass("d-none");
            data.Dataset.setupFormSnowflakeAccount();
        });
        $("#RequestAccessConsumeAwsBtn").click(function (e) {
            data.Dataset.editActiveRequestAccessBreadcrumb("AWS IAM");
            data.Dataset.requestAccessCleanActiveBreadcrumb();
            data.Dataset.addRequestAccessBreadcrumb("Create Request", "#RequestAccessFormSection");
            $("#RequestAccessConsumerTypeSection").addClass("d-none");
            $("#RequestAccessFormSection").removeClass("d-none");
            data.Dataset.setupFormAwsIam();
        });
        $("#RequestAccess_SelectedApprover").materialSelect();
        $("#RequestAccessSubmit").click(function () {
            if (data.Dataset.validateRequestAccessModal()) {
                $("#RequestAccessLoading").removeClass('d-none');
                $("#RequestAccessBody").addClass('d-none');
                $.ajax({
                    type: 'POST',
                    data: $("#AccessRequestForm").serialize(),
                    url: '/Dataset/SubmitAccessRequestCLA3723',
                    success: function (data) {
                        $("#RequestAccessLoading").addClass('d-none');
                        $("#RequestAccessBody").removeClass('d-none');
                        $("#RequestAccessBody").html(data);
                    }
                });
            }
            else {
                $("#AccessRequestValidationMessage").removeClass("d-none");
            }
        });
    },

    validateRequestAccessModal() {
        var valid;
        valid = $("#RequestAccess_BusinessReason").val() != '' && $("#RequestAccess_SelectedApprover").val != ''
        if ($("#RequestAccess_Type").val() == "1") {
            valid = valid && data.Dataset.requestAccessValidateAwsArnIam();
        }
        return valid;
    },

    onAccessToSelection(event) {
        data.Dataset.requestAccessCleanActiveBreadcrumb();
        data.Dataset.addRequestAccessBreadcrumb("Access Type", "#RequestAccessTypeSection");
        $("#RequestAccessToSection").addClass("d-none");
        $("#RequestAccessTypeSection").removeClass("d-none");
    },

    buildBreadcrumbReturnToStepHandler(element) {
        element.click(function () {
            var jumpBackTo = element.attr("value");
            $('#AccessRequestForm div.requestAccessStage:not(.d-none)').addClass('d-none');
            $(jumpBackTo).removeClass('d-none');
            element.nextAll().remove();
            element.addClass('active');
            if (jumpBackTo != "#RequestAccessFormSection") {
                data.Dataset.requestAccessHideSaveChanges();
            }
        });
    },

    requestAccessCleanActiveBreadcrumb() {
        $("#RequestAccessBreadcrumb li").removeClass("active");
    },

    addRequestAccessBreadcrumb(breadCrumbText, createdFrom) {
        $("#RequestAccessBreadcrumb").append('<li class="breadcrumb-item active" value="' + createdFrom + '"><a href="#">' + breadCrumbText + '</a></li>');
        data.Dataset.buildBreadcrumbReturnToStepHandler(data.Dataset.requestAccessGetActiveBreadcrumb());
    },

    editActiveRequestAccessBreadcrumb(breadCrumbText) {
        $("#RequestAccessBreadcrumb li.active a").text(breadCrumbText);
    },

    requestAccessGetActiveBreadcrumb() {
        return $("#RequestAccessBreadcrumb li.active");
    },

    setupFormAwsIam() {
        $("#AwsArnForm").removeClass("d-none");
        $("#RequestAccess_Type").val("1")
        data.Dataset.requestAccessShowSaveChanges();
    },

    setupFormSnowflakeAccount() {
        //$("#AccessGranteeForm").removeClass("d-none");
        //$("#SecurableObjectName").text("Snowflake Account Name");
        //data.Dataset.requestAccessShowSaveChanges();
    },

    requestAccessShowSaveChanges() {
        $("#RequestAccessSubmit").removeClass("d-none");
    },

    requestAccessHideSaveChanges() {
        $("#RequestAccessSubmit").addClass("d-none");
    },

    requestAccessValidateAwsArnIam() {
        var pattern = /^arn:aws:iam::\d{12}:role\/+./;
        var valid = pattern.test($("#RequestAccess_AwsArn").val());
        if (valid) {
            $("#AccessRequestAwsArnValidationMessage").addClass("d-none");
            return true;
        }
        else {
            $("#AccessRequestAwsArnValidationMessage").removeClass("d-none");
            return false;
        }
    },

    showDataPreviewError() {
        $("#DataPreviewNoRows").html("<p> No rows returned </p>");
        $("#DataPreviewNoRows").removeClass("d-none");
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