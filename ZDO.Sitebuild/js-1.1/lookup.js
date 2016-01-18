$(document).ready(function () {
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

function showStrokeInput() {
  if (!isMobile) {
    var searchPanelOfs = $("#search-panel").offset();
    var searchPanelWidth = $("#search-panel").width();
    var searchPanelHeight = $("#search-panel").height();
    var strokeInputWidth = $("#stroke-input").outerWidth();
    $("#stroke-input").css("top", searchPanelOfs.top + searchPanelHeight + 1);
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

function hideStrokeInput() {
  // Nothing to hide?
  if ($("#stroke-input").css("display") != "block") return;
  // Hide.
  $("#stroke-input").css("display", "none");
  $("#btn-write").attr("class", "");
}

function clearSearch() {
  $("#txtSearch").val("");
  $("#txtSearch").focus();
}

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

function hanziClicked(event) {
  $("#soaBox").css("display", "block");
  var hanziOfs = $(this).offset();
  var onRight = hanziOfs.left < $(document).width() / 2;
  var left = hanziOfs.left + $(this).width() + 20;
  if (onRight) $("#soaBox").removeClass("soaBoxLeft");
  else {
    $("#soaBox").addClass("soaBoxLeft");
    left = hanziOfs.left - $("#soaBox").width() - 20;
  }
  var top = hanziOfs.top;
  $("#soaBox").offset({ left: left, top: top });
  soaRenderBG();
  soaStartQuery($(this).text());
}

function closeStrokeAnim() {
  $("#soaBox").css("display", "none");
}

function lookupEventWireup() {
  $("#btn-clear").click(clearSearch);
  $("#btn-write").click(function () {
    if ($("#stroke-input").css("display") == "block") hideStrokeInput();
    else  showStrokeInput();
  });
  // Auto-hide stroke input when tapping away
  $('html').click(function () {
    hideStrokeInput();
  });
  // Must do explicitly for hamburger menu, b/c that stops event propagation
  if (isMobile) $('#btn-menu').click(function () {
    hideStrokeInput();
  });
  $('#btn-write').click(function (event) {
    event.stopPropagation();
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
  $("#txtSearch").change(function () {
    appendNotOverwrite = true;
  });

  $(".hanim").click(hanziClicked);
  $("#soaClose").click(closeStrokeAnim);
}

