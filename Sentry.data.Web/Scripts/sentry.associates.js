
var Associates = {
    nameCtrl: null,

    showPresencePopup: function (sipAddress, target) {
        if (this.nameCtrl) {
            var eLeft = $(target).offset().left;
            var x = eLeft - $(window).scrollLeft();

            var eTop = $(target).offset().top;
            var y = eTop - $(window).scrollTop();

            this.nameCtrl.ShowOOUI(sipAddress, 0, x, y);
        }
    },

    hidePresencePopup: function () {
        if (this.nameCtrl) {
            this.nameCtrl.HideOOUI();
        }
    }
};

(function ($) {
    "use strict";


    if (window.ActiveXObject) {
        Associates.nameCtrl = new ActiveXObject("Name.NameCtrl");
    } else {
        try {
            Associates.nameCtrl = new ActiveXObject("Name.NameCtrl");
        } catch (e) {
            Associates.nameCtrl = (function (b) {
                var c = null;
                try {
                    c = document.getElementById(b);
                    if (!Boolean(c) && (Boolean(navigator.mimeTypes) && navigator.mimeTypes[b] && navigator.mimeTypes[b].enabledPlugin)) {
                        var a = document.createElement("object");
                        a.id = b;
                        a.type = b;
                        a.width = "0";
                        a.height = "0";
                        a.style.setProperty("visibility", "hidden", "");
                        document.body.appendChild(a);
                        c = document.getElementById(b)
                    }
                } catch (d) {
                    c = null
                }
                return c
            })("application/x-sharepoint-uc");
        }
    }

    function SetPresenceStatus(element, status) {
        element.removeClass("status-available status-offline status-away status-inacall status-outofoffice status-busy status-donotdisturb status-unknown");
        //console.trace(element[0].id) + ": " + status);
        switch (status) {
            case 0:
            case 11:  //ooo
                element.addClass('status-online');
                break;
            case 1:
            case 12:  //ooo
                element.addClass('status-offline');
                break;
            case 2:
            case 4:
            case 6:
            case 8:
            case 13:  //ooo
            case 16:
            case 17:  //ooo
                element.addClass('status-away');
                break;
            case 3:
            case 5:
            case 7:
            case 10:
            case 14:  //ooo
            case 19:
            case 20:  //ooo
                element.addClass('status-busy');
                break;
            case 9:
            case 15:  //ooo
            case 21:
                element.addClass('status-donotdisturb');
                break;
            case 18:
                element.addClass('status-blocked');
                break;
        }
    }

    if (Associates.nameCtrl && Associates.nameCtrl.PresenceEnabled) {

        Associates.nameCtrl.OnStatusChange = function (userName, status, id) {
            var element = $("#" + id);
            SetPresenceStatus(element, status);
        };

    }
    
    // The server URL has no default setting; you must set it via $.assocSetup({ url: "yourUrl" })
    var _baseUrl = "",
        _apiPath = "",
        _imgPath = "/Public/Images",

        // Every presence control on a page must have a unique id; using the associate id is not enough if the
        // same associate appears more than once.
        _tagInstance = 1,

        // A unique static object to identify the special item in the autocomplete list that is the "more" ellipsis (...)
        _autocompleteMore = {},

        _getAssociatesUrl = function () {
            return _baseUrl + _apiPath;
        },

        _getImageUrl = function (assoc, height) {
            return "http://sentryphoto.sentry.com/Employee/" + assoc.Id + "/height/" + height;
        },

        _defaultTaggerOptions = {
            "id": "associate-id",     // The data attribute that contains the associate id to lookup and use to tag.
            "class": "associate-tag", // The CSS class to apply to the element
            "presence": true,         // When using the default render, show the Office Communicator presence control?
            "includeInactive": true  // By default, include inactive associates in the associate lookup
        };

    // ############## Setup Method ##############

    $.extend({
        assocSetup: function (options) {
            var opts = $.extend({}, {
                url: "" // The service API URL, e.g. https://hrempsecurequal.sentry.com/api/associates
            }, options),
                parts = opts.url.split("/"),
                origin = parts.slice(0, 3).join("/"),
                pathname = parts.slice(3).join("/");
            _baseUrl = origin;
            _apiPath = "/" + pathname;

            if (opts.tagger) {
                $.extend(_defaultTaggerOptions, opts.tagger);
            }
        }
    });

    // ############## Tagging Plugin ##############

    var _assocTagger = {

        sipAddresses: [],

        familiarName: function (assoc) {
            if (assoc.FamiliarName !== "") {
                return assoc.FamiliarName + " " + assoc.LastName;
            } else {
                return assoc.FirstName + " " + assoc.LastName;
            }
        },

        _defaultCardProperties: function (assoc) {
            return [
                ["Id", assoc.Id],
                ["Name", assoc.FullName],
                ["Title", assoc.Title],
                ["Email", assoc.WorkEmailAddress],
                ["Phone", assoc.WorkPhoneNumber],
                ["Mail Drop", assoc.MailDrop]
            ];
        },

        _createDialogCard: function (assoc, pairs, id) {
            var returnDiv = $('<div id="' + id + '">');

            returnDiv.append($("<img>", {
                "class": "associate-card-image",
                src: _getImageUrl(assoc, 160),
                alt: "profile picture"
            })
                .css({ "max-width": "160px", "float": "left", "margin": "6px" }));
            returnDiv.append($("<table>", { "class": "associate-card-props" })
                .append($.map(pairs, function (item) {
                    return $("<tr>")
                        .append($("<td>").html(item[0]).css({
                            "border-right": "1px solid #AAA",
                            "font-size": "smaller",
                            "text-align": "right",
                            "padding": "5px",
                            "min-width": "80px"
                        }))
                        .append($("<td>").html(item[1]).css({
                            "padding": "5px",
                            "min-width": "100px"
                        }));
                })));
            return returnDiv;
        },

        _createDialogElement: function (assoc, options, id) {
            return this._createDialogCard(assoc, options.cardProperties.call(this, assoc), id);
        },

        _createDialog: function ($tag, assoc, options) {
            var uniqueId = _tagInstance++;
            var closeId = assoc.Id + "_" + uniqueId + "_close",
                containerId = assoc.Id + "_" + uniqueId + "_container",
                popup = this._createDialogElement(assoc, options, containerId),
                outer = $("<div>").append(popup);

            $tag.popover({
                container: 'body',
                title: this._createCardHeader(this.familiarName(assoc), closeId),
                html: true,
                content: outer.html(),
                template: '<div class="popover assocTag-popover" role="tooltip">' +
                          '<div class="arrow"></div><h3 class="popover-title"></h3>' +
                          '<div class="popover-content"></div></div>'
            });

            $tag.on('shown.bs.popover', function () {
                $('#' + closeId).click(function () {
                    $tag.popover('hide');
                });
            });

            return outer;
        },

        _createCardIcon: function () {
            return $("<img>", {
                src: _baseUrl + _imgPath + "/card_icon.png",
                alt: "associate card icon",
                class: "associate-card-icon"
            });
        },
 
        _createCardHeader: function (name, id) {
            return "<b>" + name + "</b><span style='float: right; cursor: pointer;' id='" + id + "' class='glyphicon glyphicon-remove'></span>";;
        },

        createPresence: function (assoc) {
            var idPresenceIcon = "assoc-presence-" + assoc.Id + "-" + (_tagInstance++);

            var presence =
                $("<div>", {
                    id: idPresenceIcon,
                    class: "presense-icon status-unknown",
                    onmouseover: "Associates.showPresencePopup('" + assoc.SipAddress + "', this)",
                    onmouseout: "Associates.hidePresencePopup()"
                });

            if (Associates.nameCtrl && Associates.nameCtrl.PresenceEnabled) {
                var status = Associates.nameCtrl.GetStatus(assoc.SipAddress, idPresenceIcon);
                if (this.sipAddresses.indexOf(assoc.SipAddress) > -1) {
                    // We've already seen this sipAddress, so set the presence now (we won't get an async notification of status change)
                    SetPresenceStatus(presence, status);                 
                } else {
                    this.sipAddresses.push(assoc.SipAddress);  // make sure we record that we saw this sipAddress already.. we'll let the async notification of status change update the icon
                }
            }
             
            return presence;
        },

        _retrieveAssociates: function (assocIds, options, success) {
            var data = $.extend({ "ids": assocIds }, options);
            $.ajax({
                url: _getAssociatesUrl(),
                contentType: "application/json; charset=utf-8",
                data: data,
                dataType: "jsonp",
                traditional: true,
                success: function (data) {
                    success(data.Associates);
                }
                // TODO: handle error
            });
        },

        // Default tag rendering function.  Appends an optional presence control, a span with the associate
        // name, and a contact-card image button with a popup dialog attached.
        _buildTag: function ($tag, assoc, options) {
            if (assoc) {
            var self = this;
            $tag.addClass(options["class"]);
            $tag.html("");
            if (options.presence) {
                var pres = self.createPresence(assoc);
                pres.css({ "float": "left", "border-width": "0px" });
                $tag.append(pres);
            }

            var nameSpan = $("<span>")
                .html(self.familiarName(assoc))
                .css({ "padding": "4px" }),
                cardIcon = self._createCardIcon()
                .css({ "cursor": "pointer" });

            $tag.append(nameSpan);
            $tag.append(cardIcon);
            self._createDialog(cardIcon, assoc, options);
            //$tag.append(self._createDialog(cardIcon, assoc, options));
            return $tag;
            }
        },

        // The main tagging function, this inspects all of the selected elements and reformats the html
        // to be a "tag".  
        // If an associate object is provided, that associate is immediately applied (typically called on a
        // single element) otherwise it queries a data attribute (specified by option "id") for the associate
        // id and makes a ajax request for the associate objects.  When that request returns successfully the
        // elements are transformed.
        init: function (options) {
            var self = _assocTagger,
                opts = $.extend({
                    associate: null,        // The associate object to use to tag the element(s), if null it uses "id" option.
                    render: self._buildTag, // The function used to render the tag, with prototype function (element, associate, options).
                    cardProperties: self._defaultCardProperties // When using the default render, what properties to show on the contact card.
                }, _defaultTaggerOptions, options),
                associate = opts.associate,
                idProp = opts.id,
                elements = {},
                assocIds = [];

            this.each(function () {
                var $this = $(this);
                if (associate !== null) {
                    opts.render.call(self, $this, associate, opts);
                } else if (idProp !== null) {
                    var assocId = $this.data(idProp);
                    if (assocId) {
                        if (elements[assocId] == undefined)
                            elements[assocId] = [];
                        elements[assocId].push($this);
                        if ($.inArray(assocId, assocIds) < 0)
                            assocIds.push(assocId);
                    }
                }
            });

            if (assocIds.length > 0) {
                var serviceOpts = { includeInactive: opts.includeInactive };
                self._retrieveAssociates(assocIds, serviceOpts, function (associates) {
                    for (var i = 0; i < associates.length; ++i) {
                        var assoc = associates[i];
                        var element = elements[assoc.Id];
                        if (element !== undefined) {
                            for (var j = 0; j < element.length; ++j) {
                                opts.render.call(self, element[j], assoc, opts);
                            }
                            //remove this associate ID from the list of all associate IDs
                            assocIds = $.grep(assocIds, function (id) {
                                return id != assoc.Id
                            });
                        }
                        }
                    //if there's any leftover associate IDs that weren't found, still call the render function
                    if (assocIds.length > 0) {
                        for (var i = 0; i < assocIds.length; ++i) {
                            var element = elements[assocIds[i]];
                            if (element !== undefined) {
                                for (var j = 0; j < element.length; ++j) {
                                    opts.render.call(self, element[j], null, opts);
                    }
                            }
                        }
                    }
                });
            }

            return this;
        }
    };

    $.fn.assocTag = function (method) {
        if (_assocTagger[method]) {
            return _assocTagger[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return _assocTagger.init.apply(this, arguments);
        } else {
            $.error('Method ' + method + ' does not exist on jQuery.assocTag');
        }
    };

    // ############## Autocomplete Widget ##############
    $.fn.assocAutocomplete = function (opts) {
        var ttself = {};
        var self = this,
            options = { //Default options
                minLength: 3,
                maxResults: 20,
                filter: null,
                filterGroup: null,
                filterPermission: null,
                includeNewHires: null,
                includeInactive: null,
                associateSelected: null,
                close: null
            };

        options = $.extend({}, options, opts);

        //PUBLIC
        this.selection = null;

        this.hasSelection = function () {
            if (self.selection != null) {
                return true;
            } else {
                return false;
            }
        }

        this.createTag = function (options) {
            var opts = $.extend({ associate: self.selection }, options);
            return $("<div>").assocTag(opts);
        }

        this.clear = function () {
            self.selection = null;
            $(self).typeahead('val', "");
        }

        //PRIVATE
        function source(request, response) {
            // TODO: check cache?
            var self = this;
            searchAssociates(request, function (associateList) {
                if (typeof options.filter == 'function') {
                    associateList = options.filter(associateList);
                }

                var mapped = $.map(associateList, function (item) {
                    return {
                        label: item.FullName,
                        value: item.FullName,
                        key: item
                    };
                });

                if (mapped.length > options.maxResults) {
                    mapped = mapped.slice(0, options.maxResults);
                    mapped.push({ label: "", value: "", key: _autocompleteMore });
                }

                // TODO: put in cache?
                response(mapped);
            });
        }

        function createAutocompleteItem(item) {
            if (item.key == _autocompleteMore) {
                return _createAutocompleteMoreItem();
            }
            var assoc = item.key;
            return $("<a>").append($("<table>").append(
                        $("<tr>").append(
                            $("<td>").css({ "width": "32px", "height": "32px" }).append(
                                $("<img>", {
                                    "class": "associate-autocomplete-image",
                                    src: _getImageUrl(assoc, 32),
                                    alt: "profile picture"
                                })
                            )
                        ).append(
                            $("<td>").css({ "padding-left": "4px", "font-size": "small", "vertical-align": "middle", 'white-space': 'nowrap' }).html(assoc.FullName)
                        )
                   ));
        }

        function _createAutocompleteMoreItem() {
            return $("<span>")
                    .html("...")
                    .css({ "margin-left": "4px", "font-size": "small" });
        }

        function searchAssociates(searchStr, success) {
            $.ajax({
                url: _getAssociatesUrl(),
                dataType: "jsonp",
                data: {
                    search: searchStr,
                    limit: options.maxResults + 1,
                    filterGroup: options.filterGroup,
                    filterPermission: options.filterPermission,
                    includeNewHires: options.includeNewHires,
                    includeInactive: options.includeInactive
                },
                success: function (data) {
                    success(data.Associates);
                }
                // TODO: handle error
            });
        }

        function init() {
            //Init Typeahead
            ttself = $(self).typeahead({
                minLength: options.minLength,
                highlight: true
            }, {
                name: 'associates',
                source: source,
                templates: {
                    suggestion: createAutocompleteItem
                }
            });

            //Handle selection
            $(self).on('typeahead:selected typeahead:autocompleted', function (event, obj, dataSetName) {
                if (obj.key == _autocompleteMore)
                    self.selection = null;
                else
                    self.selection = obj.key;
                if (typeof options.associateSelected == 'function') options.associateSelected(self.selection);
            });

            //Handle close
            $(self).on('typeahead:closed', function (event, obj, dataSetName) {
                if (typeof options.close == 'function') options.close();
            });
        }
       
        init();

        return this;
    }

})(jQuery);
