var initialLoad = true;
var inRound = false;
var intervalId;

var matchStatusFailedShown = false;
var resultsFailedShown = false;

var processStandings = function (standings) {
    $(".resultRow").remove();

    $.each(standings, function (index, standing) {
        $("#resultBody").append('<tr class="resultRow"> <td class="resultColumn rightAlign">' + (standing.Rank >= 1 ? standing.Rank : "-") +
             '</td><td class="resultColumn leftAlign"><a class="content1link resultTeamName" title="View detailed game logs">' + standing.Name + '</a></td><td class="resultColumn center">' + standing.Wins + '-' + standing.Losses + '-' + standing.Ties + '</td><td class="resultColumn rightAlign">' + standing.Score + '</td></tr>');
    });

    $(".resultTeamName").click(function (event) { getTeamListAjax(event) });
}

function getTeamListAjax(event) {
    var endPoint = toAbsoluteUrl("Results/TeamList");
    $.ajax({
        url: endPoint,
        dataType: "json",
        data: {},
        success: function (data) {
            getGameLogTeams($(event.target).text(), data);
            getGameLogTextAjax();
        },
        error: function (jqXHR) {
            ajaxError(jqXHR.status, jqXHR.statusText, endPoint);
        }
    });
}

function getGameLogTeams(leftTeamName, data) {
    $(".teamDropDown").empty();
    $.each(data, function (index, item) {
        $("#teamListLeft").get(0).options[index] = new Option(item, item);
        $("#teamListRight").get(0).options[index] = new Option(item, item);
    });

    $("#teamListLeft option:[value='" + leftTeamName + "']").attr("selected", "selected");
    if ($("#teamListRight option:eq(0)").val() == leftTeamName)
        $("#teamListRight option:eq(1)").attr("selected", "selected");
    else
        $("#teamListRight option:eq(0)").attr("selected", "selected");
}

function getGameLogTextAjax() {
    var endPoint = toAbsoluteUrl("GameLog/" +
         $("#teamListLeft  option:selected").val() +
         ".vs." +
         $("#teamListRight option:selected").val());

    $.ajax({
        url: endPoint,
        dataType: "json",
        data: {},
        success: function (data) {
            if (data != null) {

                var content = data.Log;

                // replace logic to handle IE6-IE8 to preserve whitespace in <pre> tags
                if (!jQuery.support.leadingWhitespace)
                   content = content.replace(/\r\n/gi, "<br/>").replace(/ /gi, "&nbsp;").replace(/span&nbsp;/gi, "span ");
                
                $("#gameLog pre").html(content);
                $("#gameLog").scrollTop(0);
            }
            setGameLogElementVisibility(data !== null);
            $("#gameLogOverlay").dialog("open");
        },
        error: function (jqXHR) {
            setGameLogElementVisibility(false);
            ajaxError(jqXHR.status, jqXHR.statusText, endPoint);
        }
    });
};

function setGameLogElementVisibility(visibility) {
    if (visibility) {
        $("#gameLog").show();
        $("#notFound").hide();
    }
    else {
        $("#gameLog").hide();
        $("#notFound").show();
    }
}

function markRoundStarted() {
    $("#buttonPlay").attr("disabled", true).val("In battle!");
    $("#resultTable").hide();
    intervalId = setInterval(getMatchStatusAjax, 1500);
    inRound = true;
}

function markRoundEnded() {
    var numberOfBots = $(".botRow").size();
    $("#buttonPlay").attr('disabled', numberOfBots < 2).val("Start Battle!");
    if (numberOfBots < 2) {
        $("#playText").html("You need at least two bots to battle!");
    } else {
        $("#playText").html("See how your bots stack up against one another!");
    };
    if ($(".resultRow").length > 0) $("#resultTable").show();
    clearInterval(intervalId);
    inRound = false;
}


function getMatchResultsAjax() {
    var endPoint = toAbsoluteUrl("Results");
    $.ajax({
        url: endPoint,
        dataType: "json",
        data: {},
        success: function (data) {
            processStandings(data);
            markRoundEnded();
        },
        error: function (jqXHR) {
            ajaxError(jqXHR.status, jqXHR.statusText, endPoint, "The latest battle results cannot be displayed.");
            markRoundEnded();
        }
    });
}


var getMatchStatusAjax = function () {
    var endPoint = toAbsoluteUrl("Match/Status");
    $.ajax({
        url: endPoint,
        dataType: "json",
        data: {},
        success: function (data) {
            if (data) {
                if (!inRound) markRoundStarted();
            }
            else {
                if (inRound || initialLoad) {
                    getMatchResultsAjax()
                }
                initialLoad = false;
            }
        },
        error: function (jqXHR) {
            if (!matchStatusFailedShown) {
                matchStatusFailedShown = true;
                ajaxError(jqXHR.status, jqXHR.statusText, endPoint, "The status of the match cannot be determined; therefore, latest battle results cannot be displayed.");
            }
            initialLoad = false;
        }
    });
};

$(document).ready(function () {

    getMatchStatusAjax();

    $("#botFileControl").change(function (event) {
        var fileName = $(event.target).val();
        var pathSplit = fileName.split("\\");
        if (pathSplit.length > 0)
            fileName = pathSplit[pathSplit.length - 1];
        $("#botFileName").val(fileName);
        $("#botAddButton").attr("disabled", (fileName.length > 0) ? "" : "disabled");
    });

    $("#contestEntryLink").attr("href", $("#contestEntryLink").attr("href") + "?domain=" + window.location.hostname);

    $("#buttonPlay").click(function (event) {
        var endPoint = toAbsoluteUrl("Competition");
        $.ajax({
            type: "POST",
            url: endPoint,
            dataType: "json",
            data: {},
            success: function (data) {
                markRoundStarted();
            },
            error: function (jqXHR) {
                ajaxError(jqXHR.status, jqXHR.statusText, endPoint);
            }
        });
    });

    $("#botSelector").change(function (event) {
        var bot = $("#botSelector :selected").val();
        var endPoint = toAbsoluteUrl("Bots/Choose/" + bot);
        $.ajax({
            type: "POST",
            url: endPoint,
            dataType: "json",
            data: {},
            success: function (data) {
                $("#botSelector option:[value='*']").remove();
            },
            error: function (jqXHR) {
                ajaxError(jqXHR.status, jqXHR.statusText, endPoint);
            }
        });
    });

    // initialize modal dialog
    $("#gameLogOverlay").dialog({ autoOpen: false, height: 450, width: 453, modal: true, resizable: false,
        create: function (event, ui) {
            $(".teamDropDown").bind({ change: getGameLogTextAjax, keyup: getGameLogTextAjax });
        }
    });

    // initialize modal dialog
    if (typeof botErrorString != "undefined") {
        $("#errorOverlay").dialog({ autoOpen: true, height: 450, width: 453, modal: true, resizable: false,
            create: function (event, ui) {
                $("#errorList").html(botErrorString);
            }
        });
    }
    else {
        $("#errorOverlay").hide();
    }
});