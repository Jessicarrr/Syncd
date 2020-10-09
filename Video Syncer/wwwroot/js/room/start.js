$(window).on('load', function () {
    createNewName();
    document.getElementById("addButton").addEventListener("click", addVideoFromSearchBar);

    startLoadingYoutubePlayer();
    var playerHTMLObject = document.getElementById("player");
    console.log("Attempting to make custom video player...");
    createPlayerOverObject(playerHTMLObject, youtubeWidth, youtubeHeight);

    setupHidePlayerCheckbox();

    window.onclick = function (event) {
        closeDropdownsOnClick(event);
    }

    document.getElementById("username-box").addEventListener("focusout", function () {
        var newNameDefault = document.getElementById("username-box").value;
        sendChangeNameRequest(newNameDefault);
    });

    document.getElementById("username-box").addEventListener("keyup", function (e) {
        if (e.keyCode === 13) {
            var newNameDefault = document.getElementById("username-box").value;
            sendChangeNameRequest(newNameDefault);
        }
    }); 
});

var tickMs = 200; // how often to run function tick()
var networkSendPeriod = 1500; // how often function tick() sends updates to the server
var lastNetworkSend = 0; // last time in ms tick() sent updates to the server.

/**
 * Tick function that runs every tickMs milliseconds. 
 * Sends updates to the server every networkSendPeriod milliseconds.
 */
function tick() {
    var time = getVideoTime();

    setTimeout(tick, tickMs);

    var currentTime = new Date().getTime();
    var timeSinceLastNetworkUpdate = currentTime - lastNetworkSend;

    if (timeSinceLastNetworkUpdate > networkSendPeriod) {

        if (failedUpdatesInARow > 7) {
            if (!isDisconnectWarningVisible()) {
                showDisconnectWarning();
            }
            changeDisplayedDisconnectAttemptsNumber(failedJoinRequests);
            sendJoinRequest();
        }
        else {
            if (isDisconnectWarningVisible()) {
                hideDisconnectWarning();
            }
            sendUpdateRequest();
            
        }
        lastNetworkSend = new Date().getTime();
    }
    
}