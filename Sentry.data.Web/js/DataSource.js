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
            $(this).parent().parent().remove();
            data.DataSource.UpdateShowAddTokenButton();
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