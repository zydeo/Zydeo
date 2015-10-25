<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Statics.aspx.cs"
  Inherits="Site.Statics" ClientIDMode="Static" %>
<%@ Register Src="~/StaticContentCtrl.ascx" TagPrefix="ZDO" TagName="StaticContentCtrl" %>
<%@ MasterType VirtualPath="~/Site.master" %>

<asp:Content ID="content" ContentPlaceHolderID="mainContentPlaceholder" runat="server">
    <div id="page-header">
      <div id="cell-menu">
        <span id="btn-menu"><img id="img-menu" src="static/hamburger.svg" alt="Menu"/></span>
      </div>
      <h1><asp:Label ID="pageHeader" runat="server">*HanDeDict @ Zydeo</asp:Label></h1>
    </div>
    <div id="staticContent" runat="server">
    </div>
</asp:Content>
