<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Static.aspx.cs"
  Inherits="ZDO.CHSite.Static" ClientIDMode="Static" %>
<%@ MasterType VirtualPath="~/Site.master" %>

<asp:Content ID="mainContent" ContentPlaceHolderID="mainContentPlaceholder" runat="server">

<div id="content">
<asp:Literal runat="server" ID="lit" />
</div>

</asp:Content>
