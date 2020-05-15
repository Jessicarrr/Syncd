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

function isDisconnectWarningVisible() {
    var disconnectWarning = document.getElementById("alert-disconnected");

    if (disconnectWarning.style.display === "none") {
        return false;
    }
    return true;
}

function showDisconnectWarning() {
    var disconnectWarning = document.getElementById("alert-disconnected");
    disconnectWarning.style.display = "block";
}

function hideDisconnectWarning() {
    var disconnectWarning = document.getElementById("alert-disconnected");
    disconnectWarning.style.display = "none";
}

function changeDisplayedDisconnectAttemptsNumber(number) {
    var disconnectWarning = document.getElementById("alert-disconnected");

    disconnectWarning.innerHTML = disconnectWarning.innerHTML.replace(/attempts: \d+/, ('attempts: ' + number));
}