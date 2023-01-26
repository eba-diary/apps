import 'expose-loader?exposes=$,jQuery!jquery';
import 'jquery-ui/dist/jquery-ui.js';
import 'jquery-ajax-unobtrusive';
import 'imports-loader?wrapper=window!jquery-throttle-debounce';
import 'jquery-validation';
import 'jquery-validation-unobtrusive';
import '@sentry-insurance/InternalFrontendTemplate/dist/jquery.validate.unobtrusive.sentry.js';
import 'bootstrap';
import '@sentry-insurance/InternalFrontendTemplate/dist/Sentry.Associates.js';
import 'expose-loader?exposes=Sentry!@sentry-insurance/InternalFrontendTemplate/dist/Sentry.Common.js';
import 'select2';
import 'bootbox';
import 'expose-loader?exposes=toastr!toastr';
import 'expose-loader?exposes=firstBy!thenby';
import 'typeahead.js';

//Full list of MDB components: https://bitbucket.sentry.com/projects/COMDN/repos/dotnettemplate/browse/Sentry._MyApp_/Sentry._MyApp_.Web/src/js/mdb.ts
// Selectively enable only the pieces you need to keep your bundle small
import '@sentry-insurance/mdbootstrap/src/js/free/forms-free.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/buttons.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/collapsible.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/dropdown/dropdown.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/dropdown/dropdown-searchable.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/material-select/material-select.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/material-select/material-select-view.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/material-select/material-select-view-renderer.js';
import '@sentry-insurance/mdbootstrap/src/js/vendor/free/enhanced-modals.js';
import '@sentry-insurance/mdbootstrap/src/js/vendor/free/jquery.easing.js';
import '@sentry-insurance/mdbootstrap/src/js/vendor/free/waves.js';
////Using commonJS with imports loader as to not interfere with MDB's half-supported module implementaion in picker-date
import 'imports-loader?type=commonjs&imports=single|jquery|$!@sentry-insurance/mdbootstrap/src/js/vendor/pro/picker-date.js';
import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/picker-time.js';