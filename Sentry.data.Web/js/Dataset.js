﻿// ******************************************************************************************
// * Javascript methods for the Asset-related pages
// ******************************************************************************************

data.Dataset = {

    DatasetFilesTable: {},

    delroyBreadCrumbs: [],
    delroyIndex: -1,



    delroyInit: function () {

        var schemaURL = "/api/v2/metadata/dataset/230/schema/4039/revision/latest/fields";
        $.get(schemaURL, function (result)
        {
            data.Dataset.delroyAddBreadCrumb(result.Fields);
            data.Dataset.delroyTableCreate();
            data.Dataset.delroySetupClickAttack();

        }).fail(function () {
            alert('fail');
        });


       
    },

    delroyTableCreate: function () {

        //init DataTable,
        $("#delroyTable").DataTable({

            //client side setup
            pageLength: 100,

            //ajax: {
            //    url: "/api/v2/metadata/dataset/230/schema/4039/revision/latest/fields",
            //    type: "GET",
            //    dataSrc: "Fields"       //since the obj coming back from URL call has multiple properties inside it, need to sepcify WHERE array of columns exists to load
            //},

            data: data.Dataset.delroyGetLatestBreadCrumb(),     //NOTE: on initial load we are grabbing root element which has column array immediately inside, no need to drill down to Fields property

            columns: [
                { data: "Name", className: "Name" },
                { data: "Description", className: "Description" },
                { data: "FieldType", className: "FieldType" },
                { data: "Precision", className: "Precision", visible: false },
                { data: "Scale", className: "Scale", visible: false },
            ],

            aLengthMenu: [
                [20, 100, 500],
                [20, 100, 500]
            ],

            order: [0, 'desc'],

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
        $("#delroyTable").dataTable().columnFilter({
            sPlaceHolder: "head:after",
            aoColumns: [
                { type: "text" },
                { type: "text" },
                { type: "text" },

                { type: "text" },
                { type: "text" },
            ]
        });

    },

    //GRID CLICK EVENTS
    delroySetupClickAttack: function () {

        //DELROY GRID CLICK
        $('#delroyTable tbody').on('click', 'tr', function () {                         //anything clicked in delroyTable tbody, filter down to capture 'tr' class clicks only

            var table = $('#delroyTable').DataTable();
            var field = table.row(this).data();

            // click reload the grid if children exist
            if (field.Fields != null) {
                data.Dataset.delroyAddBreadCrumb(field);
                data.Dataset.delroyRefreshLatest();
            }
        });


        //BREADCRUMBS NAV CLICK EVENT
        $("#delroyBreadcrumb").on('click','li' ,function (d) {
            var myId = this.id;
            data.Dataset.delroyCleanBreadCrumbs(myId);
            data.Dataset.delroyRefreshLatest();
        });
    },

    //add new breadcrumb to list and also too global array
    delroyAddBreadCrumb: function (field) {

        data.Dataset.delroyIndex = data.Dataset.delroyIndex + 1;
        var bcName = "Home";
        if (data.Dataset.delroyIndex > 0) {
            bcName = field.Name;
        }

        var h = "<li id='" + data.Dataset.delroyIndex.toString() + "' ><a href='#'>" + bcName + "</a></li>"

        $('#delroyBreadcrumb').append(h);
        data.Dataset.delroyBreadCrumbs.push(field);
    },

    //get latest breadcrumb 
    delroyGetLatestBreadCrumb: function () {

        var index = data.Dataset.delroyBreadCrumbs.length - 1;
        var crumbRaw = data.Dataset.delroyBreadCrumbs[index];
        var crumbCooked;

        //NOTE: this is a strange concept:  but a crumb is either a ROOT level one (index 0) or a CHILD level (index > 0)
        //All levels past the ROOT (index 0) you need too specify the Fields array which is a child property 
        //whereas the root level(index 0) the array is at that level

        //if ROOT level
        if (index == 0) {
            crumbCooked = crumbRaw;     
        }
        else {
            crumbCooked = crumbRaw.Fields;
        }

        return crumbCooked;
    },


    //clear and reload grid with latest breadcrumb
    delroyRefreshLatest: function () {
        var table = $('#delroyTable').DataTable();
        var crumb = data.Dataset.delroyGetLatestBreadCrumb();

        table.clear();
        table.rows.add(crumb);       //when clicking on an item, we need to specify Fields property since all chidren of ROOT use this
        table.draw();
    },

    //create a function too clean up breadcrumbs beyond one passed in
    //then need to refresh actualy breadcrumb UI
    //then reload and latest func will work because thats the latest
    delroyCleanBreadCrumbs: function (lastIndexKeep) {

        //STEP 1: refresh array to reflect current breadcrumb selected
        var deleteStartIndex = parseInt(lastIndexKeep, 10) + 1;
        data.Dataset.delroyBreadCrumbs.splice(deleteStartIndex);

        //STEP 2: refresh UI breadcrumb to reflect current breadcrumb selected
        $('#delroyBreadcrumb').empty();
        for (let i = 0; i < data.Dataset.delroyBreadCrumbs.length; i++) {

            var field = data.Dataset.delroyBreadCrumbs[i];
            var bcName = "Home";
            if (i > 0) {
                bcName = field.Name;
            }
            var h = "<li id='" + i.toString() + "' ><a href='#'>" + bcName + "</a></li>"
            $('#delroyBreadcrumb').append(h);
        }

        //STEP 3:  refresh global property delroyIndex to reflect newly selected index
        data.Dataset.delroyIndex = lastIndexKeep;
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

        $('.input-group-addon').click(function (e) {
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
                    $('#DatasetFormContent').replaceWith(obj);
                }
            },
            failure: function () {
                alert('An error occured submiting your request.  Please try again.');
            },
            error: function (obj) {
                $('#DatasetFormContent').replaceWith(obj.responseText);
                var hrEnv = $('#HrempServiceEnv').val()
                var hrUrl = $('#HrempServiceUrl').val()
                data.Dataset.FormInit(hrUrl, hrEnv, data.Dataset.FormSubmitInit);
            }
        });
    },

    FormCancelInit: function (e) {
        e.preventDefault();
        window.location = data.Dataset.CancelLink($(this).data("id"));
    },

    FormInit: function (hrEmpUrl, hrEmpEnv, PageSubmitFunction, PageCancelFunction) {
        /// Initialize the Create Dataset view
        
        //SubmitDatasetForm
        $("[id='SubmitDatasetForm']").click(PageSubmitFunction);

        if ($("#DatasetId").val() !== undefined && $("#DatasetId").val() > 0) {
            $("#DatasetScopeTypeId").attr('readonly', 'readonly');
            //$("#FileExtensionId").attr('readonly', 'readonly');
            //$("#Delimiter").attr('readonly', 'readonly');
        }

        //Set Secure HREmp service URL for associate picker
        $.assocSetup({ url: hrEmpUrl });
        var permissionFilter = "DatasetModify,DatasetManagement," + hrEmpEnv;
        $("#PrimaryOwnerName").assocAutocomplete({
            associateSelected: function (associate) {
                $('#PrimaryOwnerId').val(associate.Id);
            },
            filterPermission: permissionFilter ,
            minLength: 0,
            maxResults:10
        });
        $("#PrimaryContactName").assocAutocomplete({
            associateSelected: function (associate) {
                $('#PrimaryContactId').val(associate.Id);
            },
            filterPermission: permissionFilter,
            minLength: 0,
            maxResults: 10
        });


        $("#DataClassification").change(function () {
            switch ($("#DataClassification").val()) {
                case "1":
                    $('#dataClassInfo').text('“Restricted” information is proprietary and has significant business value for Sentry. ' +
                        'Unauthorized disclosure or dissemination could result in severe damage to Sentry.  Examples of restricted data include secret contracts or trade secrets.  ' +
                        'This information must be limited to only the few associates that require access to it.  If it is shared, accessed, or altered without the permission ' +
                        'of the Information Owner, Information Security must be notified immediately.  Designating information as Restricted involves significant ' +
                        'costs to Sentry.  For this reason, Information Owners making classification decisions must balance the damage that could result from ' +
                        'unauthorized access to or disclosure of the information against the cost of additional hardware, software or services required to protect it.');
                    $("#IsSecured").parents('.form-group').hide();
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
                    $("#IsSecured").parents('.form-group').hide();
                    break;
                case "3":
                    $('#dataClassInfo').text('“Internal Use Only” information can be disclosed or disseminated to Sentry ' +
                        'associates, but will only be shared with other individuals or organizations when a ' +
                        'non - disclosure agreement is in place and management has approved for legitimate ' +
                        'business reasons.  Examples include items such as email correspondence, internal ' +
                        'documentation that is available to all associates.');
                    $("#IsSecured").parents('.form-group').show();
                    break;
                case "4":
                    $('#dataClassInfo').text('“Public” information can be disclosed or disseminated without any restrictions on ' +
                        'content, audience, or time of publication.  Examples are datasets that were generated by the Federal or State Governments like the Federal Motor Carrier Safety Administration or NOAA Weather Data.  ' +
                        'These datasets can be freely shared throughout Sentry.');
                    $("#IsSecured").parents('.form-group').hide();
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

                //console.log('File Upload Change Loop');
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

        //$('#Delimiter').prop("readonly", "readonly");
        //$('#HasHeader').prop("readonly", false);
        //$('#HasHeader').prop("disabled", false);

        //$("#FileExtensionId").change(function () {
        //    switch ($('#FileExtensionId option:selected').text()) {
        //        case "CSV":
        //            $('#Delimiter').text(',');
        //            $('#Delimiter').val(',');
        //            $('#Delimiter').prop("readonly", "readonly");
        //            $('#HasHeader').prop("readonly", false);
        //            $('#HasHeader').prop("disabled", false);
        //            break;
        //        case "DELIMITED":
        //            $('#Delimiter').val('');
        //            $('#Delimiter').prop("readonly", "");
        //            $('#HasHeader').prop("readonly", false);
        //            $('#HasHeader').prop("disabled", false);
        //            break;
        //        default:
        //            $('#Delimiter').val('');
        //            $('#Delimiter').prop("readonly", "readonly");
        //            $('#HasHeader').prop("readonly", true);
        //            $('#HasHeader').prop("disabled", true);
        //            break;                 
        //    }
        //});

    },

    DetailInit: function () {

        data.Dataset.delroyInit();

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

        $("[id^='PreviewLatestData']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PreviewLatestDatafileModal($(this).data("id"));
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
            $(icon).toggleClass("glyphicon-chevron-down glyphicon-chevron-up");
        });

        var Id = $('#datasetConfigList').val();
        data.Dataset.DatasetFileTableInit(Id);
        data.Dataset.DatasetBundingFileTableInit(Id);
        data.Dataset.DatasetFileConfigsTableInit(Id);

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
                success: function () { icon.toggleClass("glyphicon-star glyphicon-star-empty"); },
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
            

            Sentry.ShowModalCustom("Delete Dataset", ConfirmMessage, {
                Confirm: {
                    label: "Confirm",
                    className: "btn-danger",
                    callback: function () {
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
                    }
                }
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

    PreviewLatestDatafileModal: function (id) {
        var modal = Sentry.ShowModalWithSpinner("Preview Datafile");
        $.get("/Dataset/PreviewLatestDatafile/" + encodeURI(id), function (result) {
            modal.ReplaceModalBody(result);
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

    PreviewInit: function () {
        $("[id^='CopyToClipboard']").off('click').on('click', function (e) {
            e.preventDefault();
            var range;
            if (document.selection) {
                range = document.body.createTextRange();
                range.moveToElementText(document.getElementById("PreviewText"));
                range.select().createTextRange();
                document.execCommand("Copy");
                alert("Text Copied to Clipboard");

            } else if (window.getSelection) {
                range = document.createRange();
                range.selectNode(document.getElementById("PreviewText"));
                window.getSelection().addRange(range);
                document.execCommand("Copy");
                alert("Text Copied to Clipboard");
            }
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
                { data: "CreateDTM", className: "createdtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss a") : null; } },
                { data: "ModifiedDTM", type: "date", className: "modifieddtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss a") : null; } },
                { data: "ConfigFileName", className: "ConfigFileName" }
            ],
            language: {
                search: "<div class='input-group'><span class='input-group-addon'><i class='glyphicon glyphicon-search'></i></span>_INPUT_</div>",
                searchPlaceholder: "Search",
                processing: ""
            },
            order: [5, 'desc'],
            stateSave: true,
            "createdRow": function (row, data, dataIndex) { }
        });

        $("#datasetFilesTable").dataTable().columnFilter({
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
            '<td>' + d.s3Key + '</td>' +
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
                search: "<div class='input-group'><span class='input-group-addon'><i class='glyphicon glyphicon-search'></i></span>_INPUT_</div>",
                searchPlaceholder: "Search",
                processing: ""
            },
            order: [5, 'desc'],
            stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
            "createdRow": function (row, data, dataIndex) { }
        });

        var values = [true, false];

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
            '<td>' + d.s3Key + '</td>' +
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

    DatasetFileConfigsTableInit: function (Id) {
        $('#datasetFileConfigsTable tbody').on('click', 'td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = table.row(tr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                row.child(data.Dataset.formatDatasetFileConfigDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

        $("#datasetFileConfigsTable").DataTable({
            autoWidth: true,
            serverSide: true,
            processing: true,
            searching: false,
            paging: true,
            ajax: {
                url: "/Dataset/GetDatasetFileConfigInfoForGrid/?Id=" + Id,
                type: "POST"
            },
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px" },
                { data: "EditHref", className: "editConfig", width: "20px" },
                { data: "ConfigFileName", className: "configFileName" },
                { data: "SearchCriteria", className: "searchCriteria" },
                { data: "TargetFileName", className: "targetFileName" },
                { data: "IsRegexSearch", className: "isRegexSearch", render: function (data, type, row) { return data === true ? '<span class="glyphicon glyphicon-ok"> </span>' : '<span class="glyphicon glyphicon-remove"></span>'; } },
                { data: "OverwriteDatasetFile", type: "date", className: "overwriteDatsetFile", render: function (data, type, row) { return data === true ? '<span class="glyphicon glyphicon-ok"> </span>' : '<span class="glyphicon glyphicon-remove"></span>'; } },
                { data: "FileType", className: "fileType" },
                { data: "DatasetScopeTypeID", className: "DatasetScopeTypeID" }
            ],
            order: [1, 'asc']
        });
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

        $("#datasetFilesVersionsTable").dataTable().columnFilter({
            sPlaceHolder: "head:after",
            aoColumns: [
                null,
                null,
                null,
                { type: "text" },
                { type: "text" },
                { type: "text" },
                { type: "text" }
            ]
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
            '<td>' + d.s3Key + '</td>' +
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

    PreviewDatafileModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Preview Datafile");

        $.get("/Dataset/PreviewDatafile/" + id, function (result) {
            modal.ReplaceModalBody(result);
        });
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
    }









};