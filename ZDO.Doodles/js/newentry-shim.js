var zdNewEntryShim = (function() {
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
      setTimeout(function() {
        var trad = [];
        var pinyin = [];
        var known_hw = simp == "大家";
        for (var i = 0; i < simp.length; ++i) {
          var tradList = [];
          tradList.push(simp[i]);
          tradList.push("月");
          trad.push(tradList);
          var pyList = [];
          pyList.push("yuè");
          pyList.push("xiē");
          pinyin.push(pyList);
        }
        ready(trad, pinyin, known_hw);
      }, 200);
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
