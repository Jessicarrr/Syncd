var containerDivId = "cvpl-videoPlayerContainer";
var playerDivId = "cvpl-videoPlayerOverlay";
var volumeSliderId = "cvpl-volumeSlider";
var volumeButtonHoverId = "cvpl-customVolumeButton";
var bottomButtonDivId = "cvpl-bottomButtonDiv"
var playerAdded = false;

var playButton;
var volumeHoverButton;
var volumeSlider;
var volumeHoverSlider;
var bottomButtonDiv;

var toggleVideoPlayingCallback;
var volumeChangeCallback;

function setVolumeChangeCallback(newCallback) {
    if (!isFunction(newCallback)) {
        throw "First parameter must be a function";
    }

    volumeChangeCallback = newCallback;
}

function setToggleVideoPlayingCallback(paramTogglePlayingCallback) {
    if (!isFunction(paramTogglePlayingCallback)) {
        throw "First parameter must be a function";
    }

    toggleVideoPlayingCallback = paramTogglePlayingCallback;
    console.log("setPlayVideoCallback - video callback set to " + paramTogglePlayingCallback);
}

function isFunction(object) {
    if (typeof object === 'function') {
        return true;
    }
    return false;
}

function createPlayerOverObject(object) {
    console.log("createPlayerOverObject - Trying to create custom video player");

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
    
    newContainerDiv.setAttribute("id", containerDivId);
    newPlayerDiv.setAttribute("id", playerDivId);

    objectParent.replaceChild(newContainerDiv, object);
    newContainerDiv.appendChild(object);
    newContainerDiv.appendChild(newPlayerDiv);

    createHTML(newPlayerDiv);
    setupListeners();

    console.log("createPlayerOverObject - Finished creating custom video player")
}

function setupListeners() {
    
}

function createHTML(playerDiv) {
    createButtonDiv(playerDiv);
    createButtons(playerDiv);
}

function createButtonDiv(playerDiv) {
    bottomButtonDiv = document.createElement("div");
    bottomButtonDiv.setAttribute("id", bottomButtonDivId);
    playerDiv.appendChild(bottomButtonDiv);
}

function createButtons(playerDiv) {
    createPlayButton();
    createVolumeSlider();
}

function createPlayButton() {
    playButton = document.createElement("button");
    playButton.setAttribute("id", "cvpl-customPlayButton");
    playButton.innerHTML = "P";

    playButton.onclick = function () {
        if (isFunction(toggleVideoPlayingCallback)) {
            toggleVideoPlayingCallback();
        }
    }
    

    bottomButtonDiv.appendChild(playButton);
}



function createVolumeSlider() {
    var mouseInVolumeSlider = false;
    var mouseInVolumeButton = false;

    volumeSlider = document.createElement("input");
    volumeSlider.setAttribute("id", volumeSliderId);
    volumeSlider.setAttribute("type", "range");
    volumeSlider.setAttribute("min", "0");
    volumeSlider.setAttribute("max", "100");
    volumeSlider.setAttribute("value", "50");
    volumeSlider.style.visibility = "hidden";
    

    volumeHoverButton = document.createElement("button");
    volumeHoverButton.setAttribute("id", volumeButtonHoverId);
    volumeHoverButton.innerHTML = "V";

    volumeHoverButton.addEventListener("mouseenter", function (event) {
        mouseInVolumeButton = true;

        volumeSlider.style.visibility = "visible";
    });

    volumeHoverButton.addEventListener("mouseleave", function (event) {
        mouseInVolumeButton = false;

        if (!mouseInVolumeSlider && !mouseInVolumeButton) {
            volumeSlider.style.visibility = "hidden";
        }
    });

    volumeSlider.addEventListener("mouseenter", function (event) {
        mouseInVolumeSlider = true;
        volumeSlider.style.visibility = "visible";
    });

    volumeSlider.addEventListener("mouseleave", function (event) {
        mouseInVolumeSlider = false;

        if (!mouseInVolumeButton && !mouseInVolumeSlider) {
            volumeSlider.style.visibility = "hidden";
        }
    });

    volumeSlider.oninput = function () {
        if (isFunction(volumeChangeCallback)) {
            volumeChangeCallback();
        }
    }

    bottomButtonDiv.appendChild(volumeHoverButton);
    bottomButtonDiv.appendChild(volumeSlider);
}

function isHTMLElement(object) {
    return object instanceof Element || object instanceof HTMLDocument;
}