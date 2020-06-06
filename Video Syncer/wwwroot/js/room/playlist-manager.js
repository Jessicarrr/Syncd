var playlistItems = new Array();

var objectPlaylistIdKey = "playlistId";
var objectVideoIdKey = "videoId";
var objectDivKey = "playlistItemDiv";

var classPlaylistAreaDiv = "playlist-div";
var classPlaylistInfoDiv = "playlist-info-div";
var classPlaylistDivElement = "playlist-div-element";
var classPlaylistTitle = "playlist-title";
var classPlaylistAuthor = "playlist-author";
var classPlaylistUrlHidden = "playlist-url-hidden";
var classPlaylistItemIdHidden = "playlist-item-id-hidden";
var classDropdown = "dropdown";
var classPlaylistOptionsButton = "playlist-options-button";
var classPlaylistDropdownButton = "playlist-dropdown-button";
var classDropdownContent = "dropdown-content";

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

function updateTitlesAndAuthors(paramPlaylist) {
    for (var key in paramPlaylist) {
        var videoObject = paramPlaylist[key];
        var videoObjectUniqueId = videoObject["id"];
        var videoObjectTitle = videoObject["title"];
        var videoObjectAuthor = videoObject["author"];

        playlistItems.forEach(function (element) {
            if (element[objectPlaylistIdKey] === videoObjectUniqueId) {
                var div = element[objectDivKey];
                var titleElement = div.querySelector("." + classPlaylistTitle);
                var authorElement = div.querySelector("." + classPlaylistAuthor);

                var currentTitleText = titleElement.innerHTML;
                var currentAuthorText = authorElement.innerHTML;

                if (currentTitleText !== videoObjectTitle) {
                    titleElement.innerHTML = videoObjectTitle;
                }

                if (currentAuthorText != videoObjectAuthor) {
                    authorElement.innerHTML = videoObjectAuthor;
                }
            }
        });
    }
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

    playlistDiv.classList.add(classPlaylistAreaDiv);
    playlistInfoDiv.classList.add(classPlaylistInfoDiv);

    playlistDiv.id = idParam;

    titleElement.classList.add(classPlaylistDivElement, classPlaylistTitle);

    authorElement.classList.add(classPlaylistDivElement, classPlaylistAuthor);

    urlElement.classList.add = classPlaylistUrlHidden;
    idElement.classList.add = classPlaylistItemIdHidden;

    titleElement.innerHTML = titleParam;
    authorElement.innerHTML = authorParam;
    urlElement.innerHTML = urlParam;
    idElement.innerHTML = idParam;

    playlistDiv.setAttribute('draggable', true);

    playlistInfoDiv.appendChild(titleElement);
    
    playlistInfoDiv.appendChild(authorElement);
    
    playlistInfoDiv.appendChild(urlElement);
    playlistInfoDiv.appendChild(idElement);

    playlistDiv.appendChild(buttonElement);
    playlistDiv.appendChild(playlistInfoDiv);
    
    
    playlistInfoDiv.setAttribute("onclick", "clickPlaylistItem(\"" + idParam + "\");");

    setupPlaylistDragging(playlistDiv);
    

    return playlistDiv;
}

function setupPlaylistDragging(playlistDiv) {
    var dragCounter = 0;

    playlistDiv.ondragstart = function (e) {
        e.dataTransfer.setData("DraggedItem", e.target.id);
    }

    playlistDiv.ondragover = function (e) {
        e.preventDefault();
    }

    playlistDiv.ondragend = function (e) {
        //console.log("dropped onto " + lastThingDraggedOnto);
        e.preventDefault();
    };

    playlistDiv.ondragleave = function (e) {
        e.preventDefault();
        var relevantDiv = getPlaylistItemParentElement(e.target);

        if (relevantDiv == null) {
            return;
        }

        dragCounter -= 1;

        if (dragCounter <= 0) {
            relevantDiv.style.borderTop = "";
            dragCounter = 0;
        }
        
        //console.log("set last item left to " + lastPlaylistItemLeft.getAttribute('id'));

        
        
    }

    playlistDiv.ondragenter = function (e) {
        e.preventDefault();
        var relevantDiv = getPlaylistItemParentElement(e.target);

        if (relevantDiv == null) {
            return;
        }

        dragCounter += 1;
        relevantDiv.style.borderTop = "3px solid red";
    };

    

    playlistDiv.addEventListener("drop", function (event) {
        var draggedItemId = event.dataTransfer.getData("DraggedItem");
        var droppedOntoItem = getPlaylistItemParentElement(event.target);

        dragCounter = 0;

        if (droppedOntoItem == null) {
            return;
        }

        droppedOntoItem.style.borderTop = "";
        var droppedOntoItemId = droppedOntoItem.getAttribute('id');

        console.log('dragged ' + draggedItemId + ' on top of ' + droppedOntoItemId);
        sendRearrangePlaylistRequest(draggedItemId, droppedOntoItemId);
    });
}

/**
 * Finds the parent with the class "playlist-div" from an item that had another playlist item dragged onto it.
 * @param {any} draggedOntoElement The element that had an item dragged onto it.
 * @returns {any} The parent element with class "playlist div"
 */
