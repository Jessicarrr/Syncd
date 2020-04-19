var containerDivId = "cvpl-videoPlayerContainer";
var playerControlsDivId = "cvpl-videoPlayerOverlay";
var volumeSliderId = "cvpl-volumeSlider";
var volumeButtonHoverId = "cvpl-customVolumeButton";
var bottomButtonDivId = "cvpl-bottomButtonDiv"
var timeSliderId = "cpvl-timeSlider";
var timeDisplayId = "cvpl-timeDisplay";
var fullscreenButtonId = "cvpl-fullscreenButton";

var playerAdded = false;
var userDraggingTimeSlider = false;
var isMouseOverPlayer = false;

var originalWidth;
var originalHeight;

var containerDiv;
var playerControlsDiv;
var playerElement;
var playButton;
var volumeHoverButton;
var volumeSlider;
var timeSlider;
var volumeHoverSlider;
var bottomButtonDiv;
var timeDisplay;
var fullscreenButton;

var toggleVideoPlayingCallback;
var volumeChangeCallback;
var userClicksTimeSliderCallback;
var getCurrentVideoTimeCallback;
var getTotalVideoDurationCallback;
var changeVideoSizeCallback;

var shouldTrackTime = false;

/**
 * Disables the custom player so that the video can be clicked on and interacted with.
 */
function disablePlayer() {
    playerControlsDiv.style.display = 'none';
}

/**
 * Enables the player so that the player can be used again after disablePlayer() has been called.
 */
function enablePlayer() {
    playerControlsDiv.style.display = 'block';
}

function makeButtonAreaInvisible() {
    fadeOut(bottomButtonDiv);
}

function makeButtonAreaVisible() {
    fadeIn(bottomButtonDiv);
}

function fadeOut(element, toOpacity=0) {
    if (element == null || !isHTMLElement(element)) {
        throw "fadeOut(element) - param must be a html element, you sent \"" + element + "\"";
    }

    // no opacity is set
    if (!element.style.opacity) {
        element.style.opacity = 1;
    }

    element.style.transition = '0.2s';
    element.style.opacity = toOpacity;
    element.style.visibility = "hidden";
}

function fadeIn(element, toOpacity=1) {
    if (element == null || !isHTMLElement(element)) {
        throw "fadeIn(element, toOpacity=1) - param 1 must be an html element, you sent \"" + element + "\"";
    }

    // no opacity is set
    if (!element.style.opacity) {
        element.style.opacity = 0;
    }

    element.style.visibility = "visible";
    element.style.transition = '0.2s';
    element.style.opacity = toOpacity;

    
}

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

    if (timeSlider == null || !isHTMLElement(timeSlider)) {
        throw "startTrackingTime() - The UI for the time slider has not yet been set, so this function cannot change it.";
    }


    var percentage;
    var currentTimeFormatted;
    var totalDurationFormatted;

    if (isNullOrNaN(currentTime) || isNullOrNaN(totalDuration)) {
        percentage = timeSlider.value || 0;

        if (isNullOrNaN(currentTime)) {
            currentTimeFormatted = "--:--:--";
        }
        if (isNullOrNaN(totalDuration)) {
            totalDurationFormatted = "--:--:--";
        }
    }
    else {
        percentage = (currentTime / totalDuration) * 100;
        currentTimeFormatted = secondsToFormattedTime(currentTime);
        totalDurationFormatted = secondsToFormattedTime(totalDuration);
    }
    
    setTimeSliderPercentage(percentage);
    setTimeDisplay(currentTimeFormatted, totalDurationFormatted);
    setTimeout(startTrackingTime, 1000);
}

function setTimeDisplay(formattedCurrentTime, formattedTotalTime) {
    if (formattedCurrentTime == null || formattedTotalTime == null || timeDisplay == null) {
        return;
    }
    timeDisplay.innerHTML = formattedCurrentTime + " / " + formattedTotalTime;
}

function setTimeSliderPercentage(percentage) {
    if (timeSlider == null || !isHTMLElement(timeSlider) || isNullOrNaN(percentage)) {
        return;
    }
    if (!userDraggingTimeSlider) {
        timeSlider.value = percentage;
    }
}

