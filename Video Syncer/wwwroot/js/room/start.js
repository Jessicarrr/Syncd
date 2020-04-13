$(window).on('load', function () {
    createNewName();
    document.getElementById("addButton").addEventListener("click", addVideoFromSearchBar);

    startLoadingYoutubePlayer();
    var playerHTMLObject = document.getElementById("player");
    createPlayerOverObject(playerHTMLObject, youtubeWidth, youtubeHeight);

    setupHidePlayerCheckbox();
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

    if (!isNaN(time)) {
        document.getElementById("videoTime").innerHTML = formatVideoTime(time);
    }
    else {
        document.getElementById("videoTime").innerHTML = "N/A";

    }

    var currentTime = new Date().getTime();
    var timeSinceLastNetworkUpdate = currentTime - lastNetworkSend;

    if (timeSinceLastNetworkUpdate > networkSendPeriod) {
        sendUpdateRequest();
        lastNetworkSend = new Date().getTime();
    }
    
}