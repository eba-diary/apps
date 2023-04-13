data.DataSource = {

    FormInit: function (hrEmpUrl, hrEmpEnv) {
        //Set Secure HREmp service URL for associate picker
        $.assocSetup({ url: hrEmpUrl });
        var permissionFilter = "DatasetModify,DatasetManagement," + hrEmpEnv;
        $("#PrimaryContactName").assocAutocomplete({
            associateSelected: function (associate) {
                $('#PrimaryContactId').val(associate.Id);
            },
            filterPermission: permissionFilter,
            minLength: 0,
            maxResults: 10
        });
        if (!$("#SourceType").is("[type=hidden]")){
            $("#SourceType").materialSelect();
        }
        $("#AuthID").materialSelect();
        $("#GrantType").materialSelect();
        data.DataSource.UpdateShowAddTokenButton();

        $('#AddToken').click(function () {
            $("#AddToken").html('<span class="spinner-border spinner-border-sm mr-1" role="status" aria-hidden="true"></span>Loading...');
            $.get("/Config/AddToken", function (template) {
                $('#TokenContainer').append(template);
                $("#AddToken").html("Add Token");
                data.DataSource.UpdateShowAddTokenButton();
            });
        });

        $("#GrantType").change(function () {
            data.DataSource.UpdateShowAddTokenButton();
        });

        $('body').on('click', '.removeToken', function () {
            let tokenCard = $(this).parent().parent();
            if (tokenCard.find("input.tokenId").val()) {
                let tokenCardChildren = tokenCard.children();
                tokenCard.find("input.toDeleteMarker").val(true);
                tokenCardChildren.find("button.cancelDelete").removeClass("d-none");
                tokenCardChildren.find("button.removeToken").addClass("d-none");
            }
            else {
                tokenCard.remove();
            }
            data.DataSource.UpdateShowAddTokenButton();
        });

        $('body').on('click', '.cancelDelete', function () {
            let tokenCard = $(this).parent().parent();
            let tokenCardChildren = tokenCard.children();
            tokenCard.find("input.toDeleteMarker").val(false);
            tokenCardChildren.find("button.cancelDelete").addClass("d-none");
            tokenCardChildren.find("button.removeToken").removeClass("d-none");
            data.DataSource.UpdateShowAddTokenButton();
        });

        $('body').on('click', '.removeToken', function () {
            let tokenCard = $(this).parent().parent();
            if (tokenCard.children(".tokenId").val()) {
                tokenCard.children(".toDeleteMarker").val(true);

            }
            else {
                tokenCard.remove();
            }
            data.DataSource.UpdateShowAddTokenButton();
        });

        $('body').on('click', '.removeHeader', function () {
            $(this).parent().parent().remove();
        });

        $('body').on('click', '.removeAcceptableError', function () {
            $(this).parent().parent().remove();
        });

        $('#AddHeader').on('click', function () {
            //This approach is described at the site below
            // http://ivanz.com/2011/06/16/editing-variable-length-reorderable-collections-in-asp-net-mvc-part-1/
            $("#AddHeader").html('<span class="spinner-border spinner-border-sm mr-1" role="status" aria-hidden="true"></span>Loading...');
            $.get("/Config/HeaderEntryRow", function (template) {
                $('#RequestContainer').append(template);
                $("#AddHeader").html("Add");
            });
        });

        $('#AddAcceptableError').on('click', function () {
            $("#AddAcceptableError").html('<span class="spinner-border spinner-border-sm mr-1" role="status" aria-hidden="true"></span>Loading...');
            $.get("/Config/AddAcceptableError", function (template) {
                $('#AcceptableErrorContainer').append(template);
                $("#AddAcceptableError").html("Add");
            });
        });

        $(".toggleBackfill").on('click', function () {
            let tokenId = $(this).parent().parent().find("input.tokenId").val();
            $.post("/api/v20220609/datasource/RunMotiveBackfill/" + tokenId, null, function () {
                data.Dataset.makeToast("success", "Token backfill triggered successfully.");
                $(this).hide();
            }).fail(function (jqxhr, settings, ex) { data.Dataset.makeToast("error", "Token backfill error occurred.") });
        });
    },

    UpdateShowAddTokenButton: function () {

        if (data.DataSource.GetTokenFormCount() > 0 && $("#GrantType").val() == '0') {
            $("#AddToken").addClass("d-none");
        }
        else {
            $("#AddToken").removeClass("d-none");
        }
    },

    GetTokenFormCount: function() {
        return $("#TokenContainer .card").length
    },

    PopulateDataSources: function (in_val) {
        //var val = $('#SelectedSourceType :selected').val();
        $.ajax({
            url: "/Config/SourcesByType?sourceType=" + $('#SelectedSourceType :selected').val(),
            dataType: 'json',
            type: "GET",            
            success: function () {

                $("#SourceSelector").select2({
                multiple: true,
                allowClear: true,
                data: { sourceType: in_val },
                ajax: {
                    url: "/Config/SourcesByType?sourceType=" + $('#SelectedSourceType :selected').val(),
                    dataType: 'json',
                    dropdownPosition: 'below',
                    data: function (params) {
                        return {
                            query: params.term
                        };
                    },
                    processResults: function (data) {
                        return {
                            results: data, id: 'ItemId', text: 'ItemText'
                        };
                    },
                    cache: true
                    },
                    dropdownPosition: 'below',
                    templateResult: data.DataSource.formatSource,
                    escapeMarkup: function (markup) { return markup; },
                    templateSelection: data.DataSource.formatSourceSelection
                });

                if ($('#SourceIds').val()) {
                    $('#SourceSelector').val($("#SourceIds").val().split(","));
                    $('#SourceSelector').trigger('change');
                }
            },
            error: function (e) { }
        });

        $('#SourceSelector').on('select2:select', function (e) {
            $('#SourceIds').val($('#SourceSelector').val());
        });
        $('#SourceSelector').on('select2:unselect', function (e) {
            $('#SourceIds').val($('#SourceSelector').val());
        });
        $('#SourceSelector').on('select2:change', function (e) {
            $('#SourceIds').val($('#SourceSelector').val());
        });
    },

    formatSource: function (source) {
        if (source.loading) return source.Text;

        $('.select2-results__options li').each(function () {
            if ($(this).attr("aria-selected") === 'true') {
                $(this).hide();
            }
        });

        if (source.IsSecured) {
            return '<span class="s-tag" tooltip="Restricted Data Source"><i>' + source.Text + '</i></span>';
        }
        else {
            return '<span class="s-tag">' + source.Text + '</span>';
        }
    },

    formatSourceSelection: function (tag) {
        return tag.Name || tag.Text;
    }
}