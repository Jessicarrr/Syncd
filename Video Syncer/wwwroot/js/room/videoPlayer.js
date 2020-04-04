﻿var stateText = "[Video State]"; // the current video state (paused/playing/etc) in a readable format for UI purposes.
var stateNumber = -1; // the current video state (paused/playing/etc)

// 1. This code loads the IFrame Player API code asynchronously.
var tag = document.createElement('script');

tag.src = "https://www.youtube.com/iframe_api";
var firstScriptTag = document.getElementsByTagName('script')[0];
firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);

// 2. This function creates an <iframe> (and YouTube player)
//    after the API code downloads.
var player;
var currentVideoId;

function onYouTubeIframeAPIReady() {
    player = new YT.Player('player', {
        width: '850',
        height: '478',
        videoId: "XxuL-Y80sSM",
        events: {
            'onReady': onPlayerReady,
            'onStateChange': onPlayerStateChange
        }
    });
}

var videoTimeOnLastCheck;
var lastCheckUnixTime;
var hasNetworkChangedTime = false;

function hasTimeChanged() {

    if (videoTimeOnLastCheck == null || lastCheckUnixTime == null) {
        videoTimeOnLastCheck = getVideoTime() || 0;
        lastCheckUnixTime = new Date().getTime();
        return false;
    }
    else {
        if (hasNetworkChangedTime) {
            console.log("hasTimeChanged() - The network has changed the time.")
            hasNetworkChangedTime = false;
            videoTimeOnLastCheck = getVideoTime();
            lastCheckUnixTime = new Date().getTime();
            return false;
        }

        var currentTime = new Date().getTime();
        var timeSinceLastCheck = currentTime - lastCheckUnixTime;
        var timeSinceLastCheckSeconds = timeSinceLastCheck / 1000;
        var expectedVideoTime;
        
        var gracePeriod = 0.5;

        if (stateNumber == 1) {
            expectedVideoTime = videoTimeOnLastCheck + timeSinceLastCheckSeconds;
            //console.log("hasTimeChanged() - expected video time (" + expectedVideoTime + ") = " + videoTimeOnLastCheck + " + " + timeSinceLastCheckSeconds);
        }
        else {
            expectedVideoTime = videoTimeOnLastCheck;
        }

        // done with checking video time on last check. set it again.
        videoTimeOnLastCheck = getVideoTime();

        var videoTimeDifference = expectedVideoTime - getVideoTime();

        if (videoTimeDifference > gracePeriod || videoTimeDifference < (gracePeriod * -1)) {
            // the video time unexpectedly changed
            console.log("hasTimeChanged() - Time changed! Expected time : " + expectedVideoTime + ", actual time : " + getVideoTime() + ", difference : " + videoTimeDifference);
            lastCheckUnixTime = new Date().getTime();
            return true;
        }
        else {
            //console.log("hasTimeChanged() - Video time has not changed. Expected time : " + expectedVideoTime + ", actual time : " + getVideoTime() + ", difference : " + videoTimeDifference);
            lastCheckUnixTime = new Date().getTime();
            return false;
        }

        
    }
}

// 4. The API will call this function when the video player is ready.
function onPlayerReady(event) {
    sendJoinRequest();
}

// 5. The API calls this function when the player's state changes.
//    The function indicates that when playing a video (state=1),
//    the player should play for six seconds and then stop.

function onPlayerStateChange(event) {
    stateNumber = event.data;

    stateText = stateIntToString(stateNumber);

    switch (event.data) {
        case -1: //unstarted
            //sendVideoUnstartedRequest();
            break;
        case 0: //ended
            //sendVideoEndedRequest();
            break;
        case 1: //playing
            sendPlayRequest();
            console.log("onPlayerStateChange - Sending request to play");
            break;
        case 2: //paused
            sendPauseRequest();
            console.log("onPlayerStateChange - Sending request to pause");
            break;
        case 3: //buffering
            sendBufferingRequest();
            console.log("onPlayerStateChange - Sending request to buffer");
            break;
        case 5: //"video cued"
            
            break;

    }

    document.getElementById("videoState").innerHTML = stateText;
}

function stateIntToString(stateNumber) {
    switch (stateNumber) {
        case -1: //unstarted
            return "Unstarted";

            break;
        case 0: //ended
            return "Ended";
            break;
        case 1: //playing
            return "Playing";
            break;
        case 2: //paused
            return "Paused";
            break;
        case 3: //buffering
            return "Buffering";
            break;
        case 5: //"video cued"
            return "\"Video cued\"(sic)";
            break;

    }
}

function formatVideoTime(s) {
    var roundedSeconds = Math.floor(s);
    var date = new Date(null);
    date.setSeconds(roundedSeconds);

    var videoTime = date.toUTCString().match(/(\d\d:\d\d:\d\d)/)[0];

    if (roundedSeconds < 3600) { // lower than one hour
        // remove the hour area
        videoTime = videoTime.match(/(\d\d:\d\d$)/)[0];
    }

    return videoTime;
}

function getVideoTime() {
    return player.getCurrentTime() || 0.0;
}

function stopVideo() {
    player.stopVideo();
}

function changeVideo(paramVideoId, seconds = 0) {
    player.loadVideoById({
        videoId: paramVideoId,
        startSeconds: seconds
    });
    
    currentVideoId = paramVideoId;
    sendVideoChangeRequest(paramVideoId);
}
function pauseVideo() {
    player.pauseVideo();
}

function unpauseVideo() {
    player.playVideo();
}

function setVideoAndState(newVideo, newVideoState, newVideoTimeSeconds) {
    // variables for logging and logic
    var videoChanged = false;
    var stateChanged = false;
    var timeChanged = false;
    var logString = "setVideoAndState - ";


    // set the video
    if (newVideo != currentVideoId) {
        videoChanged = true;
        logString += "Network changed video to \"" + newVideo + "\".";
        changeVideo(newVideo, newVideoTimeSeconds);
        unpauseVideo();
    }

    // set the state of the video
    if (stateNumber != newVideoState) {
        stateChanged = true;
        logString += " Network changed state to " + stateIntToString(newVideoState) + "(" + newVideoState + ") from " + stateIntToString(stateNumber) + "(" + stateNumber + ").";

        stateNumber = newVideoState;
        stateText = stateIntToString(newVideoState);

        switch (newVideoState) {
            case -1: //unstarted
            case 0: //ended
                stopVideo();
                break;
            case 1: //playing
                unpauseVideo();
                break;
            case 2: //paused
                pauseVideo();
                break;
            case 3: //buffering
                //pauseVideo();
                break;
            case 5: //"video cued"
                break;

        }
    }

    // set the time of the video
    var gracePeriod = 1; // 1 second
    var timeDifference = newVideoTimeSeconds - getVideoTime();

    if (!videoChanged) {
        if (timeDifference > gracePeriod || timeDifference < (gracePeriod * -1)) {
            timeChanged = true;
            logString += " Changed time because video time is " + getVideoTime() + " but new time is " + newVideoTimeSeconds + " (time difference is " + timeDifference + ").";
            hasNetworkChangedTime = true;
            player.seekTo(newVideoTimeSeconds);
            
        }
    }

    if (videoChanged || stateChanged || timeChanged) {
        console.log(logString);
    }
}