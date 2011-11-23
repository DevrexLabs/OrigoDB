<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Blog.Web._Default" %>

<%@ Register src="BloggRoll.ascx" tagname="BloggRoll" tagprefix="uc1" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Welcome to the #liveDB blog site
    </h2>
    <p>
        This blog site is built using <a href="http://livedb.devrex.se" target ="_blank">#liveDB</a>, an in-memory native .net database engine.
        All blog entrys, comments, users, tags, categories and search index live in memory.
    </p>
    <h2>Blogs
    </h2>
    <uc1:BloggRoll ID="BloggRoll1" runat="server" />

    <div id="search">
    <h2>Search</h2>
    
    </div>

</asp:Content>
