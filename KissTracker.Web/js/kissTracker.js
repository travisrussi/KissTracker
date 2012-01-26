/** Page Level User Analytics using jQuery **/
/** This code tracks the following events on the page:

	-Client-Side Page Load Time (via Ready) - loaded, loadstart, loadend
    -Server-Side Render Time - rendertime, renderstart, renderend
    -Transfer Time - transfertime
	-Mouse Hover (via HoverIntent)
	-Mouse Click (via Click)
	-Link Click (via Click)
	-Scroll Depth (as percentage in 10% increments via Scroll)
	-Form Fields (captures data in input,select,textarea elements via Change)
	-Form Interaction (via Focus, Blur, Submit)
	-View Time (via OnBeforeUnload)
	
**/
/** Events are queued up into 2,000 byte chucks (Base64 Encoded; use the minQueueSizeToSend variable to change chunk size),
	then sent via GET AJAX request to server 
    
    The format of the encoded request is:
    
        t:[timeElapsedInMilliseconds];a:[EventAction];e:[ElementType];l:[ElementName];p:[PositionX]-[PositionY];v:[ElementValue]
        
    Each event is delimited with '|'.
    
    Some events force data to be sent (errors and clicks) regardless of the size.

**/

/** Add the following code to top of the page (just below the top <head> tag) for load and render times

	<head>
		var renderTimeStart = [RenderTimeStartJs]; //(optional) set this placeholder when the page starts rendering on the server to measure server rendering time
		var renderTimeEnd = [RenderTimeEndJs]; //(optional) set this placeholder when the page finishes rendering on the server to measure server rendering time
		var loadStartTime = (new Date()).getTime(); //(optional) measures page load time

**/
/** Add a reference to this JS file at the bottom of the page (just above the </body> tag)

		<script type="text/javascript">
			var trackingSessionId = "1234"; //(required) set this placeholder server-side to confirm identity of ajax request
			var trackingApiUrl = "//yoursite.com/KissTracker/"; //(optional) set the api url here, or hard-code it in the kissTracker.js file
        	window.jQuery || document.write('<script src="//ajax.googleapis.com/ajax/libs/jquery/1.6.2/jquery.min.js"><\/script>')
			function _loadTracker(u) {
				setTimeout(function () {
					var s = document.createElement('script'); 
					var f = document.getElementsByTagName('script')[0]; 
					s.type = 'text/javascript'; 
					s.async = true;
					s.src = u; 
					f.parentNode.insertBefore(s, f);
				}, 1);
			}
			_loadTracker('//yoursite.com/js/kisstracker.js');
		</script>
    </body>

**/

/** 
* jQuery captureNewEvent function - modified
*
* Version 1.0
* March 27, 2010
* http://weblogs.asp.net/wesleybakker/archive/2010/03/26/google-analytics-and-jquery-happy-together.aspx
*
* Copyright (c) 2010 Wesley Bakker
* Licensed under the GPL licenses.
* http://www.gnu.org/licenses/gpl.txt
**/

(function ($) {
    var methods = {
        getOptionValue: function (value, elem) { //, event) {
            if ($.isFunction(value)) {
                value = value.call(elem); //, event);
            }

            return value;
        },
        getCategory: function () {
            return this.nodeName;
        },
        getAction: function (event) {
            if (event)
                return event.type;
        },
        getLabel: function () {
            var self = $(this);
            try {
                if (self.is("a")) {
                    return self.attr("href");
                }
                else if (self.is("img")) {
                    return self.attr("src");
                }
                else if (self.attr("id")) {
                    return self.attr("id");
                }
                else {
                    return ""; // self.text();
                }
            } catch (e) {
                return "";
            }
        },
        getValue: function () {
            var self = $(this);
            if (self.is("input") || self.is("select") || self.is("textarea")) {
                return self.val();
            }
            else {
                return self.text();
            }
        },
        getPosition: function () {
            var self = $(this);
            if (self.offset())
                return self.offset().left + "-" + self.offset().top;
            else
                return "";
        }
    };

    $.fn.captureNewEvent = function (options) {
        var settings = {
            eventType: "click",
            category: methods.getCategory,
            action: methods.getAction,
            label: methods.getLabel,
            value: methods.getValue,
            direction: "",
            position: methods.getPosition,
            positionX: 0,
            positionY: 0,
            forceSend: false
        };

        if (options) $.extend(settings, options);

        this.each(function (i) {
            var category = methods.getOptionValue(settings.category, this);
            var action = settings.eventType;
            var label = '';
            var forceSend = settings.forceSend;
            var value = '';
            var direction = settings.direction;
            var position = '';

            if (action != 'unload') {

                label = methods.getOptionValue(settings.label, this);

                if (action != 'mouseclick' && action != 'mousedoubleclick' && action != 'loaded' && action != 'error' && action != 'scroll')
                    value = methods.getOptionValue(settings.value, this);

                if (settings.positionX && settings.positionY)
                    position = settings.positionX + '-' + settings.positionY;
                else
                    position = methods.getOptionValue(settings.position, this);
            }

            addEventToQueue("t:" + elapsedTimeInSeconds() + ";a:" + action + ";e:" + category + ";l:" + label + ";p:" + position + ";d:" + direction + ";v:" + value, forceSend);
        });

        return this;
    };
})(jQuery);

