var userList = new Array();
var min = 0;
var max = 10000;

function addUser(id, name) {
    var user = createNewUserObject(id, name);
    userList.push(user);
    addUserUI(user["div"]);
}

function addUserUI(userDiv) {
    document.getElementById("userListArea").appendChild(userDiv);
}



function updateAllUsersTest(userState, userTime) {
    userList.forEach(function (entry) {
        updateUIForUser(entry["id"], userState, userTime);
    });
}

function updateUIForUser(userId, userState, userTime) {
    var updatedUser = false;

    userList.forEach(function (entry) {
        if (userId == entry["id"]) {
            var state = document.getElementById(userId + "userState");
            var time = document.getElementById(userId + "userTime")

            if (state == null || time == null) {
                console.log("No state or time?");
                updatedUser = false;
            }

            state.innerHTML = userState;
            time.innerHTML = userTime;
            updatedUser = true;
        }

    });

    return updatedUser;
}

function createUIForUser(name, id) {
    var userDiv = document.createElement("div");
    var userName = document.createElement("p");
    var userId = document.createElement("p");
    var userState = document.createElement("p");
    var userTime = document.createElement("p");

    userDiv.classList.add("userDiv");

    userName.classList.add("userDivElement");
    userName.id = id + "userName";

    userId.classList.add("userDivElement");
    userId.id = id + "userId";

    userState.classList.add("userDivElement");
    userState.id = id + "userState";

    userTime.classList.add("userDivElement");
    userTime.id = id + "userTime";

    userName.innerHTML = name;
    userId.innerHTML = id;
    userState.innerHTML = "[Video State]";
    userTime.innerHTML = "[Video Time]";

    userDiv.appendChild(userName);
    userDiv.appendChild(userId);
    userDiv.appendChild(userState);
    userDiv.appendChild(userTime);

    return userDiv;
}

function createNewUserObject(id, name) {
    var user = new Object();
    user.name = name;
    user.id = id;
    user.div = createUIForUser(user.name, user.id);
    return user;
}

function randomUserNumber() {
    return Math.round(Math.random() * (max - min) + min);
}

function removeUsers() {
    userList.forEach(function (entry) {
        var div = entry["div"];
        div.parentNode.removeChild(div);
    });
    userList = new Array();
}