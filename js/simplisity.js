
var ajaxPostCmd = [];
var debugmode = false;

(function ($) {

    $.fn.getSimplisity = function (cmdurl, scmd, sfields, safter, spost) {
        if (debugmode) {
            // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
            console.log('[$.fn.getSimplisity] ', cmdurl, scmd, '#' + this.attr('id'), sfields, spost);
        }
        simplisityPost(cmdurl, scmd, spost, '#' + this.attr('id'), '', false, 0, sfields, true, safter);
    };

}(jQuery));


(function ($) {

    $.fn.activateSimplisityPanel = function (cmdurl) {

        simplisity_sessionfieldaction(); // set sessionfields to saved value.

        simplisity_assignevents(cmdurl);

        $(this).unbind("change");
        $(this).change(function () {
            simplisity_assignevents(cmdurl);
        });

    };

}(jQuery));

(function ($) {

    $.fn.simplisityStartUp = function (cmdurl, options) {

        var settings = $.extend({
            activatepanel: true,
            overlayclass: 'w3-overlay',
            debug: false
        }, options);

        debugmode = settings.debug;

        simplisity_createStaticPageFields(cmdurl, settings);
        simplisity_createTempPageFields();

        $('.simplisity_panel').each(function () {
            $(this).attr('s-activepanel', settings.activatepanel);
            ajaxPostCmd.push(this);
        });

        simplisity_panelAjaxFunction(ajaxPostCmd[ajaxPostCmd.length - 1]);

        $('.simplisity_loader').hide();

    };
}(jQuery));

function simplisity_createStaticPageFields(cmdurl, settings) {
    // inject static fields.
    jQuery('.simplisity_loader').remove();
    jQuery('#simplisity_systemkey').remove();
    jQuery('#simplisity_cmdurl').remove();
    jQuery('#simplisity_fileuploadlist').remove();
    jQuery('#simplisity_fileuploadbase64').remove();

    var elementstr = '<div class="' + settings.overlayclass + ' simplisity_loader " style="z-index:999;">';
    elementstr += '<span class="w3-display-middle">';
    elementstr += '<span class="material-icons w3-spin w3-jumbo">autorenew</span>';
    elementstr += '</span>';
    elementstr += '</div>';
    elementstr += '<input id="simplisity_systemkey" type="hidden" value="' + settings.systemkey + '" />';
    elementstr += '<input id="simplisity_cmdurl" type="hidden" value="' + cmdurl + '" />';
    elementstr += '<input id="simplisity_fileuploadlist" type="hidden" value="" />';
    elementstr += '<input id="simplisity_fileuploadbase64" type="hidden" value="" />';

    var elem = document.createElement('span');
    elem.innerHTML = elementstr;
    document.body.appendChild(elem);

}

function simplisity_createTempPageFields() {
    // inject any temporary fields that simplisity needs.
    var elementstr = '';
    if (jQuery('#simplisity_params').length === 0) {
        elementstr += '<input id="simplisity_params" type="hidden" value="" />';
        var elem = document.createElement('span');
        elem.innerHTML = elementstr;
        document.body.appendChild(elem);
    }
}
function simplisity_removepagefields() {
    // remove temporary fields that simplisity needs.
    jQuery('#simplisity_params').remove();
}

function simplisity_panelAjaxFunction(panelelement) {
    if ((typeof panelelement !== 'undefined') && panelelement !== '') {
        ajaxPostCmd.pop();

        var sreturn = '#' + jQuery(panelelement).attr('id');
        var activepanel = jQuery(panelelement).attr('s-activepanel');
        var cmdurl = jQuery('simplisity_cmdurl').val();

        if (activepanel) {
            jQuery(panelelement).activateSimplisityPanel(cmdurl);
        }

        simplisity_callserver(panelelement, cmdurl, sreturn);

        if (debugmode) {
            // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
            console.log('[simplisity_panelAjaxFunction()] s-cmd: ', jQuery(panelelement).attr('s-cmd'));
        }
    }
}

jQuery(document).on("simplisitypostgetcompleted", simplisity_nbxgetCompleted); // assign a completed event for the ajax calls

function simplisity_nbxgetCompleted(e) {

    if ((typeof e.safter !== 'undefined') && e.safter !== '') {
        var funclist = e.safter.split(',');
        for (var i = 0; i < funclist.length; i++) {
            window[funclist[i]]();
        }
    }

    // Action the session fields to populate the session fields.
    simplisity_sessionfieldaction();

    // clear any uploaded files after completed call
    jQuery('input[id*="simplisity_fileuploadlist"]').val('');
    jQuery('input[id*="simplisity_fileuploadbase64"]').val('');

    if (debugmode) {
        // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
        console.log('-------END AJAX CALL------- ');
    }

    simplisity_panelAjaxFunction(ajaxPostCmd[ajaxPostCmd.length - 1]);

    jQuery('.simplisity_fadeout').fadeOut(2000);

    if (e.sloader === true) {
        jQuery('.simplisity_loader').hide();
    }

}

