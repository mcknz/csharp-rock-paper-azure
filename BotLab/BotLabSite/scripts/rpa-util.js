function toAbsoluteUrl(serviceUrl) {
    return window.location.protocol + "//" + window.location.hostname + ((window.location.port == "") ? "" : ":" + window.location.port) + "/" + serviceUrl;
}

function ajaxError(status, text, request, customText) {
    alert("An Ajax request resulted in an error.\n\nURL: " + request + "\nError: [" + status + "] " + text + 
        ((customText != null) ? "\n\n" + customText : ""));
}