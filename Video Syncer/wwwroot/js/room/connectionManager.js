var socket;
var userId = -1;

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
        console.log("socket.onmessage");
        console.log("message: " + event.data);
        /*updateStateText();
        addMessageToLog("Server message: \"" + event.data + "\"");*/

        /*var obj = JSON.parse(event.data);
        var randomNumber = obj["randomNumber"];
        var message = obj["message"];
        var sessionId = obj["sessionId"];

        addMessageToLog("Parsed server message. Random number = " + randomNumber + ", message = \'" + message + "\', sessionId = " + sessionId);*/
    };
}

function sendJoinRequest() {
    var messageToSend = JSON.stringify(
        {
            requestType: RequestType.Join,
            name: name,
            roomId: roomId
        });
    console.log("Sending: " + messageToSend);
    socket.send(messageToSend);
}

function sendVideoStateChangeRequest(videoState) {
    var messageToSend = JSON.stringify(
        {
            requestType: RequestType.ChangeVideoState,
            state: videoState,
            userId: userId,
            roomId: roomId
        });
    console.log("sending: " + messageToSend);
    socket.send(messageToSend);
}