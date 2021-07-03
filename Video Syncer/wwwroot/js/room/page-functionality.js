window.onbeforeunload = function (event) {
    if (userId != -1) {
        disconnect();
    }
}

function showInitialLoadingScreen() {
    var loadingScreen = document.getElementById("initial-loading-screen");

    loadingScreen.style.display = "block";
    document.documentElement.style.overflow = "hidden";
}

function hideInitialLoadingScreen() {
    var loadingScreen = document.getElementById("initial-loading-screen");

    loadingScreen.style.display = "none";
    document.documentElement.style.overflow = "auto";
}

function showUsageLogsButton() {
    var buttonArea = document.getElementById("usage-logs-activator-button");

    buttonArea.style.display = "flex";
}

function hideUsageLogsButton() {
    var buttonArea = document.getElementById("usage-logs-activator-button");

    buttonArea.style.display = "none";
}

function decodeHtml(html) {
    var txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
}

function setupHidePlayerButton() {
    var button = document.getElementById("disable-player-button");
    var icon = document.getElementById("disable-player-icon");

    button.onclick = function () {
        if (isPlayerDisabled()) {
            enablePlayer();
            icon.style.color = null;
            
        } else {
            disablePlayer();
            icon.style.color = "red";
        }
    };

    button.ontouchend = function () {
        if (isPlayerDisabled()) {
            enablePlayer();
            icon.style.color = null;
        } else {
            disablePlayer();
            icon.style.color = "red";
            
        }
    };
}

function setupUsageLogsButton() {
    var button = document.getElementById("usage-logs-activator-button");

    button.onclick = function () {
        showAdminLogs();
        openAdminLog();
    };

    button.ontouchend = function () {
        showAdminLogs();
        openAdminLog();
    };
}

function setupUsageLogsCloseButton() {
    var button = document.getElementById("admin-log-close-button");

    button.onclick = function () {
        hideAdminLogs();
    };

    button.ontouchend = function () {
        hideAdminLogs();
    };
}

function closeDropdowns() {
    var allDropdowns = document.getElementsByClassName("dropdown-content");

    for (var i = 0; i < allDropdowns.length; i++) {
        var currentDropdown = allDropdowns[i];

        if (currentDropdown.style.display === "block") {
            currentDropdown.style.display = "none";
        }
    }
}

function closeDropdownsOnClick(onclickEvent) {
    var classNameMatches = onclickEvent.target.className === "playlist-dropdown-button";
    var classNameMatches2 = onclickEvent.target.className === "dropdown-content";
    var isOptionsButton = onclickEvent.target.matches(".playlist-options-button");

    if (!isOptionsButton && !classNameMatches && !classNameMatches2) {
        closeDropdowns();
    }
}

function isDisconnectErrorVisible() {
    var disconnectError = document.getElementById("alert-disconnected");

    if (disconnectError.style.display === "none") {
        return false;
    }
    return true;
}

function showDisconnectError() {
    var disconnectError = document.getElementById("alert-disconnected");
    disconnectError.style.display = "block";
}

function hideDisconnectError() {
    var disconnectError = document.getElementById("alert-disconnected");
    disconnectError.style.display = "none";
}

function changeDisplayedDisconnectAttemptsNumber(number) {
    var disconnectError = document.getElementById("alert-disconnected");

    disconnectError.innerHTML = disconnectError.innerHTML.replace(/attempts: \d+/, ('attempts: ' + number));
}

function displayDisconnectErrorNoMoreAttempts() {
    var disconnectError = document.getElementById("alert-disconnected");

    disconnectError.innerHTML = "You were disconnected and we were unable to reconnect you. Please try refreshing the page.";
}

function setVideoTitle(title) {
    var currentVideoTitle = document.getElementById("video-title").innerHTML;

    if (currentVideoTitle === title ) {
        return;
    }
    document.getElementById("video-title").innerHTML = title;
}