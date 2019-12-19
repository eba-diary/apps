data.DataFlow = {

    DataFlowFormInit: function () {
        console.log("hi");

        //data.DataFlow.InitCompressionCheckbox();

        $("[id$=IngestionType]").on('change', function () {
            if ($(this).val() === "2") {
                $('.retrieverPanel').show();
                Sentry.InjectSpinner($("#retrieverJobPanel"));
                $.get("/DataFlow/NewRetrieverJob", function (e) {
                    $("#retrieverJobPanel").replaceWith(e);
                    data.Job.FormInit();
                })
            }
            else {
                $('.retrieverPanel').hide();
            }
            $('.compressionPanel').show();
            $('.schemaMapPanel').show();
        });

        $("#IsCompressed").change(function () {
            //if ($(this).is(":checked")) {
            //    $("#IsCompressed").val(true);                
            //}
            //else {
            //    $("#IsCompressed").val(false);
            //}
        });

        $("#btnAddSchemaMap").on('click', function () {
            $.get("/DataFlow/NewSchemaMap", function (e) {
                $("#schemaMapPanel").append(e);
                $('[id$=__SelectedDataset]').change(function () {
                    var curRow = $(this).parent().parent();
                    var schemaSelectionDropDown = curRow.find("[id$=__SelectedSchema]");
                    var val = $(this).val();
                    console.log(val);

                    $.getJSON("/api/v2/metadata/dataset/" + val + "/schema", function (result) {
                        //var optgroup = $('<optgroup>');

                        //var previousOpt = '';
                        //$.each(result, function (index, inData) {

                        //    if (inData. != previousOpt) {
                        //        if (previousOpt != '') {
                        //            schemaSelectionDropDown.append(optgroup);
                        //        }
                        //        optgroup = $('<optgroup>');
                        //        optgroup.attr('label', inData.Group.Name);
                        //        previousOpt = inData.Group.Name;
                        //    }

                        //    optgroup.append($('<option/>', {
                        //        value: inData.Value,
                        //        text: inData.Text
                        //    }));
                        //});

                        //schemaSelectionDropDown.append(optgroup);
                        var subItems;
                        subItems += "<option value='0'>Select Schema</option>";
                        $.each(result, function (index, item) {
                            subItems += "<option value='" + item.SchemaId + "'>" + item.Name + "</option>";
                        });

                        schemaSelectionDropDown.html(subItems);
                        schemaSelectionDropDown.val("0");
                    });
                });
            });
        });

        

        //$("#SelectedSourceType").change(function () {
        //    data.Job.SetDataSourceSpecificPanels();
        //    data.Job.SetFtpPatternDefaults();
        //    $('.questionairePanel').hide();
        //    $(".editDataSourceLink").hide();
        //    $('#btnCreateDataset').hide();
        //    $('.dataSourceInfoPanel').hide();
        //    var val = $('#SelectedSourceType :selected').val();

        //    $.getJSON("/Config/SourcesByType", { sourceType: val }, function (data) {
        //        var subItems = "";
        //        $.each(data, function (index, item) {
        //            subItems += "<option value='" + item.Value + "'>" + item.Text + "</option>";
        //        });

        //        $("#SelectedDataSource").html(subItems);
        //        $("#SelectedDataSource select").val("0");
        //    });

        //    data.Job.RequestMethodDropdownPopulate();

        //    data.Job.targetFileNameDescUpdate();
        //});
    },

    //SchedulePickerInit: function () {
    //    $('#hourlyPicker').hide();
    //    $('#dailyPicker').hide();
    //    $('#weeklyPicker').hide();
    //    $('#monthlyPicker').hide();
    //    $('#yearlyPicker').hide();

    //    var a = $('#Schedule').val().split(' ');
    //    var e = jQuery.Event("keydown");
    //    e.which = 13; // # Some key code value

    //    switch ($('#SchedulePicker').val()) {
    //        case "0":
    //            $('#cronHourlyTimePicker').val(a[0]);
    //            $('#hourlyPicker').show();
    //            break;
    //        case "1":
    //            $('#cronDailyJobTimePicker').val(a[1] + ":" + a[0]);
    //            $("#cronDailyJobTimePicker").trigger(e);
    //            $('#dailyPicker').show();
    //            break;
    //        case "2":
    //            $('#cronWeeklyDayPicker').val(a[4]);
    //            $("#cronWeeklyJobTimePicker").val(a[1] + ":" + a[0]);
    //            $("#cronWeeklyJobTimePicker").trigger(e);
    //            $('#weeklyPicker').show();
    //            break;
    //        case "3":
    //            $('#cronMonthlyDayPicker').val(a[2]);
    //            $('#cronMonthlyJobTimePicker').val(a[1] + ":" + a[0]);
    //            $("#cronMonthlyJobTimePicker").trigger(e);
    //            $('#monthlyPicker').show();
    //            break;
    //        case "4":
    //            $('#cronYearlyMonthPicker').val(a[3]);
    //            $('#cronYearlyDayPicker').val(a[2]);
    //            $('#cronYearlyJobTimePicker').val(a[1] + ":" + a[0]);
    //            $("#cronYearlyJobTimePicker").trigger(e);
    //            $('#yearlyPicker').show();
    //            break;
    //    }

    //    $('#SchedulePicker').change(function () {

    //        $('#hourlyPicker').hide();
    //        $('#dailyPicker').hide();
    //        $('#weeklyPicker').hide();
    //        $('#monthlyPicker').hide();
    //        $('#yearlyPicker').hide();

    //        switch ($(this).val()) {
    //            case "0":
    //                $('#hourlyPicker').show();
    //                break;
    //            case "1":
    //                $('#dailyPicker').show();
    //                break;
    //            case "2":
    //                $('#weeklyPicker').show();
    //                break;
    //            case "3":
    //                $('#monthlyPicker').show();
    //                break;
    //            case "4":
    //            default:
    //                $('#yearlyPicker').show();
    //                break;
    //        }
    //    });


    //    $('#cronHourlyTimePicker').bind('input', function () {
    //        $('#Schedule').val($(this).val() + ' * * * *');
    //        updateFutureTimes();
    //    });

    //    function updateFutureTimes() {

    //        if ($('#Schedule').val() != 0) {

    //            $('#scheduledTimes').empty();
    //            later.date.localTime();
    //            var schedule = later.parse.cron($('#Schedule').val());
    //            var futureScheduleUTC = later.schedule(schedule).next(4);

    //            $(futureScheduleUTC).each(function (index, element) {

    //                $('#scheduledTimes').append("<p>" + element + "</p>");
    //            });

    //            $('#scheduleRow').show();
    //        } else {
    //            $('#scheduleRow').hide();
    //        }
    //    }



    //    $('#cronDailyJobTimePicker').timepicker({
    //        timeFormat: 'h:mm p',
    //        interval: 60,
    //        minTime: '0',
    //        maxTime: '23:59',
    //        defaultTime: '11',
    //        startTime: '0',
    //        dynamic: false,
    //        dropdown: true,
    //        scrollbar: true,
    //        change: function (ev) {

    //            if ($(this).timepicker('getTime')) {
    //                var d = new Date($(this).timepicker('getTime'));
    //                var h = d.getHours();
    //                var m = d.getMinutes();

    //                $('#Schedule').val(m + ' ' + h + ' * * *');
    //            }
    //            updateFutureTimes();
    //        }
    //    });


    //    $('#cronWeeklyDayPicker').bind('input', function () {
    //        changeWeek()
    //    });

    //    function changeWeek() {
    //        if ($('#cronWeeklyJobTimePicker').timepicker('getTime')) {
    //            var d = new Date($('#cronWeeklyJobTimePicker').timepicker('getTime'));
    //            var h = d.getHours();
    //            var m = d.getMinutes();
    //            var d = $('#cronWeeklyDayPicker').val();


    //            $('#Schedule').val(m + ' ' + h + ' * * ' + d);
    //        }
    //        updateFutureTimes();
    //    }

    //    $('#cronWeeklyJobTimePicker').timepicker({
    //        timeFormat: 'h:mm p',
    //        interval: 60,
    //        minTime: '0',
    //        maxTime: '23:59',
    //        defaultTime: '11',
    //        startTime: '0',
    //        dynamic: false,
    //        dropdown: true,
    //        scrollbar: true,
    //        change: function (ev) {
    //            changeWeek();
    //        }
    //    });

    //    $('#cronMonthlyDayPicker').bind('input', function () {
    //        changeMonth();
    //    });

    //    function changeMonth() {
    //        if ($('#cronMonthlyJobTimePicker').timepicker('getTime')) {
    //            var d = new Date($('#cronMonthlyJobTimePicker').timepicker('getTime'));
    //            var h = d.getHours();
    //            var m = d.getMinutes();
    //            var d = $('#cronMonthlyDayPicker').val();


    //            $('#Schedule').val(m + ' ' + h + ' ' + d + ' * *');
    //        }
    //        updateFutureTimes();
    //    }
    //    $('#cronMonthlyJobTimePicker').timepicker({
    //        timeFormat: 'h:mm p',
    //        interval: 60,
    //        minTime: '0',
    //        maxTime: '23:59',
    //        defaultTime: '11',
    //        startTime: '0',
    //        dynamic: false,
    //        dropdown: true,
    //        scrollbar: true,
    //        change: function (ev) {
    //            changeMonth();
    //        }
    //    });

    //    $('#cronYearlyDayPicker').bind('input', function () {
    //        changeYear();
    //    });

    //    $('#cronYearlyMonthPicker').bind('input', function () {
    //        changeYear();
    //    });

    //    function changeYear() {
    //        if ($('#cronYearlyJobTimePicker').timepicker('getTime')) {
    //            var d = new Date($('#cronYearlyJobTimePicker').timepicker('getTime'));
    //            var hour = d.getHours();
    //            var minute = d.getMinutes();
    //            var day = $('#cronYearlyDayPicker').val();
    //            var month = $('#cronYearlyMonthPicker').val();

    //            if (!day) {
    //                day = '*';
    //            }

    //            if (!month) {
    //                month = '*';
    //            }


    //            if (hour != null && minute != null && month != null && day != null) {
    //                $('#Schedule').val(minute + ' ' + hour + ' ' + day + ' ' + month + ' *');
    //            } else {
    //                $('#Schedule').val();
    //            }

    //        }
    //        updateFutureTimes();
    //    }
    //    $('#cronYearlyJobTimePicker').timepicker({
    //        timeFormat: 'h:mm p',
    //        interval: 60,
    //        minTime: '0',
    //        maxTime: '23:59',
    //        defaultTime: '11',
    //        startTime: '0',
    //        dynamic: false,
    //        dropdown: true,
    //        scrollbar: true,
    //        change: function (ev) {
    //            changeYear();
    //        }
    //    });

    //    $("#cronJobDatePicker").datepicker();
    //}
}