// ----------------- HELPERS ------------------------

function createCookie(name, value, expires, path, domain) {
  var cookie = name + "=" + escape(value) + ";";
  if (expires) {
    // If it's a date
    if (expires instanceof Date) {
      // If it isn't a valid date
      if (isNaN(expires.getTime()))
        expires = new Date();
    }
    else
      expires = new Date(new Date().getTime() + parseInt(expires) * 1000 * 60 * 60 * 24);
    cookie += "expires=" + expires.toGMTString() + ";";
  }
  if (path)
    cookie += "path=" + path + ";";
  if (domain)
    cookie += "domain=" + domain + ";";
  document.cookie = cookie;
}

function getCookie(name) {
  var regexp = new RegExp("(?:^" + name + "|;\s*" + name + ")=(.*?)(?:;|$)", "g");
  var result = regexp.exec(document.cookie);
  return (result === null) ? null : result[1];
}

// --------------- END HELPERS ----------------------

var isMobile = false;
var uiLang = "de";

$(document).ready(function () {
  mobileOrFull();
  initGui();
  initStrokes();

  // Debug: to work on strokes input
  //showStrokeInput();

  eventWireup();

  $("#txtSearch").focus();
  $("#txtSearch").select();

  // Debug: to work on opening screen
  //$("#resultsHolder").css("display", "none");
  //$("#welcomeScreen").css("display", "block");
});

function mobileOrFull() {
  if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|ipad|iris|kindle|Android|Silk|lge |maemo|midp|mmp|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(navigator.userAgent)
    || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(navigator.userAgent.substr(0, 4))) isMobile = true;

  //isMobile = true;
  $("body").attr("class", isMobile ? "mobile" : "full");
  $("#btn-clear").css("display", isMobile ? "block" : "none");
  if (!isMobile) {
    $("#cell-menu").remove();
    $("#txtSearch").css("padding-left", "0.3em");
  }
}

function initGui() {
  if (!isMobile) {
    $("#img-write").tooltipster({
      content: $("<span>Show or hide handwriting recognition</span>")
    });
    $("#img-search").tooltipster({
      content: $("<span>Search in HanDeDict. You can also just press Enter.</span>")
    });
  }
  var cookies = localStorage.getItem("cookies");
  if (cookies == "go") {
    $("#bittercookie").css("display", "none");
  }

  // Get cookie with language preference, if present
  // *SET* cookie with language preference (so we keep extending cookie)
  var uiFromCookie = getCookie("uilang");
  if (uiFromCookie !== null) uiLang = uiFromCookie;
  createCookie("uilang", uiLang, 365);
}

function showStrokeInput() {
  if (!isMobile) {
    var searchPanelOfs = $("#search-panel").offset();
    var searchPanelWidth = $("#search-panel").width();
    var searchPanelHeight = $("#search-panel").height();
    var strokeInputWidth = $("#stroke-input").outerWidth();
    $("#stroke-input").css("top", searchPanelOfs.top + searchPanelHeight + 1);
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
}

function hideStrokeInput() {
  $("#stroke-input").css("display", "none");
}

function eventWireup() {
  $("#btn-menu").click(toggleMenu);
  $("#btn-clear").click(clearSearch);
  $("#btn-write").click(function () {
    if ($("#stroke-input").css("display") == "block") {
      hideStrokeInput();
      $("#btn-write").attr("class", "");
    }
    else {
      showStrokeInput();
      $("#btn-write").attr("class", "active");
    }
  });
  $("#stroke-clear").click(clearCanvas);
  $("#stroke-undo").click(undoStroke);
  $("#swallowbitterpill").click(acceptCookies);

  $("#navSearch").click(function () {
    window.location = "/";
  });
  $("#navAbout").click(function () {
    window.location = "/about";
  });

  $("#langselEn").click(function () {
    selectLang("en");
  });
  $("#langselDe").click(function () {
    selectLang("de");
  });
  $("#langselJian").click(function () {
    selectLang("jian");
  });
  $("#langselFan").click(function () {
    selectLang("fan");
  });
  $("#btn-search").click(submitSearch);
  $("#txtSearch").keyup(function (e) {
    if (e.keyCode == 13) {
      submitSearch();
      return false;
    }
  });
}

function selectLang(lang) {
  var newLang = "de";
  if (lang == "en") newLang = "en";
  else if (lang == "de") newLang = "de";
  else if (lang == "jian") newLang = "jian";
  else if (lang == "fan") newLang = "fan";
  if (newLang == uiLang) return;
  uiLang = newLang;
  createCookie("uilang", uiLang, 365);
  //location.reload(); // This sends POST again, but we prefer no post (when switching language after a query)
  window.location.href = window.location.protocol + '//' + window.location.host + window.location.pathname;
}

function acceptCookies() {
  $("#bittercookie").css("display", "none");
  localStorage.setItem("cookies", "go");
}

function clearSearch() {
  $("#txtSearch").val("");
  $("#txtSearch").focus();
}

function toggleMenu() {
  var srcBase = $("#img-menu").attr("src");
  var imagePath = srcBase.slice(0, srcBase.lastIndexOf("/"));
  if ($("#menu").css("display") == "block") {
    $("#menu").css("display", "none");
    $("#img-menu").attr("src", imagePath + "/hamburger.svg");
    $("#btn-menu").css("background-color", "white");
  } else {
    $("#menu").css("display", "block");
    $("#img-menu").attr("src", imagePath + "/close.svg");
    $("#btn-menu").css("background-color", "#b4ca65");
  }
}

function submitSearch() {
  'use strict';
  var form;
  form = $('<form />', {
    action: '/',
    method: 'post',
    style: 'display: none;'
  });
  $('<input />', {
    type: 'hidden',
    name: 'query',
    value: $('#txtSearch').val()
  }).appendTo(form);
  form.appendTo('body').submit();
}
