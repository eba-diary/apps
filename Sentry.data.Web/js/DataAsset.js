/******************************************************************************************
 * Javascript methods for the DataAsset-related pages
 ******************************************************************************************/

data.DataAsset = {

    ViewMore: function (name) {
        var button = ".more_" + name;
        var newClass = ".hidden_" + name;

        $(newClass).slideToggle();

        $(button).each(function () {
            if ($(this).text() === "View Less") {
                $(this).text("View More");
            }
            else {
                $(this).text("View Less");
            }
        });
    },

    ViewLowerLevel: function (name) {
        var arrow = ".layerArrow_" + name;
        var newClass = ".hiddenLayer_" + name;

        $(newClass).slideToggle();

        $(arrow).toggleClass("fa-chevron-down fa-chevron-up");
    }
}