/**
* hoverIntent r6 // 2011.02.26 // jQuery 1.5.1+
* <http://cherne.net/brian/resources/jquery.hoverIntent.html>
* 
* @param  f  onMouseOver function || An object with configuration options
* @param  g  onMouseOut function  || Nothing (use configuration options object)
* @author    Brian Cherne brian(at)cherne(dot)net
*/
if(typeof hoverIntent!='function'){(function($){$.fn.hoverIntent=function(f,g){var cfg={sensitivity:7,interval:100,timeout:0};cfg=$.extend(cfg,g?{over:f,out:g}:f);var cX,cY,pX,pY;var track=function(ev){cX=ev.pageX;cY=ev.pageY};var compare=function(ev,ob){ob.hoverIntent_t=clearTimeout(ob.hoverIntent_t);if((Math.abs(pX-cX)+Math.abs(pY-cY))<cfg.sensitivity){$(ob).unbind("mousemove",track);ob.hoverIntent_s=1;return cfg.over.apply(ob,[ev])}else{pX=cX;pY=cY;ob.hoverIntent_t=setTimeout(function(){compare(ev,ob)},cfg.interval)}};var delay=function(ev,ob){ob.hoverIntent_t=clearTimeout(ob.hoverIntent_t);ob.hoverIntent_s=0;return cfg.out.apply(ob,[ev])};var handleHover=function(e){var ev=jQuery.extend({},e);var ob=this;if(ob.hoverIntent_t){ob.hoverIntent_t=clearTimeout(ob.hoverIntent_t)}if(e.type=="mouseenter"){pX=ev.pageX;pY=ev.pageY;$(ob).bind("mousemove",track);if(ob.hoverIntent_s!=1){ob.hoverIntent_t=setTimeout(function(){compare(ev,ob)},cfg.interval)}}else{$(ob).unbind("mousemove",track);if(ob.hoverIntent_s==1){ob.hoverIntent_t=setTimeout(function(){delay(ev,ob)},cfg.timeout)}}};return this.bind('mouseenter',handleHover).bind('mouseleave',handleHover)}})(jQuery);}

/*
* Base64 code from Tyler Akins -- http://rumkin.com
* http://rumkin.com/tools/compression/base64.php
*
* This code was written by Tyler Akins and has been placed in the
* public domain.  It would be nice if you left this header intact.
*/

if (!keyStr)
    var keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
if (!ua)
    var ua = navigator.userAgent.toLowerCase(); if (ua.indexOf(" chrome/") >= 0 || ua.indexOf(" firefox/") >= 0 || ua.indexOf(" gecko/") >= 0) { var StringMaker = function () { this.str = ""; this.length = 0; this.append = function (a) { this.str += a; this.length += a.length }; this.prepend = function (a) { this.str = a + this.str; this.length += a.length }; this.toString = function () { return this.str } } } else { var StringMaker = function () { this.parts = []; this.length = 0; this.append = function (a) { this.parts.push(a); this.length += a.length }; this.prepend = function (a) { this.parts.unshift(a); this.length += a.length }; this.toString = function () { return this.parts.join("") } } }

