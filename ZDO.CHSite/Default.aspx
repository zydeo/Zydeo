<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ZDO.CHSite.Default" ClientIDMode="Static" %>

<!DOCTYPE html>
<html lang="<% =HtmlLang %>">
<head>
  <meta charset="utf-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=0" />
  <title><% =StTitle %></title>
  <meta name="keywords" content="<% =StKeywords %>" />
  <meta name="description" content="<% =StDescription %>" />
  <meta name="google" content="notranslate">
  <link href='https://fonts.googleapis.com/css?family=Noto+Sans:400,400italic&subset=latin,latin-ext' rel='stylesheet' type='text/css'>
  <link href='https://fonts.googleapis.com/css?family=Neuton&subset=latin,latin-ext' rel='stylesheet' type='text/css'>
  <link href='https://fonts.googleapis.com/css?family=Ubuntu:700&subset=latin,latin-ext' rel='stylesheet' type='text/css'>
  <link href='https://fonts.googleapis.com/css?family=Ubuntu&subset=latin,latin-ext' rel='stylesheet' type='text/css'>
  <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.6.1/css/font-awesome.min.css">
  <% if(DebugMode) { %>
  <link rel="stylesheet" href="/style-<% =VerStr %>/tooltipster.min.css">
  <link rel="stylesheet" href="/style-<% =VerStr %>/page.min.css">
  <link rel="stylesheet" href="/style-<% =VerStr %>/forms.min.css">
  <link rel="stylesheet" href="/style-<% =VerStr %>/entry.min.css">
  <link rel="stylesheet" href="/style-<% =VerStr %>/newentry.min.css">
  <link rel="stylesheet" href="/style-<% =VerStr %>/history.min.css">
  <link rel="stylesheet" href="/style-<% =VerStr %>/lookup.min.css">
  <link rel="stylesheet" href="/style-<% =VerStr %>/diagnostics.min.css">
  <% } else { %>
  <link rel="stylesheet" href="/style-<% =VerStr %>/bundle.css">
  <% } %>
  <!--
  <script>
    !function (a) { var b = /iPhone/i, c = /iPod/i, d = /iPad/i, e = /(?=.*\bAndroid\b)(?=.*\bMobile\b)/i, f = /Android/i, g = /(?=.*\bAndroid\b)(?=.*\bSD4930UR\b)/i, h = /(?=.*\bAndroid\b)(?=.*\b(?:KFOT|KFTT|KFJWI|KFJWA|KFSOWI|KFTHWI|KFTHWA|KFAPWI|KFAPWA|KFARWI|KFASWI|KFSAWI|KFSAWA)\b)/i, i = /IEMobile/i, j = /(?=.*\bWindows\b)(?=.*\bARM\b)/i, k = /BlackBerry/i, l = /BB10/i, m = /Opera Mini/i, n = /(CriOS|Chrome)(?=.*\bMobile\b)/i, o = /(?=.*\bFirefox\b)(?=.*\bMobile\b)/i, p = new RegExp("(?:Nexus 7|BNTV250|Kindle Fire|Silk|GT-P1000)", "i"), q = function (a, b) { return a.test(b) }, r = function (a) { var r = a || navigator.userAgent, s = r.split("[FBAN"); return "undefined" != typeof s[1] && (r = s[0]), s = r.split("Twitter"), "undefined" != typeof s[1] && (r = s[0]), this.apple = { phone: q(b, r), ipod: q(c, r), tablet: !q(b, r) && q(d, r), device: q(b, r) || q(c, r) || q(d, r) }, this.amazon = { phone: q(g, r), tablet: !q(g, r) && q(h, r), device: q(g, r) || q(h, r) }, this.android = { phone: q(g, r) || q(e, r), tablet: !q(g, r) && !q(e, r) && (q(h, r) || q(f, r)), device: q(g, r) || q(h, r) || q(e, r) || q(f, r) }, this.windows = { phone: q(i, r), tablet: q(j, r), device: q(i, r) || q(j, r) }, this.other = { blackberry: q(k, r), blackberry10: q(l, r), opera: q(m, r), firefox: q(o, r), chrome: q(n, r), device: q(k, r) || q(l, r) || q(m, r) || q(o, r) || q(n, r) }, this.seven_inch = q(p, r), this.any = this.apple.device || this.android.device || this.windows.device || this.other.device || this.seven_inch, this.phone = this.apple.phone || this.android.phone || this.windows.phone, this.tablet = this.apple.tablet || this.android.tablet || this.windows.tablet, "undefined" == typeof window ? this : void 0 }, s = function () { var a = new r; return a.Class = r, a }; "undefined" != typeof module && module.exports && "undefined" == typeof window ? module.exports = r : "undefined" != typeof module && module.exports && "undefined" != typeof window ? module.exports = s() : "function" == typeof define && define.amd ? define("mobi", [], a.mobi = s()) : a.mobi = s() }(this);
  </script>
  -->