function simplisityPost(scmdurl, scmd, spost, sreturn, slist, sappend, sindex, sfields, shideloader, safter, sdropdownlist, reload, sreturntype) {

    if (debugmode) {
        // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
        console.log('[simplisityPost()] scmdurl, scmd, spost, sreturn, slist, sappend, sindex, sfields, shideloader, safter, sdropdownlist:--->    ', scmdurl, scmd, spost, sreturn, slist, sappend, sindex, sfields, shideloader, safter, sdropdownlist);
    }

    if (typeof scmd !== 'undefined' && scmd !== '') {

        // Load sessionParams into fieldParams to post to server. 
        // These are to persist data across session, for search, sort, paging, etc.
        simplisity_sessionpost();

        var systemkey = simplisity_getsystemkey(sfields);

        if (typeof reload === 'undefined' || reload === '') {
            reload = 'false';
        }

        var cmdupdate = '';
        if (scmdurl.includes("?")) {
            // possible external call and cmd param not required.
            cmdupdate = scmdurl;
        }
        else {
            cmdupdate = scmdurl + '?cmd=' + scmd + '&systemkey=' + systemkey;
        }

        var jsonData = simplisity_ConvertFormToJSON(spost, slist, sfields);
        var jsonParam = simplisity_ConvertParamToJSON(sfields);

        if ((typeof sdropdownlist !== 'undefined') && sdropdownlist !== '') {

            if (debugmode) {
                // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
                console.log('------- START AJAX CALL [dropdown] ------- ' + scmd);
            }

            jQuery.ajax({
                type: "POST",
                url: cmdupdate,
                cache: false,
                async: true,
                dataType: 'json',
                timeout: 120000,
                data: { inputjson: encodeURIComponent(jsonData), paramjson: encodeURIComponent(jsonParam), simplisity_cmd: scmd },
                success: function (json) {
                    jQuery(sdropdownlist).html('');
                    var jsonObj = simplisity_parsejson(json);
                    for (var i = 0; i < jsonObj.length; i++) {
                        var obj = jsonObj[i];
                        jQuery(sdropdownlist).append("<option value='" + obj.key + "'>" + obj.value + "</option>");
                    }
                    jQuery('.simplisity_loader').hide();
                }
            });
        }
        else {

            if (debugmode) {
                // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
                console.log('------- START AJAX CALL------- ' + scmd);
            }

            var request = jQuery.ajax({
                type: "POST",
                url: cmdupdate,
                async: true,
                cache: false,
                timeout: 120000,
                data: { inputjson: encodeURIComponent(jsonData), paramjson: encodeURIComponent(jsonParam), simplisity_cmd: scmd }
            });

            request.done(function (data) {

                if (data !== 'noaction') {
                    if (sreturntype === 'json') {
                        data = JSON.stringify(data);
                    }
                    if ((typeof sreturn !== 'undefined') && sreturn !== '') {
                        if (sreturn === 'document') {
                            // replace the document (new FULL html page)
                            document.open();
                            document.write(data);
                            document.close();
                        } else {
                            if ((typeof sappend === 'undefined') || sappend === '' || sappend === false) {
                                jQuery(sreturn).children().remove();
                                jQuery(sreturn).html(data).trigger('change');
                            } else {
                                jQuery(sreturn).append(data).trigger('change');
                            }
                        }
                    }

                    if (debugmode) {
                        // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
                        //console.log('returned data: ' + data);
                    }

                    if (reload === 'true') {
                        location.reload();
                    } else {
                        // trigger completed.
                        jQuery.event.trigger({
                            type: "simplisitypostgetcompleted",
                            cmd: scmd,
                            sindex: sindex,
                            sloader: shideloader,
                            sreturn: sreturn,
                            safter: safter
                        });
                    }

                    jQuery('#simplisity_params').val(''); // clear param fields, so each call is fresh.
                }
            });

            request.fail(function (jqXHR, textStatus) {
                jQuery('.simplisity_loader').hide();
            });

        }

    }
}

async function simplisity_callBeforeFunction(element) {
    try {
        if ((typeof jQuery(element).attr('s-before') !== 'undefined') && jQuery(element).attr('s-before') !== '') {
            var funclist = jQuery(element).attr('s-before').split(',');
            for (var i = 0; i < funclist.length; i++) {
                window[funclist[i]]();
            }
        }
    }
    catch (e) {
        console.log('Error!', e);
    }
    return;
}

function simplisity_callSessionFields(element) {
    if ((typeof jQuery(element).attr('s-sessionfield') !== 'undefined') && jQuery(element).attr('s-sessionfield') !== '') {
        var funclist = jQuery(element).attr('s-sessionfield').split(',');
        for (var i = 0; i < funclist.length; i++) {
            simplisity_setSessionField(funclist[i].replace('#', ''), jQuery(funclist[i]).val());
        }
    }
    return;
}

