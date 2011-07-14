<%@ Page Title="Rock Paper Azure" Language="C#" Inherits="System.Web.Mvc.ViewPage"
    MasterPageFile="~/Views/RockPaperAzure.Master" ContentType="text/html; charset=utf-8" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContent">
    <td width="52%" valign="top" align="left">
        <p class="headline3">
            Welcome back.</p>
        <p class="content1">
            <b>Please enter your password to continue.</b></p>
    </td>
    <td width="6%" valign="top" align="left">
        &nbsp;
    </td>
    <td width="42%" valign="top" align="left">
        <p class="headline3">
            Login</p>
        <form method="post" action="/Login">
        <fieldset>
            <p>
                <label for="password">
                    Password:</label>
                <input id="password" type="password" name="password" />
                <%= Html.ValidationMessage("password", new { @class = "failureNotification" }) %>
            </p>
            <p>
                <input id="login" type="submit" value="Login" />
            </p>
        </fieldset>
        </form>
    </td>
</asp:Content>
