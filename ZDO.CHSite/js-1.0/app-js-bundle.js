var uiStringsEn = {
  "empty-str": "",
  "tooltip-btn-brush": "Handwriting recognition",
  "tooltip-btn-settings": "Keresési beállítások",
  "tooltip-btn-search": "Keresés a CHDICT-ben (Enter)",
  "no-animation-for-char": "Ehhez az írásjegyhez sajnos nincs vonássorend-animáció.",
  "anim-query-failed": "Nem sikerült betölteni az írásjegyhez tartozó vonássorrend-animációt.",
  "anim-attribution": "Forrás: <a href='https://skishore.github.io/makemeahanzi/' target='_blank'>makemeahanzi</a>",
  "anim-title": "Vonássorrend",
  "options-title": "Keresési beállítások",
  "options-script": "Írásjegyek",
  "options-simplified": "Egyszerűsített",
  "options-traditional": "Hagyományos",
  "options-bothscripts": "Mindkettő",
  "options-tonecolors": "Hangsúly szerinti színezés",
  "options-nocolors": "Nincs",
  "options-pleco": "Pleco",
  "options-dummitt": "Dummitt",
  "tooltip-history-comment": "Megjegyzés hozzáfűzése",
  "tooltip-history-edit": "Szócikk szerkesztése",
  "tooltip-history-flag": "Szócikk megjelölése (pontatlan, téves vagy hiányos)",
};

var uiStringsHu = {
  "empty-str": "",
  "tooltip-btn-brush": "Kézírás-felismerés",
  "tooltip-btn-settings": "Keresési beállítások",
  "tooltip-btn-search": "Keresés a CHDICT-ben (Enter)",
  "no-animation-for-char": "Ehhez az írásjegyhez sajnos nincs vonássorend-animáció.",
  "anim-query-failed": "Nem sikerült betölteni az írásjegyhez tartozó vonássorrend-animációt.",
  "anim-attribution": "Forrás: <a href='https://skishore.github.io/makemeahanzi/' target='_blank'>makemeahanzi</a>",
  "anim-title": "Vonássorrend",
  "options-title": "Keresési beállítások",
  "options-script": "Írásjegyek",
  "options-simplified": "Egyszerűsített",
  "options-traditional": "Hagyományos",
  "options-bothscripts": "Mindkettő",
  "options-tonecolors": "Hangsúly szerinti színezés",
  "options-nocolors": "Nincs",
  "options-pleco": "Pleco",
  "options-dummitt": "Dummitt",
  "tooltip-history-comment": "Megjegyzés hozzáfűzése",
  "tooltip-history-edit": "Szócikk szerkesztése",
  "tooltip-history-flag": "Szócikk megjelölése<br/>(pontatlan, téves vagy hiányos)",
  "dialog-ok": "OK",
  "dialog-cancel": "Mégse",
  "history-commententry-title": "Megjegyzés hozzáfűzése",
  "history-commententry-hint": "Írd ide, amit hozzáfűznél a szócikkhez...",
  "history-commententry-successtitle": "Megjegyzés elmentve",
  "history-commententry-successmessage": "A megjegyzésedet sikeresen elmentette a CHDICT. A szócikket a változás-történet első oldalának tetején találod.",
};

/// <reference path="x-jquery-2.1.4.min.js" />
/// <reference path="x-history.min.js" />

var uiStrings = uiStringsHu;

function startsWith(str, prefix) {
  if (str.length < prefix.length)
    return false;
  for (var i = prefix.length - 1; (i >= 0) && (str[i] === prefix[i]) ; --i)
    continue;
  return i < 0;
}

