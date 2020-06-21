var socket;

const RequestType = Object.freeze(
    {
        "Join": 1,
        "ChangeVideoState": 2
    });

function setupNetworking() {
    connectToServer();
    sendJoinRequest();
    sendVideoStateChangeRequest(1);

}

function connectToServer() {
    var connectionUrl = "wss://" + location.hostname + ":" + location.port + "/Room/ConnectToWebSocket";
    console.log("Connecting to " + connectionUrl)
    socket = new WebSocket(connectionUrl);

    setupSocketEvents();
}

function setupSocketEvents() {
    socket.onopen = function (event) {
        console.log("socket.onopen");
    };

    socket.onclose = function (event) {
        console.log("socket.onclose");
    };

    socket.onerror = function (event) {
        console.log("socket.onerror");
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

function createJsonMessage(requestType, requestData) {
    var newObj = new Object();
    newObj.requestType = requestType;
    newObj.requestData = requestData;

    var jsonString = JSON.stringify(newObj);
    return jsonString;
}

function createDataFor(requestType) {
    var dataObj = new Object();

    switch (requestType) {
        case RequestType.Join:
            dataObj.name = name;
            dataObj.roomId = roomId;
            return dataObj;
        case RequestType.Play:
            dataObj.userId = userId;
            dataObj.roomId = roomId;
            return dataObj;
        default:
            console.log("Unknown request type sent to connectionManager.createDataFor(requestType)");
            break;
    }

    return null;
}