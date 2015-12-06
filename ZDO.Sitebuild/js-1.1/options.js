var clrSel = "#ffe4cc"; // Same as background-color of .optionItem.selected in CSS
var clrEmph = "#ffc898"; // Same as border-bottom of .optionHead in CSS

var optScript = "both";
var optTones = "pleco";

$(document).ready(function () {
  parseOptionCookies();
  optionsEventWireup();
});

function parseOptionCookies() {
  // Check cookie for script
  var ckScript = getCookie("uiscript");
  if (ckScript !== null) optScript = ckScript;
  if (optScript === "simp") $("#optScriptSimplified").addClass("selected");
  else if (optScript === "trad") $("#optScriptTraditional").addClass("selected");
  else if (optScript === "both") $("#optScriptBoth").addClass("selected");
  // Check cookie for tone colors
  var ckTones = getCookie("uitones");
  if (ckTones !== null) optTones = ckTones;
  if (optTones === "none") $("#optToneColorsNone").addClass("selected");
  else if (optTones === "pleco") $("#optToneColorsPleco").addClass("selected");
  else if (optTones === "dummitt") $("#optToneColorsDummitt").addClass("selected");
}

function optionsEventWireup() {
  // Event handlers for mouse (desktop)
  var handlersMouse = {
    mousedown: function(e) {
      $(this).animate({backgroundColor: clrEmph}, 200);
    },
    click: function(e) {
      $(this).animate({backgroundColor: clrSel}, 400);
      selectOption(this.id);
    },
    mouseenter: function (e) {
      $(this).animate({backgroundColor: clrSel}, 400);
    },
    mouseleave: function (e) {
      var clr = $(this).hasClass('selected') ? clrSel : "transparent";
      $(this).animate({backgroundColor: clr}, 400);
    }
  };
  // Event handlers for mobile (touch)
  var handlersTouch = {
    click: function(e) {
      $(this).animate({backgroundColor: clrSel}, 400);
      selectOption(this.id);
    }
  };
  var handlers = isMobile ? handlersTouch : handlersMouse;
  // Script option set
  $("#optScriptSimplified").on(handlers);
  $("#optScriptTraditional").on(handlers);
  $("#optScriptBoth").on(handlers);

  // Tone colors option set
  $("#optToneColorsNone").on(handlers);
  $("#optToneColorsPleco").on(handlers);
  $("#optToneColorsDummitt").on(handlers);
}

function selectOption(id) {
  function unselectOption(optId) {
    if ($(optId).hasClass("selected")) {
      $(optId).removeClass("selected");
      $(optId).animate({backgroundColor: "transparent"}, 400);
    }
  }
  // Script option set
  if (id.indexOf("optScript") === 0) {
    unselectOption("#optScriptSimplified");
    unselectOption("#optScriptTraditional");
    unselectOption("#optScriptBoth");
  }
  // Tone colors option set
  else if (id.indexOf("optToneColors") === 0) {
    unselectOption("#optToneColorsNone");
    unselectOption("#optToneColorsPleco");
    unselectOption("#optToneColorsDummitt");
  }
  $("#" + id).addClass("selected");
  optionSelected(id);
}

function optionSelected(id) {
  var future = new Date(2038, 01, 01);

  // Script options
  if (id === "optScriptSimplified") createCookie("uiscript", "simp", future);
  else if (id === "optScriptTraditional") createCookie("uiscript", "trad", future);
  else if (id === "optScriptBoth") createCookie("uiscript", "both", future);

  // Tone color options
  if (id === "optToneColorsNone") createCookie("uitones", "none", future);
  else if (id === "optToneColorsPleco") createCookie("uitones", "pleco", future);
  else if (id === "optToneColorsDummitt") createCookie("uitones", "dummitt", future);

  // Parse cookies again: adds "selected" to element (redundantly), and updates global variable
  parseOptionCookies();
}