function escapeHTML(s) {
  return s.replace(/&/g, '&amp;')
          .replace(/"/g, '&quot;')
          .replace(/</g, '&lt;')
          .replace(/>/g, '&gt;');
}

var zdPage = (function () {
  "use strict";

  var reqId = 0; // Current page load request ID. If page has moved on, earlier requests ignored when they complete.
  var location = null; // Full location, as seen in navbar
  var path = null; // Path after domain name
  var lang = null; // Language (first section of path)
  var rel = null; // Relative path (path without language ID at start)

  // Page init scripts for each page (identified by relPath).
  var initScripts = {};
  // Global init scripts invoked on documentReady.
  var globalInitScripts = [];

  // Function that provides search parameters (submitted alongside regular AJAX page requests).
  var searchParamsProvider = null;

  // Close function of currently active modal popup, or null.
  var activeModalCloser = null;

  // Incremented for subsequent alerts, so we can correctly animate new one shown before old one has expired.
  var alertId = 0;

  var alertTemplate =
    '<div class="alertBar" id="alertBarId">' +
    '  <div class="alert" id="alertId">' +
    '    <div class="alertMessage"><span class="alertTitle" /><span class="alertBody" /></div>' +
    '    <div class="alertClose"><img src="/static/close.svg" alt="" /></div>' +
    '  </div>' +
    '</div>';

  var modalPopupTemplate =
    '<div class="modalPopup" id="{{id}}">' +
    '  <div class="modalPopupInner1">' +
    '    <div class="modalPopupInner2">' +
    '      <div class="modalPopupHeader">' +
    '        <span class="modalPopupTitle">{{title}}</span>' +
    '        <span class="modalPopupClose">X</span>' +
    '      </div>' +
    '      <div class="modalPopupBody">' +
    '        {{body}}' +
    '      </div>' +
    '      <div class="modalPopupButtons">' +
    '        <span class="modalPopupButton modalPopupButtonCancel">{{Cancel}}</span>' +
    '        <span class="modalPopupButton modalPopupButtonOK">{{OK}}</span>' +
    '      </div>' +
    '    </div>' +
    '  </div>' +
    '</div>';

 
  // Parse full path, language, and relative path from URL
  function parseLocation() {
    location = window.history.location || window.location;
    var rePath = /https?:\/\/[^\/]+(.*)/i;
    var match = rePath.exec(location);
    path = match[1];
    if (startsWith(path, "/en/") || path == "/en") {
      lang = "en";
      rel = path == "/en" ? "" : path.substring(4);
      uiStrings = uiStringsEn;
    }
    else if (startsWith(path, "/hu/") || path == "/hu") {
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
    // Global script initializers
    for (var i = 0; i != globalInitScripts.length; ++i) globalInitScripts[i]();
    // Request dynamic page - async
    // Skipped if we received page with content present already
    var hasContent = $("#theBody").hasClass("has-initial-content");
    if (!hasContent) {
      ++reqId;
      var id = reqId;
      var data = { action: "dynpage", lang: lang, rel: rel };
      // Infuse extra params (search)
      infuseSearchParams(data);
      // Submit request
      var req = $.ajax({
        url: "/Handler.ashx",
        type: "POST",
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        data: data
      });
      req.done(function (data) {
        dynReady(data, id);
      });
      req.fail(function (jqXHR, textStatus, error) {
        applyFailHtml();
      });
      // Generic click-away handler to close active popup
      $('html').click(function () {
        if (activeModalCloser != null) {
          activeModalCloser();
          activeModalCloser = null;
        }
      });
    }
    // Adapt font size to window width
    $(window).resize(onResize);
    onResize();
    // If page has initial content, trigger dyn-content-loaded activities right now
    if (hasContent) dynReady(null, -1);
  });

  // Infuses additional parameters to be submitted in search requests.
  function infuseSearchParams(data) {
    if (!startsWith(data.rel, "search/")) return;
    var params = searchParamsProvider();
    for (var fld in params) data[fld] = params[fld];
  }

  // Measure m width against viewport; adapt font size
  function onResize() {
    var ww = window.innerWidth;
    var w10em = $("#emMeasure")[0].clientWidth;

    var frac = ww / w10em;
    var ptStyle;
    if (frac < 5.4) ptStyle = "pt7";
    else if (frac < 6.0) ptStyle = "pt8";
    else if (frac < 6.6) ptStyle = "pt9";
    else if (frac < 7.2) ptStyle = "pt10";
    else if (frac < 7.8) ptStyle = "pt11";
    else if (frac < 8.4) ptStyle = "pt12";
    else if (frac < 9) ptStyle = "pt13";
    else if (frac > 12) ptStyle = "pt16";
    else ptStyle = "pt14";
    var theBody = $("#theBody");
    if (!theBody.hasClass(ptStyle)) {
      theBody.removeClass("pt7");
      theBody.removeClass("pt8");
      theBody.removeClass("pt9");
      theBody.removeClass("pt10");
      theBody.removeClass("pt11");
      theBody.removeClass("pt12");
      theBody.removeClass("pt13");
      theBody.removeClass("pt14");
      theBody.removeClass("pt16");
      theBody.addClass(ptStyle);
    }
  }

  // Navigate within single-page app (invoked from link click handler)
  function dynNavigate() {
    // Make sense of location
    parseLocation();
    // Clear whatever's currently shown
    //$("#dynPage").html("");
    $("#dynPage").addClass("fading");
    // Update menu to show where I am (will soon end up being)
    updateMenuState();
    // Request dynamic page - async
    ++reqId;
    var id = reqId;
    var data = { action: "dynpage", lang: lang, rel: rel };
    // Infuse extra search parameters
    infuseSearchParams(data);
    // Submit request
    var req = $.ajax({
      url: "/Handler.ashx",
      type: "POST",
      contentType: "application/x-www-form-urlencoded; charset=UTF-8",
      data: data
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

  // Apply dynamic content: HTML body, title, description, keywords; possible other data
  function applyDynContent(data) {
    $(document).attr("title", data.title);
    $("meta[name = 'keywords']").attr("content", data.keywords);
    $("meta[name = 'description']").attr("content", data.description);
    $("#dynPage").html(data.html);
    $("#dynPage").removeClass("fading");
    // Run this page's script initializer, if any
    for (var key in initScripts) {
      if (startsWith(rel, key)) initScripts[key](data);
      // Hack: call search initializer for ""
      if (rel == "" && key == "search") initScripts[key](data);
    }
    // Scroll to top
    $(window).scrollTop(0);
  }

  function navReady(data, id) {
    // An obsolete request completing too late?
    if (id != reqId) return;

    // Show dynamic content, title etc.
    applyDynContent(data);
  }

  // Dynamic data received after initial page load (not within single-page navigation)
  function dynReady(data, id) {
    // An obsolete request completing too late?
    if (id != -1 && id != reqId) return;

    // Show dynamic content, title etc.
    // Data is null if we're call directly from page load (content already present)
    if (data != null) applyDynContent(data);

    // Set up single-page navigation
    $(document).on('click', 'a.ajax', function () {
      // Navigation closes any active modal popup
      if (activeModalCloser != null) {
        activeModalCloser();
        activeModalCloser = null;
      }
      // Trick: If we're on search page but menu is shown, link just changes display; no navigation
      if ((rel == "" || startsWith(rel, "search")) && $(this).attr("id") == "topMenuSearch") {
        $("#hdrSearch").addClass("on");
        $("#hdrMenu").removeClass("on");
        $("#subHeader").removeClass("visible");
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
    $("#thePage").css("visibility", "visible");
    // Events - toggle from lookup input to menu
    $("#toMenu").click(function () {
      $("#hdrSearch").removeClass("on");
      $("#hdrMenu").addClass("on");
      //$("#subHeader").addClass("visible");
    });
  }

  // Updates top navigation menu to reflect where we are
  function updateMenuState() {
    $(".topMenu").removeClass("on");
    $(".subMenu").removeClass("visible");
    if (rel == "" || startsWith(rel, "search")) {
      $("#hdrMenu").removeClass("on");
      $("#subHeader").removeClass("visible");
      $("#dynPage").addClass("nosubmenu");
      $("#headermask").addClass("nosubmenu");
      $("#hdrSearch").addClass("on");
    }
    else {
      $("#hdrSearch").removeClass("on");
      $("#hdrMenu").addClass("on");
      $("#subHeader").addClass("visible");
      $("#dynPage").removeClass("nosubmenu");
      $("#headermask").removeClass("nosubmenu");
      if (startsWith(rel, "edit")) {
        $("#topMenuEdit").addClass("on");
        $("#subMenuEdit").addClass("visible");
      }
      else if (startsWith(rel, "read")) {
        $("#topMenuRead").addClass("on");
        $("#subMenuRead").addClass("visible");
      }
      else if (startsWith(rel, "download")) {
        $("#topMenuDownload").addClass("on");
        $("#subMenuDownload").addClass("visible");
      }
    }
    $(".subMenu span").removeClass("on");
    if (startsWith(rel, "edit/new")) $("#smEditNew").addClass("on");
    else if (startsWith(rel, "edit/history")) $("#smEditHistory").addClass("on");
    else if (startsWith(rel, "edit/existing")) $("#smEditExisting").addClass("on");
    else if (startsWith(rel, "read/about")) $("#smReadAbout").addClass("on");
    else if (startsWith(rel, "read/articles")) $("#smReadArticles").addClass("on");
    else if (startsWith(rel, "read/etc")) $("#smReadEtc").addClass("on");
    // Language selector
    $("#langSelHu").attr("href", "/hu/" + rel);
    $("#langSelEn").attr("href", "/en/" + rel);
    $(".langSel").removeClass("on");
    if (lang == "en") $("#langSelEn").addClass("on");
    else if (lang == "hu") $("#langSelHu").addClass("on");
  }

  // Closes a standard modal dialog (shown by us).
  function doCloseModal(id) {
    $("#" + id).remove();
    zdPage.modalHidden();
  }

  return {
    // Called by page-specific controller scripts to register themselves in single-page app, when page is navigated to.
    registerInitScript: function(pageRel, init) {
      initScripts[pageRel] = init;
    },

    globalInit: function(init) {
      globalInitScripts.push(init);
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

    // Called by lookup.js's global initializer to name search params provider function.
    setSearchParamsProvider: function(providerFun) {
      searchParamsProvider = providerFun;
    },

    // Gets the current selection's bounding element (start), or null if page has no selection.
    getSelBoundElm: function () {
      var range, sel, container;
      if (document.selection) {
        range = document.selection.createRange();
        range.collapse(true);
        if (range.toString() == "") return null;
        return range.parentElement();
      } else {
        sel = window.getSelection();
        if (sel.toString() == "") return null;
        if (sel.getRangeAt) {
          if (sel.rangeCount > 0) {
            range = sel.getRangeAt(0);
          }
        } else {
          // Old WebKit
          range = document.createRange();
          range.setStart(sel.anchorNode, sel.anchorOffset);
          range.setEnd(sel.focusNode, sel.focusOffset);

          // Handle the case when the selection was selected backwards (from the end to the start in the document)
          if (range.collapsed !== sel.isCollapsed) {
            range.setStart(sel.focusNode, sel.focusOffset);
            range.setEnd(sel.anchorNode, sel.anchorOffset);
          }
        }
        if (range) {
          container = range["startContainer"];
          // Check if the container is a text node and return its parent if so
          return container.nodeType === 3 ? container.parentNode : container;
        }
      }
    },

    // Called by any code showing a modal popup. Closes any active popup, and remembers close function.
    modalShown: function (closeFun) {
      if (activeModalCloser == closeFun) return;
      if (activeModalCloser != null) activeModalCloser();
      activeModalCloser = closeFun;
    },

    // Called by code when it closes modal of its own accord.
    modalHidden: function() {
      activeModalCloser = null;
    },

    // Shows a standard modal dialog with the provided content and callbacks.
    showModal: function (params) {
      // Close any other popup
      if (activeModalCloser != null) activeModalCloser();
      activeModalCloser = null;
      // Build popup's HTML
      var html = modalPopupTemplate;
      html = html.replace("{{id}}", params.id);
      html = html.replace("{{title}}", escapeHTML(params.title));
      html = html.replace("{{body}}", params.body);
      html = html.replace("{{OK}}", uiStrings["dialog-ok"]);
      html = html.replace("{{Cancel}}", uiStrings["dialog-cancel"]);
      $("#dynPage").append(html);
      // Wire up events
      activeModalCloser = function () { doCloseModal(params.id); };
      $(".modalPopupInner2").click(function (evt) { evt.stopPropagation(); });
      $(".modalPopupClose").click(function () { doCloseModal(params.id); });
      $(".modalPopupButtonCancel").click(function () { doCloseModal(params.id); });
      $(".modalPopupButtonOK").click(function () {
        if (params.confirmed()) doCloseModal(params.id);
      });
      // Focus requested field
      if (params.toFocus) $(params.toFocus).focus();
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

/// <reference path="x-jquery-2.1.4.min.js" />
/// <reference path="page.js" />

var zdNewEntry = (function () {
  "use strict";

  var server;

  $(document).ready(function () {
    zdPage.registerInitScript("edit/new", init);
  });

  function init() {
    server = zdNewEntryServer;

    $("#newEntrySimp").bind("compositionstart", onSimpCompStart);
    $("#newEntrySimp").bind("compositionend", onSimpCompEnd);
    $("#newEntrySimp").bind("input", onSimpChanged);
    $("#newEntrySimp").keyup(onSimpKeyUp);
    $("#acceptSimp").click(onSimpAccept);
    $("#editSimp").click(onSimpEdit);
    $("#acceptTrad").click(onTradAccept);
    $("#editTrad").click(onTradEdit);
    $("#acceptPinyin").click(onPinyinAccept);
    $("#editPinyin").click(onPinyinEdit);
    $("#acceptTrg").click(onTrgAccept);
    $("#editTrg").click(onTrgEdit);
    $("#newEntrySubmit").click(onSubmit);

    $("#newEntrySimp").prop("readonly", false);
    $("#newEntrySimp").focus();
  }

  function setActive(block) {
    $(".formBlock").removeClass("active");
    $(".formBlock").removeClass("ready");
    $(".formBlock").removeClass("future");
    $("#blockRefs").addClass("hidden");
    $("#blockReview").addClass("hidden");

    $("#newEntrySimp").prop("readonly", true);
    $("#newEntryTrg").prop("readonly", true);
    $("#newEntryNote").prop("readonly", true);
    $(".formErrors").removeClass("visible");
    $(".formNote").removeClass("hidden");
    if (block == "simp") {
      $("#newEntrySimp").prop("readonly", false);
      $("#newEntrySimp").focus();
      $("#blockSimp").addClass("active");
      $("#blockTrad").addClass("future");
      $("#blockPinyin").addClass("future");
      $("#blockTrg").addClass("future");
      $("#blockRefs").addClass("future");
      $("#blockReview").addClass("future");
      $("#editTrad").removeClass("hidden");
      $("#editPinyin").removeClass("hidden");
    }
    else if (block == "trad") {
      $("#blockSimp").addClass("ready");
      $("#blockTrad").addClass("active");
      $("#blockPinyin").addClass("future");
      $("#blockTrg").addClass("future");
      $("#blockRefs").addClass("future");
      $("#blockReview").addClass("future");
      $("#editPinyin").removeClass("hidden");
    }
    else if (block == "pinyin") {
      $("#blockSimp").addClass("ready");
      $("#blockTrad").addClass("ready");
      $("#blockPinyin").addClass("active");
      $("#blockTrg").addClass("future");
      $("#blockRefs").addClass("future");
      $("#blockReview").addClass("future");
    }
    else if (block == "trg") {
      $("#newEntryTrg").prop("readonly", false);
      $("#newEntryTrg").focus();
      $("#blockSimp").addClass("ready");
      $("#blockTrad").addClass("ready");
      $("#blockPinyin").addClass("ready");
      $("#blockTrg").addClass("active");
      $("#blockRefs").addClass("active");
      $("#blockReview").addClass("future");
    }
    else if (block == "review") {
      $("#blockReview").removeClass("hidden");
      $("#newEntryNote").prop("readonly", false);
      $("#newEntryNote").focus();
      $("#blockSimp").addClass("ready");
      $("#blockTrad").addClass("ready");
      $("#blockPinyin").addClass("ready");
      $("#blockTrg").addClass("ready");
      $("#blockReview").addClass("active");
    }
  }

  // Event handler: submit button clicked.
  function onSubmit(evt) {
    if ($("#newEntrySubmit").hasClass("disabled")) return;
    // Check if user has entered a substantial note
    if ($("#newEntryNote").val().length < 6) {
      $(".formErrors").removeClass("visible");
      $("#errorsReview").addClass("visible");
      $("#newEntryNote").focus();
    }
    else {
      $("#errorsReview").removeClass("visible");
      $("#newEntrySubmit").addClass("disabled");
      server.submit($("#newEntrySimp").val(), getTrad(), getPinyin(),
        $("#newEntryTrg").val(), $("#newEntryNote").val(), onSubmitReady);
    }
  }

  // API callback: submit returned, with either success or failure.
  function onSubmitReady(success) {
    $("#newEntrySubmit").removeClass("disabled");
    if (success)
      zdCommon.showAlert("A szócikket sikeresen eltároltuk.", null, false);
    else
      zdCommon.showAlert("A szócikket nem sikerült eltárolni :(", null, true);
  }

  // Event handler: user clicked pencil to continue editing target
  function onTrgEdit(evt) {
    setActive("trg");
    $("#blockRefs").removeClass("hidden");
  }

  // Event handler: user clicked green button to accept translation
  function onTrgAccept(evt) {
    if ($("#acceptTrg").hasClass("disabled")) return;
    server.verifyTrg($("#newEntrySimp").val(), getTrad(), getPinyin(), $("#newEntryTrg").val(), onTrgVerified);
    $("#acceptTrg").addClass("disabled");
  }

  // API callback: translation verified; we might have a preview
  function onTrgVerified(res) {
    $("#acceptTrg").removeClass("disabled");
    if (!res.passed) {
      $(".formErrors").removeClass("visible");
      $("#errorsTrg").addClass("visible");
      $("#noteTrg").addClass("hidden");
      $("#errorListTrg").empty();
      for (var i = 0; i < res.errors.length; ++i) {
        var liErr = $('<li/>');
        liErr.text(res.errors[i]);
        $("#errorListTrg").append(liErr);
      }
      $("#newEntryTrg").focus();
    }
    else {
      $("#errorsTrg").removeClass("visible");
      $("#noteTrg").removeClass("hidden");
      $("#newEntryRender").html(res.preview);
      setActive("review");
    }
  }

  // Even handler: user accepts content of pinyin field
  function onPinyinAccept(evt) {
    if ($("#acceptPinyin").hasClass("disabled")) return;
    server.verifyHead($("#newEntrySimp").val(), getTrad(), getPinyin(), onHeadVerified);
    $("#acceptPinyin").addClass("disabled");
  }

  // API callback: entire headword has been verified (is it a duplicate?).
  function onHeadVerified(res) {
    $("#acceptPinyin").removeClass("disabled");
    if (!res.passed) {
      $(".formErrors").removeClass("visible");
      $("#errorsPinyin").addClass("visible");
      $("#notePinyin").addClass("hidden");
      $("#blockRefs").addClass("hidden");
    }
    else {
      $("#errorsPinyin").removeClass("visible");
      $("#notePinyin").removeClass("hidden");
      setActive("trg");
      $("#blockRefs").removeClass("hidden");
      $("#newEntryRefEntries").html(res.ref_entries_html);
    }
  }

  // Event handler: user clicks pencil to return to editing pinyin field.
  function onPinyinEdit(evt) {
    setActive("pinyin");
  }

  // Event handler: user clicks pencil to return to editing traditional field.
  function onTradEdit(evt) {
    setActive("trad");
  }

  // Event handler: traditional field is accepted by user.
  function onTradAccept(evt) {
    if ($("#acceptTrad").hasClass("disabled")) return;
    if (isPinyinUnambiguous()) {
      $("#editPinyin").addClass("hidden");
      // Instead of activating target, let's get headword verified
      onPinyinAccept();
    }
    else {
      $("#editPinyin").removeClass("hidden");
      setActive("pinyin");
    }
  }

  // Simplified field is composing (IME). Blocks API calls while field has shadow text.
  var simpComposing = false;

  // Event handler: IME composition starts in simplified field.
  function onSimpCompStart(evt) {
    simpComposing = true;
  }

  // Event handler: IME composition ends in simplified field.
  function onSimpCompEnd(evt) {
    simpComposing = false;
  }

  function onSimpKeyUp(evt) {
    if (evt.which == 13) {
      evt.preventDefault();
      onSimpAccept();
      return false;
    }
  }

  // Handles change of simplified field. Invokes server to retrieve data for subsequent HW fields.
  function onSimpChanged(evt) {
    if (simpComposing) return;
    server.processSimp($("#newEntrySimp").val(), onSimpProcessed);
  }

  // Callback when API finished processing current content of simplified field.
  function onSimpProcessed(trad, pinyin, known_hw) {
    $("#newEntryTradCtrl").empty();
    for (var  i = 0; i < trad.length; ++i) {
      var tpos = $('<div class="newEntryTradPos"/>');
      for (var j = 0; j < trad[i].length; ++j) {
        var tspan = $('<span />');
        if (j != 0) tspan.addClass("tradAlt");
        tspan.text(trad[i][j]);
        tpos.append(tspan);
      }
      $("#newEntryTradCtrl").append(tpos);
    }
    if (trad.length == 0) $("#newEntryTradCtrl").append('\xA0');
    if (known_hw) $(".newEntryKnown").addClass("visible");
    else $(".newEntryKnown").removeClass("visible");
    $(".tradAlt").unbind("click", onTradAltClicked);
    $(".tradAlt").click(onTradAltClicked);

    updatePinyin(pinyin);
  }

  // Handles simplified's "accept" event. Invokes server to check input.
  function onSimpAccept(evt) {
    if ($("#acceptSimp").hasClass("disabled")) return;
    server.verifySimp($("#newEntrySimp").val(), onSimpVerified);
    $("#acceptSimp").addClass("disabled");
  }

  // Callback when API finished checking simplified.
  // We show error notice, or move on to next field.
  function onSimpVerified(res) {
    $("#acceptSimp").removeClass("disabled");
    // Simplified is not OK - show error
    if (!res.passed) {
      $(".formErrors").removeClass("visible");
      $("#errorsSimp").addClass("visible");
      $("#noteSimp").addClass("hidden");
      $("#errorListSimp").empty();
      for (var i = 0; i < res.errors.length; ++i) {
        var liErr = $('<li/>');
        liErr.text(res.errors[i]);
        $("#errorListSimp").append(liErr);
      }
      $("#newEntrySimp").focus();
    }
    // We're good to go
    else {
      $("#errorsSimp").removeClass("visible");
      $("#noteSimp").removeClass("hidden");
      // If traditional, or even pinyin, are unambiguous: skip ahead one or two steps
      if (isTradUnambiguous()) {
        if (isPinyinUnambiguous()) {
          $("#editTrad").addClass("hidden");
          $("#editPinyin").addClass("hidden");
          // Instead of activating target, let's get headword verified
          onPinyinAccept();
        }
        else {
          $("#editPinyin").removeClass("hidden");
          $("#editTrad").addClass("hidden");
          setActive("pinyin");
        }
      }
      else {
        $("#editPinyin").removeClass("hidden");
        $("#editTrad").removeClass("hidden");
        setActive("trad");
      }
    }
  }

  // Checks if all traditional symbols are unambiguous (no user input needed).
  function isTradUnambiguous() {
    var unambiguous = true;
    var tctrl = $("#newEntryTradCtrl");
    tctrl.children().each(function () {
      if ($(this).children().length > 1) unambiguous = false;
    });
    return unambiguous;
  }

  // Checks if all pinyin syllables are unambiguous (no user input needed).
  function isPinyinUnambiguous() {
    var unambiguous = true;
    var tctrl = $("#newEntryPinyinCtrl");
    tctrl.children().each(function () {
      if ($(this).children().length > 1) unambiguous = false;
    });
    return unambiguous;
  }

  // Even handler: user clicked pencil to edit simplified field.
  function onSimpEdit(evt) {
    setActive("simp");
  }

  // Get user's choice of traditional HW.
  function getTrad() {
    var res = "";
    var tctrl = $("#newEntryTradCtrl");
    tctrl.children().each(function() {
      res += $(this).children().first().text();
    });
    return res;
  }

  // Get user's choice of pinyin in HW.
  function getPinyin() {
    var res = "";
    var tctrl = $("#newEntryPinyinCtrl");
    var first = true;
    tctrl.children().each(function () {
      if (!first) res += " ";
      first = false;
      res += $(this).children().first().text();
    });
    return res;
  }

  // Even handler: user clicked on a non-first-row traditional character to select it.
  function onTradAltClicked(evt) {
    if (!$("#blockTrad").hasClass("active")) return;

    var parent = $(this).parent();
    var tchars = [];
    tchars.push($(this).text());
    parent.children().each(function() {
      if ($(this).text() != tchars[0])
        tchars.push($(this).text());
    });
    parent.empty();
    for (var i = 0; i < tchars.length; ++i) {
      var tspan = $('<span />');
      if (i != 0) tspan.addClass("tradAlt");
      tspan.text(tchars[i]);
      parent.append(tspan);
    }
    $(".tradAlt").unbind("click", onTradAltClicked);
    $(".tradAlt").click(onTradAltClicked);

    server.processSimpTrad($("#newEntrySimp").val(), getTrad(), onSimpTradProcessed);
  }

  // Update data shown in pinyin field.
  function updatePinyin(pinyin) {
    $("#newEntryPinyinCtrl").empty();
    for (var i = 0; i != pinyin.length; ++i) {
      var ppos = $('<div class="newEntryPinyinPos"/>');
      for (var j = 0; j != pinyin[i].length; ++j) {
        var pspan = $('<span/>');
        if (j != 0) pspan.addClass("pyAlt");
        pspan.text(pinyin[i][j]);
        ppos.append(pspan);
      }
      $("#newEntryPinyinCtrl").append(ppos);
    }
    if (pinyin.length == 0) $("#newEntryPinyinCtrl").append('\xA0');
    $(".pyAlt").unbind("click", onPyAltClicked);
    $(".pyAlt").click(onPyAltClicked);
  }

  // Event handler: user clicked a pinyin alternative to select it.
  function onPyAltClicked(evt) {
    if (!$("#blockPinyin").hasClass("active")) return;

    var parent = $(this).parent();
    var tsylls = [];
    tsylls.push($(this).text());
    parent.children().each(function() {
      if ($(this).text() != tsylls[0])
        tsylls.push($(this).text());
    });
    parent.empty();
    for (var i = 0; i < tsylls.length; ++i) {
      var tspan = $('<span />');
      if (i != 0) tspan.addClass("pyAlt");
      tspan.text(tsylls[i]);
      parent.append(tspan);
    }
    $(".pyAlt").unbind("click", onPyAltClicked);
    $(".pyAlt").click(onPyAltClicked);
  }

  // API callback: server finished processing simplified+traditional.
  function onSimpTradProcessed(pinyin, known_hw) {
    updatePinyin(pinyin);
    if (known_hw) $(".newEntryKnown").addClass("visible");
    else $(".newEntryKnown").removeClass("visible");
  }
})();

var zdNewEntryServer = (function() {
  return {
    processSimp: function(simp, ready) {
      var req = $.ajax({
        url: "/Handler.ashx",
        type: "POST",
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        data: {action: "newentry_processsimp", simp: simp}
      });
      req.done(function(data) {
        ready(data.trad, data.pinyin, data.is_known_headword);
      });
    },

    verifySimp: function(simp, ready) {
      var req = $.ajax({
        url: "/Handler.ashx",
        type: "POST",
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        data: { action: "newentry_verifysimp", simp: simp }
      });
      req.done(function (data) {
        var res = {
          passed: data.passed,
          errors: data.errors
        };
        ready(res);
      });
    },

    verifyHead: function(simp, trad, pinyin, ready) {
      var req = $.ajax({
        url: "/Handler.ashx",
        type: "POST",
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        data: { action: "newentry_verifyhead", simp: simp, trad: trad, pinyin: pinyin }
      });
      req.done(function (data) {
        var res = {
          passed: data.passed,
          ref_entries_html: data.ref_entries_html,
        };
        ready(res);
      });
    },

    verifyTrg: function(simp, trad, pinyin, trg, ready) {
      var req = $.ajax({
        url: "/Handler.ashx",
        type: "POST",
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        data: { action: "newentry_verifyfull", simp: simp, trad: trad, pinyin: pinyin, trg: trg }
      });
      req.done(function (data) {
        var res = {
          passed: data.passed,
          errors: data.errors,
          preview: data.preview_html
        };
        ready(res);
      });
    },

    processSimpTrad: function(simp, trad, ready) {
      var req = $.ajax({
        url: "/Handler.ashx",
        type: "POST",
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        data: { action: "newentry_processsimptrad", simp: simp, trad: trad }
      });
      req.done(function (data) {
        ready(data.pinyin, data.is_known_headword);
      });
    },

    submit: function (simp, trad, pinyin, trg, note, ready) {
      var req = $.ajax({
        url: "/Handler.ashx",
        type: "POST",
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        data: { action: "newentry_submit", simp: simp, trad: trad, pinyin: pinyin, trg: trg, note: note }
      });
      req.done(function (data) {
        ready(data.success);
      });
      req.fail(function (jqXHR, textStatus, error) {
        ready(false);
      });
    }
  }
})();

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

/// <reference path="x-jquery-2.1.4.min.js" />
/// <reference path="x-jquery.color-2.1.2.min.js" />
/// <reference path="x-jquery.tooltipster.min.js" />
/// <reference path="strings-hu.js" />
/// <reference path="page.js" />
/// <reference path="strokeanim.js" />

var zdLookup = (function () {
  var clrSel = "#ffe4cc"; // Same as background-color of .optionItem.selected in CSS
  var clrEmph = "#ffc898"; // Same as border-bottom of .optionHead in CSS

  var optScript = "both";
  var optTones = "pleco";

  var optionsTemplate =
    '<div id="optionsTail">&nbsp;</div>\n' +
    '<div id="optionsHeader">\n' +
    '  <span id="optionsTitle">{{options-title}}</span>\n' +
    '  <span id="optionsClose">X</span>\n' +
    '</div>\n' +
    '<div id="searchOptions">\n' +
    '  <div class="optionHead">{{options-script}}</div>\n' +
    '  <div class="optionBody">\n' +
    '    <div class="optionItem" id="optScriptSimplified">\n' +
    '      <span class="optionLabel">{{options-simplified}}</span>\n' +
    '      <span class="optionExampleHan">汉语</span>\n' +
    '    </div>\n' +
    '    <div class="optionItem" id="optScriptTraditional">\n' +
    '      <span class="optionLabel">{{options-traditional}}</span>\n' +
    '      <span class="optionExampleHan">漢語</span>\n' +
    '    </div>\n' +
    '    <div class="optionItem" id="optScriptBoth">\n' +
    '      <span class="optionLabel">{{options-bothscripts}}</span>\n' +
    '      <span class="optionExampleHan">汉语 • 漢語</span>\n' +
    '    </div>\n' +
    '  </div>\n' +
    '  <div class="optionHead">{{options-tonecolors}}</div>\n' +
    '  <div class="optionBody">\n' +
    '    <div class="optionItem toneColorsNone" id="optToneColorsNone">\n' +
    '      <span class="optionLabel">{{options-nocolors}}</span>\n' +
    '      <span class="optionExampleHan"><span class="tone1">天</span><span class="tone2">人</span><span class="tone3">很</span><span class="tone4">大</span>了</span>\n' +
    '    </div>\n' +
    '    <div class="optionItem toneColorsPleco" id="optToneColorsPleco">\n' +
    '      <span class="optionLabel">{{options-pleco}}</span>\n' +
    '      <span class="optionExampleHan"><span class="tone1">天</span><span class="tone2">人</span><span class="tone3">很</span><span class="tone4">大</span>了</span>\n' +
    '    </div>\n' +
    '    <div class="optionItem toneColorsDummitt" id="optToneColorsDummitt">\n' +
    '      <span class="optionLabel">{{options-dummitt}}</span>\n' +
    '      <span class="optionExampleHan"><span class="tone1">天</span><span class="tone2">人</span><span class="tone3">很</span><span class="tone4">大</span>了</span>\n' +
    '    </div>\n' +
    '  </div>\n' +
    '</div>\n';


  zdPage.globalInit(globalInit);

  $(document).ready(function () {
    zdPage.registerInitScript("search", resultEventWireup);
  });

  function globalInit() {
    // Register search params provider
    zdPage.setSearchParamsProvider(getSearchParams);

    // If session storage says we've already loaded strokes, append script right now
    // This will happen from browser cache, i.e., page load doesn't suffer
    if (sessionStorage.getItem("strokesLoaded")) {
      var elmStrokes = document.createElement('script');
      document.getElementsByTagName("head")[0].appendChild(elmStrokes);
      elmStrokes.setAttribute("type", "text/javascript");
      elmStrokes.setAttribute("src", getStrokeDataUrl());
    }
    // Add tooltips
    $("#btn-write").tooltipster({
      content: $("<span>" + uiStrings["tooltip-btn-brush"] + "</span>")
    });
    $("#btn-settings").tooltipster({
      content: $("<span>" + uiStrings["tooltip-btn-settings"] + "</span>")
    });
    $("#btn-search").tooltipster({
      content: $("<span>" + uiStrings["tooltip-btn-search"] + "</span>")
    });

    // TO-DO
    //initStrokes();

    // Debug: to work on strokes input
    //showStrokeInput();

    $("#btn-clear").click(clearSearch);
    $("#btn-settings").click(showSettings);

    // TO-DO
    //$("#strokeClear").click(clearCanvas);
    //$("#strokeUndo").click(undoStroke);

    $("#btn-search").click(submitSearch);
    $("#txtSearch").keyup(function (e) {
      if (e.keyCode == 13) {
        submitSearch();
        return false;
      }
    });
    $("#txtSearch").focus(txtSearchFocus);
    $("#txtSearch").change(function () {
      appendNotOverwrite = true;
    });

    // Debug: to work on opening screen
    //$("#resultsHolder").css("display", "none");
    //$("#welcomeScreen").css("display", "block");
  }

  function resultEventWireup(data) {
    $("#results").append("<div id='soaBox' class='soaBoxLeft'></div>");
    zdStrokeAnim.init();
    $(".hanim").click(showStrokeAnim);
    $("#soaClose").click(hideStrokeAnim);
    $("#soaBox").click(function (e) { e.stopPropagation(); });
    $('#txtSearch').val(data.query);
    $('#txtSearch').focus();
  }

  // Show the search settings popup (generate from template; event wireup; position).
  function showSettings(event) {
    // Render HTML from template
    var html = optionsTemplate;
    html = html.replace("{{options-title}}", uiStrings["options-title"]);
    html = html.replace("{{options-script}}", uiStrings["options-script"]);
    html = html.replace("{{options-simplified}}", uiStrings["options-simplified"]);
    html = html.replace("{{options-traditional}}", uiStrings["options-traditional"]);
    html = html.replace("{{options-bothscripts}}", uiStrings["options-bothscripts"]);
    html = html.replace("{{options-tonecolors}}", uiStrings["options-tonecolors"]);
    html = html.replace("{{options-nocolors}}", uiStrings["options-nocolors"]);
    html = html.replace("{{options-pleco}}", uiStrings["options-pleco"]);
    html = html.replace("{{options-dummitt}}", uiStrings["options-dummitt"]);
    $("#searchOptionsBox").html(html);
    // Housekeeping; show search box
    zdPage.modalShown(hideSettings);
    var elmPopup = $("#searchOptionsBox");
    elmPopup.addClass("visible");
    $("#optionsClose").click(hideSettings);
    elmPopup.click(function (evt) { evt.stopPropagation(); });
    // Disable tooltip while settings are on screen
    $("#btn-settings").tooltipster('disable');
    // Stop event propagation, or we'll be closed right away
    event.stopPropagation();
    // Position search box to settings button
    var elmStgs = $("#btn-settings");
    var rectStgs = [elmStgs.offset().left, elmStgs.offset().top, elmStgs.width(), elmStgs.height()];
    var elmTail = $("#optionsTail");
    var rectTail = [elmTail.offset().left, elmTail.offset().top, elmTail.width(), elmTail.height()];
    var xMidStgs = rectStgs[0] + rectStgs[2] / 2.2;
    var xMidTail = rectTail[0] + rectTail[2] / 2;
    elmPopup.offset({ left: elmPopup.offset().left + xMidStgs - xMidTail, top: elmPopup.offset().top });
    // Load persisted/default values; update UI
    loadOptions();
    // Events
    optionsEventWireup();
  }

  // Hides the search settings popup
  function hideSettings() {
    $("#searchOptionsBox").removeClass("visible");
    zdPage.modalHidden();
    // Re-enable tooltip
    $("#btn-settings").tooltipster('enable');
  }

  // Load options (or inits to defaults); updates UI.
  function loadOptions() {
    // Check cookie for script
    var ckScript = localStorage.getItem("uiscript");
    if (ckScript !== null) optScript = ckScript;
    if (optScript === "simp") $("#optScriptSimplified").addClass("selected");
    else if (optScript === "trad") $("#optScriptTraditional").addClass("selected");
    else if (optScript === "both") $("#optScriptBoth").addClass("selected");
    // Check cookie for tone colors
    var ckTones = localStorage.getItem("uitones");
    if (ckTones !== null) optTones = ckTones;
    if (optTones === "none") $("#optToneColorsNone").addClass("selected");
    else if (optTones === "pleco") $("#optToneColorsPleco").addClass("selected");
    else if (optTones === "dummitt") $("#optToneColorsDummitt").addClass("selected");
  }

  // Interactions of options UI.
  function optionsEventWireup() {
    // Event handlers for mouse (desktop)
    var handlersMouse = {
      mousedown: function (e) {
        $(this).animate({ backgroundColor: clrEmph }, 200);
      },
      click: function (e) {
        $(this).animate({ backgroundColor: clrSel }, 400);
        selectOption(this.id);
      },
      mouseenter: function (e) {
        $(this).animate({ backgroundColor: clrSel }, 400);
      },
      mouseleave: function (e) {
        var clr = $(this).hasClass('selected') ? clrSel : "transparent";
        $(this).animate({ backgroundColor: clr }, 400);
      }
    };
    // Event handlers for mobile (touch)
    var handlersTouch = {
      click: function (e) {
        $(this).animate({ backgroundColor: clrSel }, 400);
        selectOption(this.id);
      }
    };
    var handlers = /* isMobile ? handlersTouch : */ handlersMouse;
    // Script option set
    $("#optScriptSimplified").on(handlers);
    $("#optScriptTraditional").on(handlers);
    $("#optScriptBoth").on(handlers);

    // Tone colors option set
    $("#optToneColorsNone").on(handlers);
    $("#optToneColorsPleco").on(handlers);
    $("#optToneColorsDummitt").on(handlers);
  }

  // Handler: an option is selected (clicked) in the options UI.
  function selectOption(id) {
    function unselectOption(optId) {
      if ($(optId).hasClass("selected")) {
        $(optId).removeClass("selected");
        $(optId).animate({ backgroundColor: "transparent" }, 400);
      }
    }
    // Script option set
    if (id.indexOf("optScript") === 0) {
      unselectOption("#optScriptSimplified");
      unselectOption("#optScriptTraditional");
      unselectOption("#optScriptBoth");
    }
      // Tone colors option set
    else if (id.indexOf("optToneColors") === 0) {
      unselectOption("#optToneColorsNone");
      unselectOption("#optToneColorsPleco");
      unselectOption("#optToneColorsDummitt");
    }
    $("#" + id).addClass("selected");
    // Store: Script options
    if (id === "optScriptSimplified") localStorage.setItem("uiscript", "simp");
    else if (id === "optScriptTraditional") localStorage.setItem("uiscript", "trad");
    else if (id === "optScriptBoth") localStorage.setItem("uiscript", "both");
    // Store: Tone color options
    if (id === "optToneColorsNone") localStorage.setItem("uitones", "none");
    else if (id === "optToneColorsPleco") localStorage.setItem("uitones", "pleco");
    else if (id === "optToneColorsDummitt") localStorage.setItem("uitones", "dummitt");
    // Load options again: adds "selected" to element (redundantly), and updates UI.
    loadOptions();
  }

  // TO-DO
  //function getStrokeDataUrl() {
  //  // Figure out URL by stealing from "common.js" - which is always there. Right?!
  //  var elmCommonJS = document.getElementById("elmCommonJS");
  //  var urlCommonJS = elmCommonJS.getAttribute("src");
  //  var re = /common\.js/;
  //  var urlStrokes = urlCommonJS.replace(re, "chinesestrokes.js");
  //  return urlStrokes;
  //}

  // Shows the handwriting recognition pop-up.
  function showStrokeInput() {
    // TO-DO
    return;

    // Firsty first: load the stroke data if missing
    if (typeof strokesData === 'undefined') {
      // Add element, and also event handler for completion
      var elmStrokes = document.createElement('script');
      document.getElementsByTagName("head")[0].appendChild(elmStrokes);
      var funEnabler = function () {
        setRecogEnabled(true);
        sessionStorage.setItem("strokesLoaded", true);
      }
      elmStrokes.onload = function () { funEnabler(); };
      elmStrokes.onreadystatechange = function () {
        if (this.readyState == 'complete') funEnabler();
      }
      elmStrokes.setAttribute("type", "text/javascript");
      elmStrokes.setAttribute("src", getStrokeDataUrl());
      // Make input disabled
      setRecogEnabled(false);
    }
    // Position and show panel
    if (!zdPage.isMobile()) {
      var searchPanelOfs = $("#search-panel").offset();
      var searchPanelWidth = $("#search-panel").width();
      var searchPanelHeight = $("#search-panel").height();
      var strokeInputWidth = $("#stroke-input").outerWidth();
      //$("#stroke-input").position({left: 0, top: searchPanelOfs.top + searchPanelHeight + 390});
      $("#stroke-input").css("margin-top", searchPanelOfs.top - window.pageYOffset + searchPanelHeight);
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
    if (zdPage.isMobile()) hideShowHamburger(false);
  }

  // Hides the handwriting recognition popup
  function hideStrokeInput() {
    // TO-DO
    return;

    // Nothing to hide?
    if ($("#stroke-input").css("display") != "block") return;
    // Hide.
    $("#stroke-input").css("display", "none");
    $("#btn-write").attr("class", "");
  }

  // Clears the search field
  function clearSearch() {
    $("#txtSearch").val("");
    $("#txtSearch").focus();
  }

  // When the search input field receives focus
  function txtSearchFocus(event) {
    if (zdPage.isMobile()) return;
    $("#txtSearch").select();
  }

  // Returns object with search params.
  function getSearchParams() {
    return { searchScript: optScript, searchTones: optTones };
  }

  // Submits a dictionary search as simple GET URL
  function submitSearch() {
    'use strict';
    var queryStr = $('#txtSearch').val().replace(" ", "+");
    zdPage.submitSearch(queryStr);
  }

  // Dynamically position stroke order animation popup in Desktop
  function dynPosSOA(zis) {
    // First, decide if we're showing box to left or right of character
    var hanziOfs = zis.offset();
    var onRight = hanziOfs.left < $(document).width() / 2;
    var left = hanziOfs.left + zis.width() + 20;
    if (onRight) $("#soaBox").removeClass("soaBoxLeft");
    else {
      $("#soaBox").addClass("soaBoxLeft");
      left = hanziOfs.left - $("#soaBox").width() - 20;
    }
    // Decide about Y position. Box wants char to be at vertical middle
    // But is willing to move up/down to fit in content area
    var charY = hanziOfs.top + zis.height() / 2;
    var boxH = $("#soaBox").height();
    var top = charY - boxH / 2;
    // First, nudge up if we stretch beyond viewport bottom
    var wBottom = window.pageYOffset + window.innerHeight - 10;
    if (top + boxH > wBottom) top = wBottom - boxH;
    // Then, nudge down if we're over the ceiling
    //var wTop = $("#hdrSearch").position().top + $("#hdrSearch").height() + window.pageYOffset + 20;
    var wTop = $("#headermask").position().top + $("#headermask").height() + window.pageYOffset;
    if (top < wTop) top = wTop;
    // Position box, and tail
    $("#soaBox").offset({ left: left, top: top });
    $("#soaBoxTail").css("top", (charY - top - 10) + "px");
  }

  // Positions and shows the stroke animation pop-up for the clicked hanzi.
  function showStrokeAnim(event) {
    // We get click event when mouse button is released after selecting a single hanzi
    // Don't want to show pop-up in this edge case
    var sbe = zdPage.getSelBoundElm();
    if (sbe != null && sbe.textContent == $(this).text())
      return;
    // OK, so we're actually showing. Stop propagation so we don't get auto-hidden.
    event.stopPropagation();
    // If previous show's in progress, kill it
    // Also kill stroke input, in case it's shown
    zdStrokeAnim.kill();
    // Start the whole spiel
    $("#soaBox").css("display", "block");
    // We only position dynamically in desktop version; in mobile, it's fixed
    dynPosSOA($(this));
    // Render grid, issue AJAX query for animation data
    zdStrokeAnim.renderBG();
    zdStrokeAnim.startQuery($(this).text());
    // Notify page
    zdPage.modalShown(hideStrokeAnim);
  }

  // Closes the stroke animation pop-up (if shown).
  function hideStrokeAnim() {
    zdStrokeAnim.kill();
    $("#soaBox").css("display", "none");
    zdPage.modalHidden();
  }
})();



/// <reference path="x-jquery-2.1.4.min.js" />
/// <reference path="x-jquery.color-2.1.2.min.js" />
/// <reference path="x-jquery.tooltipster.min.js" />
/// <reference path="strings-hu.js" />
/// <reference path="page.js" />

var zdHistory = (function () {

  var addCommentTemplate =
    '<i class="fa fa-commenting-o" aria-hidden="true"></i>' +
    '<textarea id="txtHistComment" placeholder="{{hint}}"></textarea>';


  $(document).ready(function () {
    zdPage.registerInitScript("edit/history", init);
  });

  function init() {
    // Add tooltips to pliant per-entry commands
    $(".opHistComment").tooltipster({
      content: $("<span>" + uiStrings["tooltip-history-comment"] + "</span>"),
      position: 'left'
    });
    $(".opHistEdit").tooltipster({
      content: $("<span>" + uiStrings["tooltip-history-edit"] + "</span>"),
      position: 'left'
    });
    $(".opHistFlag").tooltipster({
      content: $("<span>" + uiStrings["tooltip-history-flag"] + "</span>"),
      position: 'left'
    });
    // Event handlers for per-entry commands
    $(".opHistComment").click(onComment);
  }

  function onComment(evt) {
    // Find entry ID in parent with historyItem class
    var elm = $(this);
    while (!elm.hasClass("historyItem")) elm = elm.parent();
    var entryId = elm.data("entryid");
    // Prepare modal window content
    var bodyHtml = addCommentTemplate;
    bodyHtml = bodyHtml.replace("{{hint}}", uiStrings["history-commententry-hint"]);
    var params = {
      id: "dlgHistComment",
      title: uiStrings["history-commententry-title"],
      body: bodyHtml,
      confirmed: function () { return onCommentConfirmed(entryId); },
      toFocus: "#txtHistComment"
    };
    // Show
    zdPage.showModal(params);
    evt.stopPropagation();
  }

  function onCommentConfirmed(entryId) {
    var cmt = $("#txtHistComment").val();
    if (cmt.length == 0) {
      return false;
    }
    var req = $.ajax({
      url: "/Handler.ashx",
      type: "POST",
      contentType: "application/x-www-form-urlencoded; charset=UTF-8",
      data: { action: "history_commententry", entry_id: entryId }
    });
    req.done(function (data) {
      zdPage.showAlert(uiStrings["history-commententry-successtitle"], uiStrings["history-commententry-successmessage"], false);
    });
    req.fail(function (jqXHR, textStatus, error) {
      zdPage.showAlert("Csak a baj", "Elmentés nó nó.", true);
    });
    return true;
  }

})();

/// <reference path="x-jquery-2.1.4.min.js" />
/// <reference path="page.js" />
 
var zdDiagnostics = (function () {
  "use strict";

  $(document).ready(function () {
    zdPage.registerInitScript("download", function () {
      $("#recreateDB").click(onRecreateDB);
      $("#indexHDD").click(onIndexHDD);
      $("#queryPage").click(onQueryPage);
      $("#alertFail").click(onAlertFail);
      $("#alertSucc").click(onAlertSucc);
    });
  });

  function onAlertSucc() {
    zdPage.showAlert("Herzlichen Glückwunsch!", "Das hat diesmal ganz gut geklappt.", false);
  }

  function onAlertFail() {
    zdPage.showAlert("You have been pwned.", "Got anything else to say?", true);
  }

  function onQueryPage() {
    var page = $("#txtPage").val();
    var url = "/ApiHandler.ashx";
    if (window.location.protocol == "file:")
      url = "http://localhost:8000/ApiHandler.ashx";
    var req = $.ajax({
      url: url,
      type: "POST",
      contentType: "application/x-www-form-urlencoded; charset=UTF-8",
      data: { action: "debug", what: "query_page", page: page }
    });
    req.done(function (data) {
      $("#progress").css("display", "block");
      $("#progressVal").html("");
      $("#progressVal").append("<b>" + data.summary + "</b><br/>");
      for (var i = 0; i != data.items.length; ++i) {
        var item = data.items[i];
        var itemHtml = "<b>" + item.code + "</b> &nbsp; " + item.headword + " &nbsp; " + item.when + " &nbsp; <i>" + item.note + "</i><br/>";
        $("#progressVal").append(itemHtml);
      }
    });
  }

  function onIndexHDD() {
    $("#progress").css("display", "block");
    var url = "/ApiHandler.ashx";
    if (window.location.protocol == "file:")
      url = "http://localhost:8000/ApiHandler.ashx";
    var req = $.ajax({
      url: url,
      type: "POST",
      contentType: "application/x-www-form-urlencoded; charset=UTF-8",
      data: { action: "debug", what: "index_hdd" }
    });
    setTimeout(getProgressIndexHDD, 500);
  }

  function getProgressIndexHDD() {
    var url = "/ApiHandler.ashx";
    if (window.location.protocol == "file:")
      url = "http://localhost:8000/ApiHandler.ashx";
    var req = $.ajax({
      url: url,
      type: "POST",
      contentType: "application/x-www-form-urlencoded; charset=UTF-8",
      data: { action: "debug", what: "progress_index_hdd" }
    });
    req.done(function (data) {
      $("#progressVal").text(data.progress);
      if (!data.done) setTimeout(getProgressIndexHDD, 500);
    });
  }

  function onRecreateDB() {
    //$("#progress").css("display", "block");
    var url = "/ApiHandler.ashx";
    if (window.location.protocol == "file:")
      url = "http://localhost:8000/ApiHandler.ashx";
    var req = $.ajax({
      url: url,
      type: "POST",
      contentType: "application/x-www-form-urlencoded; charset=UTF-8",
      data: { action: "debug", what: "recreate_db" }
    });
  }

})();