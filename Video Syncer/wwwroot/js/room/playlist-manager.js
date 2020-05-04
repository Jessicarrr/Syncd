
function addTestVideos() {
    var ui1 = createUIForPlaylistVideo("first title", "url1234567", "maxaroon");
    var ui2 = createUIForPlaylistVideo("how to set up your first video this is a really long title", "url1234567", "stalbeers");
    var ui3 = createUIForPlaylistVideo("this will be an even longer title. this is how to set up your first video! i will be showing you guys how to do that. good job guys. i love you all. really relaly really really long title!", "url1234567", "stalley bears");
    var ui4 = createUIForPlaylistVideo("shh", "url1234567", "mau mau");

    for (var i = 0; i < 10; i++) {
        addVideoToTable(ui1);
        addVideoToTable(ui2);
        addVideoToTable(ui3);
        addVideoToTable(ui4);
    }
}

/**
 * Append newly created UI for a given user to the user list.
 * @param {any} userDiv the container for the user's UI, to be added to the user list UI.
 */
function addVideoToTable(playlistDiv) {
    document.getElementById("playlist-area").appendChild(playlistDiv);
}

function createUIForPlaylistVideo(titleParam, urlParam, authorParam) {
    var playlistDiv = document.createElement("div");
    var titleElement = document.createElement("p");
    var authorElement = document.createElement("p");
    var urlElement = document.createElement("p");

    urlElement.hidden = true;

    playlistDiv.classList.add("playlist-div");

    titleElement.classList.add("playlist-div-element", "playlist-title");

    authorElement.classList.add("playlist-div-element", "playlist-author");

    urlElement.classList.add = "playlist-url-hidden";

    titleElement.innerHTML = titleParam;
    authorElement.innerHTML = authorParam;
    urlElement.innerHTML = urlParam;

    playlistDiv.appendChild(titleElement);
    playlistDiv.appendChild(authorElement);
    playlistDiv.appendChild(urlElement);

    return playlistDiv;
}