/******************************************************************************************
 * Javascript methods for the Search-related pages
 ******************************************************************************************/

data.Search = {

    Init: function (searchParms) {
        /// <summary>
        /// Initialization function run at first load
        /// </summary>
        /// <param name="searchParms">Should include the parameters from the model:
        /// - SearchText
        /// - SearchState
        /// - SearchCategory
        /// - SearchPage
        /// </param>
        searchParms.SearchPage = 1;
        data.Search.LoadResults(searchParms);

        $("#PagerPrevious").on("click", function (e) {
            e.preventDefault();
            if (searchParms.SearchPage <= 1) { return false; }
            searchParms.SearchPage = searchParms.SearchPage - 1;
            data.Search.LoadResults(searchParms);
            return false;
        });

        $("#PagerNext").on("click", function (e) {
            e.preventDefault();
            if ($("#EndRecordNumber").text() == $("#TotalRecordCount").text()) { return false; }
            searchParms.SearchPage = searchParms.SearchPage + 1;
            data.Search.LoadResults(searchParms);
            return false;
        });
    },

    LoadResults: function (searchParms) {
        /// <summary>
        /// Asynchronously load a page of search results into the #SearchResults div
        /// </summary>
        /// <param name="searchParms">
        /// The searchParms object should include the following properties:
        /// - SearchText
        /// - SearchState
        /// - SearchCategory
        /// - SearchPage
        /// </param>
        Sentry.InjectSpinner($("#SearchResults"), 200);
        $.get("/Search/ResultList/" + searchParms.SearchText + "?SearchState=" + searchParms.SearchState + "&SearchCategory=" + searchParms.SearchCategory + "&SearchPage=" + searchParms.SearchPage, function (data) {
            if (searchParms.SearchPage <= 1) {
                $(".previous").addClass("disabled");
            } else {
                $(".previous").removeClass("disabled");
            }
            $("#SearchResults").html(data);
            if ($("#EndRecordNumber").text() == $("#TotalRecordCount").text()) {
                $(".next").addClass("disabled");
            } else {
                $(".next").removeClass("disabled");
            }
            if ($("#SearchResults a").first().length > 0) {
                //make the "Next" button in the callout match the URL of the first asset in the listing
                $("#next").attr("href", $("#SearchResults a").first().attr("href"));
            } else {
                $("#next").hide();
            }
        });
    }
}
