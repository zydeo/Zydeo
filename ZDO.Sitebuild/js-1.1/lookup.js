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

  $("#txtSearch").focus();
  $("#txtSearch").select();

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
}

function hideStrokeInput() {
  $("#stroke-input").css("display", "none");
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
  form.appendTo('body').submit();
}

function lookupEventWireup() {
  $("#btn-clear").click(clearSearch);
  $("#btn-write").click(function () {
    if ($("#stroke-input").css("display") == "block") {
      hideStrokeInput();
      $("#btn-write").attr("class", "");
    }
    else {
      showStrokeInput();
      $("#btn-write").attr("class", "active");
    }
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
}

