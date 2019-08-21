/******************************************************************************************
 * Javascript methods for the Config-related pages
 ******************************************************************************************/

data.Config = {
    IndexInit: function () {
        $('body').on('click', '.configHeader', function () {

            if ($(this).children('.tracker-menu-icon').hasClass('glyphicon-menu-down')) {
                $(this).children('.tracker-menu-icon').switchClass('glyphicon-menu-down', 'glyphicon-menu-up');
            } else {
                $(this).children('.tracker-menu-icon').switchClass('glyphicon-menu-up', 'glyphicon-menu-down');
            }
            $(this).next('.configContainer').toggle();

            if ($(this).next('.configContainer:visible').length == 0) {
                // action when all are hidden
                $(this).css('border-radius', '5px 5px 5px 5px');
            } else {
                $(this).css('border-radius', '5px 5px 0px 0px');
            }
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

        $('body').on('click', '.jobHeader', function () {
            if ($(this).children('.tracker-menu-icon').hasClass('glyphicon-menu-down')) {
                $(this).children('.tracker-menu-icon').switchClass('glyphicon-menu-down', 'glyphicon-menu-up');
            } else {
                $(this).children('.tracker-menu-icon').switchClass('glyphicon-menu-up', 'glyphicon-menu-down');
            }
            $(this).next('.jobContainer').toggle();

            if ($(this).next('.jobContainer:visible').length == 0) {
                // action when all are hidden
                $(this).css('border-radius', '5px 5px 5px 5px');
            } else {
                $(this).css('border-radius', '5px 5px 0px 0px');
            }
        });

        $('body').on('click', '.on-demand-run', function () {
            var request = $.ajax({
                url: "/Dataset/RunRetrieverJob/?id=" + $(this).attr('id'),
                method: "POST",
                dataType: 'json',
                success: function (obj) {
                    var modal = Sentry.ShowModalConfirmation(
                        obj.Message, function () { })
                },
                failure: function (obj) {
                    var modal = Sentry.ShowModalAlert(
                        obj.Message, function () { })
                },
                error: function (obj) {
                    var modal = Sentry.ShowModalAlert(
                        obj.Message, function () { })
                }
            });

        });

        $(".schedule").each(function (index, element) {
            $(this).text("Next processing time will be " + prettyCron.getNext($(this).text()));
        });

        $('body').on('click', ".btnCreateDirectory", function () {

            $(this).parent().parent().children("#configs").children().children().children(".DFSBasic").children(".filePath").each(function (index, element) {
                console.log($(this).text());

                var text = $(this).text();
                text = text.replace("file:///", "");

                var request = $.ajax({
                    url: "/Dataset/CreateFilePath/?filePath=" + text,
                    method: "POST",
                    dataType: 'json',
                    success: function (obj) {
                    }
                });
            });
        });

        $('body').on('click', '#btnDeleteConfig', function () {
            var config = $(this);
            $.ajax({
                url: "/Config/" + $(this).attr("data-id"),
                method: "DELETE",
                dataType: 'json',
                success: function (obj) {
                    alert("Yay");
                    //var modal = Sentry.ShowModalConfirmation(
                    //    obj.Message, function () { })
                },
                failure: function (obj) {
                    alert("failure");
                    //var modal = Sentry.ShowModalAlert(
                    //    obj.Message, function () { })
                },
                error: function (obj) {
                    alert("error");
                    //var modal = Sentry.ShowModalAlert(
                    //    obj.Message, function () { })
                }
            });
        });
    },

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


