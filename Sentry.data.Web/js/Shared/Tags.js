/******************************************************************************************
 * Javascript methods for the Tags
 ******************************************************************************************/

data.Tags = {

    initTags: function () {
        $.ajax({
            url: "/api/" + data.GetApiVersion() + "/tags",
            dataType: 'json',
            type: "GET",
            success: function (msg) {

                $("#TagSelector").select2({
                    multiple: true,
                    allowClear: true,
                    data: msg,
                    ajax: {
                        url: "/api/" + data.GetApiVersion() + "/tags",
                        dataType: 'json',
                        dropdownPosition: 'below',
                        data: function (params) {
                            return {
                                query: params.term
                            };
                        },
                        processResults: function (data) {
                            return {
                                results: data
                            };
                        },
                        cache: true
                    },
                    dropdownPosition: 'below',
                    templateResult: data.Tags.formatTag,
                    escapeMarkup: function (markup) { return markup; },
                    templateSelection: data.Tags.formatTagSelection
                });

                if ($('#TagIds').val()) {
                    $('#TagSelector').val($("#TagIds").val().split(","));
                    $('#TagSelector').trigger('change');
                }
            },
            error: function (e) { }
        });


        $('#TagSelector').on('select2:select', function (e) {
            $('#TagIds').val($('#TagSelector').val());
        });
        $('#TagSelector').on('select2:unselect', function (e) {
            $('#TagIds').val($('#TagSelector').val());
        });
        $('#TagSelector').on('select2:change', function (e) {
            $('#TagIds').val($('#TagSelector').val());
        });

        $('body').on('click', '#addNewTagBtn', function () {
            data.Tags.showModal();
            return false;
        });

        $('body').on('change', '#tagName', function () {

            $.ajax({
                type: "GET",
                url: "/api/" + data.GetApiVersion() + "/tags/" + $("#tagName").val(),
                dataType: "json",
                success: function (msg) {

                    if (msg === false) {
                        $("#tagName").css('border', '1px solid red');
                        $("#tagName").css('color', 'red');
                        $("#tagWarningText").show();
                        $("#tagWarningText").css('color', 'red');
                        $("#tagWarningText").text("This tag name is already taken.  If you would like to use it, press cancel and type it into the Tags Search box to the left of the Add New Tag button.");
                    }
                    else {
                        $("#tagName").css('border', '1px solid rgb(169, 169, 169)');
                        $("#tagName").css('color', 'black');
                        $("#tagWarningText").hide();
                    }
                },
                error: function (e) {
                }
            });

        });
    },

    CreateInit: function () {
        $("#SelectedTagGroup").materialSelect();

        $('body').on('change', '#TagName', function () {
            $.ajax({
                type: "GET",
                url: "/api/" + data.GetApiVersion() + "/tags/" + $("#TagName").val(),
                dataType: "json",
                success: function (msg) {

                    if (msg === false) {
                        $("#TagName").css('border', '1px solid red');
                        $("#TagName").css('color', 'red');
                        $("#tagWarningText").show();
                        $("#tagWarningText").css('color', 'red');
                        $("#tagWarningText").text("This tag name is already taken.  If you would like to use it, press cancel and type it into the Tags Search box to the left of the Add New Tag button.");
                    }
                    else {
                        $("#TagName").css('border', '1px solid rgb(169, 169, 169)');
                        $("#TagName").css('color', 'black');
                        $("#tagWarningText").hide();
                    }
                },
                error: function (e) {
                }
            });
        });
    },

    formatTag: function (tag) {
        if (tag.loading) return tag.text;

        $('.select2-results__options li').each(function () {
            if ($(this).attr("aria-selected") === 'true') {
                $(this).hide();
            }
        });

        if (tag.Description) {
            return '<span class="s-tag" tooltip="' + tag.Description + '"> ' + tag.Name + '</span><span class="item-multiplier">&nbsp; × &nbsp;' + tag.Count + '</span>';
        }
        else {
            return '<span class="s-tag">' + tag.Name + '</span><span class="item-multiplier">&nbsp; × &nbsp;' + tag.Count + '</span>';
        }
    },

    formatTagSelection: function (tag) {
        return tag.Name || tag.text;
    },

    showModal: function () {
        console.log("entered showModal");
        var modal = Sentry.ShowModalWithSpinner("Create New Tag");
        var url = "/BusinessIntelligence/CreateTag"

        $.get(url, function (e) {
            modal.ReplaceModalBody(e);

            $("[id^='CreateTagButton']").off('click').on('click', function (e) {
                e.preventDefault();
                //check validation
                var errors = "";                
                if ($("#Description").val() === undefined || $("#Description").val() === "") {
                    errors += "<div>Description is required</div>";
                }

                if (errors === "") {
                    $("#createTagSpinner").css('float', 'left');
                    Sentry.InjectSpinner($("#createTagSpinner"), 30);
                    $.ajax({
                        type: 'POST',
                        data: $("#TagForm").serialize(),
                        url: '/BusinessIntelligence/TagForm',
                        success: function (data) {
                            modal.ReplaceModalBody(data);
                        }
                    });
                } else {
                    $("#TagFormErrorBox").html(errors).show();
                }
            });

        });
        //    type: "GET",
        //    url: "/BusinessIntelligence/CreateTag",
        //    success: function (data) {
        //        modal.ReplaceModalBody(data);
        //    },
        //    error: function (e) { }
        //});

        //var string =
        //    "<div>" +
        //    "<h2>Create New Tag</h2>" +
        //    "<hr/>" +
        //    "<div class='row'>" +
        //    "<div class='col-sm-3'>" +
        //    "<label id='tagNameLabel' for='tagName'>Tag Name</label>" +
        //    "</div>" +
        //    "<div class='col-sm-9'>" +
        //    "<input id='tagName' style='width:100%; padding-left: 5px;' />" +
        //    "</div>" +
        //    "</div>" +
        //    "<br/>" +
        //    "<div class='row'>" +
        //    "<div class='col-sm-3'>" +
        //    "</div>" +
        //    "<div class='col-sm-9'>" +
        //    "<span id='tagWarningText' />" +
        //    "</div>" +
        //    "</div>" +
        //    "<br/>" +
        //    "<div class='row'>" +
        //    "<div class='col-sm-3'>" +
        //    "<label for='tagDescription'>Tag Description</label>" +
        //    "</div>" +
        //    "<div class='col-sm-9'>" +
        //    "<textarea id='tagDescription' style='width:100%;  padding-left: 5px; resize:vertical; max-width: 100%' />" +
        //    "</div>" +
        //    "</div>" +
        //    "</div>";
       
    }

};