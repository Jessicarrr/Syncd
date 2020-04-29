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