async function simplisity_callserver(element, cmdurl, returncontainer, reload) {

    try {
        var scmd = jQuery(element).attr("s-cmd");
        if (typeof scmd !== 'undefined' && scmd !== '' && scmd !== null) {

            var sshowloader = jQuery(element).attr("s-showloader");
            if (sshowloader !== 'false') {
                jQuery('.simplisity_loader').show();
            }

            await simplisity_callBeforeFunction(element);

            await simplisity_callSessionFields(element);

            if (jQuery(element).attr("s-stop") !== 'stop') {

                var scmdurl = jQuery(element).attr("s-cmdurl");
                if (typeof scmdurl === 'undefined' || scmdurl === '') {
                    scmdurl = cmdurl;
                }
                if (typeof scmdurl === 'undefined' || scmdurl === '') {
                    scmdurl = jQuery('#simplisity_cmdurl').val();
                }

                var sreturn = jQuery(element).attr("s-return");
                if (typeof sreturn === 'undefined' || sreturn === '') {
                    sreturn = returncontainer;
                    if (typeof sreturn === 'undefined' || sreturn === '') {
                        sreturn = '#simplisity_startpanel';
                    }
                }

                if (typeof jQuery(element).attr("s-reload") !== 'undefined' && jQuery(element).attr("s-reload") !== '') {
                    reload = jQuery(element).attr("s-reload");
                }

                var spost = jQuery(element).attr("s-post");
                var slist = jQuery(element).attr("s-list");
                var sappend = jQuery(element).attr("s-append");
                var sindex = jQuery(element).attr("s-index");
                var sfields = jQuery(element).attr("s-fields");
                var safter = jQuery(element).attr("s-after");
                var shideloader = jQuery(element).attr("s-hideloader");
                var sdropdownlist = jQuery(element).attr("s-dropdownlist");
                var sreturntype = jQuery(element).attr("s-returntype");

                if (typeof scmd === 'undefined') {
                    scmd = '';
                }

                if (typeof sfields === 'undefined') {
                    sfields = '';
                }

                simplisity_setParamField('activevalue', jQuery(element).val());

                if (typeof shideloader === 'undefined') {
                    shideloader = true;
                }
                if (jQuery('input[id*="simplisity_fileuploadlist"]').val() !== '') {
                    if (typeof sfields === 'undefined' || sfields === '') {
                        sfields = '{"fileuploadlist":"' + jQuery('input[id*="simplisity_fileuploadlist"]').val() + '"}';
                    } else {
                        sfields = sfields.substring(0, sfields.length - 1) + ',"fileuploadlist":"' + jQuery('input[id*="simplisity_fileuploadlist"]').val() + '"}';
                    }
                }
                if (jQuery('input[id*="simplisity_fileuploadbase64"]').val() !== '') {
                    if (typeof sfields === 'undefined' || sfields === '') {
                        sfields = '{"fileuploadbase64":"' + jQuery('input[id*="simplisity_fileuploadbase64"]').val() + '"}';
                    } else {
                        sfields = sfields.substring(0, sfields.length - 1) + ',"fileuploadbase64":"' + jQuery('input[id*="simplisity_fileuploadbase64"]').val() + '"}';
                    }
                }

                simplisityPost(scmdurl, scmd, spost, sreturn, slist, sappend, sindex, sfields, shideloader, safter, sdropdownlist, reload, sreturntype);
            }
            else {
                jQuery(element).attr('s-stop', '');
                jQuery('.simplisity_loader').hide();
            }
        }
    }
    catch (e) {
        console.log('Error!', e);
    }
    return;
}

function simplisity_ConvertParamToJSON(sfields) {

    var viewData = {
        sfield: [],
        system: []
    };

    // Put s-fields into the json object.
    var jsonDataF = {};
    if (typeof sfields !== 'undefined' && sfields !== '') {
        var obj = JSON.parse(sfields);
        jsonDataF = simplisity_mergeJson({}, jsonDataF, obj);
    }

    // add param fields
    var paramfields = jQuery('#simplisity_params').val();
    if (typeof paramfields !== 'undefined' && paramfields !== '') {
        var obj2 = JSON.parse(paramfields);
        jsonDataF = simplisity_mergeJson({}, jsonDataF, obj2);
    }

    viewData.sfield.push(jsonDataF);

    var system = '{"systemkey":"' + simplisity_getsystemkey(sfields) + '","requesturl":"' + window.location.href + '"}';
    var systemobj = JSON.parse(system);
    viewData.system.push(systemobj);

    if (debugmode) {
        // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
        console.log('[simplisity_ConvertParamToJSON(sfields)] stringify json: ' + JSON.stringify(viewData));
    }

    return JSON.stringify(viewData);
}


