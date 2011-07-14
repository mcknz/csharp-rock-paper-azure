<%@ Page Title="Rock Paper Azure" Language="C#" Inherits="System.Web.Mvc.ViewPage"
    MasterPageFile="~/Views/RockPaperAzure.Master" ContentType="text/html; charset=utf-8" %>
<%@ Import Namespace="Compete.Bot.Validation" %>
<asp:Content runat="server" ContentPlaceHolderID="HeadContent">
    <link rel="stylesheet" href="http://rps.blob.core.windows.net/downloads/jquery-ui-1.8.10.custom.css"
        type="text/css" />
    <script language="javascript" type="text/javascript" src="http://ajax.microsoft.com/ajax/jQuery/jquery-1.4.4.min.js"></script>
    <script language="javascript" type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.10/jquery-ui.min.js"></script>
    <script language="javascript" type="text/javascript" src="/scripts/rpa-util.js"></script>
    <script language="javascript" type="text/javascript" src="/scripts/rpa-home.js"></script>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="MainContent">
    <td width="29%" align="left" valign="top">
        <p class="headline3">
            Your Bots</p>
        <!-- Your bots Area start -->
        <p class="content1">
            Add or remove bots in your local Bot Lab below to experiment with various implementations.
            To see how your bots fare amongst themselves, start a battle!
        </p>
        <table id="botList">
            <tbody>
                <% foreach (String team in ViewData["teamList"] as IEnumerable<String>)
                   {
                %>
                <tr class="content1 botRow">
                    <td>
                        <%= Html.Encode(team)%>
                    </td>
                    <td width="30px">
                        <a href="/Bots/Remove/<%= Html.Encode(team) %>" class="content1link">[remove]</a>
                    </td>
                </tr>
                <% } %>
            </tbody>
        </table>

<% if (!(TempData["BotErrors"] == null)) { %>
       <script type="text/javascript">
            var botErrorString = "";
            <% foreach (var err in TempData["BotErrors"] as IEnumerable<BotValidationError>)  { %>
                botErrorString = botErrorString + '<%= err.HtmlEncode() %>' + '<br/><br/>';
            <% } %>
       </script>
<% } %>

        <!-- Upload bot Area start -->
        <div style="margin-top: 15px">
            <p class="subhead2 center">
                Add a new bot to your lab</p>
            <form method="post" enctype="multipart/form-data" action="/Bots/Upload">
            <input type="text" id="botFileName" disabled="disabled" value="Browse to your bot =&gt;" />
            <span class="file-input-span">
                <input id="botFileControl" type="file" name="botFile" class="file-input content1" /><input
                    type="button" value="..." style="margin-left: -5px;" />
            </span>
            <input id="botAddButton" type="submit" disabled="disabled" value="Add Bot" style="float: right;
                position: relative;" />
            </form>
            <p class="center">
                <a href="https://www.rockpaperazure.com/GetStarted.aspx#3" class="content1link" target="getStarted">
                    How do I implement a new bot?</a>
            </p>
        </div>
        <!-- Upload bot Area end -->
    </td>
    <td width="4%">
        &nbsp;
    </td>
    <td width="33%" valign="top" align="left">
        <p class="headline3">
            Bot Lab Battle Results</p>
        <table width="90%">
            <tr>
                <td valign="middle">
                    <input id="buttonPlay" type="button" style="margin-left: 5px; margin-right: 5px"
                        value="Start Battle!" />
                </td>
                <td valign="middle" class="content1" id="playText">
                    See how your bots stack up against one another!
                </td>
            </tr>
        </table>
        <table id="resultTable">
            <thead class="content1">
                <tr class="resultHead">
                    <th class="resultColumn">
                        Place
                    </th>
                    <th class="resultColumn">
                        Bot
                    </th>
                    <th class="resultColumn">
                        W-L-T
                    </th>
                    <th class="resultColumn">
                        Score
                    </th>
                </tr>
            </thead>
            <tbody id="resultBody" class="content1">
            </tbody>
        </table>
    </td>
    <td width="2%" style="border-left: 2px solid #006699">
        &nbsp;
    </td>
    <td width="32%" valign="top" align="left">
        <p class="headline3">
            Enter the Contest</p>
        <% if ((Boolean)ViewData["inCloud"])
           { %>
        <p class="content1">
            Ready to take the <strong>Rock Paper Azure Challenge</strong>?</p>
        <p class="content1">
            Pick the bot from your Bot Lab that you think has the best chance of winning:</p>
        <p class="content1 center">
            <select id="botSelector">
                <% var currentTeam = ViewData["CurrentTeam"].ToString();
                   if (currentTeam == "")
                %>
                <option value="*">(Choose Bot)</option>
                <%
                       foreach (String team in ViewData["teamList"] as IEnumerable<String>)
                       {
                           var attrValue = (team == ViewData["currentTeam"].ToString()) ? "selected" : "";
                %>
                <option value="<%= Html.Encode(team) %>" <%= attrValue %>>
                    <%= Html.Encode(team) %></option>
                <% } %>
            </select>
        </p>
        <p class="content1">
            ...then <a id="contestEntryLink" href="http://www.rockpaperazure.com/user/MyBot.aspx"
                target="contestPage">continue to the contest site</a> to complete your entry.
        </p>
        <% }
           else
           { %>
        <p class="content1">
            <em>To enter the Rock Paper Azure Challenge, you'll need to run your Bot Lab in Windows
                Azure.</em></p>
        <p class="center">
            <a href="http://www.rockpaperazure.com/getstarted.aspx#4" target="getStarted" class="content1link">
                How do I run my Bot Lab in Windows Azure?</a></p>
        <% } %>
    </td>
</asp:Content>
<asp:Content ContentPlaceHolderID="ModalContent" runat="server">
    <!-- jQuery UI modal dialog -->
    <div id="gameLogOverlay" title="View Game Log" class="content1">
        <select id="teamListLeft" class="teamDropDown">
        </select>
        <span style="margin-left: 15px; margin-right: 15px">vs.</span>
        <select id="teamListRight" class="teamDropDown">
        </select>
        <p id="notFound" class="headline1" style="margin-top: 20px; text-align: center;">
            No game log found for selected combatants.</p>
        <div id="gameLog">
            <pre>
            </pre>
        </div>
    </div>
</asp:Content>
<asp:Content ContentPlaceHolderID="ErrorOverlay" runat="server">
    <!-- jQuery UI modal dialog -->
    <div id="errorOverlay" title="Bot Errors" class="content1">
        <p style="text-align: center"><b>Could not load bot due to the following:</b></p>
        <div id="errorList"></div>
    </div>
</asp:Content>