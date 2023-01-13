//Include("~/Scripts/jquery-{version}.js").
import 'expose-loader?exposes=$,jQuery!jquery'

//Include("~/Scripts/jquery-ui.js").
import 'jquery-ui';

//Include("~/Scripts/jquery.unobtrusive-ajax.js").
import 'jquery-ajax-unobtrusive';

//Include("~/Scripts/jquery.ba-throttle-debounce.js").
import 'imports-loader?wrapper=window!jquery-throttle-debounce';

//Include("~/Scripts/jquery.validate.js").
import 'jquery-validation';

//Include("~/Scripts/jquery.validate.unobtrusive.js").
import 'jquery-validation-unobtrusive';

//Include("~/Scripts/jquery.validate.unobtrusive.sentry.js").
import '@sentry-insurance/InternalFrontendTemplate/dist/jquery.validate.unobtrusive.sentry.js';

//Include("~/Scripts/select2.js").
import 'select2';

//Include("~/Scripts/bootbox.js").
import 'bootbox';

//Include("~/Scripts/toastr.min.js").
import 'toastr';

//Include("~/Scripts/thenBy.min.js"));
import 'thenby';

//Include("~/Scripts/bootstrap.bundle.js").
import 'bootstrap';

//Include("~/Scripts/typeahead.bundle.js").
import 'typeahead.js';

//Include("~/Scripts/sentry.associates.js").
import '@sentry-insurance/InternalFrontendTemplate/dist/Sentry.Associates.js';

//Include("~/Scripts/sentry.common.js"));
import 'expose-loader?exposes=Sentry!@sentry-insurance/InternalFrontendTemplate/dist/Sentry.Common.js';

//Include("~/Scripts/MDB/mdb.js").
//Full list of MDB components: https://bitbucket.sentry.com/projects/COMDN/repos/dotnettemplate/browse/Sentry._MyApp_/Sentry._MyApp_.Web/src/js/mdb.ts
import '@sentry-insurance/mdbootstrap/src/js/free/forms-free.js';
import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/picker-date.js';