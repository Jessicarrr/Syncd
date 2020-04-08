var containerDivName = "videoPlayerContainer";
var playerDivName = "videoPlayerOverlay";
var playerAdded = false;

var playButton;
var playFunction;
var pauseFunction;

function setPlayAndPauseVideoCallback(paramPlayFunction, paramPauseFunction) {
    if (!isFunction(paramPlayFunction)) {
        throw "First parameter must be a function";
    }

    if (!isFunction(paramPauseFunction)) {
        throw "Second parameter must be a function";
    }

    var playBtn = document.getElementById("customPlayButton");

    if (playBtn == null) {
        throw "Cannot add play callback as the play button is null. Please load the video player before setting function callbacks.";
    }

    playFunction = paramPlayFunction;
    pauseFunction = paramPauseFunction;
    console.log("setPlayVideoCallback - video callback set to " + playFunction);
    playBtn.addEventListener("click", playFunction);
}

function isFunction(object) {
    if (typeof object === 'function') {
        return true;
    }
    return false;
}

function createPlayerOverObject(object) {
    console.log("createPlayerOverObject - Trying to create video player div");

    if (playerAdded) {
        return;
    }

    playerAdded = true;

    if (!isHTMLElement(object)) {
        throw "Cannot create video player over non HTML element. This object is not a HTML element: " + object;
    }

    var objectParent = object.parentNode;
    var newContainerDiv = document.createElement("div");
    var newPlayerDiv = document.createElement("div");
    

    newContainerDiv.setAttribute("id", containerDivName);
    newPlayerDiv.setAttribute("id", playerDivName);

    objectParent.replaceChild(newContainerDiv, object);
    newContainerDiv.appendChild(object);
    newContainerDiv.appendChild(newPlayerDiv);

    createButtons(newPlayerDiv);
    setupListeners();

    console.log("createPlayerOverObject - done")
}

function setupListeners() {
    
}

function createButtons(playerDiv) {
    playButton = document.createElement("button");
    playButton.setAttribute("id", "customPlayButton");
    playButton.innerHTML = "Play";

    playerDiv.appendChild(playButton);
}

function isHTMLElement(object) {
    return object instanceof Element || object instanceof HTMLDocument;
}