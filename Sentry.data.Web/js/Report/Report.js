﻿/******************************************************************************************
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

    FormInit: function () {

        $("#DatasetCategoryIds").select2({
            placeholder:"Select Categories"
        });

        /// Initialize the Create Dataset view
        //Set Secure HREmp service URL for associate picker
        $.assocSetup({ url: "https://hrempsecure.sentry.com/api/associates" });

        var picker = $("#SentryOwnerName");

        picker.assocAutocomplete({
            associateSelected: function (associate) {
                $('#SentryOwnerId').val(associate.Id);
            }
        });

        //determine the cancel button url
        $("[id^='CancelButton']").off('click').on('click', function (e) {
            e.preventDefault();
            window.location = data.Report.CancelLink($(this).data("id"));
        });

        data.Tags.initTags();
    },

    DetailInit: function () {
        /// Initialize the dataset detail page for data assets
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
    }

    
};