</head>
<body id="theBody" runat="server" class="pt16">
  <div id="thePage" style="visibility: hidden;">
    <div id="headerstick">
      <div id="headerHome"><span id="hdrHomeLatin">CHDICT</span><span id="hdrHomeZho">汉匈词典</span></div>
      <div id="header">
        <div id="hdrSearch" class="hdrAlt on">
          <div id="searchBox">
            <input type="text" id="txtSearch" placeholder="<% =EscStr("HintSearchField") %>" autofocus/>
            <i id="btn-write" class="fa fa-paint-brush" aria-hidden="true"></i>
            <i id="btn-settings" class="fa fa-cog" aria-hidden="true"></i>
            <i id="btn-search" class="fa fa-search" aria-hidden="true"></i>
          </div>
          <i id="toMenu" class="fa fa-chevron-left" aria-hidden="true"></i>
        </div>
        <div id="hdrMenu" class="hdrAlt">
          <a class="topMenu ajax" id="topMenuSearch" href="/<% =Lang %>"><i class="fa fa-search" aria-hidden="true"></i></a>
          <a class="topMenu ajax" id="topMenuEdit" href="/<% =Lang %>/edit/new"><% =EscStr("TopMenuEdit") %></a>
          <a class="topMenu ajax" id="topMenuRead" href="/<% =Lang %>/read/about"><% =EscStr("TopMenuRead") %></a>
          <a class="topMenu ajax" id="topMenuDownload" href="/<% =Lang %>/download"><% =EscStr("TopMenuDownload") %></a>
          <div id="hdrMenuRight">
            <span class="loginIcon"><i class="fa fa-user" aria-hidden="true"></i></span>
            <a class="langSel on" id="langSelHu" href="/hu/<% =Rel %>">HU</a>
            <a class="langSel first" id="langSelEn" href="/en/<% =Rel %>">EN</a>
          </div>
        </div>
      </div>
      <div id="subHeader">
        <div class="subMenu" id="subMenuEdit">
          <span id="smEditNew"><a class="ajax" href="/<% =Lang %>/edit/new"><% =EscStr("SMEditNew") %></a></span>
          <span id="smEditHistory"><a class="ajax" href="/<% =Lang %>/edit/history"><% =EscStr("SMEditHistory") %></a></span>
          <span id="smEditExisting"><a class="ajax" href="/<% =Lang %>/edit/existing"><% =EscStr("SMEditExisting") %></a></span>
        </div>
        <div class="subMenu" id="subMenuRead">
          <span id="smReadAbout"><a class="ajax" href="/<% =Lang %>/read/about"><% =EscStr("SMReadAbout") %></a></span>
          <span id="smReadArticles"><a class="ajax" href="/<% =Lang %>/read/articles"><% =EscStr("SMReadArticles") %></a></span>
          <span id="smReadEtc"><a class="ajax" href="/<% =Lang %>/read/etc"><% =EscStr("SMReadEtc") %></a></span>
        </div>
        <div class="subMenu" id="subMenuDownload">
          <span id="smDownload"><span><% =EscStr("SMDownload") %></span></span>
        </div>
      </div>
      <div id="searchOptionsBox"></div>
    </div>
    <div id="headermask">&nbsp;</div>
    <div id="dynPage" class="nosubmenu" runat="server"></div>
    <div id='bottomSpacer'>&nbsp;</div>
  </div>
  
  <div id="debug"></div>
  <div id="emMeasure">mmmmmmmmmm</div>

  <% if (DebugMode) { %>
  <%-- Keep include order in sync with bundling in bundleconfig.json --%>
  <script src="/js-<% =VerStr %>/x-history.min.js"></script>
  <script src="/js-<% =VerStr %>/x-jquery-2.1.4.min.js"></script>
  <script src="/js-<% =VerStr %>/x-jquery.color-2.1.2.min.js"></script>
  <script src="/js-<% =VerStr %>/x-jquery.tooltipster.min.js"></script>
  <asp:Literal ID="litJS" runat="server" />
  <% } else { %>
  <script src="/js-<% =VerStr %>/app-lib-bundle.js"></script>
  <script src="/js-<% =VerStr %>/app-js-bundle.min.js"></script>
  <% } %>
</body>
</html>
