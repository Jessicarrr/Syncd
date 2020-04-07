function createPlayButton() {
    var button =
        $('<button/>').attr({
            text: 'Play',
            id: 'customPlayButton',
            height: 10,
            width: 10
        });
    button.click = function (event) {
        button.text = "Pause";
    }
    button.css({
        "height": "80px",
        "width": "80px"
    });
    $("#videoPlayerOverlay").append(button);
    console.log("Created custom play button");
}
