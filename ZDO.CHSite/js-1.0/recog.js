// Hanzi handwriting recognition code from http://www.lab4games.net/zz85/blog/2010/02/17/js-%E4%B8%AD%E6%96%87%E7%AC%94%E7%94%BB%E8%BE%93%E5%85%A5%E6%B3%95-javascript-chinese-stroke-input/
// Joshua Koo (zz85nus -at- gmail -dot- com)
//
// Adapted by Gabor L Ugray 2015 (zydeodict -at- gmail -dot- com)
// Released under MIT license: http://www.opensource.org/licenses/mit-license.php
//
//
// Before you start:
// - jQuery must be included before this file
// - File with declaration of strokes in strokesData variable must be included before this file
// - Variable isMobile must say if current environment is a mobile browser (true/false)


// Global options ******************************
// Width of strokes drawn on screen, for desktop browsers
var strokeWidthDesktop = 5;
// Width of strokes drawn on screen, for mobile browsers
var strokeWidthMobile = 15;
// If "true", results of corner detection are drawn on top of strokes in red
var drawAnalyzedStrokes = false;
// If not null, diagnostic messages are logged to element with the provided ID
var debugId = null; // #something
// ID of canvas element where user draws input. Without the # symbol!
var canvasId = "stroke-input-canvas";
// ID of element where suggestions are displayed.
var suggestionsId = "#suggestions";
// Class of spans with retrieved suggestions.
var suggestionClass = "sugItem";
// ID of text input element that receives character when suggestion is clicked.
var insertionTargedId = "#txtSearch";
// If true, selected hanzi is appended, instead of overwriting existing text.
var appendNotOverwrite = false;


var canvas;
var ctx;
var clicking = false;
var lastTouchX = -1;
var lastTouchY = -1;
var tstamp;
var lastPt;
var recogEnabled = true;

// An array of arrays; each element is the coordinate sequence for one stroke from the canvas
// Where "stroke" is everything between button press - move - button release
var rawStrokes = [];

// Canvas coordinates of each point in current stroke, in raw (unanalyzed) form.
var currentStroke;

// Analyzed substrokes, collected in flat array from all strokes input so far.
var analyzedSubstrokes = [];

// Indexes within analyzedSubstrokes where a stroke starts.
var strokeIndexes = [];

// Initializes stroke recognition. To be called when page has fully loaded: $(document).ready
function initStrokes() {
  canvas = document.getElementById(canvasId);
  if (canvas === null) return;
  ctx = canvas.getContext("2d");

  $('#' + canvasId).mousemove(function (e) {
    if (!clicking || !recogEnabled) return;
    var x = e.pageX - $(this).offset().left;
    var y = e.pageY - $(this).offset().top;
    dragClick(x, y);
    debugOnScreen("MouseMove X: " + x + " Y: " + y);
  });
  $('#' + canvasId).mousedown(function (e) {
    if (!recogEnabled) return;
    var x = e.pageX - $(this).offset().left;
    var y = e.pageY - $(this).offset().top;
    startClick(x, y);
    debugOnScreen("MouseDown X: " + x + " Y: " + y);
  }).mouseup(function (e) {
    if (!recogEnabled) return;
    var x = e.pageX - $(this).offset().left;
    var y = e.pageY - $(this).offset().top;
    endClick(x, y);
    debugOnScreen("MouseUp");
  });

  $('#' + canvasId).bind("touchmove", function (e) {
    if (!clicking || !recogEnabled) return;
    e.preventDefault();
    var x = e.originalEvent.touches[0].pageX - $(this).offset().left;
    lastTouchX = x;
    var y = e.originalEvent.touches[0].pageY - $(this).offset().top;
    lastTouchY = y;
    dragClick(x, y);
    debugOnScreen("TouchMove X: " + x + " Y: " + y);
  });
  $('#' + canvasId).bind("touchstart", function (e) {
    if (!recogEnabled) return;
    e.preventDefault();
    document.activeElement.blur();
    var x = e.originalEvent.touches[0].pageX - $(this).offset().left;
    var y = e.originalEvent.touches[0].pageY - $(this).offset().top;
    startClick(x, y);
    debugOnScreen("TouchStart X: " + x + " Y: " + y);
  }).bind("touchend", function (e) {
    if (!recogEnabled) return;
    e.preventDefault();
    document.activeElement.blur();
    endClick(lastTouchX, lastTouchY);
    lastTouchX = lastTouchY = -1;
    debugOnScreen("TouchEnd");
  });
}