function simplisity_ConvertFormToJSON(spost, slist, sfields) {
    var viewData = {
        sfield: [],
        system: [],
        postdata: [],
        listdata: []
    };

    // put input fields into the json object
    if (typeof spost !== 'undefined' && spost !== '') {
        var sposts = spost.split(',');
        sposts.forEach((post) => {
            jQuery(post).find('input, textarea, select').each(function () {

                // if html type is NOT one supported, then convert it to hidden.
                var htmlType = this.getAttribute("type");
                if (htmlType !== '' && htmlType !== 'checkbox' && htmlType !== 'text' && htmlType !== 'radio' && htmlType !== 'select' && htmlType !== 'hidden') {
                    htmlType = 'hidden';
                }

                if (this.getAttribute("s-update") !== 'ignore' && this.id !== '') {

                    if (this.getAttribute("s-datatype") === 'coded') {
                        postvalue = this.value || '';
                        postvalue = simplisity_encode(postvalue);
                    }
                    else {
                        postvalue = this.value || '';
                    }

                    var jsonData = {};
                    jsonData['id'] = this.id || '';
                    jsonData['value'] = postvalue;
                    jsonData['s-post'] = post || '';
                    jsonData['s-update'] = this.getAttribute("s-update") || '';
                    jsonData['s-datatype'] = this.getAttribute("s-datatype") || '';
                    jsonData['s-xpath'] = this.getAttribute("s-xpath") || '';
                    jsonData['type'] = htmlType || 'select';
                    jsonData['checked'] = jQuery(this).prop('checked') || '';
                    jsonData['name'] = this.getAttribute("name") || '';
                    viewData.postdata.push(jsonData);
                }

            });
        });
    }

    // crate any lists required.
    if (typeof slist !== 'undefined' && slist !== '') {
        var slists = slist.split(',');
        slists.forEach((list) => {

            var lp2 = 1;
            jQuery(list).each(function () {
                jQuery(this).find('input, textarea, select').each(function () {

                    // if html type is NOT one supported, then convert it to hidden.
                    var htmlType = this.getAttribute("type");
                    if (htmlType !== '' && htmlType !== 'checkbox' && htmlType !== 'text' && htmlType !== 'radio' && htmlType !== 'select' && htmlType !== 'hidden') {
                        htmlType = 'hidden';
                    }

                    if (this.getAttribute("s-update") !== 'ignore' && this.id !== '') {

                        if (this.getAttribute("s-datatype") === 'coded') {
                            postvalue = this.value || '';
                            postvalue = simplisity_encode(postvalue);
                        }
                        else {
                            postvalue = this.value || '';
                        }

                        var jsonDataL = {};
                        jsonDataL['id'] = this.id || '';
                        jsonDataL['value'] = postvalue || '';
                        jsonDataL['row'] = lp2.toString() || '';
                        jsonDataL['listname'] = list || '';
                        jsonDataL['s-update'] = this.getAttribute("s-update") || '';
                        jsonDataL['s-datatype'] = this.getAttribute("s-datatype") || '';
                        jsonDataL['s-xpath'] = this.getAttribute("s-xpath") || '';
                        jsonDataL['type'] = htmlType || 'select';
                        jsonDataL['checked'] = jQuery(this).prop('checked') || '';
                        jsonDataL['name'] = this.getAttribute("name") || '';
                        viewData.listdata.push(jsonDataL);

                    }
                });
                lp2 += 1;
            });

        });
    }

    // Put s-fields into the json object.
    var jsonDataF = {};
    if (typeof sfields !== 'undefined' && sfields !== '') {
        var obj = JSON.parse(sfields);
        jsonDataF = simplisity_mergeJson({}, jsonDataF, obj);
    }

    // add param fields
    var paramfields = jQuery('#simplisity_params').val();
    if (typeof paramfields !== 'undefined' && paramfields !== '') {
        var obj2 = JSON.parse(paramfields);
        jsonDataF = simplisity_mergeJson({}, jsonDataF, obj2);
    }

    viewData.sfield.push(jsonDataF);

    var system = '{"systemkey":"' + simplisity_getsystemkey(sfields) + '"}';
    var systemobj = JSON.parse(system);
    viewData.system.push(systemobj);


    if (debugmode) {
        // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
        console.log('json: ' + JSON.stringify(viewData));
    }

    return JSON.stringify(viewData);
}


function simplisity_getpostjson(spost) {
    var viewData = {
        postdata: []
    };

    // put input fields into the json object
    if (typeof spost !== 'undefined' && spost !== '') {
        var sposts = spost.split(',');
        sposts.forEach((post) => {
            jQuery(post).find('input, textarea, select').each(function () {

                if (this.getAttribute("s-update") !== 'ignore' && this.id !== '') {

                    if (this.getAttribute("s-datatype") === 'coded') {
                        postvalue = this.value || '';
                        postvalue = simplisity_encode(postvalue);
                    }
                    else {
                        postvalue = this.value || '';
                    }

                    var jsonData = {};
                    jsonData['id'] = this.id || '';
                    jsonData['value'] = postvalue;
                    jsonData['s-post'] = post || '';
                    jsonData['s-update'] = this.getAttribute("s-update") || '';
                    jsonData['s-datatype'] = this.getAttribute("s-datatype") || '';
                    jsonData['s-xpath'] = this.getAttribute("s-xpath") || '';
                    jsonData['type'] = this.getAttribute("type") || 'select';
                    jsonData['checked'] = jQuery(this).prop('checked') || '';
                    jsonData['name'] = this.getAttribute("name") || '';
                    viewData.postdata.push(jsonData);
                }

            });
        });
    }

    if (debugmode) {
        // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
        console.log('json: ' + JSON.stringify(viewData));
    }

    return JSON.stringify(viewData);
}


