var containerDivId = "cvpl-videoPlayerContainer";
var playerDivId = "cvpl-videoPlayerOverlay";
var volumeSliderId = "cvpl-volumeSlider";
var volumeButtonHoverId = "cvpl-customVolumeButton";
var bottomButtonDivId = "cvpl-bottomButtonDiv"
var timeSliderId = "cpvl-timeSlider";
var playerAdded = false;

var playButton;
var volumeHoverButton;
var volumeSlider;
var timeSlider;
var volumeHoverSlider;
var bottomButtonDiv;

var toggleVideoPlayingCallback;
var volumeChangeCallback;
var userClicksTimeSliderCallback;
var getCurrentVideoTimeCallback;
var getTotalVideoDurationCallback;

var shouldTrackTime = false;

function startTrackingTime() {
    if (!shouldTrackTime) {
        console.log("startTrackingTime() - shouldTrackTime == false. Stopping time tracking.")
        return;
    }

    if (!isFunction(getCurrentVideoTimeCallback)) {
        throw "startTrackingTime() - param 1 getCurrentVideoTimeCallback must be a function";
    }

    if (!isFunction(getTotalVideoDurationCallback)) {
        throw "startTrackingTime() - param 2 getTotalVideoDurationCallback must be a function";
    }

    var currentTime = getCurrentVideoTimeCallback();
    var totalDuration = getTotalVideoDurationCallback();

    if (currentTime == null) {
        throw "startTrackingTime() - Error: getCurrentVideoTimeCallback returned null.";
    }
    if (totalDuration == null) {
        throw "startTrackingTime() - getTotalVideoDurationCallback returned null";
    }
    if (isNaN(currentTime)) {
        throw "startTrackingTime() - getCurrentVideoTimeCallback returned a value that is NaN."
    }
    if (isNaN(totalDuration)) {
        throw "startTrackingTime() - getTotalVideoDurationCallback returned a value that is NaN."
    }
    if (timeSlider == null || !isHTMLElement(timeSlider)) {
        throw "startTrackingTime() - The UI for the time slider has not yet been set, so this function cannot change it.";
    }


    var percentage = (currentTime / totalDuration) * 100;

    if (isNaN(percentage)) {
        setTimeout(startTrackingTime, 1000);
        return;
    }

    timeSlider.value = percentage;
    setTimeout(startTrackingTime, 1000);
}

function givePlayerAbilityToTrackTime(paramGetCurrentVideoTimeCallback, paramGetTotalVideoDurationCallback) {
    if (!isFunction(paramGetCurrentVideoTimeCallback)) {
        throw "givePlayerAbilityToTrackTime - First param getCurrentVideoTimeCallback must be a function";
    }

    if (!isFunction(paramGetTotalVideoDurationCallback)) {
        throw "givePlayerAbilityToTrackTime - Second param getTotalVideoDurationCallback must be a function";
    }

    if (paramGetCurrentVideoTimeCallback.length !== 0 || paramGetTotalVideoDurationCallback.length !== 0) {
        throw "givePlayerAbilityToTrackTime - The callbacks passed to this function must not have any parameters.";
    }

    getCurrentVideoTimeCallback = paramGetCurrentVideoTimeCallback
    getTotalVideoDurationCallback = paramGetTotalVideoDurationCallback;

    shouldTrackTime = true;
    startTrackingTime();
}

function setVolumeChangeCallback(newCallback) {
    if (!isFunction(newCallback)) {
        throw "First parameter must be a function";
    }

    if (newCallback.length !== 1) {
        throw "setVolumeChangeCallback - This callback requires one parameter (number between 0 and 100 which represents the volume)";
    }

    volumeChangeCallback = newCallback;
}

function setClickTimeSliderCallback(newCallback) {
    if (!isFunction(newCallback)) {
        throw "First parameter must be a function";
    }

    if (newCallback.length !== 1) {
        throw "setUserChangesTimeCallback - This callback requires one parameter " +
            "(number between 0 and 100 which represents the percentage progress through the video)";
    }

    userClicksTimeSliderCallback = newCallback;
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
    createTimeSlider(playerDiv);
}

function createTimeSlider(playerDiv) {
    timeSlider = document.createElement("input");
    timeSlider.setAttribute("id", timeSliderId);
    timeSlider.setAttribute("type", "range");
    timeSlider.setAttribute("min", "0");
    timeSlider.setAttribute("max", "100");
    timeSlider.setAttribute("value", "0");
    timeSlider.setAttribute("step", "0.1");

    playerDiv.appendChild(timeSlider);

    timeSlider.onclick = function () {
        console.log("change - " + timeSlider.value);

        if (isFunction(userClicksTimeSliderCallback)) {
            userClicksTimeSliderCallback(timeSlider.value);

        }
    }
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
            volumeChangeCallback(volumeSlider.value);
        }
    }

    bottomButtonDiv.appendChild(volumeHoverButton);
    bottomButtonDiv.appendChild(volumeSlider);
}

function isHTMLElement(object) {
    return object instanceof Element || object instanceof HTMLDocument;
}