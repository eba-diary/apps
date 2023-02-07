function addJoinRule() {
    var tableOptions = "";
    for (var i = 0; i < tableList.length; i++) {
        tableOptions += "<option value=\"" + tableList[i] + "\">" + tableList[i] + "</option>";
    }

    var length = joins;
    joins++;
    var id = "Join" + length;

    $("#joinContainer").append("<div class=\"rule-container\">" +
        "<select id=\"firstTable" + id + "\" class=\"form-control ruleController tableNameController\" style= \"width: 11%; display:inline-block; \">" +
        "<option selected disabled>Table Name</option>" +
        tableOptions +
        "</select>" +
        "<select class=\"form-control ruleController joinTypeController\" style= \"width: 12%; display: inline-block; \"> " +
        "<option selected disabled>Join Type</option > " +
        "<option value='INNER'> Inner Join</option> " +
        "<option value='FULL'> Full Join</option> " +
        "<option value='LEFT'> Left Join</option> " +
        "<option value='RIGHT'> Right Join</option> " +
        "</select>" +
        "<select id=\"secondTable" + id + "\"class=\"form-control ruleController tableNameController\" style= \"width: 11%; display:inline-block; \">" +
        "<option selected disabled>Table Name</option>" +
        tableOptions +
        "</select>" +
        "<span> ON </span>" +
        // "<select id=\"helpfirstTable" + id + "\" class=\"form-control ruleController\" style= \"width: 11%; display:inline-block; \" disabled>" +
        //    "<option selected>Table Name</option>" +
        //      tableOptions +
        //  "</select>" +
        "<select id=\"colfirstTable" + id + "\" class=\"form-control ruleController\" style= \"width: 15%; display:inline-block; \" disabled>" +
        "</select>" +
        "<select class=\"form-control ruleController\" style= \"width: 5%; display: inline-block; \"> " +
        "<option value='==' selected> = </option > " +
        "<option value='!='> ≠ </option > " +
        "<option value='<'> < </option > " +
        "<option value='>'> > </option > " +
        "<option value='<='> <= </option > " +
        "<option value='>='> >= </option > " +
        "</select>" +
        //    "<select id=\"helpsecondTable" + id + "\" class=\"form-control ruleController\" style= \"width: 11%; display:inline-block; \" disabled>" +
        //       "<option selected>Table Name</option>" +
        //         tableOptions +
        //    "</select>" +
        "<select id=\"colsecondTable" + id + "\" class=\"form-control ruleController\" style= \"width: 15%; display:inline-block; \" disabled>" +
        "</select>" +
        "<div class=\"btn-group pull-right rule-actions\">" +
        "<button type=\"button\" class=\"btn btn-xs btn-danger\" onclick=\"RemoveParent(this)\">" +
        "<i class=\"icon-close\"></i> Delete" +
        "</button>" +
        "</div>" +
        "</div>");

    updateTables();
    updateBadges();
}


function addJoinStart() {
    var tableOptions = "";
    for (var i = 0; i < tableList.length; i++) {
        tableOptions += "<option value=\"" + tableList[i] + "\">" + tableList[i] + "</option>";
    }

    var length = joins;
    joins++;
    var id = "Join" + length;

    $("#joinContainer").append(
        "<div class=\"rule-container\">" +
            "<select id=\"firstTable" + id + "\" class=\"form-control ruleController tableNameController\" style= \"width: 11%; display:inline-block; \">" +
                "<option selected disabled>Table Name</option>" +
                tableOptions +
            "</select>" +
        "</div>");
}



function addJoinFromTable() {


    var tableOptions = "";
    for (var i = 0; i < tableList.length; i++) {
        tableOptions += "<option value=\"" + tableList[i] + "\">" + tableList[i] + "</option>";
    }

    var length = joins;
    joins++;
    var id = "Join" + length;

    $("#joinContainer").append("<div class=\"rule-container\">" +
        "<select id=\"firstTable" + id + "\" class=\"form-control ruleController tableNameController\" style= \"width: 11%; display:inline-block; \">" +
        "<option selected disabled>Table Name</option>" +
            tableOptions +
        "</select>" +
        "<select class=\"form-control ruleController joinTypeController\" style= \"width: 12%; display: inline-block; \"> " +
        "<option selected disabled>Join Type</option > " +
        "<option value='INNER'> Inner Join</option> " +
        "<option value='FULL'> Full Join</option> " +
        "<option value='LEFT'> Left Join</option> " +
        "<option value='RIGHT'> Right Join</option> " +
        "</select>" +
        "<select id=\"secondTable" + id + "\"class=\"form-control ruleController tableNameController\" style= \"width: 11%; display:inline-block; \">" +
            "<option selected disabled>Table Name</option>" +
            tableOptions +
        "</select>" +


        "<div class=\"btn-group pull-right rule-actions\">" +
        "<button type=\"button\" class=\"btn btn-xs btn-danger\" onclick=\"RemoveParent(this)\">" +
        "<i class=\"icon-close\"></i> Delete" +
        "</button>" +
        "</div>" +
        "</div>");







}

