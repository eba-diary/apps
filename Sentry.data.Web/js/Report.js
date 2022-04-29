/******************************************************************************************
 * Javascript methods for the Business Intelligence-related pages
 ******************************************************************************************/

data.Report = {

    IndexInit: function () {
        /// Initialize the Index page for business intelligence

        $(".sentry-app-version").hide();

        $("[id^='CreateExhibit']").off('click').on('click', function (e) {
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

        // Initialize Images for thumbnails
        data.Images.InitImages();

        $("#DatasetCategoryIds").materialSelect(); //multi
        $("#DatasetBusinessUnitIds").materialSelect(); //multi 
        $("#DatasetFunctionIds").materialSelect(); //multi
        $("#DatasetFileTypeId").materialSelect();
        $("#FileTypeId").materialSelect();
        $("#FrequencyId").materialSelect();

        $("#ContactSearch").attr("placeholder", "Add Associate by name or ID");

        /// Initialize the Create Exhibit view
        //Set Secure HREmp service URL for associate picker
        $.assocSetup({ url: hrEmpUrl });
        var picker = $('#ContactSearch').assocAutocomplete({
            associateSelected: function (associate) {
                
                //Check if user is already selected
                //Generate jquery for Hidden field
                var contactInput = "[name^=ContactIds][value=" + associate.Id + "]"
                //Generate jquery for contacts-block
                var contactBlock = "#" + associate.Id + ".contacts-block"

                if ($(contactInput).length == 0 && $(contactBlock).length == 0) {
                    jQuery('<div class="contacts-block" id="' + associate.Id + '"><span class="contactinfo-remove">x</span>' + associate.LastName + ',' + associate.FirstName + '</div>').appendTo('.contactList');
                    //add hidden value for post back
                    var inputs = $("[id^=ContactIds_]").length;

                    jQuery('<input/>', {
                        id: 'ContactIds_' + inputs + '_',
                        name: 'ContactIds[' + inputs + ']',
                        type: 'hidden',
                        value: associate.Id
                    }).appendTo('#informationPanel');
                }
            },
            close: function () {
                picker.clear();
            },
            minLength: 0,
            maxResults: 10
        });

        $(".associatePicker label").addClass("active");

        $(document).on('click', '.contactinfo-remove', function (e) {
            //e.StopPropagation();
            //e.StopImmediatePropagation();
            e.preventDefault();
            //get parent div
            var parent = $(this).parent()[0];
            var blockId = parent.id;
            //Get user ID
            var userId = blockId.replace('contact_', '');
            //Generate jquery for userId
            var contactInput = "[name^=ContactIds][value=" + userId + "]"
            //Generate jquery for contacts-block
            var contactBlock = "#" + blockId + ".contacts-block"

            //Remove associated ContactId hidden input
            $(contactInput).remove();

            //Remove associated contacts-block element
            $(contactBlock).remove();
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
        data.Images.InitImageUpload();
        data.Images.InitImages();
    },

    DetailInit: function () {

        // Initialize the dataset detail page

        // Initialize Images for thumbnails
        data.Images.InitImages();

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
            var d_id = $(this).data("id");

            $.ajax({
                url: '/BusinessIntelligence/' + d_id + '/Favorites',
                method: "GET",
                dataType: 'json',
                success: function (obj) {
                    var message;
                    var objData = JSON.parse(obj);
                    var favCount = objData.Favorites.length;
                    if (favCount === 1) {
                        message = "This exhibit is favorited by 1 user.  Click <a href=" + objData.MailToAllLink + ">here</a> to email the user before deleting. If you choose to continue, the exhibit link will be permanently deleted from data.sentry.com.</br></br>Do you wish to continue?"
                    }
                    else if (favCount > 1) {
                        message = "This exhibit is favorited by " + objData.Favorites.length + " users.  Click <a href=" + objData.MailToAllLink + ">here</a> to email the users before deleting. If you choose to continue, the exhibit link will be permanently deleted from data.sentry.com.</br></br>Do you wish to continue?"
                    }
                    else {
                        message = "This will permanently delete the exhibit link from data.sentry.com.</br></br>Do you wish to continue?"
                    }

                    //var model = Sentry.ShowModalCustom("Delete Exhibit", message, Sentry.ModalButtonsOKCancel(function () { data.Report.DeleteDataset(d_id); }));
                    var modal = Sentry.ShowModalCustom("Delete Exhibit", message, { Confirm: { label: "Confirm", className: "btn-danger", callback: function () { data.Report.DeleteDataset(d_id); } } } );
                    //'<button type="button" data-dismiss="modal" class="btn btn-success ok-button">Success!</button>'
                    modal.show();
                },
                failure: function () {
                    Sentry.ShowModalAlert("We failed to delete exhibit.  Please try again later.", function () { location.reload(); });
                },
                error: function () {
                    Sentry.ShowModalAlert("We failed to delete exhibit.  Please try again later.", function () { location.reload(); });
                }
            });            
        });

        $(document).on("click", "[id^='btnFavorite']", function (e) {
            e.preventDefault();

            var icon = $(this).children();
            $.ajax({
                url: '/Favorites/SetFavorite?datasetId=' + encodeURIComponent($(this).data("id")),
                method: "GET",
                dataType: 'json',
                success: function () { icon.toggleClass("fas far"); },
                error: function () { Sentry.ShowModalAlert("Failed to toggle favorite.");}
            });
            
        });

        data.Report.SetReturntoSearchUrl();
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
                    Sentry.ShowModalAlert(obj.Message, function () { location.reload(); });
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
    },

    SetReturntoSearchUrl: function () {

        var returnUrl = "/Search/BusinessIntelligence";
        var returnLink = $('#linkReturnToBusinessIntelligenceList');
        var firstParam = true;

        //---is this neede?
        if (localStorage.getItem("searchText") !== null) {
            var text = { searchPhrase: localStorage.getItem("searchText") };

            if (firstParam) { returnUrl += "?"; firstParam = false; } else { returnUrl += "&"; }

            returnUrl += $.param(text);
            
        }

        if (localStorage.getItem("filteredIds") !== null) {
            storedNames = JSON.parse(localStorage.getItem("filteredIds"));

            if (firstParam) { returnUrl += "?"; firstParam = false; } else { returnUrl += "&"; }

            returnUrl += "ids=";

            for (i = 0; i < storedNames.length; i++) {
                returnUrl += storedNames[i] + ',';
            }
            returnUrl = returnUrl.replace(/,\s*$/, "");
        }

        if (localStorage.getItem("pageSelection") !== null) {

            if (firstParam) { returnUrl += "?"; firstParam = false; } else { returnUrl += "&"; }

            returnUrl += "page=" + localStorage.getItem("pageSelection");
        }

        if (localStorage.getItem("sortByVal") !== null) {
            if (firstParam) { returnUrl += "?"; firstParam = false; } else { returnUrl += "&"; }

            returnUrl += "sort=" + localStorage.getItem("sortByVal");
        }

        if (localStorage.getItem("itemsToShow") !== null) {
            if (firstParam) { returnUrl += "?"; firstParam = false; } else { returnUrl += "&"; }            

            returnUrl += "itemsToShow=" + localStorage.getItem("itemsToShow");
        }

        returnLink.attr('href', returnUrl);
    }
};