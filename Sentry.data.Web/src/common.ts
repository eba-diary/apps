﻿//Include("~/Scripts/jquery-{version}.js").
import 'expose-loader?exposes=$,jQuery!jquery'

//Include("~/Scripts/jquery-ui.js").
import 'jquery-ui-dist/jquery-ui';

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

//Include("~/Scripts/bootstrap.bundle.js").
import 'bootstrap';

//Include("~/Scripts/sentry.associates.js").
import '@sentry-insurance/InternalFrontendTemplate/dist/Sentry.Associates.js';

//Include("~/Scripts/sentry.common.js"));
import 'expose-loader?exposes=Sentry!@sentry-insurance/InternalFrontendTemplate/dist/Sentry.Common.js';

//Include("~/Scripts/MDB/mdb.js").
//Full list of MDB components: https://bitbucket.sentry.com/projects/COMDN/repos/dotnettemplate/browse/Sentry._MyApp_/Sentry._MyApp_.Web/src/js/mdb.ts

// Selectively enable only the pieces you need to keep your bundle small
import '@sentry-insurance/mdbootstrap/src/js/free/forms-free.js';
//import '@sentry-insurance/mdbootstrap/src/js/free/scrolling-navbar.js';
//import '@sentry-insurance/mdbootstrap/src/js/free/treeview.js';
//import '@sentry-insurance/mdbootstrap/src/js/free/wow.js';

import '@sentry-insurance/mdbootstrap/src/js/pro/buttons.js';
//import '@sentry-insurance/mdbootstrap/src/js/pro/cards.js';
//import '@sentry-insurance/mdbootstrap/src/js/pro/character-counter.js';
//import '@sentry-insurance/mdbootstrap/src/js/pro/chips.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/collapsible.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/dropdown/dropdown.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/dropdown/dropdown-searchable.js';
//import '@sentry-insurance/mdbootstrap/src/js/pro/file-input.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/material-select/material-select.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/material-select/material-select-view.js';
import '@sentry-insurance/mdbootstrap/src/js/pro/material-select/material-select-view-renderer.js';
//import '@sentry-insurance/mdbootstrap/src/js/pro/mdb-autocomplete.js';
//import '@sentry-insurance/mdbootstrap/src/js/pro/preloading.js';
//import '@sentry-insurance/mdbootstrap/src/js/pro/range-input.js';
//import '@sentry-insurance/mdbootstrap/src/js/pro/sidenav.js';
//import '@sentry-insurance/mdbootstrap/src/js/pro/smooth-scroll.js';
//import '@sentry-insurance/mdbootstrap/src/js/pro/sticky.js';

//import '@sentry-insurance/mdbootstrap/src/js/vendor/addons-pro/chat.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/addons-pro/multi-range.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/addons-pro/simple-charts.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/addons-pro/steppers.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/addons-pro/timeline.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/addons/imagesloaded.pkgd.min.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/addons/jquery.zmd.hierarchical-display.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/addons/masonry.pkgd.min.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/addons/rating.js';

//import '@sentry-insurance/mdbootstrap/src/js/vendor/free/chart.js';
import '@sentry-insurance/mdbootstrap/src/js/vendor/free/enhanced-modals.js';
import '@sentry-insurance/mdbootstrap/src/js/vendor/free/jquery.easing.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/free/velocity.js';
import '@sentry-insurance/mdbootstrap/src/js/vendor/free/waves.js';

//import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/jarallax.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/jarallax-video.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/jquery.sticky.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/lightbox.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/ofi.js';
////Using commonJS with imports loader as to not interfere with MDB's half-supported module implementaion in picker-date
//import 'imports-loader?type=commonjs&imports=single|jquery|$!@sentry-insurance/mdbootstrap/src/js/vendor/pro/picker-date.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/picker-date-time.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/scrollbar.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/toastr.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/vector-map.js';
//import '@sentry-insurance/mdbootstrap/src/js/vendor/pro/vector-map-world-mill.js';

//Include("~/Scripts/select2.js").
import 'select2';

//Include("~/Scripts/bootbox.js").
import 'bootbox';

//Include("~/Scripts/toastr.min.js").
import 'expose-loader?exposes=toastr!toastr';

//Include("~/Scripts/thenBy.min.js"));
import 'thenby';

//Include("~/Scripts/typeahead.bundle.js").
import 'typeahead.js';