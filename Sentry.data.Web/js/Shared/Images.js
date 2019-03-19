/******************************************************************************************
 * Javascript methods for the Images
 ******************************************************************************************/

data.Images = {

    InitImages: function () {
        $(".fancybox").fancybox({
            buttons: [
                "zoom",
                "close"
            ]
        });
    },

    initImageUpload: function () {

        var UploadTempFile = function (file) {
            alert('Setting temp file variable');
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
        UploadTempFile.prototype.doUpload = function () {
            alert('Uploading UploadTempFile data');
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
                    alert('Upload Susccess');
                    console.log(data);
                    alert(data.Image.StorageKey);
                    // your callback here
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
    },

    removeNestedForm: function (element, container, deleteElement) {
        $container = $(element).parents(container);
        $container.find(deleteElement).val('True');
        $container.hide();

        // If file was just uploaded, we need to ensure file data is removed.

    },

    addNestedForm: function(container, counter, ticks, content) {
    var nextIndex = $(counter).length;
    var pattern = new RegExp(ticks, "gi");
    content = content.replace(pattern, nextIndex);
    $(container).append(content);
}
    
}