if (typeof encode64 != 'function') { function encode64(a) { var b = new StringMaker; var c, d, e; var f, g, h, i; var j = 0; while (j < a.length) { c = a.charCodeAt(j++); d = a.charCodeAt(j++); e = a.charCodeAt(j++); f = c >> 2; g = (c & 3) << 4 | d >> 4; h = (d & 15) << 2 | e >> 6; i = e & 63; if (isNaN(d)) { h = i = 64 } else if (isNaN(e)) { i = 64 } b.append(keyStr.charAt(f) + keyStr.charAt(g) + keyStr.charAt(h) + keyStr.charAt(i)) } return b.toString() } }
if (typeof decode64 != 'function') { function decode64(a) { var b = new StringMaker; var c, d, e; var f, g, h, i; var j = 0; a = a.replace(/[^A-Za-z0-9\+\/\=]/g, ""); while (j < a.length) { f = keyStr.indexOf(a.charAt(j++)); g = keyStr.indexOf(a.charAt(j++)); h = keyStr.indexOf(a.charAt(j++)); i = keyStr.indexOf(a.charAt(j++)); c = f << 2 | g >> 4; d = (g & 15) << 4 | h >> 2; e = (h & 3) << 6 | i; b.append(String.fromCharCode(c)); if (h != 64) { b.append(String.fromCharCode(d)) } if (i != 64) { b.append(String.fromCharCode(e)) } } return b.toString() }}


/** 
* Ensure debugging to console is setup
* http://stackoverflow.com/questions/217957/how-to-print-debug-messages-in-the-google-chrome-javascript-console
**/
if(!window.console)console={};console.log=console.log||function(){};console.warn=console.warn||function(){};console.error=console.error||function(){};console.info=console.info||function(){}

/****************************************************************************/
/****************************************************************************/
/****************************************************************************/


/* Global Variables */
var timeRenderStart = (new Date()).getTime(),
	timeRenderEnd = (new Date()).getTime(),
	timeStart = (new Date()).getTime(),
	timeLast = 0,
	eventQueue = '',
	minQueueSizeToSend = 2000,
	scrollPrevious = 0,
	sessionId,
    apiUrl = 'http://yoursite.com/KissTracker/';

/* get elapsed time since the page was loaded */
function elapsedTimeInSeconds() {
	timeLast = (new Date()).getTime() - timeStart;
	return timeLast;
}

/* get elapsed time for rendering the page on the server */
function elapsedTimeInSecondsForRender() {
	var timeForRender = timeRenderEnd - timeRenderStart;
	return timeForRender;
}

/* get elapsed time for transmitting page to client */
function elapsedTimeInSecondsToTransferPage() {
	var timeForTransmission = timeStart - timeRenderEnd;
	return timeForTransmission;
}

/* Add a new event to the queue to be sent */
function addEventToQueue(eventString, forceSend) {

	if (!eventString)
		return;
	
	var encodedEventQueue = encode64(eventQueue + (eventQueue ? '|' : '') + eventString);

	if (encodedEventQueue.length > minQueueSizeToSend || forceSend) {
	
		if (encodedEventQueue.length > Math.floor(minQueueSizeToSend * 1.25) && !forceSend) {
			encodedEventQueue = encode64(eventQueue);
			sendEventQueue(encodedEventQueue);
			eventQueue = eventString;
		} else {
			sendEventQueue(encodedEventQueue, (forceSend != undefined ? !forceSend : true));
			eventQueue = '';
		}
	} else {
		eventQueue += (eventQueue ? '|' : '') + eventString;
	}

}

/* Send event queue to server */
function sendEventQueue(dataToSend, sendAsync) {

	if (!dataToSend)
		return;
		
	var async = (sendAsync != undefined ? sendAsync : true);
		
	$.ajax({
	  type: "GET",
	  url: apiUrl + 'TrackEvent?sessionId=' + sessionId + '&',
	  async: async,
	  data: {'jsonString':dataToSend},
	  contentType: "application/json; charset=utf-8",
	  dataType: "json",
	  success: function(msg) {
	  },
	  error: function(xhr, ajaxOptions, thrownError){
	  	try {
		  	console.error ("Error sending Ajax: " + xhr.status + "-" + thrownError);
		} catch (e) { }
	  }
	});
	
}

