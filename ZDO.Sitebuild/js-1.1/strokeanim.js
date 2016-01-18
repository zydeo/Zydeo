var soa_glyph = {
  strokes: null,
  medians: null,
  medianPaths: [],
  medianLengths: [],
};

var soa_animstate = {
  currFinished: false,
  currStroke: 0,
  currLength: 0,
};

var soa_timer = null;
var soa_increment = 20;
var soa_strokepause = 20;
var soa_msec = 20;

var soa_gridcolor = "#607026";
var soa_ghostcolor = "#d3d3d3";
var soa_finishedcolor = "#303030";
var soa_activecolor = "#606060";

var _svgNS = 'http://www.w3.org/2000/svg';

function soaGetMedianPath(median) {
  const result = [];
  for (var i = 0; i != median.length; i++) {
    var point = median[i];
    result.push(result.length === 0 ? 'M' : 'L');
    result.push('' + point[0]);
    result.push('' + point[1]);
  }
  return result.join(' ');
}

function soaGetMedianLength(median) {
  var result = 0;
  for (var i = 0; i < median.length - 1; i++) {
    var x1 = median[i][0];
    var y1 = median[i][1];
    var x2 = median[i + 1][0];
    var y2 = median[i + 1][1];
    result += Math.sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
  }
  return result;
}

function soaPrepareGlyph(strokes, medians) {
  if (soa_timer != null) {
    clearInterval(soa_timer);
    soa_timer = null;
  }
  soa_animstate.lastAnimating = -1;
  soa_animstate.currStroke = 0;
  soa_animstate.currLength = 0;
  soa_glyph.strokes = strokes;
  soa_glyph.medians = medians;
  soa_glyph.medianPaths = [];
  soa_glyph.medianLengths = [];
  for (var i = 0; i != soa_glyph.medians.length; i++) {
    soa_glyph.medianPaths.push(soaGetMedianPath(soa_glyph.medians[i]));
    soa_glyph.medianLengths.push(soaGetMedianLength(soa_glyph.medians[i]));
  }
}

function svgRect(x, y, width, height, fill, stroke, strokeWidth, strokeDasharray) {
  var rect = document.createElementNS(_svgNS, 'rect');
  rect.setAttributeNS(null, "x", x);
  rect.setAttributeNS(null, "y", y);
  rect.setAttributeNS(null, "width", width);
  rect.setAttributeNS(null, "height", height);
  rect.setAttributeNS(null, "fill", fill);
  rect.setAttributeNS(null, "stroke", stroke);
  rect.setAttributeNS(null, "stroke-width", strokeWidth);
  rect.setAttributeNS(null, "stroke-dasharray", strokeDasharray);
  return rect;
}

function svgLine(x1, y1, x2, y2, stroke, strokeWidth, strokeDasharray) {
  var line = document.createElementNS(_svgNS, 'line');
  line.setAttributeNS(null, "x1", x1);
  line.setAttributeNS(null, "y1", y1);
  line.setAttributeNS(null, "x2", x2);
  line.setAttributeNS(null, "y2", y2);
  line.setAttributeNS(null, "stroke", stroke);
  line.setAttributeNS(null, "stroke-width", strokeWidth);
  line.setAttributeNS(null, "stroke-dasharray", strokeDasharray);
  return line;
}

function svgPath(fill, stroke, d) {
  var p = document.createElementNS(_svgNS, "path");
  p.setAttributeNS(null, "fill", fill);
  p.setAttributeNS(null, "stroke", stroke);
  p.setAttributeNS(null, "d", d);
  return p;
}

function soaRenderBG() {
  // The SVG element
  var r = document.getElementById('strokeAnimSVG');
  // Remove all children
  while (r.hasChildNodes()) r.removeChild(r.lastChild);
  // Grid with dashed lines
  r.appendChild(svgRect(2, 2, 1022, 1022, "none", soa_gridcolor, 2, "10, 5"));
  r.appendChild(svgLine(0, 512, 1024, 512, soa_gridcolor, 2, "10, 5"));
  r.appendChild(svgLine(512, 0, 512, 1024, soa_gridcolor, 2, "10, 5"));
  r.appendChild(svgLine(0, 0, 1024, 1024, soa_gridcolor, 2, "10, 5"));
  r.appendChild(svgLine(1024, 0, 0, 1024, soa_gridcolor, 2, "10, 5"));
  // Group containing all the graphics
  var g = document.createElementNS(_svgNS, 'g');
  g.setAttributeNS(null, "id", "strokeAnimGroup");
  g.setAttributeNS(null, "transform", "scale(1, -1) translate(0, -900)");
  r.appendChild(g);
  // Sub-groups for: ghost; active; finished. We need this order to stand in for z-order.
  var subg = document.createElementNS(_svgNS, 'g');
  subg.setAttributeNS(null, "id", "strokeGroupGhost");
  g.appendChild(subg);
  subg = document.createElementNS(_svgNS, 'g');
  subg.setAttributeNS(null, "id", "strokeGroupActive");
  g.appendChild(subg);
  subg = document.createElementNS(_svgNS, 'g');
  subg.setAttributeNS(null, "id", "strokeGroupFinished");
  g.appendChild(subg);
}

