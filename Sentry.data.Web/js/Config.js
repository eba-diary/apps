/******************************************************************************************
 * Javascript methods for the Config-related pages
 ******************************************************************************************/

data.Config = {

    CreateInit: function () {

        $('#Delimiter').prop("readonly", "readonly");
        $('#HasHeader').prop("readonly", false);
        $('#HasHeader').prop("disabled", false);

        $("#FileExtensionID").change(function () {
            switch ($('#FileExtensionID option:selected').text()) {
                case "CSV":
                    $('#Delimiter').text(',');
                    $('#Delimiter').val(',');
                    $('#Delimiter').prop("readonly", "readonly");
                    $('#HasHeader').prop("readonly", false);
                    $('#HasHeader').prop("disabled", false);
                    break;
                case "DELIMITED":
                    $('#Delimiter').val('');
                    $('#Delimiter').prop("readonly", "");
                    $('#HasHeader').prop("readonly", false);
                    $('#HasHeader').prop("disabled", false);
                    break;
                default:
                    $('#Delimiter').val('');
                    $('#Delimiter').prop("readonly", "readonly");
                    $('#HasHeader').prop("readonly", true);
                    $('#HasHeader').prop("disabled", true);
                    break;
            }
        });

        //trigger sas-saslibrary section on checkbox change
        $("#IncludedInSAS").change(function () {
            if ($(this).is(":checked")) {
                $(".sas-saslibrary").show();
            }
            else {
                $(".sas-saslibrary").hide();
            }
        });

        //initialize sas-saslibary section on page load
        if ($("#IncludedInSAS").is(":checked")) {
            $(".sas-saslibrary").show();
        };
    },

    ExtensionInit: function() {
        $('#btnCreateExtensionMapping').click(function () {
            var data = $('#ExtensionForm').serialize();

            $.ajax({
                url: "/Config/Extension/Create",
                method: "POST",
                data: data,
                datatype: 'json',
                success: function (obj) {
                    var modal = Sentry.ShowModalAlert(
                        obj.Message, function () { location.reload() })
                },
                failure: function (obj) {
                    var modal = Sentry.ShowModalAlert(
                        obj.Message, function () { location.reload() })
                },
                error: function (obj) {
                    var modal = Sentry.ShowModalAlert(
                        obj.Message, function () { location.reload() })
                }
            });
        });

        $('body').on('click', '.removeMapping', function () {
            $(this).parent().parent().remove();
        });
    }
}


