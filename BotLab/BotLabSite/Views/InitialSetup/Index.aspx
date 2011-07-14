<%@ Page Title="Rock Paper Azure" Language="C#" Inherits="System.Web.Mvc.ViewPage"
    MasterPageFile="~/Views/RockPaperAzure.Master" ContentType="text/html; charset=utf-8" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContent">
    <td width="52%" valign="top" align="left">
        <p class="headline3">
            Welcome to your Rock Paper Azure Challenge Bot Lab.</p>
        <p class="content1">
            The <b>Rock Paper Azure Bot Lab</b> is a web application that enables you to test
            multiple Rock Paper Azure bot implementations and play them against each other.
            To get started, select and enter a password. If you revisit the site, or your session
            expires, you'll need to re-enter this password to gain access to your Bot Lab.
        </p>
        <p class="content1">
            <% if ((Boolean)ViewData["inCloud"])
               { %>
            <b>Congratulations!</b> You are currently running the Bot Lab in the Windows Azure
            cloud!
            <% }
               else
               { %>
            <em>You are currently running the Bot Lab in the Windows Azure emulator on your local
                machine. Before you can enter the contest you will need to deploy the AzureService
                cloud project to your own Windows Azure account.</em>
            <% } %>
        </p>
    </td>
    <td width="6%" valign="top" align="left">
        &nbsp;
    </td>
    <td width="42%" valign="top" align="left">
        <p class="headline3">
            One-time Configuration</p>
        <form method="post" action="/InitialSetup">
        <fieldset>
            <p>
                <label for="password">
                    Password:</label>
                <input type="password" id="password" name="password" />
                <%= Html.ValidationMessage("password", new { @class = "failureNotification" }) %>
            </p>
            <p>
                <label for="confirmPassword">
                    Confirm Password:</label>
                <input type="password" id="confirmPassword" name="confirmPassword" />
                <%= Html.ValidationMessage("confirmPassword", new { @class = "failureNotification" }) %>
            </p>
            <p>
                <input id="setup" type="submit" value="Setup" />
            </p>
        </fieldset>
        </form>
    </td>
</asp:Content>
