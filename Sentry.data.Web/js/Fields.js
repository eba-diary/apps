data.Fields = {
    Init: function () {
        
    },

    SetFavoritesOrder: function () {

        var favItems = new Array();
        var i = 1;

        // loop through each of the <li> elements in the favorites <ul>
        $("#sortable-favorites li").each(function () {
            // add the Id of the Favorite to the array
            favItems.push($(this).data("favid"));

            // set the html of the <span> that is displaying the order of the Favorites
            $(this).find(".fav-sort-order").html(i.toString());

            // increment i
            i += 1;
        });

        // place the list of ordered Favorite Ids into the hidden form field
        $("#hidFavoriteIds").val(favItems);

        // save the new order
        data.Favorites.SaveOrder();
    }
}