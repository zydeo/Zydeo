<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs"
  Inherits="ZDO.CHSite.Default" ClientIDMode="Static" %>
<%@ MasterType VirtualPath="~/Site.master" %>


<asp:Content ID="mainContent" ContentPlaceHolderID="mainContentPlaceholder" runat="server">
  <div id="search-bar">
    <table id="search-panel">
      <tr>
        <td id="cell-menu">
          <span id="btn-menu"><img id="img-menu" src="/static/hamburger.svg" alt="Menu"/></span>
        </td>
        <td id="cell-input">
          <input type="text" name="txt-search" id="txtSearch" placeholder="*Hanzi, Pinyin or German word" maxlength="64" runat="server"/>
        </td>
        <td id="cell-clear">
          <span id="btn-clear"><img id="img-clear" src="/static/clear.svg" alt="Clear text"/></span>
        </td>
        <td id="cell-write">
          <span id="btn-write"><img id="img-write" src="/static/brush.svg" alt="Show handwriting input"/></span>
        </td>
        <td id="cell-search">
          <span id="btn-search"><img id="img-search" src="/static/searchcmd.svg" alt="Look up in dictionary"/></span>
        </td>
      </tr>
    </table>
  </div>
  <!--
  <div id="stroke-input">
    <div id="stroke-inner">
      <div id="strokeDataLoading">Loading...</div>
      <canvas id="stroke-input-canvas"></canvas>
      <div id="stroke-commands">
        <div id="strokeClear" runat="server">*Clear</div>
        <div id="strokeUndo" runat="server">*Undo</div>
      </div>
      <div id="suggestions">
      </div>
    </div>
  </div>
  -->
  <div id="results">
    <div id="resultsHolder" runat="server">
      <div id="topNotice" runat="server" visible="false">
        <p class="tnTitle" id="tnTitle" runat="server">*Annotation mode</p>
        <p class="tnMessage" id="tnMessage" runat="server">
            *The text you submitted is not a word in HanDeDict, but Zydeo did find parts of it in the dictionary.
            Instead of normal lookup results, you can find an annotated version of your text below.
        </p>
      </div>
    </div>
    <div id="soaBox" class="soaBoxLeft" runat="server">
      <div id="soaBoxTail">&nbsp;</div>
      <div id="soaHead">
        <div id="soaTitle" runat="server">*Stroke order</div>
        <div id="soaClose">X</div>
      </div>
      <div id="soaGraphics">
        <svg xmlns="http://www.w3.org/2000/svg" version="1.1" viewbox='0 0 1024 1024' id="strokeAnimSVG"></svg>
        <div id="soaError"><div id="soaErrorContent"></div></div>
      </div>
      <div id="soaFooter" runat="server">
        *Animation by <a href="https://skishore.github.io/makemeahanzi/" target="_blank">makemeahanzi</a>
      </div>
    </div>
    <div id="welcomeScreen" runat="server">
      <asp:Literal ID="litWelcomeScreen" runat="server" />
    </div>
  </div>
</asp:Content>
