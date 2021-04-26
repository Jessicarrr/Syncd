function addNewAdminLogMessage(user, actionMessage) {
    var userString = user.name + "#" + user.id + ": ";

    console.log(userString + actionMessage);
}