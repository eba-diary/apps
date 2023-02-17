data.QueryTool = {

    Init: function () {

        // click event to toggle the display of the datasets selector
        $('#datasetsHeader').click(function () {

            if ($(this).children('.tracker-menu-icon').hasClass('icon-chevron-down')) {
                $(this).children('.tracker-menu-icon').switchClass('icon-chevron-down', 'icon-chevron-up');
            } else {
                $(this).children('.tracker-menu-icon').switchClass('icon-chevron-up', 'icon-chevron-down');
            }
            $(this).next('#datasetsContainer').toggle();

            if ($(this).next('#datasetsContainer:visible').length === 0) {
                // action when all are hidden
                $(this).css('border-radius', '5px 5px 5px 5px');
            } else {
                $(this).css('border-radius', '5px 5px 0px 0px');
            }

        });

        // click event for tabs used to build queries (Column Selection, Join Tables, etc.)
        $('#primaryOptions li').on('click', function () {

            $('#primaryOptions').children('li').each(function () {
                if ($(this).hasClass('active')) {
                    $(this).removeClass('active');
                }
            });

            $(this).addClass('active');

            $('#joinPanel').hide();
            $('#columnRenamer').hide();
            $('#columnSelector').hide();
            $('#orderBy').hide();
            $('#groupBy').hide();
            $('#whereClause').hide();
            $('#aggregate').hide();

            switch ($(this)[0].id) {
                case "tab1":
                    //$('#columnSelector').show();
                    break;
                case "tab2":
                    $('#joinPanel').show();
                    break;
                case "tab3":
                    $('#columnRenamer').show();
                    break;
                case "tab4":
                    $('#orderBy').show();
                    break;
                case "tab5":
                    $('#groupBy').show();
                    break;
                case "tab6":
                    $('#whereClause').show();
                    break;
                case "tab7":
                    $('#aggregate').show();
                    break;
            }
        });

        // click event for the Run Query button
        $('#runQueryBtn').click(function (e) {

            // cancel the default action
            e.preventDefault();
            e.stopPropagation();
            return false;

        });

        // click event to toggle the display of the Job Status window
        $('.tracker-header').click(function () {

            $('#tracker-menu-icon').switchClass('icon-chevron-down', 'icon-chevron-up');
            $('.tracker-content').toggle();

        });

    },

    ToggleAdminControls: function () {

        // show or hide various elements
        $('#AdminControls').toggle();
        $('textarea').toggle();

    },

    GetParameterByName: function (paramName, url) {

        // if no url supplied, grab the URL from the current window
        if (!url) url = window.location.href;

        // replace any special characters
        paramName = paramName.replace(/[\[\]]/g, "\\$&");

        var regex = new RegExp("[?&]" + paramName + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);

        if (!results) return null;
        if (!results[2]) return '';

        return decodeURIComponent(results[2].replace(/\+/g, " "));

    }
};