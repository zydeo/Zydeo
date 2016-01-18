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

function soaRenderBG() {
  // The SVG element
  var r = document.getElementById('strokeAnimSVG');
  // Remove all children
  while (r.hasChildNodes()) r.removeChild(r.lastChild);
  // Grid with dashed lines
  var rect = document.createElementNS(_svgNS, 'rect');
  rect.setAttributeNS(null, "x", "2");
  rect.setAttributeNS(null, "y", "2");
  rect.setAttributeNS(null, "width", "1022");
  rect.setAttributeNS(null, "height", "1022");
  rect.setAttributeNS(null, "fill", "none");
  rect.setAttributeNS(null, "stroke", soa_gridcolor);
  rect.setAttributeNS(null, "stroke-width", "2");
  rect.setAttributeNS(null, "stroke-dasharray", "10, 5");
  r.appendChild(rect);
  var line = document.createElementNS(_svgNS, 'line');
  line.setAttributeNS(null, "x1", "0");
  line.setAttributeNS(null, "y1", "512");
  line.setAttributeNS(null, "x2", "1024");
  line.setAttributeNS(null, "y2", "512");
  line.setAttributeNS(null, "stroke", soa_gridcolor);
  line.setAttributeNS(null, "stroke-width", "2");
  line.setAttributeNS(null, "stroke-dasharray", "10, 5");
  r.appendChild(line);
  line = document.createElementNS(_svgNS, 'line');
  line.setAttributeNS(null, "x1", "512");
  line.setAttributeNS(null, "y1", "0");
  line.setAttributeNS(null, "x2", "512");
  line.setAttributeNS(null, "y2", "1024");
  line.setAttributeNS(null, "stroke", soa_gridcolor);
  line.setAttributeNS(null, "stroke-width", "2");
  line.setAttributeNS(null, "stroke-dasharray", "10, 5");
  r.appendChild(line);
  line = document.createElementNS(_svgNS, 'line');
  line.setAttributeNS(null, "x1", "0");
  line.setAttributeNS(null, "y1", "0");
  line.setAttributeNS(null, "x2", "1024");
  line.setAttributeNS(null, "y2", "1024");
  line.setAttributeNS(null, "stroke", soa_gridcolor);
  line.setAttributeNS(null, "stroke-width", "2");
  line.setAttributeNS(null, "stroke-dasharray", "10, 5");
  r.appendChild(line);
  line = document.createElementNS(_svgNS, 'line');
  line.setAttributeNS(null, "x1", "1024");
  line.setAttributeNS(null, "y1", "0");
  line.setAttributeNS(null, "x2", "0");
  line.setAttributeNS(null, "y2", "1024");
  line.setAttributeNS(null, "stroke", soa_gridcolor);
  line.setAttributeNS(null, "stroke-width", "2");
  line.setAttributeNS(null, "stroke-dasharray", "10, 5");
  r.appendChild(line);
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
    var p = document.createElementNS(_svgNS, "path");
    p.setAttributeNS(null, "fill", "lightgrey");
    p.setAttributeNS(null, "stroke", "lightgrey");
    p.setAttributeNS(null, "d", soa_glyph.strokes[i]);
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
    var pghost = document.createElementNS(_svgNS, "path");
    pghost.setAttributeNS(null, "fill", "lightgrey");
    pghost.setAttributeNS(null, "stroke", "lightgrey");
    pghost.setAttributeNS(null, "d", stroke);
    sg.appendChild(pghost);
    // Clip path: the stroke itself
    var cl = document.createElementNS(_svgNS, 'clipPath');
    cl.setAttributeNS(null, "id", "strokeClip");
    var cp = document.createElementNS(_svgNS, 'path');
    cp.setAttributeNS(null, "d", stroke);
    cl.appendChild(cp);
    sg.appendChild(cl);
    // Big fat line, with dash offset
    var p = document.createElementNS(_svgNS, 'path');
    p.setAttributeNS(null, "clip-path", "url(#strokeClip)");
    p.setAttributeNS(null, "d", medianPath);
    p.setAttributeNS(null, "fill", "none");
    p.setAttributeNS(null, "stroke", "#505050");
    p.setAttributeNS(null, "stroke-linecap", "round");
    p.setAttributeNS(null, "stroke-width", "128");
    var dashArr = medianLength + ' ' + 2 * medianLength;
    p.setAttributeNS(null, "stroke-dasharray", dashArr);
    //var dashOfs = 128 + 0.5 * medianLength;
    var dashOfs = medianLength - soa_animstate.currLength;
    p.setAttributeNS(null, "stroke-dashoffset", dashOfs);
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
      var p = document.createElementNS(_svgNS, "path");
      p.setAttributeNS(null, "fill", "black");
      p.setAttributeNS(null, "stroke", "black");
      p.setAttributeNS(null, "d", stroke);
      sg.appendChild(p);
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

