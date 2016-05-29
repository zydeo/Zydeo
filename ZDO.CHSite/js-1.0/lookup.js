/// <reference path="x-jquery-2.1.4.min.js" />
/// <reference path="x-jquery.color-2.1.2.min.js" />
/// <reference path="x-jquery.tooltipster.min.js" />
/// <reference path="strings-hu.js" />
/// <reference path="page.js" />
/// <reference path="strokeanim.js" />

var zdLookup = (function () {
  var clrSel = "#ffe4cc"; // Same as background-color of .optionItem.selected in CSS
  var clrEmph = "#ffc898"; // Same as border-bottom of .optionHead in CSS

  var optScript = "both";
  var optTones = "pleco";

  var optionsTemplate =
    '<div id="optionsTail">&nbsp;</div>\n' +
    '<div id="optionsHeader">\n' +
    '  <span id="optionsTitle">{{options-title}}</span>\n' +
    '  <span id="optionsClose">X</span>\n' +
    '</div>\n' +
    '<div id="searchOptions">\n' +
    '  <div class="optionHead">{{options-script}}</div>\n' +
    '  <div class="optionBody">\n' +
    '    <div class="optionItem" id="optScriptSimplified">\n' +
    '      <span class="optionLabel">{{options-simplified}}</span>\n' +
    '      <span class="optionExampleHan">汉语</span>\n' +
    '    </div>\n' +
    '    <div class="optionItem" id="optScriptTraditional">\n' +
    '      <span class="optionLabel">{{options-traditional}}</span>\n' +
    '      <span class="optionExampleHan">漢語</span>\n' +
    '    </div>\n' +
    '    <div class="optionItem" id="optScriptBoth">\n' +
    '      <span class="optionLabel">{{options-bothscripts}}</span>\n' +
    '      <span class="optionExampleHan">汉语 • 漢語</span>\n' +
    '    </div>\n' +
    '  </div>\n' +
    '  <div class="optionHead">{{options-tonecolors}}</div>\n' +
    '  <div class="optionBody">\n' +
    '    <div class="optionItem toneColorsNone" id="optToneColorsNone">\n' +
    '      <span class="optionLabel">{{options-nocolors}}</span>\n' +
    '      <span class="optionExampleHan"><span class="tone1">天</span><span class="tone2">人</span><span class="tone3">很</span><span class="tone4">大</span>了</span>\n' +
    '    </div>\n' +
    '    <div class="optionItem toneColorsPleco" id="optToneColorsPleco">\n' +
    '      <span class="optionLabel">{{options-pleco}}</span>\n' +
    '      <span class="optionExampleHan"><span class="tone1">天</span><span class="tone2">人</span><span class="tone3">很</span><span class="tone4">大</span>了</span>\n' +
    '    </div>\n' +
    '    <div class="optionItem toneColorsDummitt" id="optToneColorsDummitt">\n' +
    '      <span class="optionLabel">{{options-dummitt}}</span>\n' +
    '      <span class="optionExampleHan"><span class="tone1">天</span><span class="tone2">人</span><span class="tone3">很</span><span class="tone4">大</span>了</span>\n' +
    '    </div>\n' +
    '  </div>\n' +
    '</div>\n';


  zdPage.globalInit(globalInit);

  $(document).ready(function () {
    zdPage.registerInitScript("search", resultEventWireup);
  });

  function globalInit() {
    // Register search params provider
    zdPage.setSearchParamsProvider(getSearchParams);

    // If session storage says we've already loaded strokes, append script right now
    // This will happen from browser cache, i.e., page load doesn't suffer
    if (sessionStorage.getItem("strokesLoaded")) {
      var elmStrokes = document.createElement('script');
      document.getElementsByTagName("head")[0].appendChild(elmStrokes);
      elmStrokes.setAttribute("type", "text/javascript");
      elmStrokes.setAttribute("src", getStrokeDataUrl());
    }
    // Add tooltips
    $("#btn-write").tooltipster({
      content: $("<span>" + uiStrings["tooltip-btn-brush"] + "</span>")
    });
    $("#btn-settings").tooltipster({
      content: $("<span>" + uiStrings["tooltip-btn-settings"] + "</span>")
    });
    $("#btn-search").tooltipster({
      content: $("<span>" + uiStrings["tooltip-btn-search"] + "</span>")
    });

    // TO-DO
    //initStrokes();

    // Debug: to work on strokes input
    //showStrokeInput();

    $("#btn-clear").click(clearSearch);
    $("#btn-settings").click(showSettings);

    // TO-DO
    //$("#strokeClear").click(clearCanvas);
    //$("#strokeUndo").click(undoStroke);

    $("#btn-search").click(submitSearch);
    $("#txtSearch").keyup(function (e) {
      if (e.keyCode == 13) {
        submitSearch();
        return false;
      }
    });
    $("#txtSearch").focus(txtSearchFocus);
    $("#txtSearch").change(function () {
      appendNotOverwrite = true;
    });

    // Debug: to work on opening screen
    //$("#resultsHolder").css("display", "none");
    //$("#welcomeScreen").css("display", "block");
  }

  function resultEventWireup(data) {
    $("#results").append("<div id='soaBox' class='soaBoxLeft'></div>");
    zdStrokeAnim.init();
    $(".hanim").click(showStrokeAnim);
    $("#soaClose").click(hideStrokeAnim);
    $("#soaBox").click(function (e) { e.stopPropagation(); });
    $('#txtSearch').val(data.query);
    $('#txtSearch').focus();
  }

  // Show the search settings popup (generate from template; event wireup; position).
  function showSettings(event) {
    // Render HTML from template
    var html = optionsTemplate;
    html = html.replace("{{options-title}}", uiStrings["options-title"]);
    html = html.replace("{{options-script}}", uiStrings["options-script"]);
    html = html.replace("{{options-simplified}}", uiStrings["options-simplified"]);
    html = html.replace("{{options-traditional}}", uiStrings["options-traditional"]);
    html = html.replace("{{options-bothscripts}}", uiStrings["options-bothscripts"]);
    html = html.replace("{{options-tonecolors}}", uiStrings["options-tonecolors"]);
    html = html.replace("{{options-nocolors}}", uiStrings["options-nocolors"]);
    html = html.replace("{{options-pleco}}", uiStrings["options-pleco"]);
    html = html.replace("{{options-dummitt}}", uiStrings["options-dummitt"]);
    $("#searchOptionsBox").html(html);
    // Housekeeping; show search box
    zdPage.modalShown(hideSettings);
    var elmPopup = $("#searchOptionsBox");
    elmPopup.addClass("visible");
    $("#optionsClose").click(hideSettings);
    elmPopup.click(function (evt) { evt.stopPropagation(); });
    // Disable tooltip while settings are on screen
    $("#btn-settings").tooltipster('disable');
    // Stop event propagation, or we'll be closed right away
    event.stopPropagation();
    // Position search box to settings button
    var elmStgs = $("#btn-settings");
    var rectStgs = [elmStgs.offset().left, elmStgs.offset().top, elmStgs.width(), elmStgs.height()];
    var elmTail = $("#optionsTail");
    var rectTail = [elmTail.offset().left, elmTail.offset().top, elmTail.width(), elmTail.height()];
    var xMidStgs = rectStgs[0] + rectStgs[2] / 2.2;
    var xMidTail = rectTail[0] + rectTail[2] / 2;
    elmPopup.offset({ left: elmPopup.offset().left + xMidStgs - xMidTail, top: elmPopup.offset().top });
    // Load persisted/default values; update UI
    loadOptions();
    // Events
    optionsEventWireup();
  }

  // Hides the search settings popup
  function hideSettings() {
    $("#searchOptionsBox").removeClass("visible");
    zdPage.modalHidden();
    // Re-enable tooltip
    $("#btn-settings").tooltipster('enable');
  }

  // Load options (or inits to defaults); updates UI.
  function loadOptions() {
    // Check cookie for script
    var ckScript = localStorage.getItem("uiscript");
    if (ckScript !== null) optScript = ckScript;
    if (optScript === "simp") $("#optScriptSimplified").addClass("selected");
    else if (optScript === "trad") $("#optScriptTraditional").addClass("selected");
    else if (optScript === "both") $("#optScriptBoth").addClass("selected");
    // Check cookie for tone colors
    var ckTones = localStorage.getItem("uitones");
    if (ckTones !== null) optTones = ckTones;
    if (optTones === "none") $("#optToneColorsNone").addClass("selected");
    else if (optTones === "pleco") $("#optToneColorsPleco").addClass("selected");
    else if (optTones === "dummitt") $("#optToneColorsDummitt").addClass("selected");
  }

  // Interactions of options UI.
  function optionsEventWireup() {
    // Event handlers for mouse (desktop)
    var handlersMouse = {
      mousedown: function (e) {
        $(this).animate({ backgroundColor: clrEmph }, 200);
      },
      click: function (e) {
        $(this).animate({ backgroundColor: clrSel }, 400);
        selectOption(this.id);
      },
      mouseenter: function (e) {
        $(this).animate({ backgroundColor: clrSel }, 400);
      },
      mouseleave: function (e) {
        var clr = $(this).hasClass('selected') ? clrSel : "transparent";
        $(this).animate({ backgroundColor: clr }, 400);
      }
    };
    // Event handlers for mobile (touch)
    var handlersTouch = {
      click: function (e) {
        $(this).animate({ backgroundColor: clrSel }, 400);
        selectOption(this.id);
      }
    };
    var handlers = /* isMobile ? handlersTouch : */ handlersMouse;
    // Script option set
    $("#optScriptSimplified").on(handlers);
    $("#optScriptTraditional").on(handlers);
    $("#optScriptBoth").on(handlers);

    // Tone colors option set
    $("#optToneColorsNone").on(handlers);
    $("#optToneColorsPleco").on(handlers);
    $("#optToneColorsDummitt").on(handlers);
  }

  // Handler: an option is selected (clicked) in the options UI.
  function selectOption(id) {
    function unselectOption(optId) {
      if ($(optId).hasClass("selected")) {
        $(optId).removeClass("selected");
        $(optId).animate({ backgroundColor: "transparent" }, 400);
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
    // Store: Script options
    if (id === "optScriptSimplified") localStorage.setItem("uiscript", "simp");
    else if (id === "optScriptTraditional") localStorage.setItem("uiscript", "trad");
    else if (id === "optScriptBoth") localStorage.setItem("uiscript", "both");
    // Store: Tone color options
    if (id === "optToneColorsNone") localStorage.setItem("uitones", "none");
    else if (id === "optToneColorsPleco") localStorage.setItem("uitones", "pleco");
    else if (id === "optToneColorsDummitt") localStorage.setItem("uitones", "dummitt");
    // Load options again: adds "selected" to element (redundantly), and updates UI.
    loadOptions();
  }

  // TO-DO
  //function getStrokeDataUrl() {
  //  // Figure out URL by stealing from "common.js" - which is always there. Right?!
  //  var elmCommonJS = document.getElementById("elmCommonJS");
  //  var urlCommonJS = elmCommonJS.getAttribute("src");
  //  var re = /common\.js/;
  //  var urlStrokes = urlCommonJS.replace(re, "chinesestrokes.js");
  //  return urlStrokes;
  //}

  // Shows the handwriting recognition pop-up.
  function showStrokeInput() {
    // TO-DO
    return;

    // Firsty first: load the stroke data if missing
    if (typeof strokesData === 'undefined') {
      // Add element, and also event handler for completion
      var elmStrokes = document.createElement('script');
      document.getElementsByTagName("head")[0].appendChild(elmStrokes);
      var funEnabler = function () {
        setRecogEnabled(true);
        sessionStorage.setItem("strokesLoaded", true);
      }
      elmStrokes.onload = function () { funEnabler(); };
      elmStrokes.onreadystatechange = function () {
        if (this.readyState == 'complete') funEnabler();
      }
      elmStrokes.setAttribute("type", "text/javascript");
      elmStrokes.setAttribute("src", getStrokeDataUrl());
      // Make input disabled
      setRecogEnabled(false);
    }
    // Position and show panel
    if (!zdPage.isMobile()) {
      var searchPanelOfs = $("#search-panel").offset();
      var searchPanelWidth = $("#search-panel").width();
      var searchPanelHeight = $("#search-panel").height();
      var strokeInputWidth = $("#stroke-input").outerWidth();
      //$("#stroke-input").position({left: 0, top: searchPanelOfs.top + searchPanelHeight + 390});
      $("#stroke-input").css("margin-top", searchPanelOfs.top - window.pageYOffset + searchPanelHeight);
      $("#stroke-input").css("left", searchPanelOfs.left + searchPanelWidth - strokeInputWidth + 2);
      $("#stroke-input").css("display", "block");
      $("#suggestions").html("<br/><br/>");
    }
    else {
      $("#stroke-input").css("display", "block");
      $("#suggestions").html("<br/>");
    }
    var strokeCanvasWidth = $("#stroke-input-canvas").width();
    $("#stroke-input-canvas").css("height", strokeCanvasWidth);
    var canvasElement = document.getElementById("stroke-input-canvas");
    canvasElement.width = strokeCanvasWidth;
    canvasElement.height = strokeCanvasWidth;
    $("#suggestions").css("height", $("#suggestions").height());
    clearCanvas();
    $("#btn-write").attr("class", "active");
    // Must do this explicitly: if hamburger menu is shown, got to hide it
    if (zdPage.isMobile()) hideShowHamburger(false);
  }

  // Hides the handwriting recognition popup
  function hideStrokeInput() {
    // TO-DO
    return;

    // Nothing to hide?
    if ($("#stroke-input").css("display") != "block") return;
    // Hide.
    $("#stroke-input").css("display", "none");
    $("#btn-write").attr("class", "");
  }

  // Clears the search field
  function clearSearch() {
    $("#txtSearch").val("");
    $("#txtSearch").focus();
  }

  // When the search input field receives focus
  function txtSearchFocus(event) {
    if (zdPage.isMobile()) return;
    $("#txtSearch").select();
  }

  // Returns object with search params.
  function getSearchParams() {
    return { searchScript: optScript, searchTones: optTones };
  }

  // Submits a dictionary search as simple GET URL
  function submitSearch() {
    'use strict';
    var queryStr = $('#txtSearch').val().replace(" ", "+");
    zdPage.submitSearch(queryStr);
  }

  // Dynamically position stroke order animation popup in Desktop
  function dynPosSOA(zis) {
    // First, decide if we're showing box to left or right of character
    var hanziOfs = zis.offset();
    var onRight = hanziOfs.left < $(document).width() / 2;
    var left = hanziOfs.left + zis.width() + 20;
    if (onRight) $("#soaBox").removeClass("soaBoxLeft");
    else {
      $("#soaBox").addClass("soaBoxLeft");
      left = hanziOfs.left - $("#soaBox").width() - 20;
    }
    // Decide about Y position. Box wants char to be at vertical middle
    // But is willing to move up/down to fit in content area
    var charY = hanziOfs.top + zis.height() / 2;
    var boxH = $("#soaBox").height();
    var top = charY - boxH / 2;
    // First, nudge up if we stretch beyond viewport bottom
    var wBottom = window.pageYOffset + window.innerHeight - 10;
    if (top + boxH > wBottom) top = wBottom - boxH;
    // Then, nudge down if we're over the ceiling
    //var wTop = $("#hdrSearch").position().top + $("#hdrSearch").height() + window.pageYOffset + 20;
    var wTop = $("#headermask").position().top + $("#headermask").height() + window.pageYOffset;
    if (top < wTop) top = wTop;
    // Position box, and tail
    $("#soaBox").offset({ left: left, top: top });
    $("#soaBoxTail").css("top", (charY - top - 10) + "px");
  }

  // Positions and shows the stroke animation pop-up for the clicked hanzi.
  function showStrokeAnim(event) {
    // We get click event when mouse button is released after selecting a single hanzi
    // Don't want to show pop-up in this edge case
    var sbe = zdPage.getSelBoundElm();
    if (sbe != null && sbe.textContent == $(this).text())
      return;
    // OK, so we're actually showing. Stop propagation so we don't get auto-hidden.
    event.stopPropagation();
    // If previous show's in progress, kill it
    // Also kill stroke input, in case it's shown
    zdStrokeAnim.kill();
    // Start the whole spiel
    $("#soaBox").css("display", "block");
    // We only position dynamically in desktop version; in mobile, it's fixed
    dynPosSOA($(this));
    // Render grid, issue AJAX query for animation data
    zdStrokeAnim.renderBG();
    zdStrokeAnim.startQuery($(this).text());
    // Notify page
    zdPage.modalShown(hideStrokeAnim);
  }

  // Closes the stroke animation pop-up (if shown).
  function hideStrokeAnim() {
    zdStrokeAnim.kill();
    $("#soaBox").css("display", "none");
    zdPage.modalHidden();
  }
})();