function addOnClause() {

    var tableOptions = "";
    for (var i = 0; i < tableList.length; i++) {
        tableOptions += "<option value=\"" + tableList[i] + "\">" + tableList[i] + "</option>";
    }

    var length = joins;
    joins++;
    var id = "Join" + length;

    $(this).append(
        "<div class=\"rule-container\">" +
            "<select id=\"secondTable" + id + "\"class=\"form-control ruleController tableNameController\" style= \"width: 11%; display:inline-block; \">" +
            "<option selected disabled>Table Name</option>" +
                tableOptions +
            "</select>" +
            "<span> ON </span>" +
            "<select id=\"colfirstTable" + id + "\" class=\"form-control ruleController\" style= \"width: 15%; display:inline-block; \" disabled>" +
            "</select>" +
            "<select class=\"form-control ruleController\" style= \"width: 5%; display: inline-block; \"> " +
                "<option value='==' selected> = </option > " +
                "<option value='!='> ≠ </option > " +
                "<option value='<'> < </option > " +
                "<option value='>'> > </option > " +
                "<option value='<='> <= </option > " +
                "<option value='>='> >= </option > " +
            "</select>" +
            "<select id=\"colsecondTable" + id + "\" class=\"form-control ruleController\" style= \"width: 15%; display:inline-block; \" disabled>" +
            "</select>" +
            "<div class=\"btn-group pull-right rule-actions\">" +
                "<button type=\"button\" class=\"btn btn-xs btn-danger\" onclick=\"RemoveParent(this)\">" +
                "<i class=\"icon-close\"></i> Delete" +
                "</button>" +
            "</div>" +
        "</div>");



}












function addOrRemoveColumnRename(text, selected) {
    //I'm choosing triple Pipe single Underscore cause I don't think anybody would ever choose that.
    var id = "renameT" + text.replace(' ', '|||_');
    if (selected == true) {

        $("#columnRenamerContainer").append("<div class=\"rule-container\"  id=\"" + id + "\">" +
            "<input class=\"form-control\" disabled value=\"" + text + "\" style=\"width: 25%; display:inline-block;\" />" +
            "<input class=\"form-control ruleController\" style=\"width: 45%; display:inline-block;\" placeholder=\"Renamed As... \">" +
            "<div class=\"btn-group pull-right rule-actions\">" +
            "<button type=\"button\" class=\"btn btn-xs btn-danger\" onclick=\"RemoveParent(this)\">" +
            "<i class=\"icon-close\"></i> Delete" +
            "</button>" +
            "</div>" +
            "</div>");

    }
    else {
        id = "#" + id;
        $(id).remove();
    }
    updateTables();
    updateBadges();
}

function addOrderByRule() {
    var length = $("#orderByContainer").children().length;
    var id = "orderBy" + length;

    $("#orderByContainer").append("<div class=\"rule-container\">" +
        "<select id=\"" + id + "\" class=\"form-control orderBySelector\" style=\"width: 75%; display: inline-block;\"></select>" +

        "<select id=\"option" + id + "\" class=\"form-control \" style=\"width: 10%; display: inline-block;\">" +
        "<option value='ASC'>" + "Ascending" + "</option>" +
        "<option value='DESC'>" + "Descending" + "</option>" +
        "</select>" +
        "<div class=\"btn-group pull-right rule-actions\">" +
        "<button type=\"button\" class=\"btn btn-xs btn-danger\" onclick=\"RemoveParent(this)\">" +
        "<i class=\"icon-close\"></i> Delete" +
        "</button>" +
        "</div>" +
        "</div>");

    var selector = "#" + id;

    $(selector).append($('<option/>', {
        value: 0,
        disabled: true,
        selected: true,
        text: "Pick a Column"
    }));

    for (var i = 0; i < schemaList.length; i++) {
        for (var j = 0; j < schemaList[i].schema.length; j++) {
            $(selector).append($('<option/>', {
                value: schemaList[i].schema[j].id,
                text: schemaList[i].schema[j].label
            }));
        }
    }

    updateTables();
    updateBadges();
}

