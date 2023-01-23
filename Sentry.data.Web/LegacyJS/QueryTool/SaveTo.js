function SavetoS3() {
    var tableName = $('#s3FileName').val().escapeForJson();
    var extension = $('#s3SaveAsFileExtension').val().escapeForJson();
    var delimiter = $('#s3Delimiter').val();
    var s3Location = $('#s3FileLocation').val().escapeForJson();

    var table;
    var valid = true;

    if (extension) {
        table = "queryResponse.coalesce(1).write";

        if (extension === "csv" && delimiter) {
            table += ".option('sep', '" + delimiter + "')";
        }


        switch (extension) {
            case "csv":
                table += ".format('" + extension.trim() + "').option('header', 'true')";
                break;
            case "json":
                table += ".format('" + extension.trim() + "')";
                break;
            case "parquet":
                table += ".format('" + extension.trim() + "')";
                break;
            case "orc":
                table += ".format('" + extension.trim() + "')";
                break;
            default:
                valid = false;
                break;
        }

        table += ".mode('overwrite').save('s3a://" + s3Location + tableName + "');";

    }

    if (valid) {
        var json = JSON.stringify(table);

        console.log(json);

        SendCode(json, 'Save to S3', (s3Location + tableName));
    }
}

function callFileDownload(s3Key) {
    //Trim the Bucket
    var bucket = s3Key.substring(0, s3Key.indexOf('/') + 1);
    s3Key = s3Key.replace(bucket, '');

    var s3FileName = $('#s3FileName').val() + "." + $('#s3SaveAsFileExtension').find(":selected").text();
    var key = s3Key + "/" + s3FileName;

    console.log(s3Key);
    $.ajax({
        type: "GET",
        url: "/api/v1/queryTool/files/DownloadUrl/" + key,
        dataType: "json",
        success: function (msg) {
            $('#s3DownloadLocation').attr('href', msg);
            $('#s3DownloadKeyText').text(msg);
            var countDownDate = new Date(new Date().getTime() + 2 * 60000);

            // Update the count down every 1 second
            var x = setInterval(function () {

                // Get todays date and time
                var now = new Date().getTime();

                // Find the distance between now an the count down date
                var distance = countDownDate - now;

                // Time calculations for days, hours, minutes and seconds
                var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
                var seconds = Math.floor((distance % (1000 * 60)) / 1000);

                // Display the result in the element with id="demo"

                $('#spanTimeLeft').text("(Time Left : " + minutes + "m " + seconds + "s )");


                // If the count down is finished, write some text
                if (distance < 0) {
                    clearInterval(x);
                    $('#s3DownloadLocation').attr('href', "");
                    $('#s3DownloadKeyText').text("");
                    $('#spanTimeLeft').text("(Time Left : EXPIRED.  Please press download again.)");
                }
            }, 1000);

            var win = window.open(msg, '_blank');
            win.focus();
        },
        error: function (e) {
            console.log("Unavailable");
        }
    });
}