// Plug-in initialization functions that should run on every page when it loads 
$(function () {
    $('[data-toggle="popover"]').popover();
    $('[data-toggle="tooltip"]').tooltip();
    $('[placeholder]').placeholder();
    Ladda.bind('.ladda-button');
    $(".dropdown-submenu > a").submenupicker();
});

// Some String extensions
if (!String.format) {
    String.format = function (format) {
        var args = Array.prototype.slice.call(arguments, 1);
        return format.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
              ? args[number]
              : match
            ;
        });
    };
}

// Most custom functions should be added to the Sentry object
var Sentry = Sentry || new function () {

    this.InjectSpinner = function (element, height) {
        /// <summary>
        /// Injects a spinner (loading) icon into the DOM, 
        /// centered vertically and horizontally inside the element that you pass.
        /// There is no "HideSpinner" function (yet) as it's expected that you'll replace
        /// the content of the element when the operation is completed.
        /// </summary>
        /// <param name="element" type="HTMLElement">The element to inject the spinner into.</param>
        /// <param name="height" type="Number">If the element that you pass has no height, pass a 
        /// height to this function to temporarily assign it a height.</param>
        if (element.height() > 0) {
            height = element.height();
        } else {
            element.css('min-height', height);
        };

        var spinnerWrapper = document.createElement("span");
        spinnerWrapper.style.height = "" + height + "px";
        spinnerWrapper.className = "sentry-spinner-container";
        element.prepend(spinnerWrapper);

        var spinHeight = 48;
        var spin = new Spinner({
            color: "#005285",
            lines: 12,
            radius: spinHeight * .2,
            length: spinHeight * .2 * .6,
            width: 3,
            zIndex: "auto",
            top: "50%",
            left: "50%",
            className: "",
            speed: .9
        });

        spin.spin(spinnerWrapper);
    };

    this.ShowModalAlert = function (message, callback) {
        /// <summary>
        /// Show a Modal with a single "OK" button.
        /// </summary>
        /// <param name="message" type="String">The message to display in the modal</param>
        /// <param name="callback">The function to call when the OK button is pressed.  This function should return false if you want the modal to stay open.</param>
        /// <returns type="">A modal object</returns>
        callback = callback || function () { };

        var modal = bootbox.dialog({
            message: message,
            buttons: ModifyButtons(Sentry.ModalButtonsOK(callback)),
            show: false
        });
        modal = ShowModalCommon(modal);
        return modal;
    };

    this.ShowModalConfirmation = function (message, callback) {
        /// <summary>
        /// Show a Modal with an "OK" and "Cancel" button.
        /// </summary>
        /// <param name="message" type="String">The message to display in the modal</param>
        /// <param name="callback">The function to call when the OK button is pressed.  This function should return false if you want the modal to stay open.</param>
        /// <returns type="">A modal object</returns>
        callback = callback || function () { };

        var modal = bootbox.dialog({
            message: message,
            buttons: ModifyButtons(Sentry.ModalButtonsOKCancel(callback)),
            show: false
        });
        modal = ShowModalCommon(modal);
        return modal;
    };

    this.ModalButtonsOK = function (callback) {
        /// <summary>
        /// You can provide this function to the buttons parameter of the ShowModalCustom() method
        /// in order to show just an "OK" button
        /// </summary>
        /// <param name="callback">The function to call when the OK button is pressed.  This function should return false if you want the modal to stay open.</param>
        /// <returns type="">The buttons object</returns>
        return {
            OK: {
                label: "OK",
                className: "btn-primary",
                callback: callback
            }
        }
    };

    this.ModalButtonsOKCancel = function (callback) {
        /// <summary>
        /// You can provide this function to the buttons parameter of the ShowModalCustom() method
        /// in order to show an "OK" and "Cancel" button
        /// </summary>
        /// <param name="callback">The function to call when the OK button is pressed.  This function should return false if you want the modal to stay open.</param>
        /// <returns type="">The buttons object</returns>
        return {
            OK: {
                label: "OK",
                className: "btn-primary",
                callback: callback
            },
            Cancel: {
                label: "Cancel",
                className: "btn-link"
            }
        }
    };

    this.ShowModalCustom = function (header, body, buttons) {
        /// <summary>
        /// Shows a modal with a custom header, body, and buttons
        /// </summary>
        /// <param name="header">The header to display on the modal</param>
        /// <param name="body">The function to call when the OK button is pressed.  This function should return false if you want the modal to stay open.</param>
        /// <param name="buttons">The buttons to show.  This should be in bootbox format.</param>
        /// <returns type="">A modal object</returns>
        var modal = bootbox.dialog({
            message: body,
            title: header,
            buttons: ModifyButtons(buttons),
            show: false
        });
        modal = ShowModalCommon(modal);
        return modal;
    };

    this.ShowModalWithSpinner = function (header, buttons) {
        /// <summary>
        /// Shows a modal with a header and buttons, but with a spinner in the body.
        /// Use the ReplaceModalBody method on the returned modal object to load content
        /// into the modal body via AJAX.
        /// </summary>
        /// <param name="header">The header to display on the modal</param>
        /// <param name="buttons">The buttons to show.  This should be in bootbox format.</param>
        /// <returns type="">A modal object</returns>
        var modal = bootbox.dialog({
            message: " ",
            title: header,
            buttons: ModifyButtons(buttons),
            show: false
        });
        $(".bootbox-body").css('min-height', 200); //adjust the height of the modal body
        modal.on("shown.bs.modal", function (e) {
            //if the modal body is still empty once it's become visible, show the spinner
            if ($(".bootbox-body").html().trim() == "") { 
                Sentry.InjectSpinner($(".bootbox-body"), 200);
            }
        });
        modal = ShowModalCommon(modal);
        return modal;
    };

    this.HideAllModals = function () {
        /// <summary>
        /// Hides all visible modals
        /// </summary>
        $(".modal").modal("hide");
    };

    //private function called after all the "ShowModal" methods create the modal
    function ShowModalCommon(modal) {
        modal = ExtendModal(modal);
        $(".modal").on("shown.bs.modal", function (e) {
            Ladda.bind('.ladda-button');
        });
        modal.modal('show');
        return modal;
    }

    //gives us a chance to add additional classes to buttons
    function ModifyButtons(buttons) {
        for (var buttonName in buttons) {
            if (buttons.hasOwnProperty(buttonName)) {
                var button = buttons[buttonName];
                // hack to get the buttons in a modal to work with ladda spinners
                if (!button.className) {
                    button.className = "ladda-button' data-style='zoom-in";
                } else if (button.className.indexOf("btn-link") == -1) {
                    button.className = button.className + " ladda-button' data-style='zoom-in"
                }
            }
        }
        return buttons;
    }

    //add "extension methods" to a modal object
    function ExtendModal(modal) {
        modal.ReplaceModalBody = function (html) {
            /// <summary>
            /// Replaces the "loading" spinner in the modal body with the html provided.
            /// </summary>
            modal.find(".bootbox-body").html(html);
        };

        modal.SetFocus = function (selector) {
            /// <summary>
            /// Sets the focus to an element that matches the given selector
            /// </summary>
            /// <param name="selector" type="String">A JQuery selector</param>

            //in case the modal is already visible
            modal.find(selector).focus();
            //in case the modal is not yet shown, wire the focus() to the bootstrap shown event
            modal.on("shown.bs.modal", function (e) {
                $(e.target).find(selector).focus();
            });
        };

        modal.HideModal = function () {
            /// <summary>
            /// Hides the modal
            /// </summary>
            modal.modal("hide");
        };

        return modal;
    };

    this.WasAjaxSuccessful = function (data) {
        /// <summary>
        /// Looks for a "success" property in the data that is equal to true
        /// </summary>
        /// <param name="data">The return from the AJAX call</param>
        /// <returns type="boolean">Whether the AJAX call returned the expected success indicator</returns>
        return data.Success;
    }

    this.GetDataTableParamsForExport = function (table) {
        /// <summary>
        /// Given a DataTable, returns an IDataTableRequest-compatible Params object that can be turned 
        /// into a query string to pass to the server to perform data export functionality, so that the 
        /// export carries forward any filtering and sorting that is being applied by the user in the DataTable
        /// </summary>
        /// <param name="table">The DataTable object</param>
        /// <returns type="object">The Parameters</returns>

        var params = {};
        params.search = {};
        params.order = [];
        params.columns = [];
        params.search.value = table.search();
        if (table.order().length > 0) {
            // We are sorting by at least one column
            if (table.order()[0] instanceof Array) {
                // We have an Array of Arrays
                for (var i = 0; i < table.order().length; i++) {
                    params.order[i] = {};
                    params.order[i].column = table.order()[i][0];
                    params.order[i].dir = table.order()[i][1];
                }
            } else {
                // We have a simple Array, which may be the case if we are ordering by just one column
                params.order[0] = {};
                params.order[0].column = table.order()[0];
                params.order[0].dir = table.order()[1];
            }
        }

        for (var j = 0; j < table.columns()[0].length; j++) {
            params.columns[j] = {};
            params.columns[j].data = table.column(j).dataSrc();
            params.columns[j].name = '';
            params.columns[j].search = {};
            if (table.settings()[0].getSearchForColumn)
                params.columns[j].search.value = table.settings()[0].getSearchForColumn(j);
            else
                params.columns[j].search.value = table.column(j).search();
            params.columns[j].searchable = table.settings()[0].aoColumns[j].bSearchable;
        }

        return params;
    }

}
