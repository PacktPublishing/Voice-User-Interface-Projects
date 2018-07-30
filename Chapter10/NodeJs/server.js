var  express = require('express');
var  bodyParser = require('body-parser');
var  http = require('http');

var  app = express();
var server = http.createServer(app);
app.use(bodyParser.json());

var port = process.env.PORT || 8000;
server.listen(port, function () {
    console.log("Server is up and running...");
});

app.use("/ping", function (req, res, next) {
    res.send('Welcome to Podcasts Provider');
});

app.get('/podcasts', function (req, res) {
    var secret = req.get("mysecret");
    if(secret === "12345"){
        res.json ([
        {
            id: "1",
             podcastSource: "https://henrypodsstorage.blob.core.windows.net/media/HenryPodcast-1.mp3",
            albumName: "Andromeda", artist: "Zepto Segundo", month: "January", title: "Andromeda",
            albumCoverSource: 
                "https://henrypodsstorage.blob.core.windows.net/pictures/zepto_segundo_album_cover.jpg"
        },
        {
            id: "2", 
            podcastSource: "https://henrypodsstorage.blob.core.windows.net/media/HenryPodcast-2.mp3",
            albumName: "Andromeda", artist: "Zepto Segundo", month: "January", title: "Morir con Honor",
            albumCoverSource: 
                "https://henrypodsstorage.blob.core.windows.net/pictures/zepto_segundo_album_cover.jpg"
           
        },
        {
            id: "3", podcastSource: "https://henrypodsstorage.blob.core.windows.net/media/HenryPodcast-3.mp3",
            albumName: "Andromeda", artist: "Zepto Segundo", month: "March", title: "Ars Chimica",
            albumCoverSource: "https://henrypodsstorage.blob.core.windows.net/pictures/zepto_segundo_album_cover.jpg"
        }
      ]);
    } 
    else {
      return res.status(403).end('Access denied!');
    }
});
  