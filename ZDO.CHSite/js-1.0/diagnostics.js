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

