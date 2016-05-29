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