function addHavingRule() {

    var length = $("#havingContainer").children().length;
    var id = "havingT" + length;

    $("#havingContainer").append("<div class=\"rule-container\"><select class=\"form-control\" style=\"width: 10%; display: inline-block; \">" +
        "<option selected> Aggregate </option>" +
        "<option>COUNT</option>" +
        "<option>SUM</option> " +
        "<option>AVG</option> " +
        "<option>MAX</option> " +
        "<option>MIN</option> " +
        "</select >" +
        "<select class=\"form-control havingSelector\" name=\"builder-basic_rule_0_filter\" id=\"" + id + "\" style=\"width: 40%; display: inline-block;\"></select>" +
        "<select class=\"form-control ruleController\" style= \"width: 5%; display: inline-block; \"> " +
        "<option value='!=' selected> ≠ </option > " +
        "<option value='=='> = </option > " +
        "<option value='<'> < </option > " +
        "<option value='>'> > </option > " +
        "<option value='<='> <= </option > " +
        "<option value='>='> >= </option > " +
        "</select>" +
        "<input class=\"form-control\" style= \"width: 18.4%; display: inline-block;\" />" +
        "<div class=\"btn-group pull-right rule-actions\">" +
        "<button type=\"button\" class=\"btn btn-xs btn-danger\" onclick=\"RemoveParent(this)\">" +
        "<i class=\"icon-close\"></i> Delete" +
        "</button>" +
        "</div>" +
        "</div>");

    var selector = "#" + id;

    $(selector).append($('<option/>', {
        value: 0,
        disabled: true,
        selected: true,
        text: "Pick a Column"
    }));

    for (var i = 0; i < schemaList.length; i++) {
        for (var j = 0; j < schemaList[i].schema.length; j++) {
            $(selector).append($('<option/>', {
                value: schemaList[i].schema[j].id,
                text: schemaList[i].schema[j].label
            }));
        }
    }

    updateTables();
    updateBadges();
}

function addGroupByRule() {

    var length = $("#groupByContainer").children().length;
    var id = "groupByT" + length;

    $("#groupByContainer").append("<div class=\"rule-container\">" +
        "<select id=\"" + id + "\" class=\"form-control groupBySelector\" name=\"builder-basic_rule_0_filter\" style=\"width: 90%; display: inline-block;\"></select>" +
        "<div class=\"btn-group pull-right rule-actions\">" +
        "<button type=\"button\" class=\"btn btn-xs btn-danger\" onclick=\"RemoveParent(this)\">" +
        "<i class=\"icon-close\"></i> Delete" +
        "</button>" +
        "</div>" +
        "</div>");

    var selector = "#" + id;

    $(selector).append($('<option/>', {
        value: 0,
        disabled: true,
        selected: true,
        text: "Pick a Column"
    }));

    for (var i = 0; i < schemaList.length; i++) {
        for (var j = 0; j < schemaList[i].schema.length; j++) {
            $(selector).append($('<option/>', {
                value: schemaList[i].schema[j].id,
                text: schemaList[i].schema[j].label
            }));
        }
    }

    updateTables();
    updateBadges();
}

function addAggregateRule() {

    var length = $("#aggregateContainer").children().length;
    var id = "aggT" + length;

    $("#aggregateContainer").append("<div class=\"rule-container\"><select class=\"form-control\" style=\"width: 10%; display: inline-block; \">" +
        "<option selected> Aggregate </option>" +
        "<option>COUNT</option>" +
        "<option>SUM</option> " +
        "<option>AVG</option> " +
        "<option>MAX</option> " +
        "<option>MIN</option> " +
        "</select >" +
        "<select class=\"form-control aggSelector\" id=\"" + id + "\" style=\"width: 40%; display: inline-block;\"></select>" +
        "<input class=\"form-control\" style= \"width: 18.4%; display: inline-block;\" placeholder=\"Renamed As ... (Leave blank if wanted)\" />" +
        "<div class=\"btn-group pull-right rule-actions\">" +
        "<button type=\"button\" class=\"btn btn-xs btn-danger\" onclick=\"RemoveParent(this)\">" +
        "<i class=\" icon-close\"></i> Delete" +
        "</button>" +
        "</div>" +
        "</div>");

    var selector = "#" + id;

    $(selector).append($('<option/>', {
        value: 0,
        disabled: true,
        selected: true,
        text: "Pick a Column"
    }));

    $(selector).append($('<option/>', {
        value: '*',
        text: "* (All Rows)"
    }));

    for (var i = 0; i < schemaList.length; i++) {
        for (var j = 0; j < schemaList[i].schema.length; j++) {
            $(selector).append($('<option/>', {
                value: schemaList[i].schema[j].id,
                text: schemaList[i].schema[j].label
            }));
        }
    }

    updateTables();
    updateBadges();
}