function getPlaylistItemParentElement(childElement) {
    var className = childElement.className;

    if (className === classPlaylistAreaDiv) {
        // no need to search for the id.
        return childElement;
    }
    else if (className === classPlaylistDivElement + " " + classPlaylistTitle || className === classPlaylistOptionsButton || className === classPlaylistDivElement + " " + classPlaylistAuthor) {
        return childElement.parentNode.parentNode;
    }
    else if (className === classPlaylistInfoDiv || className === classDropdown) {
        return childElement.parentNode;
    }
    else {
        return null;
    }
}

function clickPlaylistItem(idParam) {
    sendPlayPlaylistItemRequest(idParam);
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

    copyLinkButton.classList.add(classPlaylistDropdownButton);
    openInBrowserButton.classList.add(classPlaylistDropdownButton);
    deleteButton.classList.add(classPlaylistDropdownButton);
    dropdownDiv.classList.add(classDropdownContent);
    dropdownButton.classList.add(classPlaylistOptionsButton);
    wholeAreaDiv.classList.add(classDropdown);
    
    dropdownDiv.id = playlistItemId + "-dropdown";

    dropdownDiv.style.display = "none";

    dropdownDiv.appendChild(copyLinkButton);
    dropdownDiv.appendChild(openInBrowserButton);
    dropdownDiv.appendChild(deleteButton);

    wholeAreaDiv.appendChild(dropdownButton);
    wholeAreaDiv.appendChild(dropdownDiv);

    dropdownButton.onclick = function (e) {
        var currentDisplay = dropdownDiv.style.display;

        if (currentDisplay === "none") {
            dropdownDiv.style.display = "block";

            if (isNightMode) {
                copyLinkButton.classList.add(classPlaylistDropdownButton, "night-mode");
                openInBrowserButton.classList.add(classPlaylistDropdownButton, "night-mode");
                deleteButton.classList.add(classPlaylistDropdownButton, "night-mode");
                dropdownDiv.classList.add(classDropdownContent, "night-mode");
            }
            else {
                copyLinkButton.classList.remove("night-mode");
                openInBrowserButton.classList.remove("night-mode");
                deleteButton.classList.remove("night-mode");
                dropdownDiv.classList.remove("night-mode");
            }
        }
        else {
            dropdownDiv.style.display = "none";
        }
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
        closeDropdowns();
    };

    openInBrowserButton.onclick = function () {
        window.open("https://www.youtube.com/watch?v=" + playlistVideoId);
        closeDropdowns();
    };

    deleteButton.onclick = function () {
        sendDeletePlaylistItemRequest(playlistItemId);
        closeDropdowns();
    };

    return wholeAreaDiv;
}

function fixPlaylistArrangement(paramPlaylist) {
    for (var i = 0; i < paramPlaylist.length; i++) {
        var paramPlaylistObject = paramPlaylist[i];

        var paramPlaylistTitle = paramPlaylistObject["title"];
        var paramPlaylistAuthor = paramPlaylistObject["author"];
        var paramPlaylistVideoId = paramPlaylistObject["videoId"];
        var paramPlaylistUniqueId = paramPlaylistObject["id"];

        var currentlySavedPlaylistObject = playlistItems[i];

        var currentlySavedPlaylistUniqueId = currentlySavedPlaylistObject[objectPlaylistIdKey];
        var currentlySavedPlaylistVideoId = currentlySavedPlaylistObject[objectVideoIdKey];
        var div = currentlySavedPlaylistObject[objectDivKey];

        var titleElement = div.querySelector("." + classPlaylistTitle);
        var authorElement = div.querySelector("." + classPlaylistAuthor);
        var infoDivElement = div.querySelector("." + classPlaylistInfoDiv);

        var currentTitleText = titleElement.innerHTML;
        var currentAuthorText = authorElement.innerHTML;

        if (paramPlaylistTitle !== currentTitleText) {
            titleElement.innerHTML = paramPlaylistTitle;
        }

        if (paramPlaylistAuthor !== currentAuthorText) {
            authorElement.innerHTML = paramPlaylistAuthor;
        }

        if (paramPlaylistVideoId !== currentlySavedPlaylistVideoId) {
            currentlySavedPlaylistObject[objectVideoIdKey] = paramPlaylistVideoId;
        }

        if (paramPlaylistUniqueId !== currentlySavedPlaylistUniqueId) {
            currentlySavedPlaylistObject[objectPlaylistIdKey] = paramPlaylistUniqueId;
        }

        if (div.getAttribute('id') !== paramPlaylistUniqueId) {
            div.id = paramPlaylistUniqueId;
            infoDivElement.setAttribute("onclick", "clickPlaylistItem(\"" + paramPlaylistUniqueId + "\");")
        }

        //playlistInfoDiv.setAttribute("onclick", "clickPlaylistItem(\"" + idParam + "\");");
    }

    /*for (var key in paramPlaylist) {
        var videoObject = paramPlaylist[key];

        var videoObjectTitle = videoObject["title"];
        var videoObjectAuthor = videoObject["author"];
        var videoObjectVideoId = videoObject["videoId"];
        var videoObjectUniqueId = videoObject["id"];
    }*/
}

/**
 * Remove all videos from the playlist UI
 */
function removeAllPlaylistVideos() {
    var elements = document.getElementsByClassName(classPlaylistAreaDiv);

    while (elements.length > 0) {
        elements[0].parentNode.removeChild(elements[0]);
    }

    playlistItems.length = 0;
}