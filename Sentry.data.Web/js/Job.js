data.Job = {

    FormInit: function () {
        $("[id$='SelectedDataSource'] select").val("0");
        $("[id$='SelectedSourceType']").change(function () {
            var selectBox = this;
            data.Job.SetDataSourceSpecificPanels();
            data.Job.SetFtpPatternDefaults();
            $('.questionairePanel').hide();
            $(".editDataSourceLink").hide();
            $('#btnCreateDataset').hide();
            $('.dataSourceInfoPanel').hide();
            var val = $(selectBox).val();

            $.getJSON("/Config/SourcesByType", { sourceType: val }, function (data) {
                var subItems = "";
                $.each(data, function (index, item) {
                    subItems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
                });
                $("[id$='SelectedDataSource']").html(subItems);
                $("[id$='SelectedDataSource'] select").val("0");
                $("#RetrieverJob_SelectedDataSource").materialSelect();
            });

            data.Job.RequestMethodDropdownPopulate();

            data.Job.targetFileNameDescUpdate();
        });

        data.Job.RequestMethodDropdownPopulate();

        $("[id$='SelectedRequestMethod']").change(function () {
            data.Job.DisplayHttpPostPanel();
        });
        
        $("[id$='SelectedDataSource']").change(function () {
            var val = $(this).val();
            if (val != "0" && val != null) {
                $.ajax({                    
                    url: "/Config/SourceDetails/" + $("[id$='SelectedDataSource'] :selected").val(),
                    dataType: 'json',
                    type: "GET",
                    //data: { Id: $('#SelectedDataSource :selected').val() },
                    success: function (datain) {
                        //Data Source is not restricted or user has permissions to use data source
                        if (!datain.IsSecured || (datain.IsSecured && datain.Security.CanUseDataSource)) {
                            $('.securityPanel').hide();
                            $('.questionairePanel').show();
                            $('#btnCreateDataset').show();

                            if (datain.Security.CanEditDataSource) {
                                $("#editDataSource").attr("href", "/Config/Source/Edit/" + val);
                                $(".editDataSourceLink").show();
                            }
                            else {
                                $(".editDataSourceLink").hide();
                            }


                            data.Job.SetDataSourceSpecificPanels(datain.SourceType);


                            $.getJSON("/Config/IsHttpSource/", { dataSourceId: val }, function (data) {
                                if (data) {
                                    $('.httpSourcePanel').show();
                                    $('.httpPostPanel').hide();
                                }
                                else {
                                    $('.httpSourcePanel').hide();
                                    $('.httpPostPanel').hide();
                                }
                            });
                        }
                        else {
                            $('.securityPanel').show();
                            $('.questionairePanel').hide();
                            $(".editDataSourceLink").hide();
                            $('#btnCreateDataset').hide();
                            $('#btnCreateDataset').hide();
                            $('#schedulePanel').hide();
                        }

                        $('#dataSourceContactEmail').attr("href", datain.MailToLink)
                        $('#dataSourceContactEmail').text(datain.PrimaryContactName);
                        //$('#primaryContact.a').text("<a href/" + data.MailToLink + "/"adfad");
                        $('.dataSourceInfoPanel').show();
                        $("#dataSourceDescription").text(datain.Description);
                        $("#baseURLTextBox").val(datain.BaseUri);
                        $("#baseURL").text(" The Base URL of the Data Source you picked is " + datain.BaseUri + ".  What you type in the Relative URL will be appended to the end of this Base URL.");
                    }
                });
            }            
        });

        $('#jsonPreview').on('click', function () {
            try {
                var data = JSON.parse($("[id$='HttpRequestBody']").val());
                $("[id$='json-viewer']").jsonViewer(data);
                $('.jsonValidateResultsPanel').show();
            }
            catch (error) {
                alert("invalid json");
            }
        });

        $("[id^='RequestAccessButton']").off('click').on('click', function (e) {
            e.preventDefault();
            
            data.Job.AccessRequest($("[id$='SelectedDataSource'] :selected").val());
        });
        
        $("[id$='FtpPattern']").change(function () {
            data.Job.SetFtpPatternDefaults($("#RetrieverJob_FtpPattern").val());
        });

        $('#IsSourceCompressed').on('change', function () {
            $("#compressionPanel").toggle($("#IsSourceCompressed").is(':checked'));

            if ($(this).is(':checked')) {
                $('.jobquestion.targetFileName').hide();
                $('#TargetFileName').val("");
            }
            
            if (!$(this).is(':checked') && $("[id$='SelectedSourceType']").val().toLowerCase() !== "ftp") {
                $('.jobquestion.targetFileName').show();
            }
        });

        if (($("#JobID").val() !== undefined && $("#JobID").val() !== "0") || ($('#JobID').val() === 0 && $("[id$='SelectedDataSource'] :selected").val() !== "0")) {
            $.ajax({                
                url: "/Config/SourceDetails/" + $("[id$='SelectedDataSource'] :selected").val(),
                dataType: 'json',
                type: "GET",
                //data: { Id: $('#SelectedDataSource :selected').val() },
                success: function (datain) {

                    $('#dataSourceContactEmail').attr("href", datain.MailToLink)
                    $('#dataSourceContactEmail').text(datain.PrimaryContactName);
                    $("#editDataSource").attr("href", "/Config/Source/Edit/" + $("[id$='SelectedDataSource'] :selected").val());
                    $("#dataSourceDescription").text(datain.Description);

                    if (datain.Security.CanEditDataSource) {
                        $(".editDataSourceLink").show();
                    }

                    data.Job.SetDataSourceSpecificPanels(datain.SourceType);
                    data.Job.DisplayHttpPostPanel();
                    data.Job.targetFileNameDescUpdate();
                    
                    if ($("[id$='SelectedSourceType']").val().toLowerCase() === "ftp") {
                        data.Job.SetFtpPatternDefaults($('#RetrieverJob_FtpPattern').val());
                    };
                }
            });            
        };

        $("#compressionPanel").toggle($("#IsSourceCompressed").is(':checked'));

        // If this is an init of page with existing data
        var dataSourceElement = $("[id$='SelectedDataSource']")[0];
        var dataSourceVal = $("[id$='SelectedDataSource']").val();
        if (dataSourceVal !== undefined && dataSourceVal !== null) {
            var element = dataSourceElement;
            var event = new Event('change');
            element.dispatchEvent(event);

            $('#schedulePanel').show();
        }
        else {
            //If Source Type is selected, trigger changed event to ensure data source dropdown is populated
            var dataSourceTypeVal = $("[id$='SelectedSourceType']").val();
            if (dataSourceTypeVal !== undefined && dataSourceTypeVal !== null) {
                var element = document.querySelector("[id$='SelectedSourceType']");
                element.dispatchEvent(new Event('change'));
            }
            $('.securityPanel').hide();
            $('.questionairePanel').hide();
            $('.dataSourceInfoPanel').hide();
            $('.fieldDescription').hide();
            $('.editDataSourceLink').hide();
        }

    },

    SetFtpPatternDefaults: function (patternSelection) {
        if (patternSelection == undefined) {
            //reset properties and panels
            $('.searchCriteria.searchCriteriaIsRegex').show();
            $('#IsRegexSearch').prop('checked', false);
        }
        else {
            var init = true;
            if ($("#JobID").val() != "0" && $("#JobID").val() != undefined) {
                init = false;
            }

            switch (patternSelection) {
                case "0":
                    $('.searchCriteria.searchCriteriaIsRegex').show();
                    $('.jobquestion.searchCriteria').show();
                    break;
                case "2": //RegexFileNoDelete
                    $('.searchCriteria.searchCriteriaIsRegex').hide();
                    $('.jobquestion.searchCriteria').show();
                    break;
                case "4": //RegexFileSinceLastExecution
                    $('.searchCriteria.searchCriteriaIsRegex').hide();
                    $('.jobquestion.searchCriteria').show();
                    break;
                case "5": //NewFilesSinceLastexecution
                    $('.searchCriteria.searchCriteriaIsRegex').hide();
                    $('.jobquestion.searchCriteria').hide();
                    break;
            }

            if (init) {
                $('#IsRegexSearch').prop('checked', true);
            }
        }
        
    },

    SetDataSourceSpecificPanels: function (sourceType) {
        if (sourceType == undefined) {
            $('.jobquestion').hide();
            $('.ftpSourcePanel').hide();
            $('#IsSourceCompressed').prop('checked', false);
            $("#compressionPanel").toggle($("#IsSourceCompressed").is(':checked'));
            $('#scheduleRow').hide();
        }
        else {
            switch (sourceType.toLowerCase()) {
                case "ftp": 
                    $('.jobquestion.ftpPattern').show();
                    $('.jobquestion.compression').show();
                    $('.jobquestion.targetFileName').hide();
                    $('.httpSourcePanel').hide();
                    $('.httpPostPanel').hide();
                    break;
                case "googlebigqueryapi":
                case "googleapi":
                    $('.jobquestion.ftpPattern').hide();
                    $('.jobquestion.targetFileName').show();
                    $('.jobquestion.searchCriteria').hide();
                    $('.httpSourcePanel').show();
                    $('.httpPostPanel').hide();
                    break;
                case "https":
                    $('.jobquestion.ftpPattern').hide();
                    $('.jobquestion.targetFileName').show();
                    $('.jobquestion.searchCriteria').hide();
                    $('.httpSourcePanel').show();
                    $('.httpPostPanel').hide();
                    $('.jobquestion.compression').show();
                    break;
                case "s3basic":
                case "dfsbasic":
                case "dfscustom":
                    $('.jobquestion.ftpPattern').hide();
                    $('.jobquestion.compression').show();
                    $('.jobquestion.searchCriteria').hide();
            }

            //show common questions
            $('.jobquestion.sourceLocation').show();
            $('.jobquestion.schedule').show();
            data.Job.SchedulePickerInit();
        }
    },

    AccessRequest: function (sourceId) {
        var modal = Sentry.ShowModalWithSpinner("Request Access");
        var createRequestUrl = "/Config/DataSourceAccessRequest/?dataSourceId=" + encodeURI(sourceId);

        $.get(createRequestUrl, function (e) {
            modal.ReplaceModalBody(e);

            $("input[type='checkbox']").change(function () {
                var selectedPermissions = [];
                $("input[type='checkbox']:checked").each(function () {
                    selectedPermissions.push($(this).data("code"));
                });
                $("#SelectedPermissions").val(selectedPermissions);
            });

            var isRealAdGroup = false;

            $("#AdGroupName").change(function () {
                $.ajax({
                    url: '/Dataset/CheckAdGroup?adGroup=' + encodeURIComponent($(this).val()),
                    method: "GET",
                    dataType: 'json',
                    success: function (data) {
                        if (data) {
                            $("#AccessRequesrErrorBox").hide();
                        } else {
                            $("#AccessRequesrErrorBox").html("<div>AD Group is not a vaild group</div>").show();
                        }
                        isRealAdGroup = data;
                    }
                });
            });

            $("[id^='SubmitAccessRequestButton']").off('click').on('click', function (e) {
                e.preventDefault();
                //check validation
                var errors = "";
                if ($("#AdGroupName").val() === undefined || $("#AdGroupName").val() === "") {
                    errors += "<div>AD Group is required</div>";
                }
                if (!isRealAdGroup) {
                    errors += "<div>AD Group is not a vaild group</div>";
                }
                if ($("#BusinessReason").val() === undefined || $("#BusinessReason").val() === "") {
                    errors += "<div>Business Reason is required</div>";
                }
                if ($("#SelectedApprover").val() === undefined || $("#SelectedApprover").val() === "") {
                    errors += "<div>Approver is required</div>";
                }
                if ($("#SelectedPermissions").val() === undefined || $("#SelectedPermissions").val() === "") {
                    errors += "<div>Permissions are required</div>";
                }

                if (errors === "") {
                    $("#accessRequestSpinner").css('float', 'left');
                    Sentry.InjectSpinner($("#accessRequestSpinner"), 30);
                    $.ajax({
                        type: 'POST',
                        data: $("#AccessRequestForm").serialize(),
                        url: '/Config/SubmitAccessRequest',
                        success: function (data) { modal.ReplaceModalBody(data); }
                    });
                } else {
                    $("#AccessRequesrErrorBox").html(errors).show();
                }
            });
        });
    },

    SetSecurityPanel: function (secured, hasPermission) {
        if (!secured || (secured && hasPermission)) {
            $('.securityPanel').hide();
            $('.questionairePanel').show();
        }
        else {
            $('.securityPanel').show();
            $('.questionairePanel').hide();
        }
    },

    RequestMethodDropdownPopulate: function () {
        
        var val = $("[id$='SelectedSourceType'] :selected").val();

        $.getJSON(encodeURI("/Config/RequestMethodByType/" + val), function (data) {
            var subItems = "";
            $.each(data, function (index, item) {
                subItems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
            });
            
            $("[id$='SelectedRequestMethod']").html(subItems);
        });

        data.Job.DisplayHttpPostPanel();
    },

    DisplayHttpPostPanel: function () {
        
        var val = $("[id$='SelectedRequestMethod'] :selected").text();
        if (val.toUpperCase() === 'POST') {
            //data.Job.RequestDataFormatDropdownPopulate();
            $('.httpPostPanel').show();
        }
        else {
            $('.httpPostPanel').hide();
        }
    },

    RequestDataFormatDropdownPopulate: function () {
        if ($("#JobID").val() === undefined || $("#JobID").val() === "0") {
            
            var val = $("[id$='SelectedSourceType'] :selected").val();

            $.getJSON(encodeURI("/Config/RequestDataFormatByType/" + val), function (data) {
                var subItems = "";
                $.each(data, function (index, item) {
                    subItems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
                });
                
                $("[id$ ='SelectedRequestDataFormat']").html(subItems);
            });
        }        
    },

    targetFileNameDescUpdate: function () {
        
        var val = $("[id$='SelectedSourceType'] :selected").val().toLowerCase();

        if (val == 'https' || val == 'googleapi' || val == 'googlebigqueryapi') {
            $("#targetfilenamequestion").text("What should target file be named?");
            $("#targetfilenamedesc").text("Due to the type of data source, a target file name is required as we are receiving raw data and not a file.");
            $('#targetfilenamelabel').removeClass("optional");
        }
        else {
            $("#targetfilenamequestion").text("Should file be renamed?");
            $("#targetfilenamedesc").text("The incoming file will be renamed to Target File Name supplied.  If left blank, original source file name will used when saved to data.sentry.com.");
            $('#targetfilenamelabel').addClass("optional");
        }
    },

    SchedulePickerInit: function () {
        $('#hourlyPicker').hide();
        $('#dailyPicker').hide();
        $('#weeklyPicker').hide();
        $('#monthlyPicker').hide();
        $('#yearlyPicker').hide();
        
        var a = $("[id$='Schedule']").val().split(' ');
        var e = jQuery.Event("keydown");
        e.which = 13; // # Some key code value

        if (a !== undefined || a !== null) {
            $('#schedulePanel').show();
        }

        $("[id$='RetrieverJob_SchedulePicker']").change(function () {

            $('#hourlyPicker').hide();
            $('#dailyPicker').hide();
            $('#weeklyPicker').hide();
            $('#monthlyPicker').hide();
            $('#yearlyPicker').hide();

            switch ($(this).val()) {
                case "1":
                    $('#hourlyPicker').show();
                    break;
                case "2":
                    $('#dailyPicker').show();
                    break;
                case "3":
                    $('#weeklyPicker').show();
                    break;
                case "4":
                    $('#monthlyPicker').show();
                    break;
                case "5":
                default:
                    $('#yearlyPicker').show();
                    break;
            }
        });

        $('#cronHourlyTimePicker').bind('input', function () {
            $("[id$='Schedule']").val($(this).val() + ' * * * *');
            updateFutureTimes();
        });

        function updateFutureTimes() {

            if ($("[id$='Schedule']").val() !== 0) {

                $('#scheduledTimes').empty();
                later.date.localTime();
                var schedule = later.parse.cron($("[id$='Schedule']").val());
                var futureScheduleUTC = later.schedule(schedule).next(4);

                $(futureScheduleUTC).each(function (index, element) {

                    $('#scheduledTimes').append("<p>" + element + "</p>");
                });

                $('#scheduleRow').show();
            } else {
                $('#scheduleRow').hide();
            }
        }

        $('#cronDailyJobTimePicker').pickatime({});

        function changeDay() {
            if ($('#cronDailyJobTimePicker').val()) {
                var time = $("#cronDailyJobTimePicker").val();
                var timeSplit = time.split(':');
                if (timeSplit.length > 1) {
                    var h = timeSplit[0]
                    var m = timeSplit[1]
                }
                $("[id$='Schedule']").val(m + ' ' + h + ' * * *');
            }
            updateFutureTimes();
        }

        $('#cronDailyJobTimePicker').change(function () {
            changeDay();
        });

        $('#cronWeeklyDayPicker').bind('change', function () {
            changeWeek();
        });

        function changeWeek() {
            if ($('#cronWeeklyJobTimePicker').val()) {
                var time = $("#cronWeeklyJobTimePicker").val();
                var timeSplit = time.split(':');
                if (timeSplit.length > 1) {
                    var h = timeSplit[0]
                    var m = timeSplit[1]
                }
                var d = $('#cronWeeklyDayPicker').val();


                $("[id$='Schedule']").val(m + ' ' + h + ' * * ' + d);
            }
            updateFutureTimes();
        }

        $('#cronWeeklyJobTimePicker').pickatime({
        });

        $('#cronWeeklyJobTimePicker').change(function () {
            changeWeek();
        });

        $('#cronMonthlyDayPicker').bind('input', function () {
            changeMonth();
        });

        function changeMonth() {
            if ($('#cronMonthlyJobTimePicker').val()) {
                var time = $("#cronMonthlyJobTimePicker").val();
                var timeSplit = time.split(':');
                if (timeSplit.length > 1) {
                    var h = timeSplit[0]
                    var m = timeSplit[1]
                }
                var d = $('#cronMonthlyDayPicker').val();


                $("[id$='Schedule']").val(m + ' ' + h + ' ' + d + ' * *');
            }
            updateFutureTimes();
        }

        $('#cronMonthlyJobTimePicker').pickatime({});

        $('#cronMonthlyJobTimePicker').change(function () {
            changeMonth();
        });

        $('#cronYearlyJobTimePicker').pickatime({
        }); 

        $('#cronYearlyJobTimePicker').change(function () {
            changeYear();
        });

        $('#cronYearlyDayPicker').bind('input', function () {
            changeYear();
        });

        $('#cronYearlyMonthPicker').bind('change', function () {
            changeYear();
        });

        function changeYear() {
            if ($('#cronYearlyJobTimePicker').val()) {
                var time = $("#cronYearlyJobTimePicker").val();
                var timeSplit = time.split(':');
                if (timeSplit.length > 1) {
                    var hour = timeSplit[0]
                    var minute = timeSplit[1]
                }
                var day = $('#cronYearlyDayPicker').val();
                var month = $('#cronYearlyMonthPicker').val();

                if (!day) {
                    day = '*';
                }

                if (!month) {
                    month = '*';
                }

                if (hour != null && minute != null && month != null && day != null) {
                    $("[id$='Schedule']").val(minute + ' ' + hour + ' ' + day + ' ' + month + ' *');
                } else {
                    $("[id$='Schedule']").val();
                }
            }
            updateFutureTimes();
        }


        $("#cronJobDatePicker").pickatime({});
        
        var d = new Date();
        d.setHours(a[1], a[0]);
        switch ($("#RetrieverJob_SchedulePicker").val()) {
            case "1":
                $('#hourlyPicker').show();
                $('#cronHourlyTimePicker').val(a[0]);
                updateFutureTimes();
                break;
            case "2":
                $('#dailyPicker').show();
                $('#cronDailyJobTimePicker').val(a[1] + ":" + a[0]);
                $("#cronDailyJobTimePicker").change();
                break;
            case "3":
                $('#weeklyPicker').show();
                $('#cronWeeklyDayPicker').val(a[4]);
                $("#cronWeeklyJobTimePicker").val(a[1] + ":" + a[0]);
                $("#cronWeeklyJobTimePicker").change();
                break;
            case "4":
                $('#monthlyPicker').show();
                $('#cronMonthlyDayPicker').val(a[2]);
                $('#cronMonthlyJobTimePicker').val(a[1] + ":" + a[0]);
                $("#cronMonthlyJobTimePicker").change();
                break;
            case "5":
                $('#yearlyPicker').show();
                $('#cronYearlyMonthPicker').val(a[3]);
                $('#cronYearlyDayPicker').val(a[2]);
                $('#cronYearlyJobTimePicker').val(a[1] + ":" + a[0]);
                $("#cronYearlyJobTimePicker").change();
                break;
        }
    }
}