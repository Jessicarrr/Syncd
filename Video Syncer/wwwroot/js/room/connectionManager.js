﻿var socket;
var userId = -1;

var requestTypeProperty = "requestType";
var updateTypeProperty = "updateType";

const RequestType = Object.freeze(
    {
        "Join": 1,
        "ChangeVideoState": 2,
        "Leave": 3,
        "ChangeName": 4,
        "Kick": 5,
        "Ban": 6,
        "MakeAdmin": 7,
        "RearrangePlaylist": 8,
        "PlayPlaylistVideo": 9,
        "PlayVideo": 10,
        "RemoveFromPlaylist": 11,
        "AddToPlaylist": 12,
        "TimeChange": 13
    });

const UpdateType = Object.freeze(
    {
        "UserListUpdate": 1,
        "PlaylistUpdate": 2,
        "VideoUpdate": 3
    });

function setupNetworking() {
    connectToServer();
    

}

function connectToServer() {
    var connectionUrl = "wss://" + location.hostname + ":" + location.port + "/Room/ConnectToWebSocket";
    console.log("Connecting to " + connectionUrl)
    socket = new WebSocket(connectionUrl);

    setupSocketEvents();
}

function setupSocketEvents() {
    socket.onopen = function (event) {
        console.log("socket.onopen: " + event.data);
        sendJoinRequest();
        //sendVideoStateChangeRequest(1);
    };

    socket.onclose = function (event) {
        console.log("socket.onclose: " + event.data);
    };

    socket.onerror = function (event) {
        console.log("socket.onerror: " + event.data);
    };

    socket.onmessage = function (event) {
        console.log("message: " + event.data);
        var obj = JSON.parse(event.data);

        if (obj.hasOwnProperty(requestTypeProperty)) {
            handleRequestResponse(obj);
        }
        else if (obj.hasOwnProperty(updateTypeProperty)) {
            handleServerUpdate(obj);
        }
        
        

        /*updateStateText();
        addMessageToLog("Server message: \"" + event.data + "\"");*/

        /*var obj = JSON.parse(event.data);
        var randomNumber = obj["randomNumber"];
        var message = obj["message"];
        var sessionId = obj["sessionId"];

        addMessageToLog("Parsed server message. Random number = " + randomNumber + ", message = \'" + message + "\', sessionId = " + sessionId);*/
    };
}

function handleRequestResponse(obj) {
    switch (obj[requestTypeProperty]) {
        case RequestType.Join:
            handleJoinRequestResponse(obj);
            break;
        case RequestType.ChangeVideoState:
            handleVideoStateChange(obj);
            break;
        case RequestType.Leave:
            break;
        case RequestType.ChangeName:
            break;
        case RequestType.Kick:
            break;
        case RequestType.Ban:
            break;
        case RequestType.MakeAdmin:
            break;
        case RequestType.RearrangePlaylist:
            break;
        case RequestType.PlayPlaylistVideo:
            break;
        case RequestType.PlayVideo:
            break;
        case RequestType.RemoveFromPlaylist:
            break;
        case RequestType.AddToPlaylist:
            break;
        case RequestType.TimeChange:
            break;


    }
}

function handleServerUpdate(obj) {
    switch (obj[updateTypeProperty]) {
        case UpdateType.UserListUpdate:
            handleUserListUpdate(obj);
            break;
        case UpdateType.PlaylistUpdate:
            break;
        case UpdateType.VideoUpdate:
            handleVideoStateChange(obj);
            break;
    }
}

function sendJoinRequest() {
    var name = getUsername();

    var messageToSend = JSON.stringify({
        requestType: RequestType.Join,
        name: name,
        roomId: roomId
    });
    send(messageToSend);
}

function handleJoinRequestResponse(response) {
    var payload = response["payload"];

    userId = payload["userId"];
    myRights = payload["myRights"];
    var newUserList = payload["userList"];

    var newVideo = payload["currentYoutubeVideoId"]; // the YouTube video the room says we should play
    var newVideoTitle = payload["currentYoutubeVideoTitle"]; // the video title
    var newVideoState = payload["currentVideoState"]; // the state of the YouTube video. Paused/playing/ended/etc
    var newVideoTimeSeconds = payload["videoTimeSeconds"]; // the time in seconds the video should be at.

    console.log("Join Request Response - Received video " + newVideo + ", with state " + stateIntToString(newVideoState) + ", at time " + newVideoTimeSeconds);

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
        var currentUserState = user["videoState"];
        var currentUserVideoTime = user["videoTimeSeconds"];
        var currentUserRights = user["rights"];

        addUser(currentUserId, currentUserName);
        updateUIForUser(currentUserId, stateIntToString(currentUserState),
            formatVideoTime(currentUserVideoTime), currentUserRights);
    }
}

function handleUserListUpdate(obj) {
    var userList = obj["payload"];

    if (userList !== null) {
        console.log("User list in user update is " + userList);
        removeUsers();

        for (var key in userList) {
            var user = userList[key];

            var currentUserId = user["id"];
            var currentUserName = user["name"];
            var currentUserState = user["videoState"];
            var currentUserVideoTime = user["videoTimeSeconds"];
            var currentUserRights = user["rights"];

            addUser(currentUserId, currentUserName);
            updateUIForUser(currentUserId, stateIntToString(currentUserState),
                formatVideoTime(currentUserVideoTime), currentUserRights);

            //console.log("Updated user" + currentUserName + "(" + currentUserId + ") with " + currentUserState + " and " + currentUserVideoTime);
        }
    }
    else {
        console.log("User list in user update was " + userList);
    }

    
}

function sendLeaveRequest() {
    var messageToSend = JSON.stringify({
        requestType: RequestType.Leave,
        userId: userId,
        roomId: roomId
    });
    send(messageToSend);
}

function sendVideoStateChangeRequest(videoState) {
    var messageToSend = JSON.stringify({
        requestType: RequestType.ChangeVideoState,
        state: videoState,
        userId: userId,
        roomId: roomId
    });
    send(messageToSend);
}

function handleVideoStateChange(obj) {
    var videoStateData = obj["payload"];

    if (videoStateData == null) {
        return;
    }

    var newVideoState = videoStateData["currentVideoState"];
    var newTimeSeconds = videoStateData["videoTimeSeconds"];
    var newVideoId = videoStateData["currentYoutubeVideoId"];
    var newVideoTitle = videoStateData["currentYoutubeVideoTitle"];

    serverSetVideoAndState(newVideoId, newVideoState, newTimeSeconds);
    setVideoTitle(newVideoTitle);
}

function send(str) {
    console.log("sending: " + str);
    if (socket.readyState === socket.OPEN) {

        socket.send(str);
    }
    else {
        console.log("Could not send because socket is not open");
    }
}