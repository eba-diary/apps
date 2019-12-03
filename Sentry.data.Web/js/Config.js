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

        $('body').on('click', "#btnSyncSchema", function () {
            var syncBtn = $(this);
            var datasetId = syncBtn.attr("data-datasetId");
            var schemaId = syncBtn.attr("data-schemaId");

            var request = $.ajax({
                url: "/api/v2/metadata/dataset/0/schema/" + schemaId + "/syncconsumptionlayer",
                method: "POST",
                dataType: 'json',
                success: function (obj) {
                    Sentry.ShowModalAlert(
                        obj, function () { })
                },
                failure: function (obj) {
                    Sentry.ShowModalAlert(
                        "Failed to submit request", function () { })
                },
                error: function (obj) {
                    var msg;
                    if(obj.status === 400) {
                        msg = obj.responseJSON.Message;
                    }
                    else {
                        msg = "Failed to submit request";
                    };
                    Sentry.ShowModalAlert(
                        msg, function () { })
                }
            });
        });

        //Disable delete functionality if only 1 Schema exists
        if ($('.configHeader').length === 1) {
            $('#btnDeleteConfig').removeClass("btn-danger");
            $('#btnDeleteConfig').attr("disabled", "disabled");
            $('#btnDeleteConfig').attr('title', 'Dataset must contain at least one schema');
        }
        else {
            $('body').on('click', '#btnDeleteConfig', function () {
                var config = $(this);
                var ConfirmMessage = "<p>Are you sure</p><p><h3><b><font color=\"red\">THIS IS NOT A REVERSIBLE ACTION!</font></b></h3></p> </br>Deleting the schema will remove all associated data files and hive consumption layers.  </br>If at a later point " +
                    "this needs to be recreated, all files will need to be resent from source."

                Sentry.ShowModalCustom("Delete Schema", ConfirmMessage, {
                    Confirm: {
                        label: "Confirm",
                        className: "btn-danger",
                        callback: function () {
                            $.ajax({
                                url: "/Config/" + config.attr("data-id"),
                                method: "DELETE",
                                dataType: 'json',
                                success: function (obj) {
                                    Sentry.ShowModalAlert(obj.Message, function () {
                                        location.reload();
                                    })
                                },
                                failure: function (obj) {
                                    alert("failure");
                                    Sentry.ShowModalAlert(
                                        obj.Message, function () { })
                                },
                                error: function (obj) {
                                    Sentry.ShowModalAlert(
                                        obj.Message, function () { })
                                }
                            });
                        }
                    }
                });

                //Sentry.ShowModalConfirmation(ConfirmMessage, function () {
                //    $.ajax({
                //        url: "/Config/" + config.attr("data-id"),
                //        method: "DELETE",
                //        dataType: 'json',
                //        success: function (obj) {
                //            Sentry.ShowModalAlert(obj.Message, function () {
                //                location.reload();
                //            })
                //        },
                //        failure: function (obj) {
                //            alert("failure");
                //            Sentry.ShowModalAlert(
                //                obj.Message, function () { })
                //        },
                //        error: function (obj) {
                //            Sentry.ShowModalAlert(
                //                obj.Message, function () { })
                //        }
                //    });
                //})
            });
        }   
    },

    CreateInit: function () {

        data.Config.SetFileExtensionProperites($('#FileExtensionID option:selected').text());

        $("#FileExtensionID").change(function () {
            data.Config.SetFileExtensionProperites($('#FileExtensionID option:selected').text());
        });

        $("#IncludedInSAS").click(function () {
            if ($('#ConfigId').val() != "0" && $(this).is(':unchecked')) {
                Sentry.ShowModalAlert("\"Add to SAS\" option has been unchecked.  <p>This will remove all associated SAS libraries for this schema, if saved.</p>");
            }
        });

        $("#CreateCurrentView").click(function () {
            if ($('#ConfigId').val() != "0" && $(this).is(':unchecked') && $("#IncludedInSAS").is(':checked')) {
                Sentry.ShowModalAlert("\"Current View\" option has been unchecked.  <p>This will remove the current view SAS library associated with this schema, if saved.</p>");
            }
        });
    },

    EditInit: function () {
        $("#EditConfigForm").validateBootstrap(true);

        data.Config.SetFileExtensionProperites($('#FileExtensionID option:selected').text(), $('#ConfigId').val() !== "0");

        $("#FileExtensionID").change(function () {
            data.Config.SetFileExtensionProperites($('#FileExtensionID option:selected').text(), $('#ConfigId').val() !== "0");
        });
    },

    SetFileExtensionProperites: function (extension, editMode) {
        switch (extension) {
            case "CSV":
                $('.delimiter').show();
                $('.delimiterDescription').hide();
                $('#Delimiter').prop("readonly", "readonly");
                $('#HasHeader').prop("readonly", false);
                $('#HasHeader').prop("disabled", false);
                if (!editMode) {
                    $('#Delimiter').text(',');
                    $('#Delimiter').val(',');
                }
                break;
            case "DELIMITED":
            case "ANY":
                $('.delimiter').show();
                $('.delimiterDescription').show();
                if (!editMode) {
                    $('#Delimiter').val('');
                }
                $('#Delimiter').prop("readonly", "");
                $('#HasHeader').prop("readonly", false);
                $('#HasHeader').prop("disabled", false);
                break;
            default:
                $('.delimiter').hide();
                if (!editMode) {
                    $('#Delimiter').val('');
                }
                $('#Delimiter').prop("readonly", "readonly");
                $('#HasHeader').prop("readonly", true);
                $('#HasHeader').prop("disabled", true);
                break;
        }
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


