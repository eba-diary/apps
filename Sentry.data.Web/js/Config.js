/******************************************************************************************
 * Javascript methods for the Config-related pages
 ******************************************************************************************/

data.Config = {
    IndexInit: function () {

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

        $('body').on('click', '.tracker-menu-icon', function () {
            if ($(this).hasClass('fa-chevron-down')) {
                $(this).switchClass('fa-chevron-down', 'fa-chevron-up');
            } else {
                $(this).switchClass('fa-chevron-up', 'fa-chevron-down');
            }
            var parentElement = $(this).parent().parent();
            var detailContainer = parentElement.next('div');
            detailContainer.toggle();

            if (detailContainer.is(":visible")) {
                // action when all are hidden
                parentElement.css('border-radius', '5px 5px 0px 0px');
            } else {
                parentElement.css('border-radius', '5px 5px 5px 5px');
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

            var warningMsg = `<p><b><h3><font color=\"red\">WARNING</font color></h3></b></p><p>Performing this action will re-generate hive consumption layer table and views for current schema.</p>
            <p>In addition, this will generate notification to SAS Administration to refresh associated metadata.  Depending on schema change, this
            may break SAS processes referencing this library.</p>`;

            Sentry.ShowModalConfirmation(warningMsg, function () {
                $.ajax({
                    url: "/api/v2/metadata/dataset/" + datasetId + "/schema/" + schemaId + "/syncconsumptionlayer",
                    method: "POST",
                    dataType: 'json',
                    success: function (obj) {
                        Sentry.ShowModalAlert(
                            obj, function () { });
                    },
                    failure: function () {
                        Sentry.ShowModalAlert(
                            "Failed to submit request", function () { });
                    },
                    error: function (obj) {
                        var msg;
                        if (obj.status === 400) {
                            msg = obj.responseJSON.Message;
                        }
                        else {
                            msg = "Failed to submit request";
                        };
                        Sentry.ShowModalAlert(
                            msg, function () { });
                    }
                });
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

                Sentry.ShowModalCustom("Delete Schema", ConfirmMessage, 
                    '<button type="button" id="SchemeDeleteModalBtn" data-dismiss="modal" class="btn btn-danger waves-effect waves-light">Delete</button>'
                );

                $("#SchemeDeleteModalBtn").click(function () {
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

    CreateInit: function (PageSubmitFunction, PageCancelFunction) {

        $("[id='SubmitDatasetFileConfigForm']").click(PageSubmitFunction);

        $("[id^='CancelDatasetFileConfigForm']").off('click').on('click', PageCancelFunction);

        //Call SetFileExtensionProperites method on initialization, then call on change of FileExtensionId
        data.Config.SetFileExtensionProperites($('#FileExtensionID option:selected').text(), false);
        $("#FileExtensionID").change(function () {
            data.Config.SetFileExtensionProperites($('#FileExtensionID option:selected').text(), true);
        });

        data.Config.DatasetScopeTypeInit($("#DatasetScopeTypeID"));

        $("#FileType").materialSelect();
        $("#DatasetScopeTypeID").materialSelect();
        $("#FileExtensionID").materialSelect();
    },

    DatasetScopeTypeInit: function (element) {
        element.change(function () {
            var selection = $(this).val()
            data.Config.SetDatasetScopeTypeDescription(selection);
        })

        data.Config.SetDatasetScopeTypeDescription(element.val());
    },

    SetDatasetScopeTypeDescription: function (id) {
        if (id === undefined || id == null) {
            $(".DatasetScopeTypeDescription span").text("");
        }
        else {
            console.log(id);
            $.ajax({
                type: "GET",
                url: '/Config/GetDataScopeTypeDescription/' + encodeURIComponent(id),
                contentType: 'application/json',
                success: function (returnedData) {
                    $(".DatasetScopeTypeDescription span").text(returnedData.Description).html();
                },
                error: function () {
                    $(".DatasetScopeTypeDescription span").text("");
                }
            });
        }
    },

    CreateFormSubmitInit: function (e) {
        if (e.preventDefault) {
            e.preventDefault();
        }

        $.ajax({
            url: "/Config/DatasetFileConfigForm",
            method: "POST",
            data: $("#DatasetFileConfigForm").serialize(),
            dataType: 'json',
            success: function (obj) {
                if (Sentry.WasAjaxSuccessful(obj)) {
                    //redirect to dataset detail page
                    window.location.href = "/Dataset/Detail/" + encodeURIComponent(obj.dataset_id);
                }
                else {
                    $('#DatasetFileConfigForm').replaceWith(obj);
                    data.Config.CreateInit(data.Config.CreateFormSubmit, data.Config.CreateFormCancel);
                }
            },
            failure: function () {
                alert('An error occured submiting your request.  Please try again.');
            },
            error: function (obj) {
                $('#DatasetFileConfigForm').replaceWith(obj.responseText);
                data.Config.CreateInit(data.Config.CreateFormSubmitInit, data.Config.CreateFormCancelInit);
            }
        });
    },

    CreateFormCancelInit: function (e) {
        if (e.preventDefault) {
            e.preventDefault();
        }

        window.location = data.Config.CancelLink($(this).data("id"));
    },

    CancelLink: function (id) {
        if (id === undefined || id === 0) {
            return "/Dataset/Index";
        } else {
            return "/Config/Dataset/" + encodeURIComponent(id);
        }
    },

    EditInit: function () {
        $("#EditConfigForm").validateBootstrap(true);

        data.Config.SetFileExtensionProperites($('#FileExtensionID option:selected').text(), false);

        $("#FileExtensionID").change(function () {
            data.Config.SetFileExtensionProperites($('#FileExtensionID option:selected').text(), true);
        });
    },

    SetFileExtensionProperites: function (extension, clearDelimiter) {
        //Determine which container to find the delimiter field within
        // and set delimiterelement appropriately
        var delimiterelement;

        if ($("#DatasetFormContainer").css('display') === undefined) {
            //User is on the Edit.cshtml page, so look for the delimiter element
            delimiterelement = $('#Delimiter');
        }
        else if ($("#DatasetFormContainer").css('display') === 'none') {
            //User is on the _DatasetFileConfigCreate.cshtml page and creating a schema
            delimiterelement = $("#DatasetFileConfigFormContainer").find('#Delimiter');
        }
        else {
            //User is on the _DatasetFileConfigCreate.cshtml page and creating a dataset
            delimiterelement = $("#DatasetFormContainer").find('#Delimiter');
        }

        //clear delimiter on change event so that it isn't saved to schema with a non-delimiting type
        if (clearDelimiter) {
            delimiterelement.val('');
        }

        switch (extension) {
            case "CSV":
                $('.delimiterPanel').show();
                $('#HasHeader').prop("readonly", false);
                $('#HasHeader').prop("disabled", false);
                delimiterelement.prop("readonly", "readonly");

                //CSV is always ',' so set value anyway
                delimiterelement.val(',');
                break;
            case "DELIMITED":
            case "ANY":
                $('.delimiterPanel').show();
                delimiterelement.prop("readonly", "");
                $('#HasHeader').prop("readonly", false);
                $('#HasHeader').prop("disabled", false);
                break;
            default:
                $('.delimiterPanel').hide();
                delimiterelement.val('');
                delimiterelement.prop("readonly", "readonly");
                $('#HasHeader').prop("readonly", true);
                $('#HasHeader').prop("disabled", true);
                break;
        }

        data.Config.setSchemaRootPathPanel(extension);
    },

    setSchemaRootPathPanel: function (extension) {
        switch (extension) {
            case "XML":
            case "JSON":
                $('.schemaRootPathPanel').show();
                break;
            default:
                $('.schemaRootPathPanel').hide();
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