function simplisity_getlistjson(slist) {
    var viewData = {
        listdata: []
    };

    // crate any lists required.
    if (typeof slist !== 'undefined' && slist !== '') {
        var slists = slist.split(',');
        slists.forEach((list) => {

            var lp2 = 1;
            jQuery(list).each(function () {
                jQuery(this).find('input, textarea, select').each(function () {

                    if (this.getAttribute("s-update") !== 'ignore' && this.id !== '') {

                        if (this.getAttribute("s-datatype") === 'coded') {
                            postvalue = this.value || '';
                            postvalue = simplisity_encode(postvalue);
                        }
                        else {
                            postvalue = this.value || '';
                        }

                        var jsonDataL = {};
                        jsonDataL['id'] = this.id || '';
                        jsonDataL['value'] = postvalue || '';
                        jsonDataL['row'] = lp2.toString() || '';
                        jsonDataL['listname'] = list || '';
                        jsonDataL['s-update'] = this.getAttribute("s-update") || '';
                        jsonDataL['s-datatype'] = this.getAttribute("s-datatype") || '';
                        jsonDataL['s-xpath'] = this.getAttribute("s-xpath") || '';
                        jsonDataL['type'] = this.getAttribute("type") || 'select';
                        jsonDataL['checked'] = jQuery(this).prop('checked') || '';
                        jsonDataL['name'] = this.getAttribute("name") || '';
                        viewData.listdata.push(jsonDataL);
                    }
                });
                lp2 += 1;
            });

        });
    }

    if (debugmode) {
        // DEBUG ++++++++++++++++++++++++++++++++++++++++++++
        console.log('json: ' + JSON.stringify(viewData));
    }

    return JSON.stringify(viewData);
}



function simplisity_mergeJson(target) {
    for (var argi = 1; argi < arguments.length; argi++) {
        var source = arguments[argi];
        for (var key in source) {
            target[key] = source[key];
        }
    }
    return target;
}

function simplisity_removetablerow(item) {
    simplisity_remove(item, 'tr');
}

function simplisity_removelistitem(item) {
    simplisity_remove(item, 'li');
}

function simplisity_remove(item, tagName) {
    var slist = jQuery(item).attr('s-removelist').replace('.', '');
    var sindex = jQuery(item).attr('s-index');
    var liItem = jQuery(item).parents(tagName + "[s-index=" + sindex + "]").first().removeClass(slist).addClass(slist + '_deleted');
    var recylebin = jQuery(item).attr('s-recylebin');

    if (jQuery('#simplisity_recyclebin_' + recylebin).length > 0) {
        jQuery('#simplisity_recyclebin_' + recylebin).append(liItem);
    } else { liItem.remove(); }
    jQuery('.simplisity_itemundo[s-recylebin="' + recylebin + '"]').show();
}

function simplisity_undoremovelistitem(item) {
    var sreturn = jQuery(item).attr('s-return');
    var sundoselector = jQuery(item).attr('s-removelist') + "_deleted";
    var slist = jQuery(item).attr('s-removelist').replace('.', '');
    var recylebin = jQuery(item).attr('s-recylebin');

    if (jQuery('#simplisity_recyclebin_' + recylebin).length > 0) {
        jQuery(sreturn).append(jQuery('#simplisity_recyclebin_' + recylebin).find(sundoselector).last().removeClass(slist + "_deleted").addClass(slist));
    }
    if (jQuery('#simplisity_recyclebin_' + recylebin).children(sundoselector).length === 0) {
        jQuery('.simplisity_itemundo[s-recylebin="' + recylebin + '"]').hide();
    }

}

function simplisity_emptyrecyclebin(recyclebin) {
    jQuery('#simplisity_recyclebin_' + recyclebin).remove();
    jQuery('.simplisity_itemundo[s-recylebin="' + recyclebin + '"]').hide();
}


function simplisity_getCookieValue(cookiename) {
    var b = document.cookie.match('(^|;)\\s*' + cookiename + '\\s*=\\s*([^;]+)');
    return b ? b.pop() : '';
}

function simplisity_setCookieValue(cookiename, cookievalue) {
    document.cookie = cookiename + "=" + cookievalue + ";path=/;maxAge: 604800";
}

function simplisity_replaceAll(target, search, replacement) {
    return target.replace(new RegExp(search, 'g'), replacement);
}

function simplisity_setParamField(fieldkey, fieldvalue) {
    if (typeof fieldvalue !== 'undefined' && typeof fieldkey !== 'undefined' && fieldkey !== null && fieldkey !== 'null') {
        simplisity_createTempPageFields();
        var jsonParams = jQuery('#simplisity_params').val();
        var obj = {};
        if (typeof jsonParams !== 'undefined' && jsonParams !== '') {
            obj = JSON.parse(jsonParams);
        }
        obj[fieldkey] = fieldvalue;
        jQuery('#simplisity_params').val(JSON.stringify(obj));
    }
}

