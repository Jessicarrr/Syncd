function setupHidePlayerCheckbox() {
    var checkbox = document.getElementById("hide-player-checkbox");

    checkbox.addEventListener('change', function () {
        if (this.checked) {
            console.log("hide");
            //makeButtonAreaInvisible();
            disablePlayer();
        }
        else {
            //makeButtonAreaVisible();
            enablePlayer();
        }
    });
}