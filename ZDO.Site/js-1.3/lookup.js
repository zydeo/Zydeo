$(document).ready(function () {
  // If session storage says we've already loaded strokes, append script right now
  // This will happen from browser cache, i.e., page load doesn't suffer
  if (sessionStorage.getItem("strokesLoaded")) {
    var elmStrokes = document.createElement('script');
    document.getElementsByTagName("head")[0].appendChild(elmStrokes);
    elmStrokes.setAttribute("type","text/javascript");
    elmStrokes.setAttribute("src", getStrokeDataUrl());
  }
  // Add tooltips in desktop version only
  if (!isMobile) {
    $("#img-write").tooltipster({
      content: $("<span>" + uiArr["tooltip-btn-brush"] + "</span>")
    });
    $("#img-search").tooltipster({
      content: $("<span>" + uiArr["tooltip-btn-search"] + "</span>")
    });
  }

  initStrokes();

  // Debug: to work on strokes input
  //showStrokeInput();

  lookupEventWireup();

  // Do not focus input field on mobile: shows keyboard, annoying
  if (!isMobile) {
    $("#txtSearch").focus();
    $("#txtSearch").select();
  }

  // Debug: to work on opening screen
  //$("#resultsHolder").css("display", "none");
  //$("#welcomeScreen").css("display", "block");
});

function getStrokeDataUrl() {
  // Figure out URL by stealing from "common.js" - which is always there. Right?!
  var elmCommonJS = document.getElementById("elmCommonJS");
  var urlCommonJS = elmCommonJS.getAttribute("src");
  var re = /common\.js/;
  var urlStrokes = urlCommonJS.replace(re, "chinesestrokes.js");
  return urlStrokes;
}

// Shows the handwriting recognition pop-up.
function showStrokeInput() {
  // Firsty first: load the stroke data if missing
  if (typeof strokesData === 'undefined') {
    // Add element, and also event handler for completion
    var elmStrokes = document.createElement('script');
    document.getElementsByTagName("head")[0].appendChild(elmStrokes);
    var funEnabler = function() {
      setRecogEnabled(true);
      sessionStorage.setItem("strokesLoaded", true);
    }
    elmStrokes.onload = function() { funEnabler(); };
    elmStrokes.onreadystatechange = function() {
      if (this.readyState == 'complete') funEnabler();
    }
    elmStrokes.setAttribute("type","text/javascript");
    elmStrokes.setAttribute("src", getStrokeDataUrl());
    // Make input disabled
    setRecogEnabled(false);
  }
  // Position and show panel
  if (!isMobile) {
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
  if (isMobile) hideShowHamburger(false);
}

// Hides the handwriting recognition popup
function hideStrokeInput() {
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
  if (isMobile) return;
  $("#txtSearch").select();
}

// Submits a dictionary search as a POST request.
function submitSearch() {
  'use strict';
  var form;
  form = $('<form />', {
    action: '/',
    method: 'post',
    style: 'display: none;'
  });
  $('<input />', {
    type: 'hidden',
    name: 'query',
    value: $('#txtSearch').val()
  }).appendTo(form);
  $('<input />', {
    type: 'hidden',
    name: 'mobile',
    value: isMobile ? "yes" : "no"
  }).appendTo(form);
  form.appendTo('body').submit();
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
  var wTop = $("#search-bar").position().top + $("#search-bar").height() + window.pageYOffset + 20;
  if (top < wTop) top = wTop;
  // Position box, and tail
  $("#soaBox").offset({left: left, top: top});
  $("#soaBoxTail").css("top", (charY - top - 10) + "px");
}

// Position and size SAO box in mobile UI
function mobilePosSOA() {
  var dw = $(document).width();
  var soaW = dw * 0.8;
  var top = $("#search-bar").position().top + $("#search-bar").height() + window.pageYOffset + 60;
  $("#soaBox").width(soaW);
  $("#soaBox").offset({left: (dw - soaW) / 2, top: top});
  var graphW = $("#soaGraphics").innerWidth();
  var svgW = graphW - 40;
  $("#strokeAnimSVG").width(svgW);
  $("#strokeAnimSVG").height(svgW);
  var errW = svgW * 0.8;
  $("#soaError").width(errW);
  $("#soaError").css("margin-left", (graphW - errW)/2);
  $("#soaError").css("margin-top", graphW / 3);
}

// Positions and shows the stroke animation pop-up for the clicked hanzi.
function hanziClicked(event) {
  // We get click event when mouse button is released after selecting a single hanzi
  // Don't want to show pop-up in this edge case
  var sbe = getSelBoundElm();
  if (sbe != null && sbe.textContent == $(this).text())
    return;
  // OK, so we're actually showing. Stop propagation so we don't get auto-hidden.
  event.stopPropagation();
  // If previous show's in progress, kill it
  // Also kill stroke input, in case it's shown
  soaKill();
  hideStrokeInput();
  // Start the whole spiel
  $("#soaBox").css("display", "block");
  // We only position dynamically in desktop version; in mobile, it's fixed
  if (!isMobile) dynPosSOA($(this));
  else mobilePosSOA();
  // Render grid, issue AJAX query for animation data
  soaRenderBG();
  soaStartQuery($(this).text());
}

// Closes the stroke animation pop-up (if shown).
function closeStrokeAnim() {
  soaKill();
  $("#soaBox").css("display", "none");
}

function lookupEventWireup() {
  $("#btn-clear").click(clearSearch);
  $("#btn-write").click(function () {
    if ($("#stroke-input").css("display") == "block") hideStrokeInput();
    else showStrokeInput();
  });
  // Auto-hide stroke input when tapping away
  // Also for stroke animation pop-up
  $('html').click(function () {
    hideStrokeInput();
    closeStrokeAnim();
  });
  // Must do explicitly for hamburger menu, b/c that stops event propagation
  if (isMobile) $('#btn-menu').click(function () {
    hideStrokeInput();
    closeStrokeAnim();
  });
  $('#btn-write').click(function (event) {
    event.stopPropagation();
    closeStrokeAnim();
  });
  $('#stroke-input').click(function (event) {
    event.stopPropagation();
  });

  $("#strokeClear").click(clearCanvas);
  $("#strokeUndo").click(undoStroke);
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

  $(".hanim").click(hanziClicked);
  $("#soaClose").click(closeStrokeAnim);
  $("#soaBox").click(function (e) {
    e.stopPropagation();
  });
}