$(document).ready(function () {

    /** load variables from start of page load **/
    if (renderTimeStart)
        timeRenderStart = renderTimeStart;
    if (renderTimeEnd)
        timeRenderEnd = renderTimeEnd;
    if (loadStartTime)
        timeStart = loadStartTime;
    if (trackingSessionId)
    	sessionId = trackingSessionId;
    if (trackingApiUrl)
        apiUrl = trackingApiUrl;

    /** attach event handlers **/

    //trap ajax errors
    $(document).ajaxError(function (e, jqxhr, settings, exception) {
        $(this).captureNewEvent({
            eventType: "error",
            value: exception,
            forceSend: true
        });
    });

    //Mouse Hover (via HoverIntent)
    $('a,input,select,textarea').hoverIntent({
        over: function () {
            $(this).captureNewEvent({
                eventType: "mousehover"
            });
        },
        out: function () { },
        interval: 400
    });

    //Mouse Click (via Click)
    $(document).bind('click', function (e) {
        $(e.toElement).captureNewEvent({
            eventType: "mouseclick",
            positionX: e.pageX,
            positionY: e.pageY,
            forceSend: true
        });
    });

    //Link Click (via Click)
    $("a").bind('click', function () {
        $(this).captureNewEvent({
            eventType: "linkclick"
        });
    });

    //Scroll Depth (as percentage in 10% increments via Scroll)
    $(document).scroll(function () {
        var $w = $(this),
			scroll_top = $w.scrollTop(),
			total_height = $w.height(),
			viewable_area = $(window).height(),
			scrollCurrent = Math.floor((scroll_top) / (total_height - viewable_area) * 100);

        if (scrollPrevious < 99 && scrollCurrent >= 99)
            $w.captureNewEvent({
                eventType: "scroll",
                value: 100,
                direction: "down"
            });
        else if (scrollPrevious < 75 && scrollCurrent >= 75)
            $w.captureNewEvent({
                eventType: "scroll",
                value: 75,
                direction: "down"
            });
        else if (scrollPrevious > 75 && scrollCurrent <= 75)
            $w.captureNewEvent({
                eventType: "scroll",
                value: 75,
                direction: "up"
            });
        else if (scrollPrevious < 50 && scrollCurrent >= 50)
            $w.captureNewEvent({
                eventType: "scroll",
                value: 50,
                direction: "down"
            });
        else if (scrollPrevious > 50 && scrollCurrent <= 50)
            $w.captureNewEvent({
                eventType: "scroll",
                value: 50,
                direction: "up"
            });
        else if (scrollPrevious < 25 && scrollCurrent >= 25)
            $w.captureNewEvent({
                eventType: "scroll",
                value: 25,
                direction: "down"
            });
        else if (scrollPrevious > 25 && scrollCurrent <= 25)
            $w.captureNewEvent({
                eventType: "scroll",
                value: 25,
                direction: "up"
            });
        else if (scrollPrevious >= 25 && scrollCurrent <= 1)
            $w.captureNewEvent({
                eventType: "scroll",
                value: 0,
                direction: "up"
            });

        scrollPrevious = scrollCurrent;
    });

    //Form Fields (captures data in input,select,textarea elements via Change)
    //Form Interaction (via Focus, Blur)
    //does not capture text in password, hidden, read-only fields or fields with the 'protected' class
    //lastPass autopopulates textboxes, so try to capture a keypress first
    var keyPressHasHappened = false;

    $('input:not(:password,[readonly],:hidden,.protected),select,textarea')
        .bind('keydown', function () {
            keyPressHasHappened = true;
        })
        .change(function () {
            if (keyPressHasHappened) {
                $(this).captureNewEvent({
                    eventType: "textchange"
                });
            }
        })
		.blur(function () {
			$(this).captureNewEvent({
				eventType: "blur"
			});
		});

    //Form Interaction (via Submit)
    $('form').submit(function () {
        $(this).captureNewEvent({
            eventType: "submit",
            forceSend: true
        });
    });
    $("input[type='submit']").bind('click', function () {
        $(this).captureNewEvent({
            eventType: "submit",
            forceSend: true
        });
    });

    //View Time (via OnBeforeUnload)
    window.onbeforeunload = function () {
        $(this).captureNewEvent({
            eventType: "unload",
            forceSend: true
        });
    };

    //Load, Render, Transfer Time (via Ready)
    $(document).captureNewEvent({
        eventType: "loaded",
        value: elapsedTimeInSeconds()
    })
    .captureNewEvent({
        eventType: "loadstart",
        value: timeStart
    })
    .captureNewEvent({
        eventType: "loadend",
        value: timeLast
    })
	.captureNewEvent({
	    eventType: "rendertime",
	    value: elapsedTimeInSecondsForRender()
	})
    .captureNewEvent({
        eventType: "renderstart",
        value: timeRenderStart
    })
    .captureNewEvent({
        eventType: "renderend",
        value: timeRenderEnd
    })
	.captureNewEvent({
	    eventType: "transfertime",
	    value: elapsedTimeInSecondsToTransferPage()
	});

});