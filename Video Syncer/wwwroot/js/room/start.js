$(window).on('load', function () {
    createNewName();
    document.getElementById("addButton").addEventListener("click", addVideoFromSearchBar);
    var playerHTMLObject = document.getElementById("player");
    createPlayerOverObject(playerHTMLObject);
    startLoadingYoutubePlayer();
    //sendJoinRequest();
    
});

var tickMs = 200; // how often to run function tick()
var networkSendPeriod = 1500; // how often function tick() sends updates to the server
var lastNetworkSend = 0; // last time in ms tick() sent updates to the server.

/**
 * Tick function that runs every tickMs milliseconds. 
 * Sends updates to the server every networkSendPeriod milliseconds.
 */
function tick() {
    setTimeout(tick, tickMs);
    var time = getVideoTime();

    if (!isNaN(time)) {
        document.getElementById("videoTime").innerHTML = formatVideoTime(time);
    }
    else {
        document.getElementById("videoTime").innerHTML = "N/A";

    }

    /*
     * Check if the YouTube player time changed due to user input.
     */
    if (hasTimeChanged()) {
        //console.log("");
        sendTimeUpdate();
    }

    var currentTime = new Date().getTime();
    var timeSinceLastNetworkUpdate = currentTime - lastNetworkSend;

    if (timeSinceLastNetworkUpdate > networkSendPeriod) {
        sendUpdateRequest();
        lastNetworkSend = new Date().getTime();
    }
    
}