function simplisity_getParamField(fieldkey) {
    return simplisity_getField(jQuery('#simplisity_params').val(), fieldkey);
}

function simplisity_getField(sfields, fieldkey) {
    if (typeof sfields !== 'undefined' && sfields !== '') {
        if (typeof fieldkey !== 'undefined' && fieldkey !== '') {
            var obj = JSON.parse(sfields);
            return obj[fieldkey];
        }
    }
    return '';
}

function simplisity_getsystemkey(sfields) {
    var systemkey = simplisity_getField(sfields, 'systemkey');
    if (typeof systemkey === 'undefined' || systemkey === '') {
        systemkey = jQuery('#simplisity_systemkey').val();
    }
    return systemkey;
}

var simplisity_isTextInput = function (element) {
    var nodeName = element.nodeName;
    return (nodeName === 'INPUT' && element.type.toLowerCase() === 'text');
};

var simplisity_isSelect = function (element) {
    var nodeName = element.nodeName;
    return nodeName === 'SELECT';
};

function simplisity_encode(value) {
    var rtn = '';
    if (typeof value !== 'undefined' && value !== '') {
        for (var i = 0; i < value.length; i++) {
            rtn += value.charCodeAt(i) + '.';
        }
    }
    return rtn;
}

function simplisity_decode(value) {
    var rtn = '';
    if (typeof value !== 'undefined' && value !== '') {
        var valuelist = value.split('.');
        for (var i = 0; i < valuelist.length; i++) {
            if (valuelist[i] !== '') {
                rtn += String.fromCharCode(valuelist[i]);
            }
        }
    }
    return rtn;
}

function simplisity_injectlink(value) {
    var link = '<link rel="stylesheet" href="' + value + '">';
    jQuery('head').append(link);
}
function simplisity_injectscript(value) {
    jQuery.getScript(value, function () { console.log('script inject'); });
}
function simplisity_systemkey() {
    return jQuery('#simplisity_systemkey').val();
}
function simplisity_sessionjson() {
    var rtn = window.sessionStorage.getItem('simplisity_sessionparams'); // use session storage, idependant of browser.
    var out = '{"null":"null"}';
    if (rtn && typeof rtn !== 'undefined' && rtn !== '') {
        out = rtn;
    }
    try {
        var json = JSON.stringify(eval("(" + out + ")"));
        return json;
    }
    catch (err) {
        var json2 = JSON.stringify(eval("({'null':'null'})"));
        return json2;
    }
}
function simplisity_sessionremove() {
    window.sessionStorage.setItem('simplisity_sessionparams', ''); // use session storage, idependant of browser.
    simplisity_setCookieValue('simplisity_sessionparams', ''); // Cookies are created for CDSviewer modules.
}
function simplisity_sessionpost() {
    // This will post ALL data fields in the sessionParams back to the server in the param fields.
    var p = JSON.parse(simplisity_sessionjson());
    for (var key of Object.keys(p)) {
         simplisity_setParamField(key, p[key]);
    }

    // set a browser sessionid, to use serverside to identify the browser session
    var browser_sessionid = window.sessionStorage.getItem('browsersessionid');
    if (!browser_sessionid) {
        browser_sessionid = CreateUUID();
        window.sessionStorage.setItem('browsersessionid', browser_sessionid);
    }
    simplisity_setParamField('browsersessionid', browser_sessionid); // return browser_sessionid
    simplisity_setSessionField('browsersessionid', browser_sessionid) // add to session data so it is in the cookie

    // set a browserid, to use serverside to identify the browser
    var browser_id = window.localStorage.getItem('browserid');
    if (!browser_id) {
        browser_id = CreateUUID();
        window.localStorage.setItem('browserid', browser_id);
    }
    simplisity_setParamField('browserid', browser_id); // return browser_sessionid
    simplisity_setSessionField('browserid', browser_id) // add to session data so it is in the cookie

}

function CreateUUID() {
    rtn = ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    )
    // add time to try and get unique
    var d = new Date();
    rtn = rtn +"-" + d.getTime();
    return rtn;
}

function simplisity_setSessionField(fieldkey, fieldvalue) {
    if (typeof fieldvalue !== 'undefined' && fieldkey) {
        var jsonParams = simplisity_sessionjson();
        var obj = {};
        if (typeof jsonParams !== 'undefined' && jsonParams !== '') {
            obj = JSON.parse(jsonParams);
        }
        obj[fieldkey] = fieldvalue;

        window.sessionStorage.setItem('simplisity_sessionparams', JSON.stringify(obj)); // use session storage, idependant of browser.

        // cookie needed as cookie for toated version, where the module calls the toast server and we assign in the module before client based API call.
        simplisity_setCookieValue('simplisity_sessionparams', window.sessionStorage.getItem('simplisity_sessionparams'));

    }
}
function simplisity_getSessionField(fieldkey) {
    var result = JSON.parse(simplisity_sessionjson());
    return simplisity_getField(JSON.stringify(result), fieldkey);
}

