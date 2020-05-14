var playlistItems = new Array();

var objectPlaylistIdKey = "playlistId";
var objectVideoIdKey = "videoId";
var objectDivKey = "playlistItemDiv";

/**
 * Gets the video ID from the youtube video URL in the search bar, and
 * calls the function to change the video.
 */
function addVideoFromSearchBar() {
    var searchBarText = document.getElementById('searchBar').value;
    var videoId = getVideoIdFromYoutubeUrl(searchBarText);

    if (videoId != null) {
        sendAddToPlaylistRequest(videoId);
        document.getElementById('searchBar').value = "";
    }
    else {
        alert("not a valid video");
    }
}

/**
 * Logic for getting the youtube video ID out of an entire youtube url.
 * @param {any} url The url of the youtube video.
 */
function getVideoIdFromYoutubeUrl(url) {
    if (!url.includes("youtu") || !url.includes("v=")) {
        return null;
    }

    var idArea = url.split("v=")[1];

    if (idArea.includes("&")) {
        return idArea.substring(0, idArea.indexOf("&"));
    }
    else {
        return idArea;
    }
}

function addDataToPlaylist(id, title, videoId, author) {
    var alreadyContains = playlistContainsPlaylistId(id);

    if (alreadyContains) {
        return false;
    }

    var ui = createUIForPlaylistVideo(id, title, videoId, author);

    var newObject = {};
    newObject[objectPlaylistIdKey] = id;
    newObject[objectVideoIdKey] = videoId;
    newObject[objectDivKey] = ui;

    playlistItems.push(newObject);
    addVideoToTable(ui);
    return true;
}

function compareAndRemovePlaylistItems(paramPlaylist) {
    var itemsToRemove = new Array();
    var newItems = new Array();

    for (var key in paramPlaylist) {
        var videoObject = paramPlaylist[key];
        var videoObjectUniqueId = videoObject["id"];

        newItems.push(videoObjectUniqueId);
    }

    playlistItems.forEach(function (element) {
        var currentPlaylistId = element[objectPlaylistIdKey];

        if (!newItems.includes(currentPlaylistId)) {
            itemsToRemove.push(currentPlaylistId);
        }
    });

    itemsToRemove.forEach(function (element) {
        removeFromPlaylist(element);
    });
}

function compareAndAddPlaylistItems(paramPlaylist) {
    for (var key in paramPlaylist) {
        var videoObject = paramPlaylist[key];

        var videoObjectTitle = videoObject["title"];
        var videoObjectAuthor = videoObject["author"];
        var videoObjectVideoId = videoObject["videoId"];
        var videoObjectUniqueId = videoObject["id"];

        addDataToPlaylist(videoObjectUniqueId, videoObjectTitle, videoObjectVideoId, videoObjectAuthor);
    }
}

function removeFromPlaylist(id) {
    for (var i = 0; i < playlistItems.length; i++) {
        var currentPlaylistItem = playlistItems[i];
        var currentPlaylistId = currentPlaylistItem[objectPlaylistIdKey];

        if (currentPlaylistId === id) {
            var playlistArea = document.getElementById("playlist-area");
            playlistArea.removeChild(currentPlaylistItem[objectDivKey]);
            playlistItems.splice(i, 1); // remove one element at position i
        }
    }
}

function playlistContainsId(playlist, id) {
    var answer = false;

    playlist.forEach(function (element) {
        if (element[objectPlaylistIdKey] === id) {
            answer = true;
        }
    });
    return answer;
}

function playlistContainsPlaylistId(playlistId) {
    var answer = false;

    playlistItems.forEach(function (element) {
        console.log(element[objectPlaylistIdKey] + " === " + playlistId + "?");

        if (element[objectPlaylistIdKey] === playlistId) {
            answer = true;
        }
    });

    return answer;
}

/**
 * Append newly created UI for a given user to the user list.
 * @param {any} userDiv the container for the user's UI, to be added to the user list UI.
 */
function addVideoToTable(playlistDiv) {
    document.getElementById("playlist-area").appendChild(playlistDiv);
}

