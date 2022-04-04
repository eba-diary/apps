// Plug-in initialization functions that should run on every page when it loads 
$(function () {
    $('[data-toggle="popover"]').popover();
    $('[data-toggle="tooltip"]').tooltip();

    //Set DataTable defaults if included
    if ($.fn.DataTable) {
        $.extend(true, $.fn.DataTable.defaults, {
            language: {
                search: "<div class='input-group'><span class='input-group-prepend sentry-blue white-text icon-search'></span>_INPUT_</div>",
                searchPlaceholder: "Search",
                processing: ""
            },
        });
        $.extend($.fn.DataTable.ext.classes, {
            // Prevents dataTables.boostrap4.js from adding classes to length select boxes. add our own instead
            sLengthSelect: "dataTables_length_select"
        });
    }
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

    this.InjectSpinner = function (element) {
        /// <summary>
        /// Injects a spinner (loading) icon into the DOM, 
        /// centered vertically and horizontally inside the element that you pass.
        /// There is no "HideSpinner" function (yet) as it's expected that you'll replace
        /// the content of the element when the operation is completed.
        /// </summary>
        /// <param name="element" type="HTMLElement">The element to inject the spinner into.</param>

        $(element).html(
            '<div class="text-center">' +
            '<div class="preloader-wrapper big active">' +
            '    <div class="spinner-layer spinner-blue-only">' +
            '        <div class="circle-clipper left">' +
            '            <div class="circle"></div>' +
            '        </div><div class="gap-patch">' +
            '            <div class="circle"></div>' +
            '        </div><div class="circle-clipper right">' +
            '            <div class="circle"></div>' +
            '        </div>' +
            '    </div>' +
            '</div></div>');
    };

    this.CreateModal = function (title, message, buttons, callback) {
        /// <summary>
        /// Creates a modal with an Ok button
        /// </summary>
        /// <param name="title">modal title</param>
        /// <param name="message">message to display</param>
        /// <param name="buttons">HTML buttons to put on the modal</param>
        /// <param name="callback">Callback for the OK button</param>
        /// <returns>modal object</returns>

        if (buttons === undefined || buttons === null) {
            buttons = "";
        }

        var html = '<div class="modal-dialog" role="document">' +
            '<div class="modal-content">' +
            '    <div class="modal-header">' +
            '        <h5 class="modal-title" id="exampleModalLabel">' +
            title +
            '        </h5>' +
            '        <button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
            '            <span aria-hidden="true">&times;</span>' +
            '        </button>' +
            '    </div>' +
            '    <div class="modal-body">' +
            message +
            '    </div>';

        if (buttons.length > 0) {
            html += '<div class="modal-footer">' + buttons + '</div>';
        }

        html += '</div></div >';

        var modal = $('<div></div>').addClass('modal fade').attr('tabindex', '-1').attr('role', 'dialog').attr('aria-hidden', 'true').html(html);

        $('.ok-button', modal).on('click', callback);

        modal.modal({ show: false });

        //add to the page
        modal.appendTo('body');

        return modal;
    };

    this.ShowModalAlert = function (message, callback) {
        /// <summary>
        /// Show a Modal with a single "OK" button.
        /// </summary>
        /// <param name="message" type="String">The message to display in the modal</param>
        /// <param name="callback">The function to call when the OK button is pressed.  This function should return false if you want the modal to stay open.</param>
        /// <returns type="">A modal object</returns>
        callback = callback || function () { };

        var modal = this.CreateModal('', message, this.ModalButtonsOK(), callback);
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

        var modal = this.CreateModal('', message, this.ModalButtonsOKCancel(), callback);
        modal = ShowModalCommon(modal);
        return modal;
    };

    this.ModalButtonsOK = function () {
        /// <summary>
        /// OK button
        /// </summary>
        /// <returns type="">The buttons html</returns>
        return '<button type="button" data-dismiss="modal" class="btn btn-primary ok-button">OK</button>';
    };

    this.ModalButtonsOKCancel = function () {
        /// <summary>
        /// Ok and Cancel buttons
        /// </summary>
        /// <returns type="">The buttons html</returns>
        return '<button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>' +
            '<button type="button" class="btn btn-primary ok-button" data-dismiss="modal">OK</button>';
    };

    this.ShowModalCustom = function (header, body, buttons) {
        /// <summary>
        /// Shows a modal with a custom header, body, and buttons
        /// </summary>
        /// <param name="header">The header to display on the modal</param>
        /// <param name="body">Body Content</param>
        /// <param name="buttons">The buttons to show.  This should be in HTML format.</param>
        /// <returns type="">A modal object</returns>
        var modal = this.CreateModal(header, body, buttons, function () { });
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
        /// <param name="buttons">The buttons to show.  This should be in HTML format.</param>
        /// <returns type="">A modal object</returns>
        var modal = this.CreateModal(header, '', buttons, function () { });

        this.InjectSpinner($('.modal-body', modal));

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
        modal.modal('show');
        return modal;
    }

    //add "extension methods" to a modal object
    function ExtendModal(modal) {
        modal.ReplaceModalBody = function (html) {
            /// <summary>
            /// Replaces the "loading" spinner in the modal body with the html provided.
            /// </summary>
            modal.find(".modal-body").html(html);
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