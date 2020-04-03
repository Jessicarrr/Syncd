var userId = -1;

window.onbeforeunload = function (event) {
    if (userId != -1) {
        sendLeaveRequest();
    }
}

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

function onJoinSuccess(response) {
    userId = response["userId"];
    var newUserList = response["userList"];

    var newVideo = response["currentYoutubeVideoId"];
    var newVideoState = response["currentVideoState"];
    var newVideoTimeSeconds = response["videoTimeSeconds"];

    setVideoAndState(newVideo, newVideoState, newVideoTimeSeconds);

    removeUsers();

    for (var key in newUserList) {
        var user = newUserList[key];

        var currentUserId = user["id"];
        var currentUserName = user["name"];
        addUser(currentUserId, currentUserName);
    }
    tick();
}

function onJoinError(response) {
    alert("error: " + response.statusText);
}

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
        )/*,
        success: onJoinSuccess,
        error: onJoinError*/
    });
}

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

function sendUpdateRequest() {
    var videoTime = getVideoTime();

    console.log("Sending roomId " + roomId + " and userId " + userId + " youtube : " + currentVideoId + ", state: " + stateNumber + ", videoTimeSeconds: " + videoTime);

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

function sendTimeUpdate() {
    var videoTime = getVideoTime();

    console.log("Sending roomId " + roomId + " and userId " + userId + " youtube : " + currentVideoId + ", state: " + stateNumber + ", videoTimeSeconds: " + videoTime);

    $.ajax({
        url: '/room/TimeUpdate',
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(
            {
                roomId: roomId,
                userId: userId,
                videoTimeSeconds: videoTime
            }
        )
    });
}

function onUpdateSuccess(response) {
    var newUserList = response["userList"];
    var newVideo = response["currentYoutubeVideoId"];
    var newVideoState = response["currentVideoState"];
    var newVideoTimeSeconds = response["videoTimeSeconds"];

    console.log("Received from server video " + newVideo + " with state " + newVideoState + " and time " + newVideoTimeSeconds);

    setVideoAndState(newVideo, newVideoState, newVideoTimeSeconds);

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
}

function onUpdateError(response) {
    console.log("Update failure.");
}