function createUIForPlaylistVideo(idParam, titleParam, urlParam, authorParam) {
    var playlistDiv = document.createElement("div");
    var playlistInfoDiv = document.createElement("div");
    var titleElement = document.createElement("p");
    var authorElement = document.createElement("p");
    var urlElement = document.createElement("p");
    var idElement = document.createElement("p");
    var buttonElement = createDropdownButtonForItem(idParam, urlParam);

    urlElement.hidden = true;
    idElement.hidden = true;

    playlistDiv.classList.add("playlist-div");
    playlistInfoDiv.classList.add("playlist-info-div");

    titleElement.classList.add("playlist-div-element", "playlist-title");

    authorElement.classList.add("playlist-div-element", "playlist-author");

    urlElement.classList.add = "playlist-url-hidden";
    idElement.classList.add = "playlist-item-id-hidden";

    titleElement.innerHTML = titleParam;
    authorElement.innerHTML = authorParam;
    urlElement.innerHTML = urlParam;
    idElement.innerHTML = idParam;

    playlistInfoDiv.appendChild(titleElement);
    
    playlistInfoDiv.appendChild(authorElement);
    
    playlistInfoDiv.appendChild(urlElement);
    playlistInfoDiv.appendChild(idElement);

    playlistDiv.appendChild(playlistInfoDiv);
    playlistDiv.appendChild(buttonElement);
    
    playlistInfoDiv.setAttribute("onclick", "clickPlaylistItem(\"" + urlParam + "\");");
    

    return playlistDiv;
}

function clickPlaylistItem(urlParam) {
    changeVideo(urlParam);
}

function createDropdownButtonForItem(playlistItemId, playlistVideoId) {
    var wholeAreaDiv = document.createElement("div");
    var dropdownButton = document.createElement("p");

    var dropdownDiv = document.createElement("div");
    var copyLinkButton = document.createElement("p");
    var openInBrowserButton = document.createElement("p");
    var deleteButton = document.createElement("p");

    copyLinkButton.innerHTML = "Copy link";
    openInBrowserButton.innerHTML = "Open in browser";
    deleteButton.innerHTML = "Delete";
    dropdownButton.innerHTML = "...";

    copyLinkButton.classList.add("playlist-dropdown-button");
    openInBrowserButton.classList.add("playlist-dropdown-button");
    deleteButton.classList.add("playlist-dropdown-button");
    dropdownDiv.classList.add("dropdown-content");
    dropdownButton.classList.add("playlist-options-button");
    wholeAreaDiv.classList.add("dropdown");
    dropdownDiv.id = playlistItemId + "-dropdown";

    dropdownDiv.appendChild(copyLinkButton);
    dropdownDiv.appendChild(openInBrowserButton);
    dropdownDiv.appendChild(deleteButton);

    wholeAreaDiv.appendChild(dropdownButton);
    wholeAreaDiv.appendChild(dropdownDiv);

    dropdownButton.onclick = function (e) {
        var currentDisplay = dropdownDiv.style.display;

        if (currentDisplay === "none") {
            dropdownDiv.style.display = "block";
        }
        else {
            dropdownDiv.style.display = "none";
        }
        console.log("clicked on " + dropdownDiv + ", id is " + dropdownDiv.id);
        //alert("clicked on video " + urlParam);
    };

    copyLinkButton.onclick = function () {
        var newElement = document.createElement('textarea');
        newElement.value = "http://youtube.com/watch?v=" + playlistVideoId;
        document.body.appendChild(newElement);
        newElement.select();
        newElement.setSelectionRange(0, 99999); /*For mobile devices*/
        document.execCommand("copy");
        document.body.removeChild(newElement);
    };

    openInBrowserButton.onclick = function () {
        window.open("https://www.youtube.com/watch?v=" + playlistVideoId);
    };

    deleteButton.onclick = function () {
        sendDeletePlaylistItemRequest(playlistItemId);
    };

    return wholeAreaDiv;
}

/**
 * Remove all videos from the playlist UI
 */
function removeAllPlaylistVideos() {
    var elements = document.getElementsByClassName("playlist-div");

    while (elements.length > 0) {
        elements[0].parentNode.removeChild(elements[0]);
    }
}