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

function decodeHtml(html) {
    var txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
}

function setupHidePlayerCheckbox() {
    var checkbox = document.getElementById("hide-player-checkbox");

    checkbox.addEventListener('change', function () {
        if (this.checked) {
            console.log("hide");
            //makeButtonAreaInvisible();
            disablePlayer();
        }
        else {
            //makeButtonAreaVisible();
            enablePlayer();
        }
    });
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