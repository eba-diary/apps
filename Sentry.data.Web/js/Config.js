/******************************************************************************************
 * Javascript methods for the Config-related pages
 ******************************************************************************************/

data.Config = {

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


