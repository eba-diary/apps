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