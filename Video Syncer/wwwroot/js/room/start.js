$(window).on('load', function () {
    createNewName();
    document.getElementById("addButton").addEventListener("click", addVideoFromSearchBar);
    //sendJoinRequest();
    
});

var tickMs = 200;
var networkSendPeriod = 1500;
var lastNetworkSend = 0;

function tick() {
    setTimeout(tick, tickMs);
    var time = getVideoTime();

    if (!isNaN(time)) {
        document.getElementById("videoTime").innerHTML = formatVideoTime(time);
    }
    else {
        document.getElementById("videoTime").innerHTML = "N/A";

    }

    if (hasTimeChanged()) {
        console.log("WOW THE TIME CHANGED!!!!!!!!!");
        sendTimeUpdate();
    }

    var currentTime = new Date().getTime();
    var timeSinceLastNetworkUpdate = currentTime - lastNetworkSend;

    if (timeSinceLastNetworkUpdate > networkSendPeriod) {
        sendUpdateRequest();
        lastNetworkSend = new Date().getTime();
    }
    
}