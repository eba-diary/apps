/******************************************************************************************
 * Javascript methods for the Asset-related pages
 ******************************************************************************************/

data.Report = {

    IndexInit: function () {
        /// Initialize the Index page for data assets (with the categories)

        $("[id^='CreateDataset']").off('click').on('click', function (e) {
            e.preventDefault();
            window.location = data.Report.CreateLink();
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

    FormInit: function (hrEmpUrl, hrEmpEnv) {

        $("#DatasetCategoryIds").select2({
            placeholder:"Select Categories"
        });
        $("#DatasetBusinessUnitIds").select2({
            placeholder: "Select Business Units"
        });
        $("#DatasetFunctionIds").select2({
            placeholder: "Select Functions"
        });
        $("#DatasetFileTypeId").select2({
            placeholder: "Select Exhibit Type",
            allowClear: true
        });
        $("#FileTypeId").select2({
            placeholder: "Select Exhibit Type",
            allowClear: true,
            minimumResultsForSearch: 10
        });
        $("#FrequencyId").select2({
            placeholder: "Select Frequency",
            allowClear: true,
            minimumResultsForSearch: 10
        });

        /// Initialize the Create Exhibit view
        //Set Secure HREmp service URL for associate picker
        $.assocSetup({ url: hrEmpUrl });
        var permissionFilter = "ReportModify,DatasetManagement," + hrEmpEnv;
        $("#PrimaryOwnerName").assocAutocomplete({
            associateSelected: function (associate) {
                $('#PrimaryOwnerId').val(associate.Id);
            },
            filterPermission: permissionFilter,
            minLength: 0,
            maxResults: 10
        });
        $("#PrimaryContactName").assocAutocomplete({
            associateSelected: function (associate) {
                $('#PrimaryContactId').val(associate.Id);
            },
            filterPermission: permissionFilter,
            minLength: 0,
            maxResults: 10
        });

        //determine the cancel button url
        $("[id^='CancelButton']").off('click').on('click', function (e) {
            e.preventDefault();
            window.location = data.Report.CancelLink($(this).data("id"));
        });

        $("#FileTypeId").change(function () {
            data.Report.SetGetLatestPanel($(this).val());
        });

        data.Tags.initTags();

        //var $inputs = $('.image-box');
        var $inputs = $('#files')

        var isAdvancedUpload = function () {
            var div = document.createElement('div');
            return (('draggable' in div) || ('ondragstart' in div && 'ondrop' in div)) && 'FormData' in window && 'FileReader' in window;
        }();

        var droppedFiles = false;

        $.each($inputs, function (i, obj) {
            var $form = $(obj);            

            if (isAdvancedUpload) {
                $form.addClass('has-advanced-upload');                

                $form.on('drag dragstart dragend dragover dragenter dragleave drop', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                })
                    .on('dragover dragenter', function () {
                        $form.addClass('is-dragover');
                    })
                    .on('dragleave dragend drop', function () {
                        $form.removeClass('is-dragover');
                    })
                    .on('drop', function (e) {
                        //droppedFiles = e.originalEvent.dataTransfer.files;
                        //addDroppedFile(obj, e.originalEvent.dataTransfer.files);
                        //showFiles(droppedFiles);
                        //previewFile();
                    });
            }

            var $input = $form.find('input[type="file"]'),
                $label = $form.find('label'),
                showFiles = function (files) {
                    $label.text(files.length > 1 ? ($input.attr('data-multiple-caption') || '').replace('{count}', files.length) : files[0].name);
                };

            $input.id

            $input.on('change', function (e) {
                showFiles(e.target.files);
                previewFile($form.find('#preview'));
            });

            //function onDrop(evt) {
            //    var $someDiv = $('div');

            //    $.each(evt.originalEvent.dataTransfer.files, function (index, file) {
            //        var img = document.createElement('img');
            //        img.onload = function () {
            //            window.URL.revokeObjectURL(this.src);
            //        };
            //        img.height = 100;
            //        img.src = window.URL.createObjectURL(file);
            //        $someDiv.append(img);
            //    });
            //}

        })

        function addDroppedFile(input, e) {


            var ajaxData = new FormData($('#BusinessIntelligenceForm').get(0));

            ajaxData.append($(input).attr('name'), e);

            //if (droppedFiles) {
            //    $.each(droppedFiles, function (i, file) {
                    
            //    });
            //}
        }

        function previewFile(obj) {
            //var preview = $('#imgPreview1');
            var preview = $(obj);
            var file = document.querySelector('input[type=file]').files[0];
            var reader = new FileReader();

            reader.addEventListener("load", function () {
                //$('#imgPreview1').attr('src', reader.result)
                preview.src = reader.result;
            }, false);

            if (file) {
                reader.readAsDataURL(file);
            }
        }

        function previewFiles() {

            var preview = document.querySelector('#preview');
            var files = document.querySelector('input[type=file]').files;

            function readAndPreview(file) {

                // Make sure `file.name` matches our extensions criteria
                if (/\.(jpe?g|png|gif)$/i.test(file.name)) {
                    var reader = new FileReader();

                    reader.addEventListener("load", function () {
                        var image = new Image();
                        image.height = 100;
                        image.title = file.name;
                        image.src = this.result;
                        $('#preview').empty();
                        preview.appendChild(image);
                    }, false);

                    reader.readAsDataURL(file);
                }

            }

            if (files) {
                [].forEach.call(files, readAndPreview);
            }

        }

        //$('#btnSubmit').on('click', function (e) {
        //$("form").submit(function (e) {
        //    e.preventDefault();
        //    alert('hit submit trigger');
        //    var $postForm = $(this);

        //    if (isAdvancedUpload) {
        //        var ajaxData = new FormData($postForm.get(0));

        //        if (droppedFiles) {
        //            $.each(droppedFiles, function (i, file) {
        //                ajaxData.append($input.attr('name'), file);
        //            });
        //        }

        //        $.ajax({
        //            url: $postForm.attr('action'),
        //            type: $postForm.attr('method'),
        //            data: ajaxData,
        //            dataType: 'json',
        //            cache: false,
        //            contentType: false,
        //            processData: false,
        //            complete: function () {
        //                //$form.removeClass('is-uploading');
        //                alert('Hit ajax complete method');
        //            },
        //            success: function (data) {
        //                //$form.addClass(data.success == true ? 'is-success' : 'is-error');
        //                //if (!data.success) $errorMsg.text(data.error);
        //                alert('hit ajax success method');
        //            },
        //            error: function () {
        //                // Log the error, show an alert, whatever works for you
        //                alert('hit ajax error method')
        //            }
        //        });
        //    }
        //})        
    },

    DetailInit: function () {

        // Initialize the dataset detail page

        // Initialize Images for thumbnails
        data.Images.InitImages();

        $(".detailNameLink").click(function () {
            var artifactLink = $(this).data('artifactlink');
            var locationType = $(this).data('locationtype');
            if (locationType === 'file') {
                artifactLink = encodeURI(artifactLink);
            }
            data.Report.OpenReport(locationType, artifactLink);
        });

        $("[id^='EditDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            window.location = "/BusinessIntelligence/Edit/?" + "id=" + encodeURI($(this).data("id"));
        });

        $('#deleteLink').click(function (e) {
            e.preventDefault();

            var modal = Sentry.ShowModalConfirmation("Delete Exhibit", function () { data.BusinessIntelligenceDetail.DeleteDataset($(this).data("id")); });
            modal.ReplaceModalBody("This will <u>permanently</u> delete this Exhibit (<b>not the object which it references</b>). </br></br> Do you wish to continue?");
            modal.show();
        });

        $(document).on("click", "[id^='btnFavorite']", function (e) {
            e.preventDefault();

            var icon = $(this).children();
            $.ajax({
                url: '/Dataset/SetFavorite?datasetId=' + encodeURIComponent($(this).data("id")),
                method: "GET",
                dataType: 'json',
                success: function () { icon.toggleClass("glyphicon-star glyphicon-star-empty"); },
                error: function () { Sentry.ShowModalAlert("Failed to toggle favorite.");}
            });
            
        });

    },

    DeleteDataset: function (id) {

        var returnUrl = "/BusinessIntelligence/Index";
        $.ajax({
            url: "/BusinessIntelligence/Delete/" + encodeURI(id),
            method: "POST",
            dataType: 'json',
            success: function (obj) {
                if (obj.Success) {
                    Sentry.ShowModalConfirmation(obj.Message, function () { window.location = returnUrl; });
                } else {
                    Sentry.ShowModalAlert(obj.Message, function () { window.location = returnUrl; });
                }
            },
            failure: function (obj) {
                Sentry.ShowModalAlert(obj.Message, function () { location.reload(); });
            },
            error: function (obj) {
                Sentry.ShowModalAlert(obj.Message, function () { location.reload(); });
            }
        });

    },

    CreateLink: function () {
        /// link to create report
        return "/BusinessIntelligence/Create";
    },

    CancelLink: function (id) {
        if (id === undefined || id === 0) {
            return "/BusinessIntelligence/Index";
        } else {
            return "/BusinessIntelligence/Detail/" + encodeURIComponent(id);
        }
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
                    } else {
                        Sentry.ShowModalAlert("User does not have sufficient permissions to selected file.");
                    }
                },
                error: function (obj) {
                    Sentry.ShowModalAlert("Failed permissions check, please try again. If problem persists, please contact <a mailto:DSCSupport@sentry.com></a>");
                }
            });

        } else {
            var win = window.open(artifactPath, '_blank');
            if (win) {
                //Browser has allowed it to be opened
                win.focus();
            } else {
                //Browser has blocked it
                alert('Please allow popups for this website');
            }
        }
    },

    SetGetLatestPanel: function (fileType) {
        // check for the BusinessObjects file type; value is stored in a hidden field on BusinessIntelligenceForm.cshtml
        if (fileType === $("#hidBizObjEnumVal").val()) {
            $('#BOGetLatestPanel').removeClass("hidden");
        }
        else {
            $('#BOGetLatestPanel').addClass("hidden");
            $('#GetLatest').prop('checked', false);
        }
    }    
};