﻿/******************************************************************************************
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

        // Initialize Images for thumbnails
        data.Images.InitImages();

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
        $("#ContactSearch").attr("placeholder", "Add Associate by name or ID");

        /// Initialize the Create Exhibit view
        //Set Secure HREmp service URL for associate picker
        $.assocSetup({ url: hrEmpUrl });
        var picker = $('#ContactSearch').assocAutocomplete({
            associateSelected: function (associate) {
                
                //Check if user is already selected
                //Generate jquery for Hidden field
                var contactInput = "[name^=ContactIds][value=" + associate.Id + "]"
                //Generate jquery for contacts-block
                var contactBlock = "#" + associate.Id + ".contacts-block"

                if ($(contactInput).length == 0 && $(contactBlock).length == 0) {
                    jQuery('<div class="contacts-block" id="' + associate.Id + '"><span class="contactinfo-remove">x</span>' + associate.LastName + ',' + associate.FirstName + '</div>').appendTo('.contactList');
                    //add hidden value for post back
                    var inputs = $("[id^=ContactIds_]").length;

                    jQuery('<input/>', {
                        id: 'ContactIds_' + inputs + '_',
                        name: 'ContactIds[' + inputs + ']',
                        type: 'hidden',
                        value: associate.Id
                    }).appendTo('#informationPanel');
                }
            },
            close: function () {
                picker.clear();
            },
            minLength: 0,
            maxResults: 10
        });

        $(document).on('click', '.contactinfo-remove', function (e) {
            e.preventDefault();
            //get parent div
            var parent = $(this).parent()[0];
            var blockId = parent.id;
            //Get user ID
            var userId = blockId.replace('contact_', '');
            //Generate jquery for userId
            var contactInput = "[name^=ContactIds][value=" + userId + "]"
            //Generate jquery for contacts-block
            var contactBlock = "#" + blockId + ".contacts-block"

            //Remove associated ContactId hidden input
            $(contactInput).remove();

            //Remove associated contacts-block element
            $(contactBlock).remove();
        });

        //determine the cancel button url
        $("[id^='CancelButton']").off('click').on('click', function (e) {
            e.preventDefault();
            window.location = data.Report.CancelLink($(this).data("id"));
        });

        //Initialize BO Get Latest option when FileTypeID is already pouplated on load
        data.Report.SetGetLatestPanel($("#FileTypeId").val());

        //Show\Hide BO Get Latest option based on FileTypeId selection
        $("#FileTypeId").change(function () {
            data.Report.SetGetLatestPanel($(this).val());
        });

        

        data.Tags.initTags();
        data.Images.InitImages();

        $('#addNewImage').on('click', function (e) {
            e.preventDefault();
            //$.get('/BusinessIntelligence/NewImage', function (template) {
            //    $('.detail-thumbnail-list').append(template)
            //});
            $('input[type="file"]').click();
        });

        function TriggerInput(obj) {
            var parentContainer = $(this).parent().parent()
            var fileInput = $(parentContainer).find("input[name$='ImageFileData']:last")
            //$(fileInput).trigger('click');
            $(fileInput).click();
        }

        //var $inputs = $('.image-box');
        var $inputs = $('input[type="file"]')

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

        var UploadTempFile = function (file) {
            //alert('Setting temp file variable');
            this.file = file;
        };

        UploadTempFile.prototype.getType = function () {
            return this.file.type;
        };
        UploadTempFile.prototype.getSize = function () {
            return this.file.size;
        };
        UploadTempFile.prototype.getName = function () {
            return this.file.name;
        };
        UploadTempFile.prototype.doUpload = function (previewBoxData) {
            //alert('Uploading UploadTempFile data');
            //alert('was passed this data:' + previewBoxData)
            var that = this;
            var formData = new FormData();

            // add assoc key values, this will be posts values
            formData.append("file", this.file, this.getName());
            formData.append("upload_file", true);

            $.ajax({
                type: "POST",
                url: "/BusinessIntelligence/UploadPreviewImage",
                xhr: function () {
                    var myXhr = $.ajaxSettings.xhr();
                    if (myXhr.upload) {
                        myXhr.upload.addEventListener('progress', that.progressHandling, false);
                    }
                    return myXhr;
                },
                success: function (data) {
                    //alert('Upload Susccess');
                    //console.log(data);

                    $('.detail-thumbnail-list').append(data)
                    //data.Images.InitImages();
                    $('head').append('<link rel="stylesheet" href="https://cdn.jsdelivr.net/gh/fancyapps/fancybox@3.5.6/dist/jquery.fancybox.min.css" />');
                    //previewFile(previewBoxData, data);
                    //return data;
                },
                error: function (error) {
                    alert('Upload Error');
                    // handle error
                },
                async: true,
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
                timeout: 60000
            });
        };

        $(document).on('change', "input[name$='ImageFileData']", function () {
            //showFiles(e.target.files);
            //previewFile(this);

            var file = $(this)[0].files[0];
            var upload = new UploadTempFile(file);

            //execute upload
            upload.doUpload(this);

            ////Ensure associated Image is displayed
            //$(this).closest('.Image').css({ "display": "" });

            //Hide Add button when there are three images on page, however,
            //  third element is not rendered before this count takes plage.
            //Since call has been made for the third, only count if to images
            //  if two images are available.   
            if ($('.detail-thumbnail-list .Image:visible').length === 2) {
                $('.add-thumbnail').hide("fast");
            };

            

        });

        function addDroppedFile(input, e) {


            var ajaxData = new FormData($('#BusinessIntelligenceForm').get(0));

            ajaxData.append($(input).attr('name'), e);

            //if (droppedFiles) {
            //    $.each(droppedFiles, function (i, file) {
                    
            //    });
            //}
        }

        function previewFile(obj, data) {
            //var imageName = $(obj).parent().parent();

            var imgObj = $(obj).closest('div.Image');
            var imgSelector = $(imgObj).find('a');
            //var imagePreview = ".thumbnail-preview-" + imageName;
            //var imageBox = "#PreviewBox_" + imageName;
            //var imageNumber = imageName.substr(imageName.length - 1);
            var fancybox = $(imgObj).find('fancybox-images');
            var hrefSelector = $(imgObj).find('a');
            var imgSelector = $(imgObj).find('img');
            var imageUrl = "/BusinessIntelligence/GetImage?url=" + data.StorageKey;

            $(hrefSelector).attr('href', imageUrl);
            $(imgSelector).attr('src', imageUrl + "&t=2")
            $(fancybox).css({ "display": "" });
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
            var d_id = $(this).data("id");

            $.ajax({
                url: '/BusinessIntelligence/' + d_id + '/Favorites',
                method: "GET",
                dataType: 'json',
                success: function (obj) {
                    var message;
                    var objData = JSON.parse(obj);
                    var favCount = objData.Favorites.length;
                    if (favCount === 1) {
                        message = "This exhibit is favorited by 1 user.  Click <a href=" + objData.MailToAllLink + ">here</a> to email the user before deleting. If you choose to continue, the exhibit link will be permanently deleted from data.sentry.com.</br></br>Do you wish to continue?"
                    }
                    else if (favCount > 1) {
                        message = "This exhibit is favorited by " + objData.Favorites.length + " users.  Click <a href=" + objData.MailToAllLink + ">here</a> to email the users before deleting. If you choose to continue, the exhibit link will be permanently deleted from data.sentry.com.</br></br>Do you wish to continue?"
                    }
                    else {
                        message = "This will permanently delete the exhibit link from data.sentry.com.</br></br>Do you wish to continue?"
                    }

                    var model = Sentry.ShowModalCustom("Delete Exhibit", message, Sentry.ModalButtonsOKCancel(function () { data.Report.DeleteDataset(d_id); }));
                    modal.show();
                },
                failure: function () {
                    Sentry.ShowModalAlert("We failed to delete exhibit.  Please try again later.", function () { location.reload(); });
                },
                error: function () {
                    Sentry.ShowModalAlert("We failed to delete exhibit.  Please try again later.", function () { location.reload(); });
                }
            });            
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

        data.Report.SetReturntoSearchUrl();
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
                    Sentry.ShowModalAlert(obj.Message, function () { location.reload(); });
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
        }
    },

    SetReturntoSearchUrl: function () {

        var returnUrl = "/Search/BusinessIntelligence";
        var returnLink = $('#linkReturnToBusinessIntelligenceList');

        //---is this neede?
        if (localStorage.getItem("searchText") !== null) {
            var text = localStorage.getItem("searchText");
            returnUrl += "?searchPhrase=" + text;
            var storedNames;
            if (localStorage.getItem("filteredIds") !== null) {
                storedNames = JSON.parse(localStorage.getItem("filteredIds"));
                returnUrl += "&ids=";

                for (i = 0; i < storedNames.length; i++) {
                    returnUrl += storedNames[i] + ',';
                }
                returnUrl = returnUrl.replace(/,\s*$/, "");
            }
        }
        else if (localStorage.getItem("filteredIds") !== null) {
            storedNames = JSON.parse(localStorage.getItem("filteredIds"));
            returnUrl += "?ids=";

            for (i = 0; i < storedNames.length; i++) {
                returnUrl += storedNames[i] + ',';
            }
            returnUrl = returnUrl.replace(/,\s*$/, "");
        }
        returnLink.attr('href', returnUrl);
    }
};