var stateText = "[Video State]"; // the current video state (paused/playing/etc) in a readable format for UI purposes.
var stateNumber = -1; // the current video state (paused/playing/etc)

var youtubeWidth = '850';
var youtubeHeight = '478';
function startLoadingYoutubePlayer() {
    // 1. This code loads the IFrame Player API code asynchronously.
    var tag = document.createElement('script');

    tag.src = "https://www.youtube.com/iframe_api";
    var firstScriptTag = document.getElementsByTagName('script')[0];
    firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
}

// 2. This function creates an <iframe> (and YouTube player)
//    after the API code downloads.
var player;
var currentVideoId;

function onYouTubeIframeAPIReady() {
    player = new YT.Player('player', {
        width: youtubeWidth,
        height: youtubeHeight,
        wmode: 'opaque',
        events: {
            'onReady': onPlayerReady,
            'onStateChange': onPlayerStateChange
        },
        playerVars: {
            'wmode': 'opaque',
            'disablekb': 1,
            'modestbranding': 1,
            'mute' : 1
            
        }
    });

    
}

function getVideoTime() {
    return player.getCurrentTime() || 0.0;
}

function stopVideo() {
    player.stopVideo();
}

function pauseVideo() {
    player.pauseVideo();
}

function unpauseVideo() {
    player.playVideo();
}

function setYoutubePlayerSize(width, height) {
    player.setSize(width, height);
}

function toggleVideoPlaying() {
    // if the video is playing somehow
    if (stateNumber == 1 || stateNumber == 3) {
        sendPauseRequest();
        console.log("toggleVideoPlaying - Sending request to pause");
        pauseVideo();
    }
    // if the video is paused somehow
    else if (stateNumber == 2 || stateNumber == -1 || stateNumber == 0 || stateNumber == 5) {
        sendPlayRequest();
        console.log("toggleVideoPlaying - Sending request to play");
        unpauseVideo();
    }
}

/**
 * Adjust volume of the youtube player.
 * @param {any} volume a number between 0 and 100 representing the volume.
 */
function adjustVolume(volume) {
    if (player.isMuted()) {
        player.unMute();
    }
    
    player.setVolume(volume);
}

function adjustTime(percentage) {
    var totalVideoLength = getVideoTotalDuration();

    var newVideoTime = parseFloat(((totalVideoLength * percentage) / 100).toFixed(2));
    sendTimeUpdate(newVideoTime);
    player.seekTo(newVideoTime);
}

function getVideoTotalDuration() {
    return player.getDuration();
}

// 4. The API will call this function when the video player is ready.
function onPlayerReady(event) {
    setToggleVideoPlayingCallback(toggleVideoPlaying);
    setVolumeChangeCallback(adjustVolume);
    setClickTimeSliderCallback(adjustTime);
    givePlayerAbilityToTrackTime(getVideoTime, getVideoTotalDuration);
    setChangeVideoSizeCallback(setYoutubePlayerSize);

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
            sendVideoEndedRequest();
            console.log("onPlayerStateChange - Sending request to end the video");
            break;
        case 1: //playing
            // now handled in toggleVideoPlaying()
            
            /*sendPlayRequest();
            console.log("onPlayerStateChange - Sending request to play");*/
            break;
        case 2: //paused
            // now handled in toggleVideoPlaying()

            /*sendPauseRequest();
            console.log("onPlayerStateChange - Sending request to pause");*/
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

function serverChangeVideo(paramVideoId, seconds = 0) {
    player.loadVideoById({
        videoId: paramVideoId,
        startSeconds: seconds
    });

    currentVideoId = paramVideoId;

    var ytUrl = buildYoutubeUrl(currentVideoId);
    callApiWithCallback(ytUrl, function (data) {
        document.getElementById("video-title").innerHTML = data.title;
    });
}

function changeVideo(paramVideoId, seconds = 0) {
    player.loadVideoById({
        videoId: paramVideoId,
        startSeconds: seconds
    });
    
    currentVideoId = paramVideoId;
    sendVideoChangeRequest(paramVideoId);

    var ytUrl = buildYoutubeUrl(currentVideoId);
    callApiWithCallback(ytUrl, function (data) {
        document.getElementById("video-title").innerHTML = data.title;
    });
}


/**
 * Sets the video playing to what the server thinks the video should be,
 * if the client is not already playing that video.
 * @param {any} newVideo
 * @param {any} newTimeSeconds
 */
function serverSetVideo(newVideo, newTimeSeconds) {
    if (newVideo != currentVideoId) {
        serverChangeVideo(newVideo, newTimeSeconds);
        return true;
    }
    return false;
}

/**
 * Sets the video state (playing, paused, etc) if the server's video state
 * is different from the client.
 * @param {any} newVideoState The video state according to the server.
 */
function serverSetVideoState(newVideoState) {

    if (stateNumber != newVideoState) {
        stateNumber = newVideoState;
        stateText = stateIntToString(newVideoState);

        switch (newVideoState) {
            case -1: //unstarted
            case 0: //ended
                //stopVideo();
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
        return true;
    }
    return false;
}

/**
 * Logic for setting the time of the video if the server sends a different
 * time to what the client currently has.
 * @param {any} newTimeSeconds The video time in seconds.
 */
function serverSetVideoTime(newTimeSeconds) {
    // set the time of the video
    var gracePeriod = 1; // 1 second
    var timeDifference = newTimeSeconds - getVideoTime();

    if (timeDifference > gracePeriod || timeDifference < (gracePeriod * -1)) {
        player.seekTo(newTimeSeconds);
        return true;
    }
    return false;
}

/**
 * Function is called when the server sends info about the video playing,
 * the state its in, and the time it should be at. This function compares the
 * server's data to what the client has. If the server has different video parameters
 * to the client, this function changes the client video to match the server.
 * @param {any} newVideo The YouTube video ID that the server sent.
 * @param {any} newVideoState The state of the video sent by the server (playing, paused, etc)
 * @param {any} newTimeSeconds The time of the video in seconds sent by the server.
 */
function serverSetVideoAndState(newVideo, newVideoState, newTimeSeconds) {
    // variables for logging and logic
    var videoChanged = false;
    var stateChanged = false;
    var timeChanged = false;
    var logString = "serverSetVideoAndState - ";
    var oldStateText = stateIntToString(stateNumber);
    var oldStateNumber = stateNumber;


    // set the video
    if (serverSetVideo(newVideo, newTimeSeconds)) {
        videoChanged = true;
        logString += "Network changed video to \"" + newVideo + "\" with time of " + newTimeSeconds + " seconds.";
    }

    if (!videoChanged) {
        if (serverSetVideoTime(newTimeSeconds)) {
            timeChanged = true;
            logString += " Changed time because video time is " + getVideoTime() + " but new time is " + newTimeSeconds + " (time difference is " + (newTimeSeconds - getVideoTime()) + ").";
            hasNetworkChangedTime = true;
        }
    }

    // set the state of the video
    if (serverSetVideoState(newVideoState)) {
        stateChanged = true;
        logString += " Network changed state to " + stateIntToString(newVideoState) + "(" + newVideoState + ") from " + oldStateText + "(" + oldStateNumber + ").";
    }

    if (videoChanged || stateChanged || timeChanged) {
        console.log(logString);
    }
}