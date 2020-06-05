var userId = -1;
var failedUpdatesInARow = 0;
var failedJoinRequests = 0; // represents how many times we tried to reconnect the disconnected user in a row

/**
 * An event handler for when the user leaves the page. This includes refreshing, or navigating
 * to another page. This does not include the browser unexpectedly closing.
 */
window.onbeforeunload = function (event) {
    if (userId != -1) {
        sendLeaveRequest();
    }
}

/**
 * AJAX function for asking the server to join the room.
 */
function sendJoinRequest() {
    var name = getUsername();
    

    $.ajax({
        url: '/room/Join',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                name: name,
                roomId: roomId
            }
        ),
        success: onJoinSuccess,
        error: onJoinError
    });


}

/**
 * Function executes when sendJoinRequest() is successful, meaning we successfully
 * joined the room and are ready to sync up with the server and what it thinks the
 * YouTube video and its attributes should be.
 */
function onJoinSuccess(response) {
    userId = response["userId"];
    var newUserList = response["userList"];

    var newVideo = response["currentYoutubeVideoId"]; // the YouTube video the room says we should play
    var newVideoTitle = response["currentYoutubeVideoTitle"]; // the video title
    var newVideoState = response["currentVideoState"]; // the state of the YouTube video. Paused/playing/ended/etc
    var newVideoTimeSeconds = response["videoTimeSeconds"]; // the time in seconds the video should be at.

    console.log("onJoinSuccess - Received video " + newVideo + ", with state " + stateIntToString(newVideoState) + ", at time " + newVideoTimeSeconds);

    // set up the YouTube player with the data from the server.
    serverSetVideoAndState(newVideo, newVideoState, newVideoTimeSeconds);
    setVideoTitle(newVideoTitle);

    /*
     * For loop to populate the user list with all users in the room
     * (including yourself).
     */
    for (var key in newUserList) {
        var user = newUserList[key];

        var currentUserId = user["id"];
        var currentUserName = user["name"];
        addUser(currentUserId, currentUserName);
    }
    failedUpdatesInARow = 0;
    failedJoinRequests = 0;
    console.log("successfully joined room");
    tick();
}

/**
 * Executes when there was an error attempting to join via sendJoinRequest()
 * @param {any} response
 */
function onJoinError(response) {
    failedJoinRequests++;
}

function sendChangeNameRequest(newName) {
    $.ajax({
        url: '/room/ChangeName',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                userId: userId,
                roomId: roomId,
                newName: newName
            }
        )/*,
        success: onJoinSuccess,
        error: onJoinError*/
    });
}

function sendRearrangePlaylistRequest(onTopId, onBottomId) {
    $.ajax({
        url: '/room/RearrangePlaylist',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                userId: userId,
                roomId: roomId,
                onTopId: onTopId,
                onBottomId: onBottomId
            }
        ),
        success: onRearrangeSuccess,
        error: onRearrangeError
    });
}

function onRearrangeError(response) {

}

function onRearrangeSuccess(response) {
    var success = response["success"];
    var playlist = response["newPlaylist"];

    if (success == true && playlist != null) {
        removeAllPlaylistVideos();
        compareAndAddPlaylistItems(playlist);
        updateTitlesAndAuthors(playlist);
    }
}

/**
 * Send a request to the server to play the video. This is normally executed
 * when the user presses play on the YouTube video player.
 */
function sendPlayRequest() {
    $.ajax({
        url: '/room/PlayVideo',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                userId: userId,
                roomId: roomId
            }
        )/*,
        success: onJoinSuccess,
        error: onJoinError*/
    });
}

/**
 * Send a request to the server to pause the video. This is normally executed
 * when the user presses pause on the YouTube video player.
 */
function sendPauseRequest() {
    $.ajax({
        url: '/room/PauseVideo',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                userId: userId,
                roomId: roomId
            }
        )/*,
        success: onJoinSuccess,
        error: onJoinError*/
    });
}

/**
 * Send information to the server telling it that the user is buffering.
 */
function sendBufferingRequest() {
    $.ajax({
        url: '/room/BufferVideo',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                userId: userId,
                roomId: roomId
            }
        )
    });
}

/**
 * Send information to the server telling it that the user video has ended.
 */
function sendVideoEndedRequest() {
    $.ajax({
        url: '/room/EndVideo',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                userId: userId,
                roomId: roomId
            }
        )/*,
        success: onJoinSuccess,
        error: onJoinError*/
    });
}

/**
 * Send a message to the server asking to change the currently playing video.
 * @param {any} videoIdParam The YouTube video ID. Usually after the "v=" part of a youtube video URL.
 */
function sendAddToPlaylistRequest(videoIdParam) {
    $.ajax({
        url: '/room/AddToPlaylist',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                userId: userId,
                roomId: roomId,
                youtubeVideoId: videoIdParam
            }
        )
    });
}

/**
 * Send a message to the server asking to change the currently playing video.
 * @param {any} videoIdParam The YouTube video ID. Usually after the "v=" part of a youtube video URL.
 */
function sendVideoChangeRequest(videoIdParam) {
    $.ajax({
        url: '/room/ChangeVideo',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                userId: userId,
                roomId: roomId,
                youtubeVideoId : videoIdParam
            }
        )
    });
}

