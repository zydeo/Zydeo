var strokeWidthDesktop = 5;
var strokeWidthMobile = 15;
var drawAnalyzedStrokes = false;

function c1() {
  d = new Date();
}

function c2(r) {
  d2 = new Date();
  $('#word').html(r + " " + (d2.getTime() - d.getTime()) + "ms");
}

c1();

c2('Okay! Data files loaded in ');
$('#word').append('. Please draw a chinese character in the left box.');

var canvas;
var ctx;
var clicking = false;
var mousestrokes = [];
var lastTouchX = -1;
var lastTouchY = -1;

function initStrokes() {
  canvas = document.getElementById('stroke-input-canvas');
  ctx = canvas.getContext("2d");

  $('#stroke-input-canvas').mousemove(function (e) {
    if (!clicking)return;
    var x = e.pageX - $(this).offset().left;
    var y = e.pageY - $(this).offset().top;
    dragClick(x, y);
    $('#omw_debug').html("moving X: " + x + " Y: " + y);
  });
  $('#stroke-input-canvas').mousedown(function (e) {
    var x = e.pageX - $(this).offset().left;
    var y = e.pageY - $(this).offset().top;
    startClick(x, y);
    $('#omw_debug').html("Clicked--> X: " + x + " Y: " + y);
  }).mouseup(function (e) {
    var x = e.pageX - $(this).offset().left;
    var y = e.pageY - $(this).offset().top;
    endClick(x, y);
    //$('#omw_debug').html("Done Clicking");
  });

  $('#stroke-input-canvas').bind("touchmove", function (e) {
    if (!clicking)return;
    e.preventDefault();
    var x = e.originalEvent.touches[0].pageX - $(this).offset().left;
    lastTouchX = x;
    var y = e.originalEvent.touches[0].pageY - $(this).offset().top;
    lastTouchY = y;
    dragClick(x, y);
    $('#omw_debug').html("Moving X: " + x + " Y: " + y);
  });
  $('#stroke-input-canvas').bind("touchstart", function (e) {
    e.preventDefault();
    document.activeElement.blur();
    var x = e.originalEvent.touches[0].pageX - $(this).offset().left;
    var y = e.originalEvent.touches[0].pageY - $(this).offset().top;
    startClick(x, y);
    $('#omw_debug').html("Touched--> X: " + x + " Y: " + y);
  }).bind("touchend", function (e) {
    e.preventDefault();
    document.activeElement.blur();
    endClick(lastTouchX, lastTouchY);
    lastTouchX = lastTouchY = -1;
    //$('#omw_debug').html("Done Touching");
  });
}

function clearcanvas() {
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
  mousestrokes = [];
  strokeDescriptor = [];
}

var strokeXYs;
var lastPt;
var minx, miny, maxx, maxy;
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
  d = new Date();
}

var d;

function dragClick(x, y) {
  if ((new Date().getTime() - d) < 50)return;
  d = new Date();
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
  $('#omw_debug').html("Analysis start");

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
  c1();
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
  $('#suggestions').html('');
  for (var i = 0; ((i < 8) && possible[i]); i++) {
    var sug = document.createElement('span');
    $(sug).click(function () {
      $('#txt-search').val($('#txt-search').val() + $(this).html());
      clearcanvas();
      $('#suggestions').html('');
    }).append(getEntity(possible[i].w)).attr('class', 'sugItem');
    $('#suggestions').append(sug);
  }
  c2('Matched character in ');
  $('#omw_debug').html("Analysis end");
}

function analyzeBestMatch(bestmatch) {
  cdi = [];
  var ctx2 = document.getElementById('stroke-input-canvas').getContext("2d");
  ctx2.clearRect(0, 0, 800, 600);
  for (var i = 1; i < bestmatch.length; i++) {
    cdi.push({d: bestmatch[i][0], l: bestmatch[i][1]});
    var d = bestmatch[i][0];
    var o = {x: 100, y: i / bestmatch.length * 300};
    ctx2.beginPath();
    ctx2.arc(o.x, o.y, 5, 0, Math.PI * 2, true);
    ctx2.closePath();
    ctx2.fill();
    ctx2.fillText($('#c').html() + i, o.x - 20, o.y - 20);
    ctx2.beginPath();
    ctx2.moveTo(o.x, o.y);
    var h = bestmatch[i][1] * 100;
    var d = -bestmatch[i][0];
    var x = Math.cos(d) * h;
    var y = Math.sin(d) * h;
    ctx2.lineTo(o.x + x, o.y + y);
    ctx2.closePath();
    ctx2.stroke();
  }
}

function match(strokeDescriptor, charDescriptor) {
  var score = 0;
  if (strokeDescriptor.length != charDescriptor.length)return -1;
  for (var i in strokeDescriptor) {
    var ls = Math.abs(strokeDescriptor[i].l - charDescriptor[i].l);
    dl = (1 - ls);
    var ds = Math.abs(strokeDescriptor[i].d - charDescriptor[i].d);
    if (ds > Math.PI)ds = 2 * Math.PI - ds;
    ds = 100 * (Math.PI * 2 - ds) / (Math.PI * 2);
    score += ds + dl * charDescriptor[i].l;
  }
  return score;
}
