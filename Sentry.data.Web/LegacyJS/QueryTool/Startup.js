
var sessionStartUpSpeed = 0;

function UpdateLivyURL() {
    $('#sessionID').text(sessionID);

    var url = $("#LivyURL").attr('href') + sessionID;
    var to = url.lastIndexOf('/')
    to = to === -1 ? url.length : to + 1;
    url = url.substring(0, to);
    url = url + sessionID;
    $("#LivyURL").attr("href", url);
}

function CreateSession(careAboutInterval) {
    $.ajax({
        type: "POST",
        url: "/api/v1/queryTool/sessions/" + $('#LanguageDropDown').val(),
        dataType: "json",
        success: function (msg) {
            json = JSON.parse(msg);
            sessionID = json.id;
            UpdateLivyURL();

            var obj = { 'id': sessionID, 'time': Date.now() };

            localStorage.setItem("SparkSession", JSON.stringify(obj));
            $('#jobInfo').text("Your Session is currently " + json.state + ".");

            if (careAboutInterval) {
                sessionStartInterval = setInterval("CheckSession()", 1000);
            }

            CheckSession();
        },
        error: function (e) {
            console.log("Unavailable");
        }
    });
}

function CheckSession() {
    sessionStartUpSpeed++;
    $('#sessionSpinner').show();
    $('#panelSpinner').show();
    $('#schemaInformationButton').hide();

    $.ajax({
        type: "GET",
        url: "/api/v1/queryTool/sessions/" + sessionID,
        dataType: "json",
        success: function (msg) {
            json = JSON.parse(msg);
            $('#jobInfo').text("Your Session is currently " + json.state + ".");

            if (json.state === "idle") {
                clearInterval(sessionStartInterval);
                $('#sessionSpinner').hide();
                $('#panelSpinner').hide();
                $('#schemaInformationButton').show();
                $('#statementInfo').text("Apache Spark has created a new session in " + sessionStartUpSpeed + " second(s).");
                sessionStartUpSpeed = 0;

                UpdateLivyURL();

            } else {
                $('#statementInfo').text("Apache Spark is currently creating a new session for the last " + sessionStartUpSpeed + " second(s).");
            }
        },
        error: function (e) {
            clearInterval(sessionStartInterval);
            CreateSession(true);
        }
    });
}