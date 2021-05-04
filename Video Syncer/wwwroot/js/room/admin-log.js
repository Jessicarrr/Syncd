var adminLogAreaOriginalWidth;
var adminLogAreaOriginalPadding;

function atBottom(ele) {
    var sh = ele.scrollHeight;
    var st = ele.scrollTop;
    var ht = ele.offsetHeight;
    if (ht == 0) {
        return true;
    }
    if (st == sh - ht) { return true; }
    else { return false; }
}

function addNewAdminLogMessage(user, actionMessage) {
    var adminViewerBody = document.getElementById("admin-log-viewer-body");
    var shouldScrollToBottom = false;
    var userName = user.name;

    if (atBottom(adminViewerBody)) {
        shouldScrollToBottom = true;
    }

    var newMessage = document.createElement("p");
    newMessage.classList.add("admin-log-viewer-item");

    if (typeof (decodeHtml) !== 'undefined') {
        actionMessage = decodeHtml(actionMessage);
        userName = decodeHtml(userName);
    }

    var userString = userName + "#" + user.id;

    if (user.id !== userId) {
        var name = document.createElement("button");
        name.innerText = userString;
        name.classList.add("admin-log-viewer-name", "can-have-night-mode");

        if (typeof (isNightMode) !== 'undefined') {
            if (isNightMode === true) {
                name.classList.add("night-mode");
            }
        }

        name.onclick = function () {
            setActionsAreaUser(user);
            openActionsArea();
        };

        newMessage.appendChild(name);


        newMessage.append(" " + actionMessage);
    }
    else {
        newMessage.innerText = "You " + actionMessage;
    }
   
    
    adminViewerBody.appendChild(newMessage);

    if (shouldScrollToBottom === true) {
        adminViewerBody.scrollTop = adminViewerBody.scrollHeight;
    }
}

function openAdminLog() {
    var adminLogViewerBody = document.getElementById("admin-log-viewer-body");
    var adminLogArea = document.getElementById("admin-log-area");

    closeActionsArea();
    adminLogViewerBody.style.display = "flex";
    adminLogArea.style.paddingBottom = adminLogAreaOriginalPadding;
}

function minimizeAdminLog() {
    var adminLogViewerBody = document.getElementById("admin-log-viewer-body");
    var adminLogArea = document.getElementById("admin-log-area");

    closeActionsArea();
    adminLogViewerBody.style.display = "none";
    adminLogArea.style.paddingBottom = "0";
}

function setActionsAreaUser(user) {
    var nameText = document.getElementById("admin-log-actions-name");
    var idText = document.getElementById("admin-log-actions-id");
    var kickButton = document.getElementById("admin-log-action-kick-button");
    var banButton = document.getElementById("admin-log-action-ban-button");
    var userName = user.name;

    if (typeof (decodeHtml) !== 'undefined') {
        userName = decodeHtml(userName);
    }

    nameText.innerText = userName
    idText.innerText = "#" + user.id;

    kickButton.onclick = function () {
        sendKickRequest(user.id);
    };

    banButton.onclick = function () {
        sendBanRequest(user.id);
    };
}

function openActionsArea() {
    var actionsArea = document.getElementById("admin-log-actions-area");
    var viewingArea = document.getElementById("admin-log-viewer");
    var adminLogArea = document.getElementById("admin-log-area");

    adminLogArea.style.width = "60%";
    viewingArea.style.width = "60%";
    actionsArea.style.display = "flex";
}

function closeActionsArea() {
    var actionsArea = document.getElementById("admin-log-actions-area");
    var viewingArea = document.getElementById("admin-log-viewer");
    var adminLogArea = document.getElementById("admin-log-area");

    viewingArea.style.width = "100%";
    adminLogArea.style.width = adminLogAreaOriginalWidth;
    actionsArea.style.display = "none";
}

function setupOriginalDimensions() {
    var adminLogArea = document.getElementById("admin-log-area");
    adminLogAreaOriginalWidth = adminLogArea.style.width;
    adminLogAreaOriginalPadding = adminLogArea.style.paddingBottom;
}

function hideAdminLogs() {
    var adminLog = document.getElementById("admin-log-area");

    closeActionsArea();
    minimizeAdminLog();
    adminLog.style.display = "none";

}

function showAdminLogs() {
    var adminLog = document.getElementById("admin-log-area");
    adminLog.style.display = "flex";
}

function setupAdminLogs() {
    setupOriginalDimensions();

    var closeButton = document.getElementById("admin-log-actions-close-button");
    var adminViewerBody = document.getElementById("admin-log-viewer-body");
    var adminLogHeader = document.getElementById("admin-log-viewer-header");

    adminViewerBody.scrollTop = adminViewerBody.scrollHeight;

    closeButton.onclick = function () {
        closeActionsArea();
    };

    adminLogHeader.onclick = function () {
        if (adminViewerBody.style.display == "flex") {
            minimizeAdminLog();
        } else {
            openAdminLog();
        }
    }

    openActionsArea();
    minimizeAdminLog();
}