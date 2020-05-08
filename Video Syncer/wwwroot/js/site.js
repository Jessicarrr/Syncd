// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var usernameCookie = "username";
var usernameCharacterLimit = 25;

var nightModeCookie = "night-mode";

window.onload = function (event) {
    createNewName();
    var errorMsg = document.getElementById("alert-username-too-long");
    errorMsg.innerHTML =
        "Name must be less than " + usernameCharacterLimit + " characters"
    errorMsg.style.display = "none";

    var javascriptWarning = document.getElementById("alert-no-javascript");

    if (javascriptWarning != null) {
        console.log("Javascript enabled");
        javascriptWarning.style.display = "none";
    }

    /* Check if the user is using Internet explorer, and display an error */
    if (isUsingInternetExplorer()) {
        var ieWarning = document.getElementById("alert-using-internet-explorer");
        ieWarning.style.display = "block";
    }

    var nightToggle = document.getElementById("night-mode-toggle");

    if (getCookie(nightModeCookie) === "true") {
        toggleNightMode(true);
        nightToggle.checked = true;
        setCookie(nightModeCookie, true, 14);
    }

    nightToggle.onclick = function () {
        toggleNightMode(nightToggle.checked);
        setCookie(nightModeCookie, nightToggle.checked, 14);
    }
};

document.getElementById("usernameBox").onfocusout = function () {
    userSetName();
}

document.getElementById("usernameBox").addEventListener("keyup", function (e) {
    if (e.keyCode === 13) {
        userSetName();
    }
}); 

function toggleNightMode(nightmode) {
    var body = document.querySelector('body');
    var textInputs = document.querySelectorAll("input[type=text]");
    var links = document.querySelectorAll("a");
    var navs = document.querySelectorAll("nav");
    /*var h3s = document.querySelectorAll("h3");
    var h2s = document.querySelectorAll("h2");*/
    /*var h1s = document.querySelectorAll("h1");
    var ps = document.querySelectorAll("p");*/

    var allElementsToChange = new Array();

    Array.prototype.push.apply(allElementsToChange, textInputs);
    Array.prototype.push.apply(allElementsToChange, links);
    Array.prototype.push.apply(allElementsToChange, navs);
    /*Array.prototype.push.apply(allElementsToChange, h3s);
    Array.prototype.push.apply(allElementsToChange, h2s);*/
    /*Array.prototype.push.apply(allElementsToChange, h1s);
    Array.prototype.push.apply(allElementsToChange, ps);*/

    if (nightmode) {
        body.classList.add("night-mode");

        allElementsToChange.forEach(function (item) {
            item.classList.add("night-mode");
        });
    }
    else {
        body.classList.remove("night-mode");

        allElementsToChange.forEach(function (item) {
            item.classList.remove("night-mode");
        });
    }
}

function isUsingInternetExplorer() {
    var edgeString = /Edge/;

    if (edgeString.test(navigator.userAgent)) {
        return false;
    }

    if (window.document.documentMode) {
        return true;
    }
    return false;

    /*var userAgent = window.navigator.userAgent;
    var msieVersion = userAgent.indexOf('MSIE ');
    var tridentVersion = userAgent.indexOf('Trident/');
    var edgeVersion = ua.indexOf('Edge/');

    if (edgeVersion > 0) {
        return false;
    }

    if (msieVersion > 0 || tridentVersion > 0) {
        return true;
    }
    return false;*/

}

function userSetName() {
    var newNameDefault = document.getElementById("usernameBox").value;
    var errorMsg = document.getElementById("alert-username-too-long");

    if (newNameDefault.length > usernameCharacterLimit) {
        errorMsg.style.display = "block";

    }
    else {
        errorMsg.style.display = "none";
    }

    var newName = newNameDefault.slice(0, usernameCharacterLimit);
    setName(newName);
    console.log("Set new name to \"" + newName + "\"");
}

function getUsername() {
    if (getCookie(usernameCookie) != "") {
        return getCookie(usernameCookie);
    }
    else {
        createNewName();
        if (getCookie(usernameCookie) == "") {
            return "[No name set]";
        }
        else {
            return getCookie(usernameCookie);
        }
    }
}

function createNewName() {
    if (getCookie(usernameCookie) == "") {
        var name = "ChubbyBunny" + Math.round(Math.random() * (10000 - 0) + 0);
        setName(name);
    }
    else {
        document.getElementById("usernameBox").value = getCookie(usernameCookie);
    }
}

function setName(name) {
    if (name.length < 1) {
        name = "ChubbyBunny" + Math.round(Math.random() * (10000 - 0) + 0);
    }
    setCookie(usernameCookie,
        name,
        7);
    document.getElementById("usernameBox").value = name;
}

function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}