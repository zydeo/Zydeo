/// <reference path="x-jquery-2.1.4.min.js" />
/// <reference path="x-jquery.tooltipster.min.js" />
/// <reference path="strings-hu.js" />
/// <reference path="page.js" />

var zdStrokeAnim = (function () {

  var soaTemplate =
    '<div id="soaBoxTail">&nbsp;</div>\n' +
    '<div id="soaHead">\n' +
    '  <div id="soaTitle">{{soa-title}}</div>\n' +
    '  <div id="soaClose">X</div>\n' +
    '</div>\n' +
    '<div id="soaGraphics">\n' +
    '  <svg xmlns="http://www.w3.org/2000/svg" version="1.1" viewbox="0 0 1024 1024" id="strokeAnimSVG"></svg>\n' +
    '  <div id="soaError"><div id="soaErrorContent"></div></div>\n' +
    '</div>\n' +
    '<div id="soaFooter">{{soa-attribution}}</div>\n';

  // The glyph currently being animated.
  var soa_glyph = {
    strokes: null, // Received info about glyph
    medians: null, // Received info about glyph
    medianPaths: [], // Calculated locally
    medianLengths: [], // Calculated locally
  };

  // Current animation state; updated in timer callback, before each new frame render.
  var soa_animstate = {
    currFinished: false,
    currStroke: 0,
    currLength: 0,
  };

  // ID of lookup we're currently waiting for. (So we can discard moot lookups just completing.)
  var soa_lookupid = parseInt((Math.random() * 1000), 10);
  // Animation timer.
  var soa_timer = null;
  // Length to advance at each frame, along median path.
  var soa_increment = 20;
  // Show finished stroke for this number of frames, before moving on to animation of next stroke.
  var soa_strokepause = 20;
  // Frame interval.
  var soa_msec = 20;

  var soa_gridcolor = "#607026";
  var soa_ghostcolor = "#d3d3d3";
  var soa_finishedcolor = "#303030";
  var soa_activecolor = "#606060";

  var _svgNS = 'http://www.w3.org/2000/svg';

  function init() {
    var html = soaTemplate;
    html = html.replace("{{soa-title}}", uiStrings["anim-title"]);
    html = html.replace("{{soa-attribution}}", uiStrings["anim-attribution"]);
    $("#soaBox").html(html);
    $("#soaGraphics").click(function () {
      // Restart animation if we have a glyph but no timer anymore
      if (soa_timer == null && soa_glyph.strokes != null && soa_glyph.strokes.length > 0) {
        soaPrepareGlyph(soa_glyph.strokes, soa_glyph.medians);
        soaRenderBG();
        soaPreRender();
        soa_timer = setInterval(soaTimerFun, soa_msec);
      }
    });
  }

  // Gets an SVG path from a stroke's median points.
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

  // Calculates length of a stroke's median.
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

  // Prepares new glyph (median paths and lengths), clears animation state
  function soaPrepareGlyph(strokes, medians) {
    if (soa_timer != null) {
      clearInterval(soa_timer);
      soa_timer = null;
    }
    soa_animstate.currFinished = false;
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

  // Creates a <rect> that can be inserted into an SVG's DOM.
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

  // Creates a <line> that can be inserted into an SVG's DOM.
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

  // Creates a <path> that can be inserted into an SVG's DOM.
  function svgPath(fill, stroke, d) {
    var p = document.createElementNS(_svgNS, "path");
    p.setAttributeNS(null, "fill", fill);
    p.setAttributeNS(null, "stroke", stroke);
    p.setAttributeNS(null, "d", d);
    return p;
  }

  // Renders the background (grid) of the SVG, ready to receive strokes.
  function renderBG() {
    // No error message; animation on
    $("#soaError").css("display", "none");
    // The SVG element
    var r = document.getElementById('strokeAnimSVG');
    // Remove all children
    while (r.hasChildNodes()) r.removeChild(r.lastChild);
    // Grid with dashed lines
    r.appendChild(svgRect(2, 2, 1022, 1022, "none", soa_gridcolor, 4, "10, 5"));
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

  // Pre-renders a glyph before animation: all strokes are "ghosts"; groups for each.
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

  // Renders the glyph corresponding to the current anim state.
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

  // Kills anything in progress (ongoing queries will be ignored; animation is stopped).
  function kill() {
    // If a previous request completes, we'll discard it
    ++soa_lookupid;
    // If we're just animating, stop it
    if (soa_timer != null) {
      clearInterval(soa_timer);
      soa_timer = null;
    }
  }

  // Animation timer callback: advances anim state, and renders.
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

  // AJAX request success
  function onSoaReqDone(id, res) {
    // This is a different request just completing.
    if (id != soa_lookupid) return;
    // No result
    if (res == null) {
      $("#soaError").css("display", "block");
      $("#soaErrorContent").text(uiStrings["no-animation-for-char"]);
    }
      // Render result
    else {
      soaPrepareGlyph(res.strokes, res.medians);
      soaRenderBG();
      soaPreRender();
      soa_timer = setInterval(soaTimerFun, soa_msec);
    }
  }

  // AJAX request failure
  function onSoaReqFail(id, xhr, status, error) {
    // This is a different request just failing; don't care
    if (id != soa_lookupid) return;
    $("#soaError").css("display", "block");
    $("#soaErrorContent").text(uiStrings["anim-query-failed"]);
  }

  // Prepare an issue AJAX query for a Hanzi's stroke order info.
  function startQuery(hanzi) {
    soaPrepareGlyph([], []);
    ++soa_lookupid;
    var id = soa_lookupid;
    // Query URL: localhost for sandboxing only
    var url = "/ApiHandler.ashx";
    if (window.location.protocol == "file:")
      url = "http://localhost:65100/ApiHandler.ashx";
    var req = $.ajax({
      url: url,
      type: "POST",
      contentType: "application/x-www-form-urlencoded; charset=UTF-8",
      data: { action: "hanzi", hanzi: hanzi, lookupid: soa_lookupid }
    });
    req.done(function (res) {
      onSoaReqDone(id, res);
    });
    req.fail(function (xhr, status, error) {
      onSoaReqFail(id, xhr, status, error);
    });
  }

  return {
    init: function () { init(); },
    kill: function () { kill(); },
    renderBG: function () { renderBG(); },
    startQuery: function (hanzi) { startQuery(hanzi); }
  };

})();