function soaPreRender() {
  // Transform group
  var g = document.getElementById('strokeGroupGhost');
  // Render all strokes as unfinished, each within a named group
  for (var i = 0; i < soa_glyph.strokes.length; i++) {
    // Build stroke's path
    var p = svgPath(soa_ghostcolor, soa_ghostcolor, soa_glyph.strokes[i]);
    // Create group
    var sg = document.createElementNS(_svgNS, 'g');
    sg.setAttributeNS(null, "id", "strokeGroup" + i);
    sg.appendChild(p);
    g.appendChild(sg);
  }
}

function soaRender() {
  if (soa_animstate.currStroke >= soa_glyph.strokes.length)
    return;
  // Current stroke's data
  var stroke = soa_glyph.strokes[soa_animstate.currStroke];
  var medianPath = soa_glyph.medianPaths[soa_animstate.currStroke];
  var medianLength = soa_glyph.medianLengths[soa_animstate.currStroke];
  // Get the containing group
  var sgId = "strokeGroup" + soa_animstate.currStroke;
  var sg = document.getElementById(sgId);
  // Only animate with clip region if we're *not* beyond its length already
  if (soa_animstate.currLength < medianLength) {
    // Clear stroke's group
    while (sg.hasChildNodes()) sg.removeChild(sg.lastChild);
    // Add stroke's group to active
    var ag = document.getElementById("strokeGroupActive");
    ag.appendChild(sg);
    // Re-render ghost in active group
    sg.appendChild(svgPath(soa_ghostcolor, soa_ghostcolor, stroke));
    // Clip path: the stroke itself
    var cl = document.createElementNS(_svgNS, 'clipPath');
    cl.setAttributeNS(null, "id", "strokeClip");
    var cp = document.createElementNS(_svgNS, 'path');
    cp.setAttributeNS(null, "d", stroke);
    cl.appendChild(cp);
    sg.appendChild(cl);
    // Big fat line, with dash offset
    var p = svgPath("none", soa_activecolor, medianPath);
    p.setAttributeNS(null, "clip-path", "url(#strokeClip)");
    p.setAttributeNS(null, "stroke-linecap", "round");
    p.setAttributeNS(null, "stroke-width", "128");
    p.setAttributeNS(null, "stroke-dasharray", medianLength + ' ' + 2 * medianLength);
    p.setAttributeNS(null, "stroke-dashoffset", medianLength - soa_animstate.currLength);
    sg.appendChild(p);
  }
  // If we're past active stroke's length, draw it as finished - this is our post-anim delay
  else {
    // Need to draw only once; but at that stage, also append a "use" to SVG,
    // To make sure finished strokes are always on top
    if (!soa_animstate.currFinished) {
      soa_animstate.currFinished = true;
      // Clear stroke's group
      while (sg.hasChildNodes()) sg.removeChild(sg.lastChild);
      // Add stroke's group to finished
      var ag = document.getElementById("strokeGroupFinished");
      ag.appendChild(sg);
      // Draw finished stroke
      sg.appendChild(svgPath(soa_finishedcolor, soa_finishedcolor, stroke));
    }
  }
}

function soaTimerFun() {
  // All strokes done? Exit now, stop timer.
  if (soa_animstate.currStroke == soa_glyph.strokes.length) {
    clearInterval(soa_timer);
    soa_timer = null;
    return;
  }
  // Current animated stroke just finished? Move on to next.
  if (soa_animstate.currLength > soa_glyph.medianLengths[soa_animstate.currStroke] + soa_increment * soa_strokepause) {
    soa_animstate.currLength = 0;
    soa_animstate.currStroke++;
    soa_animstate.currFinished = false;
  }
  // Nop, current stroke still in progress: increase length
  else soa_animstate.currLength += soa_increment;
  // Render
  soaRender();
}

function onSoaReqDone(res) {
  if (res == null) {
    var msg = "Sorry, we don't seem to know this character."
    alert(msg);
  }
  else {
    soaPrepareGlyph(res.strokes, res.medians);
    soaRenderBG();
    soaPreRender();
    soa_timer = setInterval(soaTimerFun, soa_msec);
  }
}


function onSoaReqFail(xhr, status, error) {
  var msg = "Ooops... Error\n\n" + xhr.status + ": " + xhr.responseText;
  alert(msg);
}

function soaStartQuery() {
  var hanzi = $('#txtHanzi').val();
  var req = $.ajax({
    url: "http://localhost:65100/ApiHandler.ashx",
    type: "POST",
    contentType: "application/x-www-form-urlencoded; charset=UTF-8",
    data: {action: "hanzi", hanzi: hanzi}
  });
  req.done(function(res) {
      onSoaReqDone(res);
    });
  req.fail(function(xhr, status, error) {
    onSoaReqFail(xhr, status, error);
  });
  $("#txtHanzi").focus();
  $("#txtHanzi").select();
}

$(document).ready(function () {
  soaRenderBG();
  $("#btnHanzi").click(soaStartQuery);
  $("#txtHanzi").keyup(function (e) {
    if (e.keyCode == 13) {
      soaStartQuery();
      return false;
    }
  });
});

