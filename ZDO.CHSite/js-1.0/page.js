/// <reference path="/lib/jquery-2.1.4.min.js" />
/// <reference path="/lib/history.min.js" />

var zdPage = (function () {
  "use strict";

  var reqId = 0; // Current page load request ID. If page has moved on, earlier requests ignored when they complete.
  var location = null; // Full location, as seen in navbar
  var path = null; // Path after domain name
  var lang = null; // Language (first section of path)
  var rel = null; // Relative path (path without language ID at start)

  var registeredScripts = {};

  // Incremented for subsequent alerts, so we can correctly animate new one shown before old one has expired.
  var alertId = 0;

  var alertTemplate =
    '<div class="alertBar" id="alertBarId">' +
    '  <div class="alert" id="alertId">' +
    '    <div class="alertMessage"><span class="alertTitle" /><span class="alertBody" /></div>' +
    '    <div class="alertClose"><img src="/static/close.svg" alt="" /></div>' +
    '  </div>' +
    '</div>';

  // Parse full path, language, and relative path from URL
  function parseLocation() {
    location = window.history.location || window.location;
    var rePath = /https?:\/\/[^\/]+(.*)/i;
    var match = rePath.exec(location);
    path = match[1];
    if (path.startsWith("/en/") || path == "/en") {
      lang = "en";
      rel = path == "/en" ? "" : path.substring(4);
    }
    else if (path.startsWith("/hu/") || path == "/hu") {
      lang = "hu";
      rel = path == "/hu" ? "" : path.substring(4);
    }
    else {
      lang = "hu";
      rel = path;
    }
  }

  // Page just loaded: time to get dynamic part asynchronously, wherever we just landed
  $(document).ready(function () {
    // Make sense of location
    parseLocation();
    // Update menu to show where I am (will soon end up being)
    updateMenuState();
    // Request dynamic page - async
    ++reqId;
    var id = reqId;
    var req = $.ajax({
      url: "/Handler.ashx",
      type: "POST",
      contentType: "application/x-www-form-urlencoded; charset=UTF-8",
      data: { action: "dynpage", lang: lang, rel: rel }
    });
    req.done(function (data) {
      dynReady(data, id);
    });
    req.fail(function (jqXHR, textStatus, error) {
      applyFailHtml();
    });
  });

  // Navigate within single-page app (invoked from link click handler)
  function dynNavigate() {
    // Make sense of location
    parseLocation();
    // Clear whatever's currently shown
    //$("#dynPage").html("");
    // Update menu to show where I am (will soon end up being)
    updateMenuState();
    // Request dynamic page - async
    ++reqId;
    var id = reqId;
    var req = $.ajax({
      url: "/Handler.ashx",
      type: "POST",
      contentType: "application/x-www-form-urlencoded; charset=UTF-8",
      data: { action: "dynpage", lang: lang, rel: rel }
    });
    req.done(function (data) {
      navReady(data, id);
    });
    req.fail(function (jqXHR, textStatus, error) {
      applyFailHtml();
    });
  }

  // Show error content in dynamic area
  function applyFailHtml() {
    $("#dynPage").html("Ouch.");
    // TO-DO: fail title; keywords; description
  }

  // Apply dynamic content: HTML body, title, description, keywords
  function applyDynContent(html, title, description, keywords) {
    $(document).attr("title", title);
    $("meta[name = 'keywords']").attr("content", keywords);
    $("meta[name = 'description']").attr("content", description);
    $("#dynPage").html(html);
    // Run this page's script initializer, if any
    for (var key in registeredScripts) {
      if (rel.startsWith(key)) registeredScripts[key]();
      // Hack: call search initializer for ""
      if (rel == "" && key == "search") registeredScripts[key]();
    }
  }

  function navReady(data, id) {
    // An obsolete request completing too late?
    if (id != reqId) return;

    // Show dynamic content, title etc.
    applyDynContent(data.html, data.title, data.description, data.keywords);
  }

  // Dynamic data received after initial page load (not within single-page navigation)
  function dynReady(data, id) {
    // An obsolete request completing too late?
    if (id != reqId) return;

    // Show dynamic content, title etc.
    applyDynContent(data.html, data.title, data.description, data.keywords);

    // Set up single-page navigation
    $(document).on('click', 'a.ajax', function () {
      // Trick: If we're on search page but menu is shown, link just changes display; no navigation
      if ((rel == "" || rel.startsWith("search")) && $(this).attr("id") == "topMenuSearch") {
        $("#hdrSearch").addClass("on");
        $("#hdrMenu").removeClass("on");
        $("#subHeader").removeClass("on");
        return false;
      }
      // Navigate
      history.pushState(null, null, this.href);
      dynNavigate();
      return false;
    });
    $(window).on('popstate', function (e) {
      dynNavigate();
    });

    // *NOW* that we're all done, show page.
    $("#thePage").addClass("visible");
    // Events - toggle from lookup input to menu
    $("#toMenu").click(function () {
      $("#hdrSearch").removeClass("on");
      $("#hdrMenu").addClass("on");
      $("#subHeader").addClass("on");
    });
  }

  // Updates top navigation menu to reflect where we are
  function updateMenuState() {
    $(".topMenu").removeClass("on");
    $(".subMenu").removeClass("visible");
    if (rel == "" || rel.startsWith("search")) {
      $("#hdrMenu").removeClass("on");
      $("#dynPage").addClass("nosubmenu");
      $("#hdrSearch").addClass("on");
    }
    else {
      $("#hdrSearch").removeClass("on");
      $("#hdrMenu").addClass("on");
      $("#subHeader").addClass("visible");
      $("#dynPage").removeClass("nosubmenu");
      if (rel.startsWith("edit")) {
        $("#topMenuEdit").addClass("on");
        $("#subMenuEdit").addClass("visible");
      }
      else if (rel.startsWith("read")) {
        $("#topMenuRead").addClass("on");
        $("#subMenuRead").addClass("visible");
      }
      else if (rel.startsWith("download")) {
        $("#topMenuDownload").addClass("on");
        $("#subMenuDownload").addClass("visible");
      }
    }
    $(".subMenu span").removeClass("on");
    if (rel.startsWith("edit/new")) $("#smEditNew").addClass("on");
    else if (rel.startsWith("edit/history")) $("#smEditHistory").addClass("on");
    else if (rel.startsWith("edit/existing")) $("#smEditExisting").addClass("on");
    else if (rel.startsWith("read/about")) $("#smReadAbout").addClass("on");
    else if (rel.startsWith("read/articles")) $("#smReadArticles").addClass("on");
    else if (rel.startsWith("read/etc")) $("#smReadEtc").addClass("on");
    // Language selector
    $("#langSelHu").attr("href", "/hu/" + rel);
    $("#langSelEn").attr("href", "/en/" + rel);
    $(".langSel").removeClass("on");
    if (lang == "en") $("#langSelEn").addClass("on");
    else if (lang == "hu") $("#langSelHu").addClass("on");
  }

  return {
    // Called by page-specific controller scripts to register themselves in single-page app.
    registerScript: function (pageRel, init) {
      registeredScripts[pageRel] = init;
    },

    getLang: function() {
      return lang;
    },

    isMobile: function() {
      return false;
    },

    submitSearch: function(query) {
      history.pushState(null, null, "/" + lang + "/search/" + query);
      dynNavigate();
    },

    // Shows an alert at the top of the page.
    showAlert: function (title, body, isError) {
      // Remove old alert
      $(".alertBar").remove();
      // Class for current one
      ++alertId;
      var currBarId = "alertbar" + alertId;
      var currAlertId = "alert" + alertId;
      var templ = alertTemplate.replace("alertBarId", currBarId);
      templ = templ.replace("alertId", currAlertId);
      var elm = $(templ);
      $("body").append(elm);
      $("#" + currAlertId + " .alertTitle").text(title);
      if (body) {
        $("#" + currAlertId + " .alertTitle").append("<br>");
        $("#" + currAlertId + " .alertBody").text(body);
      }
      if (isError) $("#" + currAlertId).addClass("alertFail");
      else $("#" + currAlertId).addClass("alertOK");

      $("#" + currAlertId + " .alertClose").click(function () {
        $("#" + currBarId).remove();
        //$("#" + currAlertId).addClass("hidden");
        //setTimeout(function () {
        //  $("#" + currBarId).remove();
        //}, 5000);
      });

      setTimeout(function () {
        $("#" + currAlertId).addClass("visible");
        setTimeout(function () {
          $("#" + currAlertId).addClass("hidden");
          setTimeout(function () {
            $("#" + currBarId).remove();
          }, 1000)
        }, 5000);
      }, 50);
    }

  };

})();
