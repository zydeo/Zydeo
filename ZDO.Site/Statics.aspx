<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Statics.aspx.cs"
  Inherits="Site.Statics" ClientIDMode="Static" %>
<%@ Register Src="~/StaticContentCtrl.ascx" TagPrefix="ZDO" TagName="StaticContentCtrl" %>
<%@ MasterType VirtualPath="~/Site.master" %>

<asp:Content ID="content" ContentPlaceHolderID="mainContentPlaceholder" runat="server">
  <div id="theContent" runat="server">
  </div>
</asp:Content>
