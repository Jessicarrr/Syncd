var socket;
var userId = -1;
var myRights = 0;

var requestTypeProperty = "requestType";
var updateTypeProperty = "updateType";

var reconnectionAttempts = 0;
var maxReconnectionAttempts = 50;
var reconnectionAttemptInterval = 5000; // 5 seconds

var shouldSocketBeClosed = false;

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
        "VideoUpdate": 3,
        "RedirectToPage": 4,
        "AdminLogMessage": 5
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

        
        hideDisconnectError();
        reconnectionAttempts = 0;
        changeDisplayedDisconnectAttemptsNumber(0);
        sendJoinRequest();
        //sendVideoStateChangeRequest(1);

        setTimeout(hideInitialLoadingScreen, 1000);
    };

    socket.onclose = function (event) {
        console.log("socket.onclose: " + event.data);
        
        if (!shouldSocketBeClosed) {
            showDisconnectError();

            if (reconnectionAttempts >= 50) {
                displayDisconnectErrorNoMoreAttempts();
            }
            else {
                tryReconnect();
            }
            
        }
    };

    socket.onerror = function (event) {
        console.log("socket.onerror: " + event.data);
    };

    socket.onmessage = function (event) {
        try {
            var obj = JSON.parse(event.data);

            //console.log("json message: \"" + event.data + "\"");

            if (obj.hasOwnProperty(requestTypeProperty)) {
                handleRequestResponse(obj);
            }
            else if (obj.hasOwnProperty(updateTypeProperty)) {
                handleServerUpdate(obj);
            }
        }
        catch (e) {
            //console.log("non-json message: \"" + event.data + "\"");
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

function tryReconnect() {
    socket = null;

    var connectionUrl = "wss://" + location.hostname + ":" + location.port + "/Room/ConnectToWebSocket";
    console.log("Connecting to " + connectionUrl)
    socket = new WebSocket(connectionUrl);

    setupSocketEvents();

    reconnectionAttempts++;
    changeDisplayedDisconnectAttemptsNumber(reconnectionAttempts);

    /*if (socket.readyState !== socket.OPEN) {
        console.log("Socket was not successfully opened. ready state = " + socket.readyState);

        if (reconnectionAttempts <= maxReconnectionAttempts) {
            setTimeout(tryReconnect, reconnectionAttemptInterval);
            reconnectionAttempts += 1;
            changeDisplayedDisconnectAttemptsNumber(reconnectionAttempts)
        }
        else {
            console.log("Socket was successfully opened!! ready state = " + socket.readyState);
            displayDisconnectErrorNoMoreAttempts();
            setupSocketEvents();
        }
    }*/
}

function disconnect() {
    sendLeaveRequest();
    shouldSocketBeClosed = true;
    socket.close();
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
            // nothing is required
            break;
        case RequestType.ChangeName:
            handleUserListRequestResponse(obj);
            break;
        case RequestType.Kick:
            break;
        case RequestType.Ban:
            break;
        case RequestType.MakeAdmin:
            handleUserListRequestResponse(obj);
            break;
        case RequestType.RearrangePlaylist:
            handlePlaylistUpdate(obj);
            break;
        case RequestType.PlayPlaylistVideo:
            handleVideoStateChange(obj);
            break;
        case RequestType.PlayVideo:
            break;
        case RequestType.RemoveFromPlaylist:
            handlePlaylistUpdate(obj);
            break;
        case RequestType.AddToPlaylist:
            handlePlaylistUpdate(obj);
            break;
        case RequestType.TimeChange:
            handleVideoStateChange(obj);
            break;
    }
}

function handleServerUpdate(obj) {
    switch (obj[updateTypeProperty]) {
        case UpdateType.UserListUpdate:
            handleUserListUpdate(obj);
            break;
        case UpdateType.PlaylistUpdate:
            handlePlaylistUpdate(obj);
            break;
        case UpdateType.VideoUpdate:
            handleVideoStateChange(obj);
            break;
        case UpdateType.RedirectToPage:
            var page = obj["payload"];
            window.location.replace(page);
            break;
        case UpdateType.AdminLogMessage:
            handleAdminLogMessage(obj);
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
    var playlist = payload["playlist"];

    var newVideo = payload["currentYoutubeVideoId"]; // the YouTube video the room says we should play
    var newVideoTitle = payload["currentYoutubeVideoTitle"]; // the video title
    var newVideoState = payload["currentVideoState"]; // the state of the YouTube video. Paused/playing/ended/etc
    var newVideoTimeSeconds = payload["videoTimeSeconds"]; // the time in seconds the video should be at.

    console.log("Join Request Response - Received video " + newVideo + ", with state " + stateIntToString(newVideoState) + ", at time " + newVideoTimeSeconds);

    // set up the YouTube player with the data from the server.
    serverSetVideoAndState(newVideo, newVideoState, newVideoTimeSeconds);
    setVideoTitle(newVideoTitle);

    if (playlist != null) {
        removeAllPlaylistVideos();
        addPlaylistItems(playlist);
    }

    /*
     * For loop to populate the user list with all users in the room
     * (including yourself).
     */
    removeUsers();
    for (var key in newUserList) {
        
        var user = newUserList[key];

        var currentUserId = user["id"];
        var currentUserName = user["name"];
        var currentUserRights = user["rights"];

        if (currentUserId == userId && currentUserRights != myRights) {
            console.log("changed rights");
            myRights = currentUserRights;
        }

        addUser(currentUserId, currentUserName);
        updateUIForUser(currentUserId, currentUserRights);
    }
}

function handleUserListRequestResponse(obj) {
    var payload = obj["payload"];
    var userList = payload["userList"];

    if (userList !== null) {
        console.log("User list in user update is " + userList);
        removeUsers();

        for (var key in userList) {
            var user = userList[key];
            var currentUserId = user["id"];
            var currentUserRights = user["rights"];

            if (currentUserId == userId) {
                if ((currentUserRights == 1 || currentUserRights == 0) && myRights != currentUserRights) {
                    myRights = currentUserRights;
                }
                break;
            }
        }

        for (var key in userList) {
            var user = userList[key];

            var currentUserId = user["id"];
            var currentUserName = user["name"];
            var currentUserRights = user["rights"];

            if (currentUserId == userId && currentUserRights != myRights) {
                console.log("changed rights");
                myRights = currentUserRights;
            }

            addUser(currentUserId, currentUserName);
            updateUIForUser(currentUserId, currentUserRights);

            //console.log("Updated user" + currentUserName + "(" + currentUserId + ") with " + currentUserState + " and " + currentUserVideoTime);
        }
    }
    else {
        console.log("User list in user update was " + userList);
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
            var currentUserRights = user["rights"];

            if (currentUserId == userId) {
                if ((currentUserRights == 1 || currentUserRights == 0) && myRights != currentUserRights) {
                    myRights = currentUserRights;
                }
                break;
            }
        }

        for (var key in userList) {
            var user = userList[key];

            var currentUserId = user["id"];
            var currentUserName = user["name"];
            var currentUserRights = user["rights"];

            addUser(currentUserId, currentUserName);
            updateUIForUser(currentUserId, currentUserRights);

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

function sendVideoTimeChangeRequest(newTime) {
    var messageToSend = JSON.stringify({
        requestType: RequestType.TimeChange,
        roomId: roomId,
        userId: userId,
        videoTimeSeconds: newTime
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

function sendChangeNameRequest(newName) {
    messageToSend = JSON.stringify({
        requestType: RequestType.ChangeName,
        userId: userId,
        roomId: roomId,
        newName: newName
    });
    send(messageToSend);
}

function sendAddToPlaylistRequest(videoIdParam) {
    messageToSend = JSON.stringify({
        requestType: RequestType.AddToPlaylist,
        userId: userId,
        roomId: roomId,
        youtubeVideoId: videoIdParam
    });
    send(messageToSend);
}

function handlePlaylistUpdate(obj) {
    var payload = obj["payload"];
    var playlist = payload["playlist"];

    if (playlist != null) {
        removeAllPlaylistVideos();
        addPlaylistItems(playlist);
    }
    else {
        console.log("playlist is null");
    }
}

function handleAdminLogMessage(obj) {
    var payload = obj["payload"];
    var user = payload["user"];
    var actionMessage = payload["actionMessage"];

    if (actionMessage != null) {
        addNewAdminLogMessage(user, actionMessage);
    }
}

function sendDeletePlaylistItemRequest(playlistItemId) {
    messageToSend = JSON.stringify({
        requestType: RequestType.RemoveFromPlaylist,
        roomId: roomId,
        userId: userId,
        playlistItemId: playlistItemId
    });
    send(messageToSend);
}

function sendPlayPlaylistItemRequest(playlistItemId) {
    messageToSend = JSON.stringify({
        requestType: RequestType.PlayPlaylistVideo,
        roomId: roomId,
        userId: userId,
        playlistItemId: playlistItemId
    });
    send(messageToSend);

}
function sendKickRequest(recipientId) {
    if (myRights == 0) {
        return;
    }

    messageToSend = JSON.stringify({
        requestType: RequestType.Kick,
        userId: userId,
        roomId: roomId,
        userIdToKick: recipientId
    });

    send(messageToSend);
}

function sendBanRequest(recipientId) {
    if (myRights == 0) {
        return;
    }

    messageToSend = JSON.stringify({
        requestType: RequestType.Ban,
        userId: userId,
        roomId: roomId,
        userIdToBan: recipientId
    });

    send(messageToSend);
}

function sendMakeAdminRequest(recipientId) {
    if (myRights == 0) {
        return;
    }

    messageToSend = JSON.stringify({
        requestType: RequestType.MakeAdmin,
        userId: userId,
        roomId: roomId,
        userIdToMakeAdmin: recipientId
    });

    send(messageToSend);
}

function sendRearrangePlaylistRequest(onTopId, onBottomId) {
    messageToSend = JSON.stringify({
        requestType: RequestType.RearrangePlaylist,
        userId: userId,
        roomId: roomId,
        onTopId: onTopId,
        onBottomId: onBottomId
    });
    send(messageToSend);
}

function send(str) {
    //console.log("sending: " + str);
    if (socket.readyState === socket.OPEN) {

        socket.send(str);
    }
    else {
        console.log("Could not send because socket is not open");
    }
}