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


var canvas;
var ctx;
var clicking = false;
var mousestrokes = [];
var lastTouchX = -1;
var lastTouchY = -1;
var tstamp;

// Initializes stroke recognition. To be called when page has fully loaded: $(document).ready
function initStrokes() {
  canvas = document.getElementById(canvasId);
  if (canvas === null) return;
  ctx = canvas.getContext("2d");

  $('#' + canvasId).mousemove(function (e) {
    if (!clicking)return;
    var x = e.pageX - $(this).offset().left;
    var y = e.pageY - $(this).offset().top;
    dragClick(x, y);
    debugOnScreen("MouseMove X: " + x + " Y: " + y);
  });
  $('#' + canvasId).mousedown(function (e) {
    var x = e.pageX - $(this).offset().left;
    var y = e.pageY - $(this).offset().top;
    startClick(x, y);
    debugOnScreen("MouseDown X: " + x + " Y: " + y);
  }).mouseup(function (e) {
    var x = e.pageX - $(this).offset().left;
    var y = e.pageY - $(this).offset().top;
    endClick(x, y);
    debugOnScreen("MouseUp");
  });

  $('#' + canvasId).bind("touchmove", function (e) {
    if (!clicking)return;
    e.preventDefault();
    var x = e.originalEvent.touches[0].pageX - $(this).offset().left;
    lastTouchX = x;
    var y = e.originalEvent.touches[0].pageY - $(this).offset().top;
    lastTouchY = y;
    dragClick(x, y);
    debugOnScreen("TouchMove X: " + x + " Y: " + y);
  });
  $('#' + canvasId).bind("touchstart", function (e) {
    e.preventDefault();
    document.activeElement.blur();
    var x = e.originalEvent.touches[0].pageX - $(this).offset().left;
    var y = e.originalEvent.touches[0].pageY - $(this).offset().top;
    startClick(x, y);
    debugOnScreen("TouchStart X: " + x + " Y: " + y);
  }).bind("touchend", function (e) {
    e.preventDefault();
    document.activeElement.blur();
    endClick(lastTouchX, lastTouchY);
    lastTouchX = lastTouchY = -1;
    debugOnScreen("TouchEnd");
  });
}

// Clear canvas and resets gathered strokes data for new input.
function clearCanvas() {
  // Redraw canvas (gridlines)
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
  // Clear previous suggestions
  $(suggestionsId).html('');
  // Reset gathered strokes input
  mousestrokes = [];
  strokeDescriptor = [];
}

// Logs diagnostic message to designated element, or keeps quiet.
function debugOnScreen(msg) {
  if (debugId === null) return;
  $(debugId).html(msg);
}

var strokeXYs;
var lastPt;
var strokeDescriptor = [];

function startClick(x, y) {
  clicking = true;
  strokeXYs = [];
  lastPt = {x: x, y: y};
  strokeXYs.push(lastPt);
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
  strokeXYs.push(pt);
  lastPt = pt;
  ctx.lineTo(x, y);
  ctx.stroke();
}

function endClick(x, y) {
  clicking = false;
  if (x == -1) return;
  ctx.lineTo(x, y);
  ctx.stroke();
  strokeXYs.push({x: x, y: y});
  mousestrokes.push(strokeXYs);
  analyze(strokeXYs);
}

function undoStroke() {
  // TO-DO
}


function getEntity(a) {
  return ('\&#0' + parseInt(a, 16) + ';');
}

function analyze(stroke) {
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
  for (var i1 in mousestrokes) {
    for (var i2 in mousestrokes[i1]) {
      if (mousestrokes[i1][i2].x > xmax)xmax = mousestrokes[i1][i2].x;
      if (mousestrokes[i1][i2].x < xmin)xmin = mousestrokes[i1][i2].x;
      if (mousestrokes[i1][i2].y > ymax)ymax = mousestrokes[i1][i2].y;
      if (mousestrokes[i1][i2].y < ymin)ymin = mousestrokes[i1][i2].y;
    }
  }
  var w = xmax - xmin;
  var h = ymax - ymin;
  var dimensionSquared = (w > h) ? w * w : h * h;
  var normalizer = Math.pow(dimensionSquared * 2, 1 / 2);
  for (var i = 1; i < corners.length; i++) {
    var p1 = corners[i - 1];
    var p2 = corners[i];
    var dy = p1.y - p2.y;
    var dx = p1.x - p2.x;
    var length = Math.pow(dy * dy + dx * dx, 1 / 2);
    var normalized = length / normalizer;
    var direction = Math.PI - Math.atan2(dy, dx);
    strokeDescriptor.push({d: direction, l: normalized});
  }
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
    score = match(strokeDescriptor, cdi);
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
      $(insertionTargedId).val($(insertionTargedId).val() + $(this).html());
      clearCanvas();
      $(suggestionsId).html('');
    }).append(getEntity(possible[i].w)).attr('class', suggestionClass);
    $(suggestionsId).append(sug);
  }
  debugOnScreen("Analysis done");
}

function match(strokeDescriptor, charDescriptor) {
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
