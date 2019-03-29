/******************************************************************************************
 * Javascript methods for the Images
 ******************************************************************************************/

data.Images = {

    InitImages: function () {
        $().fancybox({
            selector: '.detail-thumbnail-list .fancybox:visible',
            buttons: [
                "zoom",
                "close"
            ]
        });
    },

    InitImageUpload: function () {

        $('#addNewImage').on('click', function (e) {
            e.preventDefault();
            $('input[type="file"]').click();
        });

        function TriggerInput(obj) {
            var parentContainer = $(this).parent().parent()
            var fileInput = $(parentContainer).find("input[name$='ImageFileData']:last")
            $(fileInput).click();
        }

        var UploadTempFile = function (file) {
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
                    $('.detail-thumbnail-list').append(data)
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
            var file = $(this)[0].files[0];
            var upload = new UploadTempFile(file);

            //limit image types
            if (upload.getType().indexOf("jpeg") >= 0 || upload.getType().indexOf("png") >= 0) {
                //execute upload
                upload.doUpload(this);

                //Hide Add button when there are three images on page, however,
                //  third element is not rendered before this count takes plage.
                //Since call has been made for the third, only count if to images
                //  if two images are available.   
                if ($('.detail-thumbnail-list .Image:visible').length === 2) {
                    $('.add-thumbnail').hide("fast");
                };
            }
            else {
                Sentry.ShowModalAlert("We only support jpeg and png image formats.");
            }
        }); 
    },

    removeNestedForm: function (element, container, deleteElement) {
        //find parent image container
        $container = $(element).parents(container);
        //mark object for deletion
        $container.find(deleteElement).val('True');
        //hide image container
        $container.hide();

        //find fancybox href and remove data-fancybox attribute to ensure
        // removed image does not showup in preview.
        $href = $container.find('.fancybox');
        $href.removeAttr("data-fancybox");

        //Show add image button if visible images are less than 3
        if ($('.detail-thumbnail-list .Image:visible').length < 3) {
            $('.add-thumbnail').show("fast");
        }; 

        // If file was just uploaded, we need to ensure file data is removed.

    },

    addNestedForm: function(container, counter, ticks, content) {
    var nextIndex = $(counter).length;
    var pattern = new RegExp(ticks, "gi");
    content = content.replace(pattern, nextIndex);
    $(container).append(content);
}
    
}