/**
 * Send information to the server saying the video has not started yet.
 */
function sendVideoUnstartedRequest() {
    $.ajax({
        url: '/room/UnstartedVideo',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                userId: userId,
                roomId: roomId
            }
        )/*,
        success: onJoinSuccess,
        error: onJoinError*/
    });
}

/**
 * Send a request to the server to leave the room. This is usually done when the user
 * navigates away from the page.
 */
function sendLeaveRequest() {
    $.ajax({
        url: '/room/Leave',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                userId: userId,
                roomId: roomId
            }
        )/*,
        success: onLeaveSuccess,
        error: onLeaveError*/
    });
}

function onLeaveSuccess() {

}

function onLeaveError() {

}

/**
 * Send information to the server about the user's Youtube player.
 */
function sendUpdateRequest() {
    var videoTime = getVideoTime();
    //console.log("sendUpdateRequest - Sending roomId " + roomId + " and userId " + userId + " youtube : " + currentVideoId + ", state: " + stateNumber + ", videoTimeSeconds: " + videoTime);

    $.ajax({
        url: '/room/Update',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                roomId: roomId,
                userId: userId,
                videoTimeSeconds: videoTime,
                videoState: stateNumber,
                currentYoutubeVideoId: currentVideoId
            }
        ),
        success: onUpdateSuccess,
        error: onUpdateError
    });
}

/**
 * Update the server about what time the user is at in their YouTube video.
 */
function sendTimeUpdate(newVideoTime) {
    var timeTest = getVideoTime();

    console.log("Test: timeTest = " + timeTest + ", newVideoTime = " + newVideoTime + ". timeTest type is " + typeof(timeTest) + " where as type of newVideoTime is " + typeof(newVideoTime));

    console.log("sendTimeUpdate - Sending roomId " + roomId + " and userId " + userId + " youtube : " + currentVideoId + ", state: " + stateNumber + ", newVideoTime: " + newVideoTime);

    $.ajax({
        url: '/room/TimeUpdate',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                roomId: roomId,
                userId: userId,
                videoTimeSeconds: newVideoTime
            }
        )
    });
}

/**
 * Update the server about what time the user is at in their YouTube video.
 */
function sendPlayPlaylistItemRequest(playlistItemId) {
    $.ajax({
        url: '/room/PlayPlaylistVideo',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                roomId: roomId,
                userId: userId,
                playlistItemId: playlistItemId
            }
        )
    });
}

/**
 * Send the server a request to delete a playlist item
 */
function sendDeletePlaylistItemRequest(playlistItemId) {
    $.ajax({
        url: '/room/RemoveFromPlaylist',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                roomId: roomId,
                userId: userId,
                playlistItemId: playlistItemId
            }
        )
    });
}

/**
 * Fetched a response to sendUpdateRequest() - the server sent back information about where our
 * YouTube video player should be, including the video id, the state of the video, and the video
 * time.
 * @param {any} response Data sent from the server.
 */
function onUpdateSuccess(response) {
    var newUserList = response["userList"];
    var newVideo = response["currentYoutubeVideoId"];
    var newVideoTitle = response["currentYoutubeVideoTitle"]; // the video title
    var newVideoState = response["currentVideoState"];
    var newVideoTimeSeconds = response["videoTimeSeconds"];
    var playlist = response["playlist"];

    //console.log("onUpdateSuccess - Received from server video " + newVideo + " with state " + newVideoState + " and time " + newVideoTimeSeconds);

    serverSetVideoAndState(newVideo, newVideoState, newVideoTimeSeconds);
    setVideoTitle(newVideoTitle);

    if (playlist != null) {
        compareAndRemovePlaylistItems(playlist);
        compareAndAddPlaylistItems(playlist);
        updateTitlesAndAuthors(playlist);
        fixPlaylistArrangement(playlist);


        /*removeAllPlaylistVideos();

        for (var key in playlist) {
            var videoObject = playlist[key];

            var videoObjectTitle = videoObject["title"];
            var videoObjectAuthor = videoObject["author"];
            var videoObjectVideoId = videoObject["videoId"];
            var videoObjectUniqueId = videoObject["id"];

            addDataToPlaylist(videoObjectUniqueId, videoObjectTitle, videoObjectVideoId, videoObjectAuthor);
        }*/
    }
    else {
        console.log("playlist is null");
    }

    if (newUserList != null) {
        removeUsers();

        for (var key in newUserList) {
            var user = newUserList[key];

            
            var currentUserId = user["id"];
            var currentUserName = user["name"];
            var currentUserState = user["videoState"];
            var currentUserVideoTime = user["videoTimeSeconds"];
            addUser(currentUserId, currentUserName);
            updateUIForUser(currentUserId, stateIntToString(currentUserState), formatVideoTime(currentUserVideoTime));

            //console.log("Updated user" + currentUserName + "(" + currentUserId + ") with " + currentUserState + " and " + currentUserVideoTime);
        }
    }    
    failedUpdatesInARow = 0;
}

function onUpdateError(response) {
    failedUpdatesInARow += 1;
    console.log("Update failure.");
}