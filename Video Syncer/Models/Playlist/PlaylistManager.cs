using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Video_Syncer.api.receiver;

namespace Video_Syncer.Models.Playlist
{
    public class PlaylistManager
    {
        public List<PlaylistObject> playlist = new List<PlaylistObject>();

        private int uniqueIdLength = 15;
        private NoEmbedHandler noembed = new NoEmbedHandler();

        public void AddToPlaylist(string youtubeId)
        {
            PlaylistObject obj = new PlaylistObject();
            Random random = new Random();

            obj.id = GenerateUniqueId();
            obj.title = youtubeId;
            obj.author = "[author]";
            obj.videoId = youtubeId;

            CancellationTokenSource source = new CancellationTokenSource();
            Task.Run(async () =>
            {
                await noembed.GetYoutubeData(youtubeId, source).ContinueWith(result =>
                {
                    JObject jResult = result.Result;

                    if(jResult != null)
                    {
                        JToken authorToken;
                        JToken titleToken;

                        jResult.TryGetValue("author_name", out authorToken);
                        jResult.TryGetValue("title", out titleToken);

                        obj.title = titleToken.ToObject<string>();
                        obj.author = authorToken.ToObject<string>();
                    }
                    playlist.Add(obj);
                });
            });
        }

        private string GenerateUniqueId()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string possibleCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
            Random random = new Random();

            for(int i = 0; i < uniqueIdLength; i++)
            {
                int randomNumber = random.Next(0, possibleCharacters.Length);
                char randomChar = possibleCharacters[randomNumber];
                stringBuilder.Append(randomChar);
            }

            string finalId = stringBuilder.ToString();

            if(PlaylistContainsId(finalId))
            {
                return GenerateUniqueId();
            }
            
            return finalId;
        }

        private bool PlaylistContainsId(string id)
        {
            foreach(PlaylistObject obj in playlist)
            {
                if(String.Equals(id, obj.id))
                {
                    return true;
                }
            }
            return false;
        }

        public bool DeleteVideo(string id)
        {
            return false;
        }

        public void nextVideo()
        {

        }
    }
}