function isNullOrNaN(number) {
    if (number == null || isNaN(number)) {
        return true;
    }
    return false;
}

function setChangeVideoSizeCallback(newCallback) {
    if (!isFunction(newCallback)) {
        throw "setChangeVideoSizeCallback - newCallback parameter must be a function";
    }

    changeVideoSizeCallback = newCallback;
    addFullscreenButtonClick();
}

function addFullscreenButtonClick() {
    addFullscreenListeners();

    fullscreenButton.onclick = function () {

        if (!isFullscreen()) {
            console.log("fullscreen - tryin full screen");
            tryFullScreen();
        }
        else {
            console.log("fullscreen - exiting fullscreen");
            tryExitFullscreen();
        }
    }
}

function doFullscreenCallbacks() {
    if (isFullscreen()) {
        changeVideoSizeCallback(screen.width, screen.height);
    }
    else {
        changeVideoSizeCallback(originalWidth, originalHeight);
    }
}

function addFullscreenListeners() {
    /* Standard syntax */
    document.addEventListener("fullscreenchange", function () {
        doFullscreenCallbacks();
    });

    /* Firefox */
    document.addEventListener("mozfullscreenchange", function () {
        doFullscreenCallbacks();
    });

    /* Chrome, Safari and Opera */
    document.addEventListener("webkitfullscreenchange", function () {
        doFullscreenCallbacks();
    });

    /* IE / Edge */
    document.addEventListener("msfullscreenchange", function () {
        doFullscreenCallbacks();
    });
}

function isFullscreen() {
    return document.isFullScreen || document.fullscreenElement
        || document.webkitFullscreenElement || document.mozFullScreenElement
        || document.msFullscreenElement;
}

function tryFullScreen() {
    if (changeVideoSizeCallback == null || !isFunction(changeVideoSizeCallback)) {
        throw "tryFullscreen() - Change video size callback must be set first. " +
            "Please use setChangeVideoSizeCallback(..)";
    }

    if (containerDiv.requestFullscreen) {
        containerDiv.requestFullscreen();

    }
    else if (containerDiv.mozRequestFullScreen) { /* Firefox */
        containerDiv.mozRequestFullScreen();

    }
    else if (containerDiv.webkitRequestFullscreen) { /* Chrome, Safari and Opera */
        containerDiv.webkitRequestFullscreen();

    }
    else if (containerDiv.msRequestFullscreen) { /* IE/Edge */
        containerDiv.msRequestFullscreen();
    }
}

function tryExitFullscreen() {

    if (changeVideoSizeCallback == null || !isFunction(changeVideoSizeCallback)) {
        throw "tryExitFullscreen() - changeVideoSizeCallback must be set first. " +
        "Please use setChangeVideoSizeCallback(...)";
    }

    if (document.exitFullscreen) {
        document.exitFullscreen();
    }

    else if (document.mozCancelFullScreen) { /* Firefox */
        document.mozCancelFullScreen();
    }

    else if (document.webkitExitFullscreen) { /* Chrome, Safari and Opera */
        document.webkitExitFullscreen();
    }

    else if (document.msExitFullscreen) { /* IE/Edge */
        document.msExitFullscreen();
    }
}

