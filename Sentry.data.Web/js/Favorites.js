﻿/******************************************************************************************
 * Javascript methods for Favorites
 ******************************************************************************************/

data.Favorites = {

    Init: function () {

        $(function () {

            // initialize the delete favorite click event
            data.Favorites.InitDeleteClick();

            // makes the <ul> be draggable/droppable; stop event is when you stop dragging the <li>
            $("#sortable-favorites").sortable({
                stop: function (event, ui) {
                    data.Favorites.SetFavoritesOrder();
                }
            });

            $("#btnDeleteFav").click(function () {
                data.Favorites.DeleteFavorite();
            });

            // when the modal window closes, clear out the value in the hidden field that holds the Id of the Favorite to delete
            $("#fav-delete-confirmation").on("hidden.bs.modal", function () {
                $("#hidDeleteFavoriteId").val("");
            });
        });

    },

    InitDeleteClick: function () {

        $(".fav-delete").click(function () {
            // capture the Id of the Favorite to delete
            $("#hidDeleteFavoriteId").val("favId="$(this).data("favid") + "&isLegacyFavorite=" + $(this).data("legacy"));
        });

    },

    SetFavoritesOrder: function () {

        var favItems = [];
        var i = 1;

        // loop through each of the <li> elements in the favorites <ul>
        $("#sortable-favorites li").each(function () {
            var isLegacy = $(this).data("legacy");
            var id = $(this).data("favid");

            var kvp = {};
            kvp[id] = isLegacy;

            favItems.push(kvp);

            // set the html of the <span> that is displaying the order of the Favorites
            $(this).find(".fav-sort-order").html(i.toString());

            // increment i
            i += 1;
        });

        data.Favorites.HideFavorites();

        console.log(favItems);

        // ajax form post
        $.ajax({
            type: 'POST',
            data: JSON.stringify({ orderedFavoriteIds: favItems }),
            url: '/Favorites/Sort',
            success: function (data) {
                // do nothing; screen shows the new order, user does not need to see any confirmation
            },
            error: function (data) {
                $("#favorites-wrapper").html("<br /><br /><div class='alert alert-danger'><strong>Error!</strong> There was a problem re-ordering Favorites.</div>");
            },
            complete: function () {
                data.Favorites.ShowFavorites();
            }
        });
    },

    DeleteFavorite: function () {

        //CADEN - is legacy indicator will get here somehow and will need to send that as a parameter to the delete controller action

        // close the modal window
        $("#fav-delete-confirmation").modal("hide");

        // show a spinner in place of the sortable <ul>
        data.Favorites.HideFavorites();

        // ajax call to delete the favorite and re-populate the list of Favorites
        $.ajax({
            url: '/Favorites/Delete?' + $("#hidDeleteFavoriteId").val(),
            method: 'GET',
            dataType: 'html',
            success: function (html) {
                $("#sortable-favorites").html(html);

                // show the sortable <ul> and not the spinner
                data.Favorites.ShowFavorites();
            },
            error: function () {
                $("#favorites-wrapper").html("<br /><br /><div class='alert alert-danger'><strong>Error!</strong> There was a problem attempting to delete the Favorite.</div>");

                data.Favorites.ShowFavorites();
            },
            complete: function () {
                // make sure the delete Favorite click event is wired up
                data.Favorites.InitDeleteClick();
            }
        });

    },

    ShowFavorites: function () {

        // show the sortable <ul> and not the spinner
        $("#favorites-wrapper").removeClass("hidden");
        $("#spinner-wrapper").addClass("hidden");
    },

    HideFavorites: function () {

        // show the sortable <ul> and not the spinner
        $("#favorites-wrapper").addClass("hidden");
        $("#spinner-wrapper").removeClass("hidden");
    }
};