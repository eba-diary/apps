/******************************************************************************************
 * Javascript methods for the Asset-related pages
 ******************************************************************************************/

data.DatasetDetail = {

    DatasetFilesTable: {},

    Init: function () {
        /// <summary>
        /// Initialize the dataset detail page for data assets
        /// </summary>

        var Id = $('#datasetConfigList').val();

        $("[id^='EditDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.ViewEdit($(this).data("id"));
        });

        $("[id^='DownloadLatest']").off('click').on('click', function (e) {
            e.preventDefault();
            data.DatasetDetail.DownloadLatestDatasetFile($(this).data("id"));
        });

        $("[id^='PushtoSAS_']").off('click').on('click', function (e) {
            //console.log(e);
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
            var category = "#hide_" + id;
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
                data.DatasetDetail.PushToBundler(datasetID, listOfFilesToBundle);
            }
        });

        $("#bundle_allFiltered").click(function (e) {
            e.preventDefault();

            var datasetID = window.location.pathname.substr(window.location.pathname.lastIndexOf('/') + 1);

            var params = Sentry.GetDataTableParamsForExport($('#datasetFilesTable').DataTable());

            $.ajax({
                url: "/Dataset/GetDatasetFileInfoForGrid/?Id=" + Id + "&bundle=" + true,
                method: "POST",
                data: params,
                dataType: 'json',
                success: function (obj) {
                    //console.log('success');
                    //console.log(obj);

                    var listOfFilesToBundle = [];
                    var configName = obj.data[0].ConfigFileName;
                    var multipleConfigs = false;

                    for (i = 0; i < obj.data.length; i++) {
                        listOfFilesToBundle.push(obj.data[i].Id);

                        if (obj.data[i].ConfigFileName != configName) {
                            multipleConfigs = true;
                        }
                    }
                    //console.log(listOfFilesToBundle);

                    if (multipleConfigs) {

                        Sentry.ShowModalAlert(
                            "There are multiple configuration files associated with the set of files you want to bundle.  Please only select file from a single configuration."
                        );
                    }
                    else {
                        data.DatasetDetail.PushToBundler(datasetID, listOfFilesToBundle);
                    }
                },
                failure: function (obj) {
                    //console.log('failed');
                    //console.log(obj);
                },
                error: function (obj) {
                    //console.log('error');
                    //console.log(obj);
                }
            });
        });

        $('body').on('click', '.jobstatus', function () {

            if ($(this).hasClass('jobstatus_enabled')) {
                var controllerurl = "/Dataset/DisableRetrieverJob/?id=";
            }
            else {
                var controllerurl = "/Dataset/EnableRetrieverJob/?id=";
            }

            var request = $.ajax({
                url: controllerurl + $(this).attr('id'),
                method: "POST",
                dataType: 'json',
                success: function (obj) {
                    var modal = Sentry.ShowModalConfirmation(
                        obj.Message, function () { location.reload(); })
                },
                failure: function (obj) {
                    var modal = Sentry.ShowModalAlert(
                        obj.Message, function () { location.reload(); })
                },
                error: function (obj) {
                    var modal = Sentry.ShowModalAlert(
                        obj.Message, function () { location.reload(); })
                }
            });
        });
    },

    PushToBundler: function (dataSetID, listOfFilesToBundle) {
        for (i = 0; i < listOfFilesToBundle.length; i++) {
            //console.log("Bundle " + i + " : " + listOfFilesToBundle[i]);
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
                    //console.log(obj);
                },
                error: function (xhr, e) {
                    //console.log(e);
                    //console.log(xhr);
                }
            });
        }

        Sentry.ShowModalCustom(
            "Upload Results:",
            "<div> Please supply the new name of your bundled file: (Please Do NOT include the file extension)</div><hr/><div><input id='inputNewName' placeholder='New Name: '/></div>",
            Sentry.ModalButtonsOKCancel(function (result) { DoWork(dataSetID, listOfFilesToBundle, $('#inputNewName').val()) })
        );

        $('.btn-primary').prop("disabled", true);

        $('#inputNewName').keyup(function (e) {
            e.preventDefault();

            //console.log($('#inputNewName').val());
            if ($('#inputNewName').val() !== "" && $('#inputNewName').val() !== undefined && $('#inputNewName').val() != null) {
                $('.btn-primary').prop("disabled", false);
            }
            else {
                $('.btn-primary').prop("disabled", true);
            }

        });

    },

    ///<summary>
    ///Initialize the PushToFilenameOverride partial view
    ///</summary>
    PushToBundleInit: function () {
        $("[id^='BundleFilename']").off('click').on('click', function (e) {
            e.preventDefault();
            data.Dataset.PushToSAS_Filename($(this).data("id"), $("#BundleFilename").val());
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
                //{ data: "PreviewHref", className: "previewFile", width: "20px" },
                //{ data: "Id", width: "40px", type: "num", className: "datasetfileid" },
                { data: "Name", width: "40%", className: "Name" },
                { data: "ConfigFileName", className: "configFileName" },
                { data: "UploadUserName", className: "UploadUserName" },
                { data: "CreateDTM", className: "createdtm", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ModifiedDTM", type: "date", className: "modifieddtm", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } }
            ],
            order: [6, 'desc'],
            //stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
            "createdRow": function (row, data, dataIndex) {
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

    UploadModalInit: function (id) {
        var configs;
        var idFromSelectList = $('#datasetList').find(":selected").val();
        var dID;

        //console.log(idFromSelectList + "," + dID);

        if (idFromSelectList === undefined) {
            dId = id;
        } else {
            dId = idFromSelectList;
        }

        $("#categoryList").prepend($("<option />").val(0).html("-- Select a Category --"));

        $("#categoryList").each(function (i) {
            $(this).val(i);
        });

        $('#btnUploadFile').prop("disabled", true);

        if (id == 0 || id == 1) {
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

            $('#configList').parent().parent().show();
            $("#configDescription").show();

            if (id > 0) {
                //console.log('Main Loop');
                getConfigs();
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
                //console.log('File Upload Process Started');
                var fileUpload = $("#DatasetFileUpload").get(0);
                var files = fileUpload.files;

                //Create FormData object
                var fileData = new FormData();

                fileData.append(files[0].name, files[0]);

                if ((files[0].size / 1000000) > 100) {

                    var configID = $("#configList").find(":selected").val();
                    var dropLocation;

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
                        '<p>If you don\'t have access to this drop location please contact <a href="mailto:DSCSupport@sentry.com"> Data.sentry.com Administration </a> for further assistance </p>'
                    );

                    $('.modal-footer button').prop("disabled", false);
                }
                else {
                    var datasetID;

                    if ($('#datasetList').find(":selected").val() === undefined) {
                        datasetID = id;
                    } else {
                        datasetID = $('#datasetList').find(":selected").val();
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
                        //modal.ReplaceModalBody('Successful');
                        modal.ReplaceModalBody(e.currentTarget.response.replace(/"/g, ''));
                        //console.log(e);
                    });

                    function cancelUpload() {
                        xhr.abort();
                        //console.log('The Upload Process was aborted');
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

                        if (itemData.Value === id) {
                            select.append($('<option/>', {
                                value: itemData.Value,
                                selected: true,
                                text: itemData.Text
                            }));
                        } else {
                            select.append($('<option/>', {
                                value: itemData.Value,
                                text: itemData.Text
                            }));
                        }
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
            else if ($("#datasetList option:selected").text("Select Dataset")) {
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

        $("#datasetList").change(function () {
            if ($('#datasetList').find(":selected").val() == 0) {
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
            else if ($('#datasetList').find(":selected").val() == 1) {
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
            else {

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

                //console.log('Dataset List Change Loop');
                getConfigs();

            }
        });

        function getConfigs() {
            var idFromSelectList = $('#datasetList').find(":selected").val();
            var dID;

            //console.log(idFromSelectList + "," + dID);

            if (idFromSelectList === undefined) {
                dID = id;
            } else {
                dID = idFromSelectList;
            }

            var controllerURL = "/Dataset/GetDatasetFileConfigInfo/?id=" + encodeURI(dID);
            $.get(controllerURL, function (result) {
                configs = result;
                var select = $("#configList");

                select.empty();

                var fileUpload = $("#DatasetFileUpload").get(0);
                var files = fileUpload.files;

                if (files.length > 0) {

                    var matchFound = false;
                    var indexes = [];
                    var matchIndex;
                    var amountMatched = 0;
                    var j;

                    var extension = files[0].name.substr((files[0].name.lastIndexOf('.') + 1));  //https://stackoverflow.com/questions/3042312/jquery-find-file-extension-from-string

                    //console.log(configs);

                    for (i = 1; i < configs.length; i++) {
                        for (j = 0; j < configs[i].SearchCriteria.length; j++) {
                            if (configs[i].FileExtension.Name.toLowerCase().trim() == 'any' || configs[i].FileExtension.Name.toLowerCase().trim() == extension.toLowerCase()) {
                                if (configs[i].IsRegexSearch[j] && files[0].name.match(configs[i].SearchCriteria[j])) {
                                    if (indexes.indexOf(i) == -1) {
                                        if (!matchFound) {
                                            matchIndex = i;
                                            matchFound = true;
                                        }

                                        select.append($('<option/>', {
                                            value: configs[i].ConfigId,
                                            selected: matchIndex == i ? true : false,
                                            text: configs[i].ConfigFileName
                                        }));

                                        indexes.push(i);
                                        amountMatched++;
                                    }
                                }
                                else if (!configs[i].IsRegexSearch[j] && files[0].name === configs[i].SearchCriteria[j]) {
                                    if (indexes.indexOf(i) == -1) {
                                        if (!matchFound) {
                                            matchIndex = i;
                                            matchFound = true;
                                        }

                                        select.append($('<option/>', {
                                            value: configs[i].ConfigId,
                                            selected: matchIndex == i ? true : false,
                                            text: configs[i].ConfigFileName
                                        }));

                                        indexes.push(i);
                                        amountMatched++;
                                    }
                                }
                            }
                        }
                    }

                    if (matchFound) {
                        $("#configDescription").html(configs[matchIndex].ConfigFileDesc);

                        if (amountMatched > 1) {
                            $("#configList").prop('disabled', false);
                        }
                        $('#btnUploadFile').prop("disabled", false);
                    }
                    else {
                        select.append($('<option/>', {
                            value: configs[0].ConfigId,
                            selected: true,
                            text: configs[0].ConfigFileName
                        }));
                        $("#configList").prop('disabled', false);
                        $('#btnUploadFile').prop("disabled", false);
                        $("#configDescription").html(configs[0].ConfigFileDesc);
                    }
                } else {
                    $("#configList").prop('disabled', true);
                    $('#btnUploadFile').prop("disabled", true);
                    $("#configDescription").html("Please choose a file from the Choose File button.");
                }

            });
        }

        $("#configList").change(function () {
            var configID = $(this).val();

            for (i = 0; i < configs.length; i++) {
                if (configs[i].ConfigId == configID) {
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

    DownloadLatestDatasetFile: function (id) {
        /// <summary>
        /// Send temp URL (containing the dataset, from S3) to a new window
        /// This will initiate the download process
        /// </summary>

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

    PushToSAS: function (caller, id, filename) {
        // //console.log($(caller).parent().next().text());
        // //console.log(id);
        // //console.log(filename);
        data.Dataset.FileNameModal(filename);
        data.Dataset.PushToSAS_Filename(filename, $(caller).parent().next().text());
    },

    PreviewDatafileModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Preview Datafile");

        $.get("/Dataset/PreviewDatafile/" + id, function (result) {
            modal.ReplaceModalBody(result);
        });
    },

    PreviewLatestDatafileModal: function (id) {

        var modal = Sentry.ShowModalWithSpinner("Preview Datafile");

        $.get("/Dataset/PreviewLatestDatafile/" + id, function (result) {
            modal.ReplaceModalBody(result);
        });
    },

    EditDataFileInformation: function (id) {
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


    UploadFileModal: function (id) {
        /// <summary>
        /// Load Modal for Uploading new Datafile
        /// </summary>
        var modal = Sentry.ShowModalWithSpinner("Upload Data File");
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
            ajax: {
                url: "/Dataset/GetBundledFileInfoForGrid/?Id=" + Id,
                type: "POST",
            },
            columns: [
                { data: null, className: "details-control", orderable: false, defaultContent: "", width: "20px", searchable: false },
                { data: "ActionLinks", className: "downloadFile", width: "100px", searchable: false, orderable: false },
                {
                    data: "Name", width: "40%", className: "Name"
                },
                { data: "UploadUserName", className: "UploadUserName" },
                { data: "CreateDTM", className: "createdtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ModifiedDTM", type: "date", className: "modifieddtm", width: "auto", render: function (data) { return data ? moment(data).format("MM/DD/YYYY h:mm:ss") : null; } },
                { data: "ConfigFileName", className: "ConfigFileName" },
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
                { type: "text" }
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


    DatasetFileTableInit: function (Id) {

        data.Dataset.DatasetFilesTable = $("#datasetFilesTable").DataTable({
            //$("#datasetFilesTable").dataTable({
            width: "100%",
            serverSide: true,
            //responsive: true,
            processing: true,
            searching: true,
            paging: true,
            destroy: true,
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
                { type: "text" }
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
            if ($('#datasetFilesTable >tbody >tr').length >= 1 && $($('#datasetFilesTable >tbody >tr>td')[0]).hasClass('dataTables_empty') == false) {
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
                { data: "SearchCriteria", className: "searchCriteria" },
                { data: "TargetFileName", className: "targetFileName" },
                { data: "IsRegexSearch", className: "isRegexSearch", render: function (data, type, row) { return (data == true) ? '<span class="glyphicon glyphicon-ok"> </span>' : '<span class="glyphicon glyphicon-remove"></span>'; } },
                { data: "OverwriteDatasetFile", type: "date", className: "overwriteDatsetFile", render: function (data, type, row) { return (data == true) ? '<span class="glyphicon glyphicon-ok"> </span>' : '<span class="glyphicon glyphicon-remove"></span>'; } },
                { data: "FileType", className: "fileType", },
                { data: "DatasetScopeTypeID", className: "DatasetScopeTypeID" }
            ],
            order: [1, 'asc']
            //stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
        });


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

    OpenReport: function (artifactType, artifactPath) {

        // check what type we're working with
        if (artifactType === 'file') {

            // check if the user has permission and if so, download the file
            $.ajax({
                url: '/ExternalFile/HasReadPermissions?pathAndFilename=' + artifactPath,
                method: "GET",
                dataType: 'json',
                success: function (obj) {
                    if (obj.HasPermission) {
                        var url = '/ExternalFile/DownloadExternalFile?pathAndFilename=' + artifactPath;
                        window.open(url);
                    }
                    else {
                        Sentry.ShowModalAlert(
                            "User does not have sufficient permissions to selected file."
                        );
                    }
                },
                error: function (obj) {
                    Sentry.ShowModalAlert("Failed permissions check, please try again. If problem persists, please contact <a mailto:DSCSupport@sentry.com></a>");
                }
            });

        } else {

            console.log('type: ' + artifactType);
            console.log('path: ' + artifactPath);

            // open the link in a new window
            var win = window.open(artifactPath, '_blank');
            if (win) {
                //Browser has allowed it to be opened
                win.focus();
            } else {
                //Browser has blocked it
                alert('Please allow popups for this website');
            }

        }

        
    }
};