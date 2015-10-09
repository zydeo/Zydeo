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

var chinesestrokes = strokes;
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
  if (!ctx) {
    alert("Your browser isn't modern enough to run this application! :(");
    return;
  }
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

function d2h(d) {
  return d.toString(16);
}

function h2d(h) {
  return parseInt(h, 16);
}

function chineseword(a) {
  return ('\&#0' + h2d(a) + ';');
}

function analyze(stroke) {
  $('#omw_debug').html("Analysis start");

  var corners = shortStraw(stroke);

  if (drawAnalyzedStrokes) {
    ctx.strokeStyle = "red";
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
  for (var c in chinesestrokes) {
    cd = chinesestrokes[c];
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
  $('#b').val(bestmatch[0]);
  $('#word').html(chineseword(bestmatch[0]));
  for (var i = 1; ((i < 10) && possible[i]); i++) {
    $('#word').append(chineseword(possible[i].w) + '<small>' + possible[i].s + '|' + JSON.stringify(possible[i].huh) + '</small>');
  }
  $('#suggestions').html('');
  for (var i = 0; ((i < 8) && possible[i]); i++) {
    var sug = document.createElement('span');
    $(sug).click(function () {
      $('#txt-search').val($('#txt-search').val() + $(this).html());
      clearcanvas();
    }).dblclick(function () {
      spin($(this));
    }).append(chineseword(possible[i].w)).attr('class', 'sugItem');
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

function normalizeLength(a, b) {
  w = b.w
  h = b.h;
  dimensionSquare = w > h ? w * w : h * h;
  normalizer = Math.pow(dimensionSquared * 2, 1 / 2);
  normalized = distance(b) / normailizer;
  normalized = Math.min(normalized, 1.0);
  return normalized;
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

function drawdot(col, line) {
  ctx.strokeStyle = "black";
  ctx.beginPath();
  ctx.arc(startx + col * objWidth + objWidth * 3 / 4, line / 2 * staffHeight + starty - 5, 2, 0, Math.PI * 2, true);
  ctx.stroke();
}

function radiansToDegress(x) {
  return x * 180 / Math.PI
}

function debug(o) {
}

function clearcanvas() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  mousestrokes = [];
  strokeDescriptor = [];
  jcss3rotate($('#suggestions').css('background', '').html(''), 0);
}

function drawline(x1, y1, x2, y2) {
  ctx.beginPath();
  ctx.moveTo(x1, y1);
  ctx.lineTo(x2, y2);
  ctx.closePath();
  ctx.stroke();
}

function drawrect(x1, y1, width, height) {
  ctx.strokeRect(x1, y1, width, height);
}

function spin(w) {
  $('#suggestions').html(w);
  $(w).animate({fontSize: '250px'}, 1000, spin2);
}

function spin2() {
  var i = 0;
  (function (i) {
    i += 10;
    var s = (90 - Math.abs(90 - i)) / 90 * 0.5 + 1;
    if ((i - 100) > 0)s += 0.5;
    var css = 'rotate(' + i + 'deg)';
    $('.sugItem').css('-webkit-transform', css).css('-moz-transform', css).css('-o-transform', css);
    if (i < 180) {
      window.setTimeout(arguments.callee, 25, i);
    } else {
      jcss3rotate($('#suggestions').css('background', 'red'), 45);
      jcss3rotate($('.sugItem'), 135);
    }
  })(i);
}

function jcss3rotate(j, d) {
  var css = 'rotate(' + d + 'deg)';
  j.css('-webkit-transform', css).css('-moz-transform', css).css('-o-transform', css);
}

var ctx2, ctx3, customFont;
function letsDraw() {
  ctx2 = document.getElementById('stroke-input-canvas').getContext("2d");
  ctx2.strokeWidth = strokeWidthDesktop;
  if (isMobile) ctx2.strokeWidth = strokeWidthMobile;
  customFont = {family: 'Times', size: '160pt', color: 'black', borderWidth: '1px', lineHeight: '1.3em', coo: {}};
  if (customFont.borderColor)ctx2.strokeStyle = customFont.borderColor;
  if (customFont.color)ctx2.fillStyle = customFont.color;
  if (customFont.borderWidth)ctx2.lineWidth = parseFloat(customFont.borderWidth);
  customFont.scale = 1 / (716 / parseInt(customFont.size));
  customFont.cursor = {x: 0, y: Math.round(parseInt(customFont.size) * parseFloat(customFont.lineHeight))};
  alert(customFont.cursor.y);
  ctx2.moveTo(customFont.cursor.x, customFont.cursor.y);
  customFont.p = {x: customFont.cursor.x, y: customFont.cursor.y};
  customFont.points = Array();
  parse($('#d568').val());
}

function parse(inp) {
  var hsbw = null;
  var l = inp.split('\n');
  for (var i = 0; i < l.length; i++) {
    var parts = l[i].split(' ');
    if (parts.length > 0) {
      var args = new Array();
      for (var j = 0; j < parts.length - 1; j++) {
        args[args.length] = parts[j].replace('null', '');
      }
      var comm = parts[parts.length - 1];
      if (comm != '') {
        if (typeof(eval('ctx2.' + comm)) == 'function') {
          var call = 'ctx2.' + comm + '(' + args.join(', ') + '); ';
          if (comm == 'hsbw') {
            hsbw = call;
          } else {
            if (comm.indexOf('move') == -1) {
              var from = {x: customFont.p.x, y: customFont.p.y}
              customFont.points[customFont.points.length] = {x: customFont.p.x, y: customFont.p.y}
            }
            eval(call);
          }
        }
      }
    }
  }
  if (hsbw != null) {
    eval(hsbw);
  }
}

function counterclockwise(points) {
  summe = 0;
  for (i = 0; i < points.length; i++) {
    var next2 = (i + 2) % points.length;
    var next1 = (i + 1) % points.length;
    dx1 = points[next1].x - points[i].x;
    dy1 = points[next1].y - points[i].y;
    dx2 = points[next2].x - points[next1].x;
    dy2 = points[next2].y - points[next1].y;
    l1 = Math.sqrt(Math.pow(dx1, 2) + Math.pow(dy1, 2));
    l2 = Math.sqrt(Math.pow(dx2, 2) + Math.pow(dy2, 2));
    l1 = l1 > 0 ? l1 : 1;
    l2 = l2 > 0 ? l2 : 1;
    nx1 = dx1 / l1;
    ny1 = dy1 / l1;
    nx2 = dx2 / l2;
    ny2 = dy2 / l2;
    normx = -ny1;
    normy = nx1;
    p = normx * nx2 + normy * ny2;
    p2 = nx1 * nx2 + ny1 * ny2;
    teil = Math.atan2(p, p2);
    summe += teil;
  }
  return (Math.round(summe) >= 0);
}

function expand_points(points) {
  var out = '';
  for (var i = 0; i < points.length; i++) {
    out += '{x:' + Math.round(points[i].x) + ',y:' + Math.round(points[i].y) + '}';
    if (i < points.length - 1) {
      out += ', ';
    }
  }
  return out;
}

CanvasRenderingContext2D.prototype.closepath = function () {
  if (counterclockwise(customFont.points)) {
    ctx2.globalCompositeOperation = 'xor';
  }
  customFont.points = Array();
  ctx2.closePath();
  if (customFont.color) {
    ctx2.fill();
  }
  if (customFont.borderColor) {
    ctx2.stroke();
  }
  ctx2.globalCompositeOperation = 'source-over';
  ctx2.beginPath();
}

CanvasRenderingContext2D.prototype.vhcurveto = function (y1, x2, y2, x3) {
  if (customFont.counterclockwise == -1) {
    customFont.counterclockwise = (y1 + y2 > 0) ? 1 : 0;
  }
  this.rrcurveto(0, y1, x2, y2, x3, 0);
}

CanvasRenderingContext2D.prototype.hvcurveto = function (x1, x2, y2, y3) {
  this.rrcurveto(x1, 0, x2, y2, 0, y3);
}

CanvasRenderingContext2D.prototype.rrcurveto = function (x1, y1, x2, y2, x3, y3) {
  if (customFont.counterclockwise == -1) {
    customFont.counterclockwise = 0;
  }
  this.rcurveto(x1, y1, x1 + x2, y1 + y2, x1 + x2 + x3, y1 + y2 + y3);
}

CanvasRenderingContext2D.prototype.rcurveto = function (x1, y1, x2, y2, x3, y3) {
  this.bezierCurveTo(customFont.p.x + x1 * customFont.scale, customFont.p.y - y1 * customFont.scale, customFont.p.x + x2 * customFont.scale, customFont.p.y - y2 * customFont.scale, customFont.p.x + x3 * customFont.scale, customFont.p.y - y3 * customFont.scale);
  customFont.p = {x: customFont.p.x + x3 * customFont.scale, y: customFont.p.y - y3 * customFont.scale}
}

CanvasRenderingContext2D.prototype.drawLine = function (x1, y1, x2, y2) {
  ctx3.beginPath();
  ctx3.moveTo(x1, y1);
  ctx3.strokeStyle = 'red';
  ctx3.lineTo(x2, y2);
  ctx3.closePath();
  ctx3.stroke();
}

CanvasRenderingContext2D.prototype.hsbw = function (sbx, wx) {
  customFont.cursor.x += Math.round(wx * customFont.scale);
  this.moveTo(customFont.cursor.x, customFont.cursor.y);
  customFont.p = {x: customFont.cursor.x, y: customFont.cursor.y};
}

CanvasRenderingContext2D.prototype.rlineto = function (dx, dy) {
  customFont.p.x += Math.round(dx * customFont.scale);
  customFont.p.y -= Math.round(dy * customFont.scale);
  this.lineTo(customFont.p.x, customFont.p.y);
}

CanvasRenderingContext2D.prototype.hlineto = function (dx) {
  customFont.p.x += Math.round(dx * customFont.scale);
  this.lineTo(customFont.p.x, customFont.p.y);
}

CanvasRenderingContext2D.prototype.vlineto = function (dy) {
  customFont.p.y -= Math.round(dy * customFont.scale);
  this.lineTo(customFont.p.x, customFont.p.y);
}

CanvasRenderingContext2D.prototype.rmoveto = function (dx, dy) {
  customFont.p.x += Math.round(dx * customFont.scale);
  customFont.p.y -= Math.round(dy * customFont.scale);
  this.moveTo(customFont.p.x, customFont.p.y);
}

CanvasRenderingContext2D.prototype.vmoveto = function (dy) {
  customFont.p.y -= Math.round(dy * customFont.scale);
  this.moveTo(customFont.p.x, customFont.p.y);
}

CanvasRenderingContext2D.prototype.hmoveto = function (dx) {
  customFont.p.x += Math.round(dx * customFont.scale);
  this.moveTo(customFont.p.x, customFont.p.y);
}

function crazy() {
  var compare = $('#e').val();
  var found = false;
  for (var c in chinesestrokes) {
    cd = chinesestrokes[c];
    $('#c').html(chineseword(cd[0]));
    if ($('#c').html() == compare) {
      found = true;
      $('#c').append(' ' + h2d(cd[0]) + ' ' + cd[0]);
      break;
    }
  }
  alert(found);
}
