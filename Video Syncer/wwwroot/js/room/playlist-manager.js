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
    var ui = createUIForPlaylistVideo(id, title, videoId, author);
    addVideoToTable(ui);
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
    var buttonElement = document.createElement("button");

    urlElement.hidden = true;
    idElement.hidden = true;

    playlistDiv.classList.add("playlist-div");
    playlistInfoDiv.classList.add("playlist-info-div");

    titleElement.classList.add("playlist-div-element", "playlist-title");

    authorElement.classList.add("playlist-div-element", "playlist-author");

    urlElement.classList.add = "playlist-url-hidden";
    idElement.classList.add = "playlist-item-id-hidden";

    buttonElement.classList.add("playlist-options-button");

    buttonElement.innerHTML = "...";

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
    /*buttonElement.onclick = function (e) {
        e.preventDefault();
    }*/

    return playlistDiv;
}

function clickPlaylistItem(urlParam) {
    changeVideo(urlParam);
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