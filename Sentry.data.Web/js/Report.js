/******************************************************************************************
 * Javascript methods for the Asset-related pages
 ******************************************************************************************/

data.Report = {

    IndexInit: function () {
        /// Initialize the Index page for data assets (with the categories)

        $("[id^='CreateDataset']").off('click').on('click', function (e) {
            e.preventDefault();
            window.location = data.Report.CreateLink();
        });

        $('.tile').click(function (e) {
            var storedNames = JSON.parse(localStorage.getItem("filteredIds"));

            if (storedNames !== null) {
                for (i = 0; i < storedNames.length; i++) {
                    $('#' + storedNames[i]).prop('checked', false);
                }

                localStorage.removeItem("filteredIds");
            }

            localStorage.removeItem("searchText");
        });

        $('#DatasetSearch').submit(function (e) {
            localStorage.removeItem("filteredIds");

            localStorage.removeItem("searchText");

            localStorage.setItem("searchText", $('#SearchText')[0].value);
        });

        $('.input-group-addon').click(function (e) {
            $('#DatasetSearch').submit();
        });
    },

    FormInit: function (hrEmpUrl, hrEmpEnv) {

        $("#DatasetCategoryIds").select2({
            placeholder:"Select Categories"
        });
        $("#DatasetBusinessUnitIds").select2({
            placeholder: "Select Business Units"
        });
        $("#DatasetFunctionIds").select2({
            placeholder: "Select Functions"
        });
        $("#DatasetFileTypeId").select2({
            placeholder: "Select Exhibit Type",
            allowClear: true
        });
        $("#FileTypeId").select2({
            placeholder: "Select Exhibit Type",
            allowClear: true,
            minimumResultsForSearch: 10
        });
        $("#FrequencyId").select2({
            placeholder: "Select Frequency",
            allowClear: true,
            minimumResultsForSearch: 10
        });

        /// Initialize the Create Exhibit view
        //Set Secure HREmp service URL for associate picker
        $.assocSetup({ url: hrEmpUrl });
        var permissionFilter = "ReportModify,DatasetManagement," + hrEmpEnv;
        $("#PrimaryOwnerName").assocAutocomplete({
            associateSelected: function (associate) {
                $('#PrimaryOwnerId').val(associate.Id);
            },
            minLength: 0,
            maxResults: 10
        });
        $("#PrimaryContactName").assocAutocomplete({
            associateSelected: function (associate) {
                $('#PrimaryContactId').val(associate.Id);
            },
            minLength: 0,
            maxResults: 10
        });

        //determine the cancel button url
        $("[id^='CancelButton']").off('click').on('click', function (e) {
            e.preventDefault();
            window.location = data.Report.CancelLink($(this).data("id"));
        });

        //Initialize BO Get Latest option when FileTypeID is already pouplated on load
        data.Report.SetGetLatestPanel($("#FileTypeId").val());

        //Show\Hide BO Get Latest option based on FileTypeId selection
        $("#FileTypeId").change(function () {
            data.Report.SetGetLatestPanel($(this).val());
        });

        

        data.Tags.initTags();
    },

    DetailInit: function () {

        // Initialize the dataset detail page

        $(".detailNameLink").click(function () {
            var artifactLink = $(this).data('artifactlink');
            var locationType = $(this).data('locationtype');
            if (locationType === 'file') {
                artifactLink = encodeURI(artifactLink);
            }
            data.Report.OpenReport(locationType, artifactLink);
        });

        $("[id^='EditDataset_']").off('click').on('click', function (e) {
            e.preventDefault();
            window.location = "/BusinessIntelligence/Edit/?" + "id=" + encodeURI($(this).data("id"));
        });

        $('#deleteLink').click(function (e) {
            e.preventDefault();

            var modal = Sentry.ShowModalConfirmation("Delete Exhibit", function () { data.BusinessIntelligenceDetail.DeleteDataset($(this).data("id")); });
            modal.ReplaceModalBody("This will <u>permanently</u> delete this Exhibit (<b>not the object which it references</b>). </br></br> Do you wish to continue?");
            modal.show();
        });

        $(document).on("click", "[id^='btnFavorite']", function (e) {
            e.preventDefault();

            var icon = $(this).children();
            $.ajax({
                url: '/Dataset/SetFavorite?datasetId=' + encodeURIComponent($(this).data("id")),
                method: "GET",
                dataType: 'json',
                success: function () { icon.toggleClass("glyphicon-star glyphicon-star-empty"); },
                error: function () { Sentry.ShowModalAlert("Failed to toggle favorite.");}
            });
            
        });

    },

    DeleteDataset: function (id) {

        var returnUrl = "/BusinessIntelligence/Index";
        $.ajax({
            url: "/BusinessIntelligence/Delete/" + encodeURI(id),
            method: "POST",
            dataType: 'json',
            success: function (obj) {
                if (obj.Success) {
                    Sentry.ShowModalConfirmation(obj.Message, function () { window.location = returnUrl; });
                } else {
                    Sentry.ShowModalAlert(obj.Message, function () { window.location = returnUrl; });
                }
            },
            failure: function (obj) {
                Sentry.ShowModalAlert(obj.Message, function () { location.reload(); });
            },
            error: function (obj) {
                Sentry.ShowModalAlert(obj.Message, function () { location.reload(); });
            }
        });

    },

    CreateLink: function () {
        /// link to create report
        return "/BusinessIntelligence/Create";
    },

    CancelLink: function (id) {
        if (id === undefined || id === 0) {
            return "/BusinessIntelligence/Index";
        } else {
            return "/BusinessIntelligence/Detail/" + encodeURIComponent(id);
        }
    },

    OpenReport: function (artifactType, artifactPath) {

        // check what type we're working with
        if (artifactType === 'file') {

            // check if the user has permission and if so, download the file
            $.ajax({
                url: '/ExternalFile/HasReadPermissions?pathAndFilename=' + artifactPath,
                method: "GET",
                dataType: 'json',
                success: function (obj) {
                    if (obj.HasPermission) {
                        var url = '/ExternalFile/DownloadExternalFile?pathAndFilename=' + artifactPath;
                        window.open(url);
                    } else {
                        Sentry.ShowModalAlert("User does not have sufficient permissions to selected file.");
                    }
                },
                error: function (obj) {
                    Sentry.ShowModalAlert("Failed permissions check, please try again. If problem persists, please contact <a mailto:DSCSupport@sentry.com></a>");
                }
            });

        } else {
            var win = window.open(artifactPath, '_blank');
            if (win) {
                //Browser has allowed it to be opened
                win.focus();
            } else {
                //Browser has blocked it
                alert('Please allow popups for this website');
            }
        }
    },

    SetGetLatestPanel: function (fileType) {
        // check for the BusinessObjects file type; value is stored in a hidden field on BusinessIntelligenceForm.cshtml
        if (fileType === $("#hidBizObjEnumVal").val()) {
            $('#BOGetLatestPanel').removeClass("hidden");
        }
        else {
            $('#BOGetLatestPanel').addClass("hidden");
        }
    }    
};