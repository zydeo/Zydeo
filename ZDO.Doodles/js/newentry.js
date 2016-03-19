var zdNewEntry = (function() {
  var template =
    '<div class="formBlock active" id="blockSimp">' +
    '  <div class="formBlockLabel">Egyszerűsített</div>' +
    '  <div class="formBlockFrame">' +
    '    <input id="newEntrySimp" maxlength="8" readonly/>' +
    '    <div class="newEntryKnown">&bull;</div>' +
    '    <div class="formButtonRight accept" id="acceptSimp">' +
    '      <img src="static/sign.svg" alt=""/>' +
    '    </div>' +
    '    <div class="formButtonRight edit" id="editSimp">' +
    '      <img src="static/draw.svg" alt=""/>' +
    '    </div>' +
    '    <div class="formNote" id="noteSimp">' +
    '      A folytatáshoz kattints a zöld <i>Jóváhagyás</i> gombra, vagy üss Entert.' +
    '    </div>' +
    '    <div class="formErrors" id="errorsSimp">' +
    '      Az automatikus ellenőrzés az alábbi problémákat találta.' +
    '      Kérlek, korrigáld ezeket, majd kattints ismét a zöld <i>Jóváhagyás</i> gombra.' +
    '      <ul id="errorListSimp"></ul>' +
    '    </div>' +
    '  </div>' +
    '</div>' +
    '<div class="formBlock future" id="blockTrad">' +
    '  <div class="formBlockLabel">Hagyományos</div>' +
    '  <div class="formBlockFrame">' +
    '    <div id="newEntryTradCtrl">' +
    '      &nbsp;' +
    '    </div>' +
    '    <div class="newEntryKnown">&bull;</div>' +
    '    <div class="formButtonRight accept" id="acceptTrad">' +
    '      <img src="static/sign.svg" alt=""/>' +
    '    </div>' +
    '    <div class="formButtonRight edit" id="editTrad">' +
    '      <img src="static/draw.svg" alt=""/>' +
    '    </div>' +
    '    <div class="formNote">' +
    '      Ha nem a megfelelő hagyományos írásjegy áll az első helyen, a kívánt elemre' +
    '      kattintva helyesbítheted. Ha kész, kattints a zöld <i>Jóváhagyás</i> gombra.' +
    '    </div>' +
    '  </div>' +
    '</div>' +
    '<div class="formBlock future" id="blockPinyin">' +
    '  <div class="formBlockLabel">Pinyin</div>' +
    '  <div class="formBlockFrame">' +
    '    <div id="newEntryPinyinCtrl">' +
    '      &nbsp;' +
    '    </div>' +
    '    <div class="newEntryKnown">&bull;</div>' +
    '    <div class="formButtonRight accept" id="acceptPinyin">' +
    '      <img src="static/sign.svg" alt=""/>' +
    '    </div>' +
    '    <div class="formButtonRight edit" id="editPinyin">' +
    '      <img src="static/draw.svg" alt=""/>' +
    '    </div>' +
    '    <div class="formNote" id="notePinyin">' +
    '      Ellenőrizd a szótagok pinyin-átiratát. Ha a szó szerepel a CEDICT-ben vagy a HanDeDict-ben,' +
    '      akkor az ezekből ismert olvasat áll az első helyen, amúgy az egyes írásjegyek leggyakoribb olvasata.' +
    '      Ha kész, kattints a zöld <i>Jóváhagyás</i> gombra.' +
    '    </div>' +
    '    <div class="formErrors" id="errorsPinyin">' +
    '      Duplikátum: ilyen címszó már létezik (megegyező egyszerűsített és hagyományos írásjegyek,' +
    '      azonos kiejtéssel).' +
    '    </div>' +
    '  </div>' +
    '</div>' +
    '<div class="formBlock future" id="blockTrg">' +
    '  <div class="formBlockLabel">Magyar</div>' +
    '  <div class="formBlockFrame">' +
    '    <textarea id="newEntryTrg" maxlength="1024" readonly></textarea>' +
    '    <div class="formButtonRight accept" id="acceptTrg">' +
    '      <img src="static/sign.svg" alt=""/>' +
    '    </div>' +
    '    <div class="formButtonRight edit" id="editTrg">' +
    '      <img src="static/draw.svg" alt=""/>' +
    '    </div>' +
    '    <div class="formNote" id="noteTrg">' +
    '      Add meg, újsorokkal elválasztva, a címszó magyar jelentéseit.' +
    '      Ha kész, kattints a zöld <i>Jóváhagyás</i> gombra.' +
    '    </div>' +
    '    <div class="formErrors" id="errorsTrg">' +
    '      Az automatikus ellenőrzés az alábbi problémákat találta.' +
    '      Kérlek, korrigáld ezeket, majd kattints ismét a zöld <i>Jóváhagyás</i> gombra.' +
    '      <ul id="errorListTrg"></ul>' +
    '    </div>' +
    '  </div>' +
    '</div>' +
    '<div class="formBlock future hidden" id="blockReview">' +
    '  <div class="formBlockLabel">Előnézet</div>' +
    '  <div class="formBlockFrame">' +
    '    <div id="newEntryRender"></div>' +
    '    <input id="newEntryNote" maxlength="128" placeholder="Megjegyzés, forrásmegjelölés" readonly/>' +
    '    <div class="formErrors" id="errorsReview">' +
    '      Kérlek, fűzz hozzá egy rövid megjegyzést vagy forrásmegjelölést.' +
    '    </div>' +
    '    <div class="formSubmit" id="newEntrySubmit">Eltárolom</div>' +
    '  </div>' +
    '</div>';

  var server;

  function documentReady() {
    $("#newEntrySimp").bind("compositionstart", onSimpCompStart);
    $("#newEntrySimp").bind("compositionend", onSimpCompEnd);
    $("#newEntrySimp").bind("input", onSimpChanged);
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

  $(document).ready(function () {
    documentReady();
  });

  function setActive(block) {
    $(".formBlock").removeClass("active");
    $(".formBlock").removeClass("ready");
    $(".formBlock").removeClass("future");
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
      $("#blockReview").addClass("future");
    }
    else if (block == "trad") {
      $("#blockSimp").addClass("ready");
      $("#blockTrad").addClass("active");
      $("#blockPinyin").addClass("future");
      $("#blockTrg").addClass("future");
      $("#blockReview").addClass("future");
    }
    else if (block == "pinyin") {
      $("#blockSimp").addClass("ready");
      $("#blockTrad").addClass("ready");
      $("#blockPinyin").addClass("active");
      $("#blockTrg").addClass("future");
      $("#blockReview").addClass("future");
    }
    else if (block == "trg") {
      $("#newEntryTrg").prop("readonly", false);
      $("#newEntryTrg").focus();
      $("#blockSimp").addClass("ready");
      $("#blockTrad").addClass("ready");
      $("#blockPinyin").addClass("ready");
      $("#blockTrg").addClass("active");
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

  function onSubmit(evt) {
    if ($("#newEntryNote").val().length < 6) {
      $("#errorsReview").addClass("visible");
      $("#newEntryNote").focus();
    }
    else {
      $("#errorsReview").removeClass("visible");
    }
  }

  function onTrgEdit(evt) {
    setActive("trg");
  }

  function onTrgAccept(evt) {
    if ($("#acceptTrg").hasClass("disabled")) return;
    server.verifyTrg($("#newEntryTrg").val(), onTrgVerified);
    $("#acceptTrg").addClass("disabled");
  }

  function onTrgVerified(res) {
    $("#acceptTrg").removeClass("disabled");
    if (!res.passed) {
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

  function onPinyinAccept(evt) {
    if ($("#acceptPinyin").hasClass("disabled")) return;
    server.verifyHead($("#newEntrySimp").val(), getTrad(), getPinyin(), onHeadVerified);
    $("#acceptPinyin").addClass("disabled");
  }

  function onHeadVerified(res) {
    $("#acceptPinyin").removeClass("disabled");
    if (!res.passed) {
      $("#errorsPinyin").addClass("visible");
      $("#notePinyin").addClass("hidden");
    }
    else {
      $("#errorsPinyin").removeClass("visible");
      $("#notePinyin").removeClass("hidden");
      setActive("trg");
    }
  }

  function onPinyinEdit(evt) {
    setActive("pinyin");
  }

  function onTradEdit(evt) {
    setActive("trad");
  }

  function onTradAccept(evt) {
    if ($("#acceptTrad").hasClass("disabled")) return;
    setActive("pinyin");
  }

  var simpComposing = false;

  function onSimpCompStart(evt) {
    simpComposing = true;
  }

  function onSimpCompEnd(evt) {
    simpComposing = false;
  }

  function onSimpChanged(evt) {
    if (simpComposing) return;
    server.processSimp($("#newEntrySimp").val(), onSimpProcessed);
  }

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

  function onSimpAccept(evt) {
    if ($("#acceptSimp").hasClass("disabled")) return;
    server.verifySimp($("#newEntrySimp").val(), onSimpVerified);
    $("#acceptSimp").addClass("disabled");
  }

  function onSimpVerified(res) {
    $("#acceptSimp").removeClass("disabled");
    if (!res.passed) {
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
    else {
      $("#errorsSimp").removeClass("visible");
      $("#noteSimp").removeClass("hidden");
      setActive("trad");
    }
  }

  function onSimpEdit(evt) {
    setActive("simp");
  }

  function getTrad() {
    var res = "";
    var tctrl = $("#newEntryTradCtrl");
    tctrl.children().each(function() {
      res += $(this).children().first().text();
    });
    return res;
  }

  function getPinyin() {
    var res = "";
    var tctrl = $("#newEntryPinyinCtrl");
    tctrl.children().each(function() {
      res += $(this).children().first().text();
    });
    return res;
  }

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

  function updatePinyin(pinyin) {
    $("#newEntryPinyinCtrl").empty();
    for (var i = 0; i != pinyin.length; ++i) {
      var ppos = $('<div class="newEntryPinyinPos"/>');
      for (j = 0; j != pinyin[i].length; ++j) {
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

  function onSimpTradProcessed(pinyin) {
    updatePinyin(pinyin);
  }

  return {
    init: function(id) {
      $(id).html(template);
    },
    setServer: function(srv) {
      server = srv;
    }
  }
})();

var zdNewEntryServer = (function() {
  var dummyEntry =
    '      <div class="entry">' +
    '        <span class="hw-simp"><span class="tone3">夫</span><span class="tone4 hanim">妇</span></span>' +
    '        <span class="hw-sep faint">&bull;</span>' +
    '        <span class="hw-trad"><span class="tone3">夫</span><span class="tone4 hanim">婦</span></span>' +
    '        <span class="hw-pinyin">fū fù</span>' +
    '        <div class="senses">' +
    '          <span class="sense"><span class="sense-nobr"><span class="sense-ix">1</span> házaspár</span></span>' +
    '          <span class="sense"><span class="sense-nobr"><span class="sense-ix">2</span> férj és feleség</span></span>' +
    '          <br/>' +
    '          <span class="sense"><i>Számlálószó:</i> 对&bull;對 [duì]</span>' +
    '        </div>' +
    '      </div>';

  return {
    processSimp: function(simp, ready) {
      // Query URL: localhost for sandboxing only
      var url = "/ApiHandler.ashx";
      if (window.location.protocol == "file:")
        url = "http://localhost:8000/ApiHandler.ashx";
      var req = $.ajax({
        url: url,
        type: "POST",
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        data: {action: "newentry_processsimp", simp: simp}
      });
      req.done(function(res) {
        ready(res.trad, res.pinyin, res.is_known_headword);
      });
    },

    verifySimp: function(simp, ready) {
      setTimeout(function() {
        var res = {
          passed: true
        };
        if (simp == "x") {
          res.passed = false;
          res.errors = [];
          res.errors.push("Ez <egy> & hiba.");
          res.errors.push("Van masik is!");
        }
        ready(res);
      }, 200);
    },

    verifyHead: function(simp, trad, pinyin, ready) {
      setTimeout(function() {
        var res = {
          passed: true
        };
        if (simp == "y") {
          res.passed = false;
        }
        ready(res);
      }, 200);
    },

    verifyTrg: function(trg, ready) {
      setTimeout(function() {
        var res = {
          passed: true,
          preview: dummyEntry
        };
        if (trg == "x") {
          res.passed = false;
          res.errors = [];
          res.errors.push("Ez <egy> & hiba.");
          res.errors.push("Van masik is!");
        }
        ready(res);
      }, 200);
    },

    processSimpTrad: function(simp, trad, ready) {
      setTimeout(function() {
        var pinyin = [];
        for (var i = 0; i < simp.length; ++i) {
          var pyList = [];
          pyList.push("huài");
          pyList.push("yuè");
          pyList.push("xiē");
          pinyin.push(pyList);
        }
        ready(pinyin);
      }, 200);
    }
  }
})();


zdNewEntry.init("#newEntry");
zdNewEntry.setServer(zdNewEntryServer);
//zdNewEntry.setServer(zdNewEntryShim);
