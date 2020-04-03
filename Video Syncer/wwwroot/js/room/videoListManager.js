
/**
 * Gets the video ID from the youtube video URL in the search bar, and
 * calls the function to change the video.
 */
function addVideoFromSearchBar() {
    var searchBarText = document.getElementById('searchBar').value;
    var videoId = getVideoIdFromYoutubeUrl(searchBarText);

    if (videoId != null) {
        changeVideo(videoId);
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