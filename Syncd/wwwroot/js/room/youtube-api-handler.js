function buildYoutubeUrl(youtubeVideoId) {
    var firstPart = "http://youtube.com/watch?v=";

    return firstPart + youtubeVideoId;
}

function callApiWithCallback(url, callback) {
    if (typeof callback !== 'function') {
        throw "callback must be a function";
    }

    $.getJSON("https://noembed.com/embed?callback=?",
        { "format": "json", "url": url },
        function (data) {
            callback(data);
        });
}