function setRecogEnabled(enabled) {
  recogEnabled = enabled;
  if (!enabled) {
    $("#stroke-input-canvas").addClass("loading");
    $("#strokeDataLoading").css("display", "block");
  }
  else {
    $("#stroke-input-canvas").removeClass("loading");
    $("#strokeDataLoading").css("display", "none");
  }
}

// Draws a clear canvas, with gridlines
function drawClearCanvas() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  ctx.setLineDash([1, 1]);
  ctx.lineWidth = 0.5;
  ctx.strokeStyle = "grey";
  ctx.beginPath();
  ctx.moveTo(0, 0);
  ctx.lineTo(canvas.width, 0);
  ctx.lineTo(canvas.width, canvas.height);
  ctx.lineTo(0, canvas.height);
  ctx.lineTo(0, 0);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(0, 0);
  ctx.lineTo(canvas.width, canvas.height);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(canvas.width, 0);
  ctx.lineTo(0, canvas.height);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(canvas.width / 2, 0);
  ctx.lineTo(canvas.width / 2, canvas.height);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(0, canvas.height / 2);
  ctx.lineTo(canvas.width, canvas.height / 2);
  ctx.stroke();
}

// Clear canvas and resets gathered strokes data for new input.
function clearCanvas() {
  // Redraw canvas (gridlines)
  drawClearCanvas();
  // Clear previous suggestions
  $(suggestionsId).html('');
  // Reset gathered strokes input
  rawStrokes = [];
  analyzedSubstrokes = [];
  strokeIndexes = [];
}

// Logs diagnostic message to designated element, or keeps quiet.
function debugOnScreen(msg) {
  if (debugId === null) return;
  $(debugId).html(msg);
}

function startClick(x, y) {
  clicking = true;
  currentStroke = [];
  lastPt = {x: x, y: y};
  currentStroke.push(lastPt);
  ctx.strokeStyle = "black";
  ctx.setLineDash([]);
  ctx.lineWidth = strokeWidthDesktop;
  if (isMobile) ctx.lineWidth = strokeWidthMobile;
  ctx.beginPath();
  ctx.moveTo(x, y);
  tstamp = new Date();
}

function dragClick(x, y) {
  if ((new Date().getTime() - tstamp) < 50)return;
  tstamp = new Date();
  var pt = {x: x, y: y};
  if ((pt.x == lastPt.x) && (pt.y == lastPt.y))return;
  currentStroke.push(pt);
  lastPt = pt;
  ctx.lineTo(x, y);
  ctx.stroke();
}

function endClick(x, y) {
  clicking = false;
  if (x == -1) return;
  ctx.lineTo(x, y);
  ctx.stroke();
  currentStroke.push({x: x, y: y});
  rawStrokes.push(currentStroke);
  analyzeStroke(currentStroke);
  findChars();
}

// Undoes the last stroke input by the user.
function undoStroke() {
  // Sanity check: nothing to do if input is empty (no strokes yet)
  if (rawStrokes.length == 0) return;

  // Remove last stroke
  rawStrokes.length = rawStrokes.length - 1;
  var lastIX = strokeIndexes[strokeIndexes.length - 1];
  strokeIndexes.length = strokeIndexes.length - 1;
  analyzedSubstrokes.length = lastIX;

  // Clear canvas
  drawClearCanvas();
  // Redraw input (raw strokes) from scratch
  redrawInput();

  // Lookup best matching characters for what's left on canvas now
  findChars();
}

// Redraws raw strokes on the canvas.
function redrawInput() {
  for (var i1 in rawStrokes) {
    ctx.strokeStyle = "black";
    ctx.setLineDash([]);
    ctx.lineWidth = strokeWidthDesktop;
    if (isMobile) ctx.lineWidth = strokeWidthMobile;
    ctx.beginPath();
    ctx.moveTo(rawStrokes[i1][0].x, rawStrokes[i1][0].y);
    var len = rawStrokes[i1].length;
    for (var i2 = 0; i2 < len - 1; i2++) {
      ctx.lineTo(rawStrokes[i1][i2].x, rawStrokes[i1][i2].y);
      ctx.stroke();
    }
    ctx.lineTo(rawStrokes[i1][len - 1].x, rawStrokes[i1][len - 1].y);
    ctx.stroke();
  }
}

