/******************************************************************************************
 * Javascript methods for the Asset-related pages
 ******************************************************************************************/

data.DatasetDetail = {

    DatasetFilesTable: {},

    Init: function () {
        /// <summary>
        /// Initialize the dataset detail page for data assets
        /// </summary>
        $("[id^='EditDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.ViewEdit($(this).data("id"));
        });

        $("[id^='DownloadLatest']").off('click').on('click', function (e) {
            e.preventDefault();
            data.DatasetDetail.DownloadLatestDatasetFile($(this).data("id"));
        });

        $("[id^='PushtoSAS_']").off('click').on('click', function (e) {
            console.log(e);
            e.preventDefault();
            data.Dataset.PushToSAS($(this).data("id"));
        });

        $("[id^='Pushtofilename_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.FileNameModal($(this).data("id"));
        });

        $("[id^='PreviewLatestData']").off('click').on('click', function (e) {
            e.preventDefault();
            data.DatasetDetail.PreviewLatestDatafileModal($(this).data("id"));
        });

        $("#btnCreateDirectory").off('click').on('click', function (e) {
            var request = $.ajax({
                url: "/Dataset/CreateFilePath/?filePath=" + $($(this).parent().parent().children()[1]).text(),
                method: "POST",
                dataType: 'json',
                success: function (obj) {
                }
            });
        });

        $("[id^='SubscribeModal']").click(function (e) {
            e.preventDefault();

            data.DatasetDetail.SubscribeModal($(this).data("id"));
        });

        $("[id^='UploadModal']").click(function (e) {
            e.preventDefault();
            //data.DatasetDetail.ProgressModalStatus();
            data.DatasetDetail.UploadFileModal($(this).data("id"));
        });

        $("[id^='detailSectionHeader_']").click(function (e) {
            e.preventDefault();

            var id = $(this).attr("id");
            var category = "#hide_" + id
            var icon = "#icon_" + id;

            $(category).slideToggle();
            $(icon).toggleClass("glyphicon-chevron-down glyphicon-chevron-up");
        });


        data.DatasetDetail.DatasetFileTableInit(Id);
        data.DatasetDetail.DatasetBundingFileTableInit(Id);

        data.DatasetDetail.DatasetFileConfigsTableInit(Id);

        localStorage.setItem("listOfFilesToBundle", JSON.stringify([]));

        $("#bundle_selected").click(function (e) {
            e.preventDefault();

            var datasetID = window.location.pathname.substr(window.location.pathname.lastIndexOf('/') + 1);
            var listOfFilesToBundle = JSON.parse(localStorage.getItem("listOfFilesToBundle"));

            var configName = $('#' + listOfFilesToBundle[0]).children('td.ConfigFileName').text();
            var multipleConfigs = false;

            for (i = 0; i < listOfFilesToBundle.length; i++) {
                if ($('#' + listOfFilesToBundle[i]).children('td.ConfigFileName').text() != configName) {
                    multipleConfigs = true;
                    break;
                }
            }

            if (multipleConfigs) {

                var modal = Sentry.ShowModalCustom(
                    "Upload Warning:",
                    "There are multiple configuration files associated with the set of files you want to bundle.  Do you want to continue?",
                    Sentry.ModalButtonsOKCancel(function (result) { data.DatasetDetail.PushToBundler(datasetID, listOfFilesToBundle) })
                );
            }
            else {
                data.DatasetDetail.PushToBundler(datasetID, listOfFilesToBundle);
            }
        });

        $("#bundle_allFiltered").click(function (e) {
            e.preventDefault();

            var datasetID = window.location.pathname.substr(window.location.pathname.lastIndexOf('/') + 1);

            var params = Sentry.GetDataTableParamsForExport($('#datasetFilesTable').DataTable());

            var request = $.ajax({
                url: "/Dataset/GetDatasetFileInfoForGrid/?Id=" + datasetID + "&bundle=" + true,
                method: "POST",
                data:  params,
                dataType: 'json',
                success: function (obj) {
                    console.log('success');
                    console.log(obj);

                    var listOfFilesToBundle = [];
                    var configName = obj.data[0].ConfigFileName;
                    var multipleConfigs = false;

                    for (i = 0; i < obj.data.length; i++)
                    {
                        listOfFilesToBundle.push(obj.data[i].Id);

                        if (obj.data[i].ConfigFileName != configName)
                        {
                            multipleConfigs = true;
                        }
                    }
                    console.log(listOfFilesToBundle);

                    if (multipleConfigs) {

                        var modal = Sentry.ShowModalCustom(
                            "Upload Warning:",
                            "There are multiple configuration files associated with the set of files you want to bundle.  Do you want to continue?",
                            Sentry.ModalButtonsOKCancel(function (result) { data.DatasetDetail.PushToBundler(datasetID, listOfFilesToBundle) })
                        );
                    }
                    else {
                        data.DatasetDetail.PushToBundler(datasetID, listOfFilesToBundle);
                    }
                },
                failure: function (obj) {
                    console.log('failed');
                    console.log(obj);
                },
                error: function (obj)
                {
                    console.log('error');
                    console.log(obj);
                }
            });        
        });
    },

    PushToBundler: function (dataSetID, listOfFilesToBundle) {
        for (i = 0; i < listOfFilesToBundle.length; i++) {
            console.log("Bundle " + i + " : " + listOfFilesToBundle[i]);
        }

        function DoWork(dataSetID, listOfFilesToBundle, newName) {
            var request = $.ajax({
                url: "/Dataset/BundleFiles/?listOfIds=" + encodeURI(listOfFilesToBundle) + "&newName=" + encodeURI(newName) + "&datasetID=" + encodeURI(dataSetID),
                method: "POST",
                success: function (obj) {
                    var modal = Sentry.ShowModalCustom(
                        "Upload:",
                        obj.Message, {
                            Confirm: {
                                label: "Confirm",
                                class: "btn-success"
                            }
                        }
                    );
                },
                failure: function (obj) {
                    console.log(obj);
                },
                error: function (xhr, e) {
                    console.log(e);
                    console.log(xhr);
                }
            });        
        }

        var modal = Sentry.ShowModalCustom(
            "Upload Results:",
            "<div> Please supply the new name of your bundled file: (Please Do NOT include the file extension)</div><div><input id='inputNewName' placeholder='New Name: '/></div>",
            Sentry.ModalButtonsOKCancel(function (result) { DoWork(dataSetID, listOfFilesToBundle, $('#inputNewName').val()) })
        );

        $('.btn-primary').prop("disabled", true);

        $('#inputNewName').keyup(function (e) {
            e.preventDefault();

            console.log($('#inputNewName').val());
            if ($('#inputNewName').val() != "" && $('#inputNewName').val() != undefined && $('#inputNewName').val() != null)
            {
                $('.btn-primary').prop("disabled", false);
            }
            else {
                $('.btn-primary').prop("disabled", true);
            }

        });
        
    },

    PushToBundleInit: function () {
        ///<summary>
        ///Initialize the PushToFilenameOverride partial view
        ///</summary>
        // $("PushToForm").validateBootstrap(true);

        $("[id^='BundleFilename']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PushToSAS_Filename($(this).data("id"), $("#BundleFilename").val());
        });
    },

    VersionsModalInit: function (Id) {
        $('.modal-dialog').css('width','900px');
        //m.addClass('datasetversionmodal');

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
                row.child(data.DatasetDetail.formatDatasetFileVersionDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

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
                        //{ data: "PreviewHref", className: "previewFile", width: "20px" },
                        //{ data: "Id", width: "40px", type: "num", className: "datasetfileid" },
                        { data: "Name", width: "40%", className: "Name" },
                        { data: "ConfigFileName", className: "configFileName" },
                        { data: "UploadUserName", className: "UploadUserName" },
                        { data: "CreateDTM", className: "createdtm", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null;}},
                        { data: "ModifiedDTM", type: "date", className: "modifieddtm", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null;}}
            ],
            order: [6, 'desc'],
            //stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
            "createdRow": function (row, data, dataIndex) {
                if (data["IsUsable"] == false)
                {
                    $(row).css('background-color', '#ffd1d1');
                }
            }
            
        });

        $("#datasetFilesVersionsTable").dataTable().columnFilter({
            sPlaceHolder: "head:after",
            aoColumns: [
                    null,
                    null,
                    null,
                    //{ type: "number-range" },
                    { type: "text" },
                    { type: "text" },
                    { type: "text" },
                    { type: "text" }
            ]
        });

        var DataFilesTable = $('#datasetFilesVersionsTable').dataTable();

        // DataTable
        var table = $('#datasetFilesVersionsTable').DataTable();

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

        $("#exportToExcel").click(function () {
            alert("exportToExcel Function");
        });
    },

    UploadModalInit: function (id) {
        // Initialize SignalR Hub
        //data.DatasetDetail.ProgressModalStatus();

        var configs;

        $("#categoryList").prepend($("<option />").val(0).html("-- Select a Category --"));

        $("#categoryList").each(function (i) {
            $(this).val(i);
        })

        if (id == 0 || id == 1) {

            //Hide Upload Button
            $("[id^='btnUploadFile']").attr('data-id', id);
            $("[id^='btnUploadFile']").prop("disabled", true);

            //Hide Create Button
            $("#btnCreateDatasetAtUpload").prop("disabled", true);
            $("#btnCreateDatasetAtUpload").hide();

            //Hide File Upload
            $("#DatasetFileUpload").prop("disabled", true);
            $("#DatasetFileUpload").parent().parent().hide();

            //Hide Dataset List
            $("#datasetList").parent().parent().hide();

            //Hide Configuration List
            $('#configList').parent().parent().hide();
            $("#configDescription").hide();
        }
        else {
            //Hide Create Button
            $("#btnCreateDatasetAtUpload").prop("disabled", true);
            $("#btnCreateDatasetAtUpload").hide();

            //Hide Upload Button
            $("[id^='btnUploadFile']").attr('data-id', id);
            $("[id^='btnUploadFile']").prop("disabled", true);


            $('#configList').parent().parent().show();
            $("#configDescription").show();

            if (id > 0) {
                var controllerURL = "/Dataset/GetDatasetFileConfigInfo/?id=" + encodeURI(id);
                $.get(controllerURL, function (result) {
                    configs = result;
                    var select = $("#configList");

                    select.empty();

                    $.each(result, function (index, itemData) {
                        select.append($('<option/>', {
                            value: itemData.ConfigId,
                            text: itemData.ConfigFileName
                        }));
                    });
                    $("#configDescription").html(configs[0].ConfigFileDesc);
                    
                });
            }
        }

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


            $('.modal-footer btn-success').prop("disabled", true);

            // This approach is from the following site:
            // http://www.c-sharpcorner.com/UploadFile/manas1/upload-files-through-jquery-ajax-in-Asp-Net-mvc/
            if (window.FormData !== undefined) {
                console.log('File Upload Process Started');
                var fileUpload = $("#DatasetFileUpload").get(0);
                var files = fileUpload.files;

                //Create FormData object
                var fileData = new FormData();

                fileData.append(files[0].name, files[0]);

                if ((files[0].size / 1000000) > 100) {

                    var configID = $("#configList").find(":selected").val();
                    var dropLocation 

                    for (i = 0; i < configs.length; i++) {
                        if (configs[i].ConfigId == configID) {
                            dropLocation = configs[i].DropPath;
                            break;
                        }
                    }


                    modal.ReplaceModalBody('<h3> The file you are attempting to upload to is too large to upload through the browser. </h3>' +
                        '<p>Please use the following location to drop files for this dataset. </p>' +
                        '<br />' + 
                        '<p>' + dropLocation + '</p>' +
                        '<br />' + 
                        '<p>If you don\'t have access to this drop location please contact <a href="mailto:BIPortalAdmin@Sentry.com"> Data.sentry.com Administration </a> for further assistance </p>'
                        );

                    $('.modal-footer button').prop("disabled", false);
                }
                else {
                    var datasetID = window.location.pathname.substr(window.location.pathname.lastIndexOf('/') + 1);

                    if (datasetID == "" || datasetID == null || datasetID == undefined || isNaN(datasetID)) {
                        datasetID = $("[id^='btnUploadFile']").attr('data-id');
                    }

                    var configID = $("#configList").val();

                    var token = $('input[name="__RequestVerificationToken"]').val();

                    var xhr = new XMLHttpRequest();

                    modal.ReplaceModalBody('<p> Large files may take a long time to upload through the browser. </p>' +
                        '<p>Please do not close the window as your file is uploading. </p>' +
                        '<p> Progress: <span id=\'progressKB\'/></p>' +
                        '<h3><b><span id=\'percentTotal\'></span></b ></h3>' +
                        '<div>' +
                            '<div class="progress progress-striped active">' +
                                '<div class="progress-bar" id="progressBar"></div>' +
                            '</div>' +
                        '</div>'
                    );


                    (xhr.upload || xhr).addEventListener('progress', function (e) {
                        var done = e.position || e.loaded
                        var total = e.totalSize || e.total;

                        $('#percentTotal').text(Math.round(done / total * 100) + '%');
                        $('#progressKB').text('(' + Math.round(done / 1024) + ' KB / ' + Math.round(total / 1024) + ' KB)');
                        $('#progressBar').width(Math.round(done / total * 100) + '%');

                        $('.btn-success').prop("disabled", true);
                    });
                    xhr.addEventListener('load', function (e) {
                        $('.modal-footer button').prop("disabled", false);
                        modal.ReplaceModalBody(e.currentTarget.response);
                        console.log(e);
                    });

                    function cancelUpload() {
                        xhr.abort();
                        console.log('The Upload Process was aborted');
                    }

                    $('.btn-cancel')[0].addEventListener('click', cancelUpload, false);
                    $('.bootbox-close-button').hide();
                    var url = '/Dataset/UploadDatafile/?id=' + encodeURI(datasetID) + "&configId=" + encodeURI(configID);
                    xhr.open('post', url, true);
                    xhr.setRequestHeader('__RequestVerificationToken', token);
                    xhr.send(fileData);
                }

            } else {
                alert("FormData is not supported");
            }
        });

        // Dropdown selection drives populating secondary dropdown list
        // https://www.codeproject.com/Questions/696829/MVC-Dropdown-onchange-load-another-dropdown
        $("#categoryList").change(function () {
            var cID = $(this).val();

            if (cID > 0) {
                var controllerURL = "/Dataset/LoadDatasetList/?id=" + encodeURI(cID);
                $.get(controllerURL, function (result) {
                    var select = $("#datasetList");
                    select.empty();
                    select.append($('<option/>', {
                        value: 0,
                        text: "Select Dataset"
                    }));

                    select.append($('<option/>', {
                        value: 1,
                        text: "{Create New Dataset}"
                    }));

                    $.each(result, function (index, itemData) {
                        select.append($('<option/>', {
                            value: itemData.Value,
                            text: itemData.Text
                        }));
                    });
                });
            }

            //Simple Field Validation 
            //  Enables the button if both fields are picked.
            //  If Create New Dataset is Picked, a different button will be shown to the user.

            if (cID == 0) {
                //Hide Upload Button
                $("[id^='btnUploadFile']").prop("disabled", true);

                //Hide Create Button
                $("#btnCreateDatasetAtUpload").prop("disabled", true);
                $("#btnCreateDatasetAtUpload").hide();

                //Hide File Upload
                $("#DatasetFileUpload").prop("disabled", true);
                $("#DatasetFileUpload").parent().parent().hide();

                //Hide Dataset List
                $("#datasetList").parent().parent().hide();

                //Hide Configuration List
                $('#configList').parent().parent().hide();
                $("#configDescription").hide();
            }
            else if ($("#datasetList option:selected").text("Select Dataset"))
            {
                //Hide Upload Button
                $("[id^='btnUploadFile']").prop("disabled", true);

                //Hide Create Button
                $("#btnCreateDatasetAtUpload").prop("disabled", true);
                $("#btnCreateDatasetAtUpload").hide();

                //Hide File Upload
                $("#DatasetFileUpload").prop("disabled", true);
                $("#DatasetFileUpload").parent().parent().hide();

                //Hide Configuration List
                $('#configList').parent().parent().hide();
                $("#configDescription").hide();

                //Show Dataset List
                $("#datasetList").parent().parent().show();
            }
            else {
                //Show Dataset List
                $("#datasetList").parent().parent().show();

                var fileUpload = $("#DatasetFileUpload").get(0);
                var files = fileUpload.files;

                if (files.length > 0) {

                    if (cID != 0 && cID != 1 && files[0].name != null) {
                        $("[id^='btnUploadFile']").prop("disabled", false);
                    } else if (cID == 1) {
                        $("#btnCreateDatasetAtUpload").prop("disabled", false);
                        $("#btnCreateDatasetAtUpload").show();
                    }
                }
            }
        });

        $("#btnCreateDatasetAtUpload").click(function (e) {
            e.preventDefault();
            url = "/Dataset/Create";
            window.location = url;
        });


        $("#DatasetFileUpload").change(function () {
            var dID = $(this).val();

            //Hide Configuration List
            $('#configList').parent().parent().show();
            $("#configDescription").show();

            var fileUpload = $("#DatasetFileUpload").get(0);
            var files = fileUpload.files;

            if (files.length > 0) {
                if (dID != 0 && dID != 1 && files[0].name != null) {
                    $("[id^='btnUploadFile']").prop("disabled", false);
                }

                var select = $("#configList");

                select.empty();

                if (files.length > 0) {
                    var matchFound = false;
                    var matchIndex;
                    for (i = 1; i < configs.length; i++) {
                        if (configs[i].IsRegexSearch && files[0].name.match(configs[i].SearchCriteria)) {
                            select.append($('<option/>', {
                                value: configs[i].ConfigId,
                                text: configs[i].ConfigFileName
                            }));

                            if (i != 0) {
                                matchFound = true;
                                matchIndex = i;
                            }
                        }
                        else if (!configs[i].IsRegexSearch && files[0].name === configs[i].SearchCriteria) {
                            select.append($('<option/>', {
                                value: configs[i].ConfigId,
                                text: configs[i].ConfigFileName
                            }));

                            if (i != 0) {
                                matchFound = true;
                                matchIndex = i;
                            }
                        }
                    }

                    if (matchFound) {
                        $("#configDescription").html(configs[matchIndex].ConfigFileDesc);
                    }
                    else {
                        select.append($('<option/>', {
                            value: configs[0].ConfigId,
                            text: configs[0].ConfigFileName
                        }));
                        $("#configDescription").html(configs[0].ConfigFileDesc);
                    }
                }
            }   
            else
            {
                $("[id^='btnUploadFile']").prop("disabled", true);
                //Hide Configuration List
                $('#configList').parent().parent().hide();
                $("#configDescription").hide();
            }
        });

        $("#datasetList").change(function () {
            var dID = $(this).val();
            console.log("DatasetList : " + dID);
            $("[id^='btnUploadFile']").attr('data-id', dID);

            if (dID == 0)
            {
                //Hide Upload Button
                $("[id^='btnUploadFile']").prop("disabled", true);

                //Hide Create Button
                $("#btnCreateDatasetAtUpload").prop("disabled", true);
                $("#btnCreateDatasetAtUpload").hide();

                //Hide File Upload
                $("#DatasetFileUpload").prop("disabled", true);
                $("#DatasetFileUpload").parent().parent().hide();

                //Hide Configuration List
                $('#configList').parent().parent().hide();
                $("#configDescription").hide();
            }
            else if (dID == 1)
            {
                //Hide Upload Button
                $("[id^='btnUploadFile']").prop("disabled", true);

                //Show Create Button
                $("#btnCreateDatasetAtUpload").prop("disabled", false);
                $("#btnCreateDatasetAtUpload").show();

                //Hide File Upload
                $("#DatasetFileUpload").prop("disabled", true);
                $("#DatasetFileUpload").parent().parent().hide();

                //Hide Configuration List
                $('#configList').parent().parent().hide();
                $("#configDescription").hide();
            }
            else
            {
                //Show Upload Button
                //$("[id^='btnUploadFile']").prop("disabled", false);

                //Hide Create Button
                $("#btnCreateDatasetAtUpload").prop("disabled", true);
                $("#btnCreateDatasetAtUpload").hide();

                //Show File Upload
                $("#DatasetFileUpload").prop("disabled", false);
                $("#DatasetFileUpload").parent().parent().show();


                var fileUpload = $("#DatasetFileUpload").get(0);
                var files = fileUpload.files;

                if (files.length > 0) {
                    //Show Configuration List
                    $('#configList').parent().parent().show();
                    $("#configDescription").show();
                }
                else {
                    //Hide Configuration List
                    $('#configList').parent().parent().hide();
                    $("#configDescription").hide();
                }

                var fileUpload = $("#DatasetFileUpload").get(0);
                var files = fileUpload.files;

                if (files.length > 0) {

                    if (dID != 0 && dID != 1 && files[0].name != null) {
                        $("[id^='btnUploadFile']").prop("disabled", false);
                    }
                }

                if (dID > 0) {
                    var controllerURL = "/Dataset/GetDatasetFileConfigInfo/?id=" + encodeURI(dID);
                    $.get(controllerURL, function (result) {
                        configs = result;
                        var select = $("#configList");

                        select.empty();

                        var fileUpload = $("#DatasetFileUpload").get(0);
                        var files = fileUpload.files;

                        if (files.length > 0) {
                            var matchFound = false;
                            var matchIndex;
                            for (i = 1; i < configs.length; i++) {
                                if (configs[i].IsRegexSearch && files[0].name.match(configs[i].SearchCriteria)) {
                                    select.append($('<option/>', {
                                        value: configs[i].ConfigId,
                                        text: configs[i].ConfigFileName
                                    }));

                                    if (i != 0) {
                                        matchFound = true;
                                        matchIndex = i;
                                    }
                                }
                                else if (!configs[i].IsRegexSearch && files[0].name === configs[i].SearchCriteria) {
                                    select.append($('<option/>', {
                                        value: configs[i].ConfigId,
                                        text: configs[i].ConfigFileName
                                    }));

                                    if (i != 0) {
                                        matchFound = true;
                                        matchIndex = i;
                                    }
                                }
                            }

                            if (matchFound) {
                                $("#configDescription").html(configs[matchIndex].ConfigFileDesc);
                            }
                            else {
                                select.append($('<option/>', {
                                    value: configs[0].ConfigId,
                                    text: configs[0].ConfigFileName
                                }));
                                $("#configDescription").html(configs[0].ConfigFileDesc);
                            }
                        }

                    });
                }
            }
        });

        $("#configList").change(function () {
            var configID = $(this).val();

            for (i = 0; i < configs.length; i++)
            {
                if (configs[i].ConfigId == configID)
                {
                    $("#configDescription").html(configs[i].ConfigFileDesc);
                    break;
                }
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
            var jrUrl = result;
            window.open(jrUrl, "_blank");
        });
    },

    PushToSAS: function (caller, id, filename) {
       // console.log($(caller).parent().next().text());
       // console.log(id);
       // console.log(filename);
        data.Dataset.FileNameModal(filename);
        data.Dataset.PushToSAS_Filename(filename, $(caller).parent().next().text());
    },

    DownloadLatestDatasetFile: function (id) {
        /// <summary>
        /// Send temp URL (containing the dataset, from S3) to a new window
        /// This will initiate the download process
        /// </summary>

        var getLatestURL = "/Dataset/GetLatestDatasetFileIdForDataset/?id=" + encodeURI(id);
        $.get(getLatestURL, function (e) {
            var controllerURL = "/Dataset/GetDatasetFileDownloadURL/?id=" + encodeURI(e);
            $.get(controllerURL, function (result) {
                var jrUrl = result;
                window.open(jrUrl, "_blank");
            });
        });
    },

    PreviewDatafileModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Preview Datafile");

        $.get("/Dataset/PreviewDatafile/" + id, function (result) {
            modal.ReplaceModalBody(result);
        });
    },

    EditDataFileInformation: function(id){
        var modal = Sentry.ShowModalWithSpinner("Edit Data File");   

        $.get("/Dataset/EditDatasetFile/" + id, function (result) {
            modal.ReplaceModalBody(result);
           // data.DatasetDetail.VersionsModalInit(id);
        })
    },

    GetDatasetFileVersions: function (id) {
        var modal = Sentry.ShowModalWithSpinner("Versions");   

        $.get("/Dataset/GetDatasetFileVersions/" + id, function (result) {
            modal.ReplaceModalBody(result);
            data.DatasetDetail.VersionsModalInit(id);
        })
    },

    PreviewLatestDatafileModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Preview Datafile");

        $.get("/Dataset/PreviewLatestDatafile/" + id, function (result) {
            modal.ReplaceModalBody(result);
        });
    },


    UploadFileModal: function (id) {
        /// <summary>
        /// Load Modal for Uploading new Datafile
        /// </summary>
        var modal = Sentry.ShowModalWithSpinner("UploadDataFile");
        var createDatafileUrl = "/Dataset/GetDatasetUploadPartialView/?datasetId=" + encodeURI(id);

        $.get(createDatafileUrl, function (e) {
            modal.ReplaceModalBody(e);
            data.DatasetDetail.UploadModalInit(id);
        });       
    },

    SubscribeModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Subscribe");
        var Url = "/Dataset/Subscribe/?id=" + encodeURI(id);

        $.get(Url, function (e) {
            modal.ReplaceModalBody(e);
        });      
    },



    ProgressModalStatus: function () {
        // --- progress bar stuff : start ---
        // Reference the auto-generated proxy for the hub.

        var progress = $.connection.progressHub;

        // Create a function that the hub can call back to display messages.
        progress.client.AddProgress = function (message, percentage) {
            ProgressBarModal("show", message, "Progress: " + percentage);
            $('#ProgressMessage').width(percentage);
            if (percentage == "100%") {
                ProgressBarModal();
            }
        };

        $.connection.hub.start().done(function () {
            var connectionId = $.connection.hub.id;
        });
        // --- progress bar stuff : end ---
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
            select: {
                selector: 'td:not(:last-child)',
                selector: 'td:not(:first-child)'
            },
            ajax: {
                url: "/Dataset/GetBundledFileInfoForGrid/?Id=" + Id,
                type: "POST",
            },
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px", searchable: false },
                { data: "ActionLinks", className: "downloadFile", width: "100px", searchable: false, orderable: false },
                {
                    data: "Name", width: "40%", className: "Name", render: function (data, type, row) {
                        return "<a href = \"#\" onclick=\"data.DatasetDetail.GetDatasetFileVersions(" + row.Id
                            + ")\" title=\"View File Versions\">" + row.Name
                            + "</a>"
                    }
                },
                { data: "UploadUserName", className: "UploadUserName" },
                { data: "CreateDTM", className: "createdtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ModifiedDTM", type: "date", className: "modifieddtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ConfigFileName", className: "ConfigFileName" },
                {
                    data: "IsUsable",
                    width: "65px",
                    render: function (data, type, row) {
                        if (data === true) {
                            return '<input type=\"checkbox\" disabled checked value="' + data + '">';
                        } else {
                            return '<input type=\"checkbox\" disabled value="' + data + '">';
                        }
                        //return data;
                    },
                    className: "dt-body-center"
                },
            ],
            language: {
                search: "<div class='input-group'><span class='input-group-addon'><i class='glyphicon glyphicon-search'></i></span>_INPUT_</div>",
                searchPlaceholder: "Search",
                processing: ""
            },
            order: [5, 'desc'],
            stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
            "createdRow": function (row, data, dataIndex) {
                if (data["IsUsable"] == false) {
                    $(row).addClass('unUsable');
                }
            }
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
                { type: "text" },
                { type: "select", values: values }
            ],
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
                row.child(data.DatasetDetail.formatDatasetBundlingFileDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

    },


    DatasetFileTableInit: function(Id) {
       
        data.Dataset.DatasetFilesTable = $("#datasetFilesTable").DataTable({
            //$("#datasetFilesTable").dataTable({
            width: "100%",
            serverSide: true,
            //responsive: true,
            processing: true,
            searching: true,
            paging: true,
            rowId: 'Id',
            ajax: {
                url: "/Dataset/GetDatasetFileInfoForGrid/?Id=" + Id + "&bundle=" + false,
                type: "POST"
            },
            iDisplayLength: 10,
            aLengthMenu: [
                [10, 25, 50, 100, 200, -1],
                [10, 25, 50, 100, 200, "All"]
            ],
            select: {
                selector: 'td:not(:last-child)',
                selector: 'td:not(:first-child)'
            },
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px", searchable: false },
                { data: "ActionLinks", className: "downloadFile", width: "100px", searchable: false, orderable: false },
                {
                    data: "Name", width: "40%", className: "Name", render: function (data, type, row)
                    {
                        return "<a href = \"#\" onclick=\"data.DatasetDetail.GetDatasetFileVersions(" + row.Id
                            + ")\" title=\"View File Versions\">" + row.Name
                            + "</a>"
                    }
                },
                { data: "UploadUserName", className: "UploadUserName" },
                { data: "CreateDTM", className: "createdtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ModifiedDTM", type: "date", className: "modifieddtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ConfigFileName", className: "ConfigFileName" },
                {
                    data: "IsUsable",
                    width: "65px",
                    render: function (data, type, row) {
                        if (data === true) {
                            return '<input type=\"checkbox\" disabled checked value="' + data + '">';
                        } else {
                            return '<input type=\"checkbox\" disabled value="' + data + '">';
                        }
                        //return data;
                    },
                    className: "dt-body-center"
                },
            ],
            language: {
                search: "<div class='input-group'><span class='input-group-addon'><i class='glyphicon glyphicon-search'></i></span>_INPUT_</div>",
                searchPlaceholder: "Search",
                processing: ""
            },
            order: [5, 'desc'],
            stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
            "createdRow": function (row, data, dataIndex) {
                if (data["IsUsable"] == false) {
                    $(row).addClass('unUsable');
                }
            }
        });

        var values = [true, false];

        $("#datasetFilesTable").dataTable().columnFilter({
            sPlaceHolder: "head:after",
            aoColumns: [
                null,
                null,
                { type: "text" },
                { type: "text" },
                { type: "date-range" },
                { type: "date-range" },
                { type: "text" },
                { type: "select", values: values }
            ],
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
                row.child(data.DatasetDetail.formatDatasetFileDetails(row.data())).show();
                tr.addClass('shown');
            }
        });

        $('#datasetFilesTable tbody').on('click', 'td', function () {
            var tr = $(this).parent();

            if (!$(this).hasClass('details-control')) {
                if (!tr.hasClass('unUsable') && (tr.hasClass('even') || tr.hasClass('odd'))) {
                    if (tr.hasClass('active')) {
                        tr.removeClass('active');

                        var listOfFilesToBundle = JSON.parse(localStorage.getItem("listOfFilesToBundle"));

                        if ($(tr).prop('id')) {

                            if (listOfFilesToBundle != null) {
                                listOfFilesToBundle.splice(listOfFilesToBundle.indexOf($(tr).prop('id')), 1);
                            }

                            localStorage.setItem("listOfFilesToBundle", JSON.stringify(listOfFilesToBundle));

                            $('#bundleCountSelected').html(parseInt($('#bundleCountSelected').html(), 10) - 1);
                        }
                    }
                    else {
                        //table.$('tr.active').removeClass('active');
                        tr.addClass('active');

                        var listOfFilesToBundle = JSON.parse(localStorage.getItem("listOfFilesToBundle"));
                        if ($(tr).prop('id')) {

                            if (listOfFilesToBundle != null) {
                                listOfFilesToBundle[listOfFilesToBundle.length] = $(tr).prop('id');
                            }
                            else {
                                var listOfFilesToBundle = [];
                                listOfFilesToBundle[0] = $(tr).prop('id');
                            }

                            localStorage.setItem("listOfFilesToBundle", JSON.stringify(listOfFilesToBundle));

                            $('#bundleCountSelected').html(parseInt($('#bundleCountSelected').html(), 10) + 1);
                        }
                    }
                    if (parseInt($('#bundleCountSelected').html(), 10) === 0) {
                        $('#bundle_selected').attr("disabled", true);
                    }
                    else {
                        $('#bundle_selected').attr("disabled", false);
                    }
                }
            }
        
        });

        $('#datasetFilesTable').on('draw.dt', function () {
            $('#bundleCountFiltered').html(data.Dataset.DatasetFilesTable.page.info().recordsDisplay);
            $('#bundleCountSelected').html(0);
            localStorage.setItem("listOfFilesToBundle", JSON.stringify([]));

            if (data.Dataset.DatasetFilesTable.page.info().recordsDisplay === 0)
            {
                $('#bundle_allFiltered').attr("disabled", true);
            }
            else {
                $('#bundle_allFiltered').attr("disabled", false);
            }

            if (parseInt($('#bundleCountSelected').html(), 10) === 0)
            {
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
                row.child(data.DatasetDetail.formatDatasetFileConfigDetails(row.data())).show();
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
                        { data: "SearchCriteria", className: "searchCriteria"},
                        { data: "TargetFileName", className: "targetFileName" },
                        { data: "IsRegexSearch", className: "isRegexSearch", render: function (data, type, row) { return (data == true) ? '<span class="glyphicon glyphicon-ok"> </span>' : '<span class="glyphicon glyphicon-remove"></span>';} },
                        { data: "OverwriteDatasetFile", type: "date", className: "overwriteDatsetFile", render: function (data, type, row) { return (data == true) ? '<span class="glyphicon glyphicon-ok"> </span>' : '<span class="glyphicon glyphicon-remove"></span>'; } },
                        { data: "FileType", className: "fileType", },
                        { data: "CreationFreq", className: "CreationFreq" },
                        { data: "DatasetScopeTypeID", className: "DatasetScopeTypeID"}
            ],
            order: [1, 'asc']
            //stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
        });

        //$("#datasetFileConfigsTable").dataTable().columnFilter({
        //    sPlaceHolder: "head:after",
        //    aoColumns: [
        //            null,
        //            null,
        //            null,
        //            { type: "number-range" },
        //            { type: "text" },
        //            { type: "text" },
        //            { type: "text" },
        //            { type: "text" }
        //    ]
        //});

        var DataFilesTable = $('#datasetFileConfigsTable').dataTable();

        // DataTable
        var table = $('#datasetFileConfigsTable').DataTable();


        $('#datasetFileConfigsTable tbody').on('click', 'tr', function () {
            if ($(this).hasClass('active')) {
              //  $(this).removeClass('active');
            }
            else {
                //table.$('tr.active').removeClass('active');
             //   $(this).addClass('active');
            }
        });


        //// Apply the filter
        //table.columns().every(function () {
        //    var column = this;

        //    $('input', this.footer()).on('keyup change', function () {
        //        column
        //            .search(this.value)
        //            .draw();
        //    });
        //});

        //$("#userTable_wrapper .dt-toolbar").html($("#userToolbar"));

        //$("#exportToExcel").click(function () {
        //    alert("exportToExcel Function");
        //});
    },

    formatDatasetFileConfigDetails: function (d) {
        // `d` is the original data object for the row
    return '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">' +
        '<tr>' +
            '<td><b>Description</b>:</td>' +
            '<td>' + d.ConfigFileDesc + '</td>' +
        '</tr>' +
        '<tr>' +
            '<td><b>Drop Location Type</b>: </td>' +
            '<td>' + d.DropLocationType + '</td>' +
        '</tr>' +
        '<tr>' +
            '<td><b>Drop Path</b>: </td>' +
            '<td>' + d.DropPath + '</td>' +
        '</tr>' +
    '</table>';
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

        if (d.IsUsable !== undefined)
        {
            table +=
                '<tr>' +
                    '<td><b>Usable</b>: </td>' +
                    '<td>' + d.IsUsable + '</td>' +
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
}