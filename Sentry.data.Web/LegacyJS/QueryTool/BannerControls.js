var AlertLevel = "";
var Banner = "";
var listOfDismissedBanners = [];

function PassHelpText(location, type, information) {

    if (listOfDismissedBanners.indexOf(information) == -1) {

        var banner;
        var header;
        var text;

        if ($("#bannerContainer").children().length == 0) {
            $("#bannerContainer").append("<div id=\"schemaWarningBanner\" class=\"alert alert-dismissable alert-warning\">" +
                "<button type=\"button\" class=\"close\" data-dismiss=\"alert\">×</button>" +
                "<h4 id=\"schemaWarningHeader\">Warning!</h4>" +
                "<p id=\"schemaWarningText\"></p>" +
                "</div>");
        }

        banner = $('#schemaWarningBanner');
        header = $('#schemaWarningHeader');
        text = $('#schemaWarningText');

        text.empty();

        if (information.startsWith('u\'')) {
            information = information.substr(2, information.length - 1);
        }

        if (information.includes('cancelled job group')) {
            information = "Job successfully cancelled.";
            type = "Success";
        }


        text.append(information);
        Banner = information;
        AlertLevel = type;

        if (type == "Warning") {
            banner.switchClass("alert-success", "alert-warning", 10);
            banner.switchClass("alert-info", "alert-warning", 10);
            header.text("Warning!");

        } else if (type == "Success") {
            banner.switchClass("alert-warning", "alert-success", 10);
            banner.switchClass("alert-info", "alert-success", 10);
            header.text("Success");
        } else if (type == "Error") {
            banner.switchClass("alert-warning", "alert-danger", 10);
            banner.switchClass("alert-info", "alert-danger", 10);
            header.text("Error");
        } else {
            banner.switchClass("alert-warning", "alert-info", 10);
            banner.switchClass("alert-success", "alert-info", 10);
            header.text("Information");
        }

        banner.show();
    }
}

function HideBanner() {
    $('#schemaWarningBanner').hide();
    console.log(Banner);

    if (AlertLevel != "Error" && AlertLevel != "Success") {
        listOfDismissedBanners.push(Banner);
    }
}