// Analyzes a new "raw" stroke and appends to analyzed substrokes.
// Includes re-normalizing the square that the drawn character occupies within the canvas.
function analyzeStroke(stroke) {
  debugOnScreen("Analysis starts");

  var corners = shortStraw(stroke);

  if (drawAnalyzedStrokes) {
    ctx.strokeStyle = "red";
    ctx.setLineDash([]);
    ctx.lineWidth = 1.5;
    ctx.beginPath();
    ctx.moveTo(corners[0].x, corners[0].y);
    for (var i in corners) {
      ctx.lineTo(corners[i].x, corners[i].y);
    }
    ctx.stroke();
  }
  var ymin = Number.POSITIVE_INFINITY;
  var xmin = Number.POSITIVE_INFINITY;
  var ymax = Number.NEGATIVE_INFINITY;
  var xmax = Number.NEGATIVE_INFINITY;
  for (var i1 in rawStrokes) {
    for (var i2 in rawStrokes[i1]) {
      if (rawStrokes[i1][i2].x > xmax)xmax = rawStrokes[i1][i2].x;
      if (rawStrokes[i1][i2].x < xmin)xmin = rawStrokes[i1][i2].x;
      if (rawStrokes[i1][i2].y > ymax)ymax = rawStrokes[i1][i2].y;
      if (rawStrokes[i1][i2].y < ymin)ymin = rawStrokes[i1][i2].y;
    }
  }
  var w = xmax - xmin;
  var h = ymax - ymin;
  var dimensionSquared = (w > h) ? w * w : h * h;
  var normalizer = Math.pow(dimensionSquared * 2, 1 / 2);
  strokeIndexes.push(analyzedSubstrokes.length);
  for (var i = 1; i < corners.length; i++) {
    var p1 = corners[i - 1];
    var p2 = corners[i];
    var dy = p1.y - p2.y;
    var dx = p1.x - p2.x;
    var length = Math.pow(dy * dy + dx * dx, 1 / 2);
    var normalized = length / normalizer;
    var direction = Math.PI - Math.atan2(dy, dx);
    analyzedSubstrokes.push({d: direction, l: normalized});
  }
  debugOnScreen("Analysis done");
}

// Finds characters whose substroke list best matches current anaylized substrokes.
function findChars() {
  debugOnScreen("Character lookup starts");

  var score;
  var possible = [];
  var bestmatch = '';
  var bestscore = 0;
  var cd, cdi;
  for (var c in strokesData) {
    cd = strokesData[c];
    cdi = [];
    for (var i = 1; i < cd.length; i++) {
      cdi.push({d: cd[i][0], l: cd[i][1]});
    }
    score = getCharScore(analyzedSubstrokes, cdi);
    if (score > -1) {
      possible.push({w: cd, s: score, huh: cdi});
    }
    if (score > bestscore) {
      bestmatch = cd;
      bestscore = score;
    }
  }
  function sortByLength(a, b) {
    return b.s - a.s;
  }

  possible.sort(sortByLength);
  $(suggestionsId).html('');
  for (var i = 0; ((i < 8) && possible[i]); i++) {
    var sug = document.createElement('span');
    $(sug).click(function () {
      if (appendNotOverwrite)
        $(insertionTargedId).val($(insertionTargedId).val() + $(this).html());
      else
        $(insertionTargedId).val($(this).html());
      appendNotOverwrite = true;
      clearCanvas();
      $(suggestionsId).html('');
    }).append('\&#0' + parseInt(possible[i].w, 16) + ';').attr('class', suggestionClass);
    $(suggestionsId).append(sug);
  }
  debugOnScreen("Character lookup done");
}

// Calculates score (match rate) of input against one know character.
function getCharScore(strokeDescriptor, charDescriptor) {
  var score = 0;
  if (strokeDescriptor.length != charDescriptor.length)return -1;
  for (var i in strokeDescriptor) {
    var ls = Math.abs(strokeDescriptor[i].l - charDescriptor[i].l);
    var dl = (1 - ls);
    var ds = Math.abs(strokeDescriptor[i].d - charDescriptor[i].d);
    if (ds > Math.PI)ds = 2 * Math.PI - ds;
    ds = 100 * (Math.PI * 2 - ds) / (Math.PI * 2);
    score += ds + dl * charDescriptor[i].l;
  }
  return score;
}
