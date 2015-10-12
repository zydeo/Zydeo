<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs"
  Inherits="Site.Default" ClientIDMode="Static" %>
<%@ Register Src="~/OneResultCtrl.ascx" TagPrefix="ZDO" TagName="OneResultControl" %> 

<asp:Content ID="content" ContentPlaceHolderID="mainContentPlaceholder" runat="server">
    <div id="search-bar">
      <table id="search-panel">
        <tr>
          <td id="cell-menu">
            <span id="btn-menu"><img id="img-menu" src="static/hamburger.svg" alt="Menu"/></span>
          </td>
          <td id="cell-input">
            <input type="text" name="txt-search" id="txtSearch" placeholder="Hanzi, Pinyin or German word" autofocus runat="server"/>
          </td>
          <td id="cell-clear">
            <span id="btn-clear"><img id="img-clear" src="static/clear.svg" alt="Clear text"/></span>
          </td>
          <td id="cell-write">
            <span id="btn-write"><img id="img-write" src="static/brush.svg" alt="Show handwriting input"/></span>
          </td>
          <td id="cell-search">
            <span id="btn-search"><img id="img-search" src="static/search.svg" alt="Look up in dictionary"/></span>
          </td>
        </tr>
      </table>
    </div>
    <div id="stroke-input">
      <div id="stroke-inner">
        <canvas id="stroke-input-canvas"></canvas>
        <div id="stroke-commands">
          <div id="stroke-clear">Clear</div>
          <div id="stroke-undo">Undo</div>
        </div>
        <div id="suggestions"></div>
       </div>
     </div>
     <div id="results">
      <div id="resultsHolder" runat="server">
      </div>
    </div>
</asp:Content>
