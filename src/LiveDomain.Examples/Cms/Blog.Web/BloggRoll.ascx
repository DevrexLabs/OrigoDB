<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BloggRoll.ascx.cs" Inherits="Blog.Web.BloggRoll" %>

<asp:DataList ID="BlogRollDataList" runat="server">
    <ItemTemplate>
        <h3><asp:HyperLink ID="HyperLink1" runat="server" 
            NavigateUrl='<%# "~/blog.aspx?name=" + Server.UrlEncode((string)Eval("Title")) %>' 
            Text='<%# Eval("Title") %>'></asp:HyperLink></h3>
        <asp:Label ID="Label2" runat="server" Text='This is a long description describing the blog'></asp:Label>
    </ItemTemplate>
</asp:DataList>