function simplisity_parsejson(json) {
    var retval;
    if (typeof (json) === "string") {
        retval = JSON.parse(json);
    } else {
        // it's probably already an object
        retval = json;
    }
    return retval;
}


async function simplisity_initFileUpload(fileuploadselector) {
    try {
        var systemkey = simplisity_getsystemkey(jQuery(fileuploadselector).attr('s-fields'));  // use systemkey so we can have multiple cookie for Different systems.
        if (systemkey === '' || typeof systemkey === 'undefined') {
            systemkey = jQuery('#simplisity_systemkey').val();
        }

        var rexpr = jQuery(fileuploadselector).attr('s-regexpr');
        if (rexpr === '') {
            rexpr = '/(\.|\/)(gif|jpe?g|png|pdf|zip|xml|json)jQuery/i';
        }
        var maxFileSize = parseInt(jQuery(fileuploadselector).attr('s-maxfilesize'));
        if (maxFileSize === '') {
            maxFileSize = 5000000000; // 5GB
        }
        var maxChunkSize = parseInt(jQuery(fileuploadselector).attr('s-maxchunksize'));
        if (maxChunkSize === '') {
            maxChunkSize = 10000000; //10MB 
        }

        jQuery.cleanData(jQuery(fileuploadselector));

        jQuery(fileuploadselector).off();


        jQuery(fileuploadselector).parent().unbind("fileuploadprogressall");
        jQuery(fileuploadselector).parent().unbind("fileuploadsubmit");
        jQuery(fileuploadselector).parent().unbind("fileuploadadd");
        jQuery(fileuploadselector).parent().unbind("fileuploaddrop");
        jQuery(fileuploadselector).parent().unbind("fileuploadstop");

        jQuery(fileuploadselector).fileupload({
            url: jQuery(fileuploadselector).attr('s-uploadcmdurl'),
            maxFileSize: maxFileSize,
            maxChunkSize: maxChunkSize,
            acceptFileTypes: rexpr,
            dataType: 'json',
            dropZone: jQuery(fileuploadselector).parent(),
            formData: { systemkey: systemkey }
        }).prop('disabled', !jQuery.support.fileInput).parent().addClass(jQuery.support.fileInput ? undefined : 'disabled')
            .bind('fileuploadprogressall', function (e, data) {
                var progress = parseInt(data.loaded / data.total * 100, 10);
                jQuery('#simplisity-file-progress-bar').show();
                jQuery('.simplisity-file-progress-bar').css('width', progress + '%');
                jQuery('.simplisity-file-progress-bar').text(progress + '%');
            })
            .bind('fileuploadsubmit', function (e, data) {
                var identifier = simplisity_generateFileUniqueIdentifier(data);
                data.headers = jQuery.extend(data.headers, { "X-File-Identifier": identifier });
            })
            .bind('fileuploadadd', function (e, data) {
                jQuery('#simplisity-file-progress-bar').show();
                jQuery.each(data.files, function (index, file) {
                    jQuery('input[id*="simplisity_fileuploadlist"]').val(jQuery('input[id*="simplisity_fileuploadlist"]').val() + simplisity_encode(file.name) + ';');
                });
            })
            .bind('fileuploaddrop', function (e, data) {
                jQuery('#simplisity-file-progress-bar').show();
                jQuery.each(data.files, function (index, file) {
                    jQuery('input[id*="simplisity_fileuploadlist"]').val(jQuery('input[id*="simplisity_fileuploadlist"]').val() + simplisity_encode(file.name) + ';');
                });
                jQuery('.processing').show();
            })
            .bind('fileuploadstop', function (e) {

                if (jQuery('input[id*="simplisity_fileuploadlist"]').val() !== '') {

                    var reload = jQuery(fileuploadselector).attr('s-reload');
                    if (typeof reload === 'undefined' || reload === '') {
                        reload = 'true';
                    }
                    simplisity_callserver(jQuery(fileuploadselector), '', '', reload);

                    filesdone = 0;
                    jQuery('.processing').hide();
                    jQuery('#progress .progress-bar').css('width', '0');

                }

            });
    }
    catch (e) {
        console.log('Error!', e);
    }
}

function simplisity_generateFileUniqueIdentifier(data) {
    var file = data.files[0];
    result = file.relativePath || file.webkitRelativePath || file.fileName || file.name;
    return result;
}