function secondsToFormattedTime(seconds) {
    if (seconds == null) {
        return null;
    }

    var roundedSeconds = Math.floor(seconds);
    var date = new Date(null);
    date.setSeconds(roundedSeconds);

    var videoTime = date.toUTCString().match(/(\d\d:\d\d:\d\d)/)[0];

    if (roundedSeconds < 3600) { // lower than one hour
        // remove the hour area
        videoTime = videoTime.match(/(\d\d:\d\d$)/)[0];
    }

    return videoTime;
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

/**
 * Changes the width and height of the video player.
 * @param {any} playerWidth The new width for the video player
 * @param {any} playerHeight The new height for the video player
 */
function changePlayerDimensions(playerWidth, playerHeight) {
    containerDiv.style.height = playerHeight + "px";
    containerDiv.style.width = playerWidth + "px";
}

/**
 * Create the video player over any html object. It will create a div around the html object of
 * the same height and width. Then insert the object inside along with an overlay div. 
 * @param object The player appears on top of this object
 * @param {any} playerWidth The width of the player. Should be the same width as your video player. Can be a string or a whole number. 
 * @param {any} playerHeight The height of the player. Should be the same height as your video player. Can be a string or a whole number.
 */
function createPlayerOverObject(object, playerWidth, playerHeight) {
    console.log("createPlayerOverObject - Trying to create custom video player");

    if (playerAdded) {
        return;
    }

    playerAdded = true;

    if (!isHTMLElement(object)) {
        throw "Cannot create video player over non HTML element. This object is not a HTML element: " + object;
    }

    var objectParent = object.parentNode;
    containerDiv = document.createElement("div");
    playerControlsDiv = document.createElement("div");
    
    containerDiv.setAttribute("id", containerDivId);
    playerControlsDiv.setAttribute("id", playerControlsDivId);

    // setting the height and width to match the object height and width
    containerDiv.style.height = playerHeight + "px";
    containerDiv.style.width = playerWidth + "px";

    originalWidth = playerWidth;
    originalHeight = playerHeight;
    currentWidth = playerWidth;
    currentHeight = playerHeight;
    playerElement = object;

    objectParent.replaceChild(containerDiv, object);
    containerDiv.appendChild(object);
    containerDiv.appendChild(playerControlsDiv);

    createHTML(playerControlsDiv);
    setupListeners();

    console.log("createPlayerOverObject - Finished creating custom video player")
}

function setupListeners() {
    containerDiv.onmouseenter = function () {
        isMouseOverPlayer = true;
        fadeIn(bottomButtonDiv);
        fadeIn(timeSlider);
    }

    containerDiv.onmouseleave = function () {
        isMouseOverPlayer = false;

        setTimeout(function () {
            if (!isMouseOverPlayer) {
                fadeOut(bottomButtonDiv);
                fadeOut(timeSlider);
            }
            
        }, 1000);
        
    }
}



function createHTML(playerControlsDiv) {
    createButtonDiv(playerControlsDiv);
    createTimeDisplay(playerControlsDiv);
    createButtons(playerControlsDiv);
    createTimeSlider(playerControlsDiv);
    createFullscreenButton();
}

function createFullscreenButton() {
    fullscreenButton = document.createElement("button");
    fullscreenButton.setAttribute("id", fullscreenButtonId);
    fullscreenButton.innerHTML = "FS";
    bottomButtonDiv.appendChild(fullscreenButton);
}

function createTimeDisplay(playerControlsDiv) {
    timeDisplay = document.createElement("p");
    timeDisplay.setAttribute("id", timeDisplayId);
    timeDisplay.innerHTML = "--:--:-- / --:--:--";
    bottomButtonDiv.appendChild(timeDisplay);
}

function createTimeSlider(playerControlsDiv) {
    timeSlider = document.createElement("input");
    timeSlider.setAttribute("id", timeSliderId);
    timeSlider.setAttribute("type", "range");
    timeSlider.setAttribute("min", "0");
    timeSlider.setAttribute("max", "100");
    timeSlider.setAttribute("value", "0");
    timeSlider.setAttribute("step", "0.1");

    playerControlsDiv.appendChild(timeSlider);

    timeSlider.onmousedown = function () {
        userDraggingTimeSlider = true;
    }

    timeSlider.onmouseup = function () {
        userDraggingTimeSlider = false;
    }

    timeSlider.onclick = function () {
        console.log("change - " + timeSlider.value);

        if (isFunction(userClicksTimeSliderCallback)) {
            userClicksTimeSliderCallback(timeSlider.value);

        }
    }
}

function createButtonDiv(playerControlsDiv) {
    bottomButtonDiv = document.createElement("div");
    bottomButtonDiv.setAttribute("id", bottomButtonDivId);
    playerControlsDiv.appendChild(bottomButtonDiv);
}

function createButtons(playerControlsDiv) {
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
            console.log("volume slider value = " + volumeSlider.value);
            volumeChangeCallback(volumeSlider.value);
        }
    }

    bottomButtonDiv.appendChild(volumeHoverButton);
    bottomButtonDiv.appendChild(volumeSlider);
}

function isHTMLElement(object) {
    return object instanceof Element || object instanceof HTMLDocument;
}