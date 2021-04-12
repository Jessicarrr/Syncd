var userList = new Array();
var min = 0;
var max = 10000;

/*
 * The main method for adding a new user to the user list.
 */
function addUser(id, name) {
    var user = createNewUserObject(id, name);
    userList.push(user);
    addUserUI(user["div"]);
}

/**
 * Append newly created UI for a given user to the user list.
 * @param {any} userDiv the container for the user's UI, to be added to the user list UI.
 */
function addUserUI(userDiv) {
    document.getElementById("userListArea").appendChild(userDiv);
}

/**
 * Update the user list for a specific user.
 * @param {any} userId The identifier for which user to update.
 * @param {any} userRights The user's rights (admin or regular user)
 */
function updateUIForUser(userId, userRights) {
    var updatedUser = false;

    userList.forEach(function (entry) {
        if (userId == entry["id"]) {
            var icon = document.getElementById(userId + "userIcon");

            if (userRights === 1) {

                icon.classList.remove("fa-user");
                icon.classList.add("fa-crown");
                icon.style.color = "darkgoldenrod";
                icon.style.marginLeft = "-2px"
            }

            updatedUser = true;
        }

    });

    return updatedUser;
}

/**
 * Create new UI elements to accommodate a new user in the user list.
 * @param {any} name The user name 
 * @param {any} id The user id
 */
function createUIForUser(name, id) {
    var userDiv = document.createElement("div");

    var topLineDiv = document.createElement("div");
    var bottomLineDiv = document.createElement("div");
    var userIconDiv = document.createElement("div");

    var userName = document.createElement("p");
    var userIdElement = document.createElement("p");
    var userIcon = document.createElement("i");

    var userKick = document.createElement("p");
    var userBan = document.createElement("p");
    var userMakeAdmin = document.createElement("p");

    userDiv.classList.add("userDiv");

    userIcon.classList.add("fa", "fa-user", "icon");
    userIcon.id = id + "userIcon";

    userIconDiv.id = id + "userIconDiv";

    userName.classList.add("userDivElement");
    userName.id = id + "userName";

    userIdElement.classList.add("userDivElement");
    userIdElement.id = id + "userId";

    userKick.classList.add("userDivElement");
    userKick.id = id + "userKick";

    userBan.classList.add("userDivElement");
    userBan.id = id + "userBan";

    userMakeAdmin.classList.add("userDivElement");
    userMakeAdmin.id = id + "userMakeAdmin";

    
    userName.innerHTML = name;
    userIdElement.innerHTML = "#" + id;
    userKick.innerHTML = "Kick";
    userBan.innerHTML = "Ban";
    userMakeAdmin.innerHTML = "Make Admin";

    if (isNightMode === true) {
        userIcon.classList.add("night-mode");
    }

    if (myRights == 1 && id != userId) {
        userKick.style.display = "block";
        userBan.style.display = "block";
        userMakeAdmin.style.display = "block";
    }
    else {
        userKick.style.display = "none";
        userBan.style.display = "none";
        userMakeAdmin.style.display = "none";
    }

    userMakeAdmin.onclick = function (e) {
        sendMakeAdminRequest(id);
    };

    userKick.onclick = function (e) {
        sendKickRequest(id);
    };

    userBan.onclick = function (e) {
        sendBanRequest(id);
    };

    userDiv.appendChild(topLineDiv);
    userDiv.appendChild(bottomLineDiv);

    userIconDiv.appendChild(userIcon);

    topLineDiv.appendChild(userIconDiv);
    topLineDiv.appendChild(userName);
    topLineDiv.appendChild(userIdElement);
    bottomLineDiv.appendChild(userKick);
    bottomLineDiv.appendChild(userBan);
    bottomLineDiv.appendChild(userMakeAdmin);

    return userDiv;
}

/**
 * Creating a new user object, containing their name, id, and the div containing their
 * personal UI in the user list.
 * @param {any} id User id
 * @param {any} name User name
 */
function createNewUserObject(id, name) {
    var user = new Object();
    user.name = name;
    user.id = id;
    user.div = createUIForUser(user.name, user.id);
    return user;
}

/**
 * Remove all users from the user list.
 */
function removeUsers() {
    userList.forEach(function (entry) {
        var div = entry["div"];
        div.parentNode.removeChild(div);
    });
    userList = new Array();
}