function simplisity_assignevents(cmdurl) {

    jQuery('.simplisity_change').each(function (index) {
        jQuery(this).attr("s-index", index);

        jQuery(this).unbind("change");
        jQuery(this).change(function () {
            simplisity_callserver(this, cmdurl);
            return false;
        });
    });

    jQuery('.simplisity_click').each(function (index) {
        jQuery(this).attr("s-index", index);

        jQuery(this).unbind("click");
        jQuery(this).click(function () {
            simplisity_callserver(this, cmdurl);
            // add to browser bar and history
            var stateObj = jQuery(this).attr("s-fields");
            if (typeof (jQuery(this).attr("href")) !== 'undefined') {
                if (jQuery(this).attr("href").includes(window.location.hostname)) {
                    history.pushState(stateObj, "Title", jQuery(this).attr("href"));
                }
            }
            return false;
        });
    });

    jQuery('.simplisity_confirmclick').each(function (index) {

        jQuery(this).attr("s-index", index);

        jQuery(this).unbind("click");
        jQuery(this).click(function () {
            if (confirm(jQuery(this).attr("s-confirm"))) {
                simplisity_callserver(this, cmdurl);
                return false;
            }
        });

    });

    jQuery('.simplisity_removelistitem').each(function (index) {
        jQuery(this).attr("s-index", index);
        jQuery(this).parents('li').first().attr("s-index", index);

        jQuery(this).unbind("click");
        jQuery(this).click(function () {
            simplisity_removelistitem(this);
        });
    });

    jQuery('.simplisity_removetablerow').each(function (index) {
        jQuery(this).attr("s-index", index);
        jQuery(this).parents('tr').first().attr("s-index", index);

        jQuery(this).unbind("click");
        jQuery(this).click(function () {
            simplisity_removetablerow(this);
        });
    });

    jQuery('.simplisity_itemundo').each(function (index) {
        if (typeof jQuery(this).attr("s-recylebin") !== 'undefined') {
            if (typeof jQuery('#simplisity_recyclebin_' + jQuery(this).attr("s-recylebin")).val() === 'undefined') {
                var elementstr = '<div id="simplisity_recyclebin_' + jQuery(this).attr("s-recylebin") + '" style="display:none;" ></div>';
                var elem = document.createElement('span');
                elem.innerHTML = elementstr;
                document.body.appendChild(elem);
            }
        }
        jQuery(this).unbind("click");
        jQuery(this).click(function () {
            simplisity_undoremovelistitem(this);
        });
    });

    jQuery('.simplisity_fileupload').each(function (index) {
        simplisity_initFileUpload('#' + jQuery(this).attr('id'));
    });

    jQuery('.simplisity_base64upload').each(function (index) {
        jQuery(this).attr("s-index", index);

        jQuery(this).unbind("change");
        jQuery(this).change(function () {
            simplisity_base64wait(this, cmdurl);
            return false;
        });
    });


    jQuery('.simplisity_filedownload').each(function (index) {
        var params = "cmd=" + jQuery(this).attr('s-cmd');
        var sfields = jQuery(this).attr('s-fields');

        if (typeof sfields !== 'undefined' && sfields !== '') {

            var obj = JSON.parse(sfields);
            for (x in obj) {
                params = params + '&' + x + '=' + simplisity_encode(simplisity_getField(sfields, x));
            }

            var systemkey = simplisity_getsystemkey(sfields);  // use systemkey so we can have multiple cookie for Different systems.
            params = params + '&systemkey=' + systemkey;
        }
        var cmdurl = jQuery("#simplisity_cmdurl").val();
        jQuery(this).attr({
            href: cmdurl + '?' + params
        });
    });

}

// Actions the session fields to populate them.
function simplisity_sessionfieldaction() {

    jQuery('input.simplisity_sessionfield').each(function () {
        var v = simplisity_getSessionField(jQuery(this).attr('id'));
        if (typeof v !== 'undefined' && v !== '') {
            jQuery(this).val(v);
        }
    });

    jQuery('select.simplisity_sessionfield').each(function () {
        var v = simplisity_getSessionField(jQuery(this).attr('id'));
        if (typeof v !== 'undefined' && v !== '') {
            var selectctrl = jQuery(this);
            jQuery('#' + jQuery(this).attr('id') + ' > option').each(function () {
                if (this.value === v) {
                    jQuery(selectctrl).val(v);
                }
            });
        }
    });


}


async function simplisity_base64wait(element, cmdurl) {
    var filelist = $(element)[0].files;
    var flist = '';
    for (let i = 0; i < filelist.length; i++) {
        flist += simplisity_encode(filelist[i].name) + '*';
    }
    flist = flist.substring(0, flist.length - 1);
    $('#simplisity_fileuploadlist').val(flist);

    var result = await simplisity_fileListToBase64(filelist);
    var base64str = '';
    for (let i2 = 0; i2 < result.length; i2++) {
        base64str += result[i2];
    }
    base64str = base64str.substring(0, base64str.length - 1);
    $('#simplisity_fileuploadbase64').val(base64str);
    simplisity_callserver(element, cmdurl);
}

async function simplisity_fileListToBase64(fileList) {
    // create function which return resolved promise
    // with data:base64 string
    function getBase64(file) {
        const reader = new FileReader()
        return new Promise(resolve => {
            reader.onload = ev => {
                resolve(ev.target.result + '*')
            }
            reader.readAsDataURL(file)
        })
    }
    // here will be array of promisified functions
    const promises = []

    // loop through fileList with for loop
    for (let i = 0; i < fileList.length; i++) {
        promises.push(getBase64(fileList[i]))
    }

    // array with base64 strings
    return await Promise.all(promises)
}