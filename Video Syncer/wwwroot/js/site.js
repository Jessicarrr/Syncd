// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var usernameCookie = "username";
var usernameCharacterLimit = 25;

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
};

document.getElementById("usernameBox").onfocusout = function () {
    userSetName();
}

document.getElementById("usernameBox").addEventListener("keyup", function (e) {
    if (e.keyCode === 13) {
        userSetName();
    }
}); 

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