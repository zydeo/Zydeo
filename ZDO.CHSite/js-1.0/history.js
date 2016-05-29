/// <reference path="x-jquery-2.1.4.min.js" />
/// <reference path="x-jquery.color-2.1.2.min.js" />
/// <reference path="x-jquery.tooltipster.min.js" />
/// <reference path="strings-hu.js" />
/// <reference path="page.js" />

var zdHistory = (function() {

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

  }

})();
