<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="History.aspx.cs"
  Inherits="ZDO.CHSite.History" ClientIDMode="Static" %>

<%@ MasterType VirtualPath="~/Site.master" %>

<asp:Content ID="mainContent" ContentPlaceHolderID="mainContentPlaceholder" runat="server">

  <div id="content">
    <!-<asp:Literal runat="server" ID="lit" />-->
    <div id="pager">
      <div id="lblPage" runat="server">Oldal</div>
      <div id="pageLinks" runat="server">
        <asp:Literal ID="litLinks" runat="server" />
      </div>
    </div>
    <div id="changeList" runat="server" />
  </div>

</asp:Content>
