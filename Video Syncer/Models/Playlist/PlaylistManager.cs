using Microsoft.Extensions.Logging;
using Microsoft.Security.Application;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Video_Syncer.api.receiver;
using Video_Syncer.logging;

namespace Video_Syncer.Models.Playlist
{
    public class PlaylistManager : IPlaylistManager
    {
        public List<PlaylistObject> playlist = new List<PlaylistObject>();

        private int uniqueIdLength = 15;
        private NoEmbedHandler noembed = new NoEmbedHandler();

        private PlaylistObject currentItemPlaying = null;

        private ILogger logger;

        public PlaylistManager()
        {
            logger = LoggingHandler.CreateLogger<PlaylistManager>();
        }

        public bool RemoveFromPlaylist(string itemId)
        {
            foreach(PlaylistObject obj in playlist)
            {
                if(String.Equals(obj.id, itemId))
                {
                    playlist.Remove(obj);
                    return true;
                }
            }
            return false;
        }
        public void AddToPlaylist(string youtubeId)
        {
            PlaylistObject obj = new PlaylistObject();
            Random random = new Random();

            obj.id = GenerateUniqueId();
            obj.title = "http://youtube.com/watch?v=" + youtubeId + " (title not found)";
            obj.author = "(unable to find video author)";
            obj.videoId = youtubeId;
            playlist.Add(obj);

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

                        string author = authorToken.ToObject<string>();
                        string title = titleToken.ToObject<string>();

                        author = Microsoft.Security.Application.Encoder.HtmlEncode(author);
                        title = Microsoft.Security.Application.Encoder.HtmlEncode(title);

                        obj.title = title;
                        obj.author = author;
                    }
                    else
                    {
                        logger.LogWarning("[VSY] noembed.GetYoutubeData returned null for video with youtube id " + youtubeId);
                    }
                });
            });
        }

        public bool RearrangePlaylist(string onTopId, string onBottomId)
        {
            PlaylistObject onTop = GetPlaylistObject(onTopId);
            PlaylistObject onBottom = GetPlaylistObject(onBottomId);

            if(onTop == null || onBottom == null)
            {
                return false;
            }

            int onTopOldIndex = GetPositionOf(onTop);
            int onBottomOldIndex = GetPositionOf(onBottom);
            

            if(onTopOldIndex == -1 || onBottomOldIndex == -1)
            {
                return false;
            }

            int newIndex = onBottomOldIndex;

            if(onTopOldIndex < onBottomOldIndex)
            {
                newIndex--;
            }

            if (newIndex <= -1)
            {
                playlist.RemoveAt(onTopOldIndex);
                playlist.Insert(0, onTop);
                return true;
            }
            else
            {
                playlist.RemoveAt(onTopOldIndex);

                if (newIndex > onBottomOldIndex)
                {
                    newIndex--;
                }

                playlist.Insert(newIndex, onTop);
                return true;
            }
        }

        public int GetPositionOf(PlaylistObject obj)
        {
            for(int i = 0; i < playlist.Count; i++)
            {
                if(playlist[i].id == obj.id)
                {
                    return i;
                }
            }
            return -1;
        }

        public PlaylistObject GetPlaylistObject(string playlistObjectId)
        {
            IEnumerable<PlaylistObject> n = playlist.Where(obj => obj.id == playlistObjectId);

            if(n.Count() > 1 || n.Count() <= 0)
            {
                return null;
            }

            return n.First();
        }

        public List<PlaylistObject> GetPlaylist()
        {
            return playlist;
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

        public bool PlaylistContainsId(string id)
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

        public PlaylistObject PlayPlaylistObject(string id)
        {
            IEnumerable<PlaylistObject> list = playlist.Where(item => item.id == id);

            if(list.Count() == 0)
            {
                return null;
            }

            PlaylistObject requestedObject = list.First();
            currentItemPlaying = requestedObject;
            return requestedObject;
        }

        public void ChangeItemPlaying(PlaylistObject obj)
        {
            this.currentItemPlaying = obj;
        }

        public PlaylistObject GoToNextVideo()
        {
            if(playlist.Count == 0)
            {
                return null;
            }

            if(currentItemPlaying == null)
            {
                currentItemPlaying = playlist.First();
                return playlist.First();
            }
            else
            {
                int maxIndex = playlist.Count;
                int currentIndex = playlist.FindIndex(item => item.id == currentItemPlaying.id);
                
                if(currentIndex == -1)
                {
                    return null;
                }

                if(currentIndex == maxIndex)
                {
                    return null;
                }
                else
                {
                    int nextIndex = currentIndex + 1;
                    currentItemPlaying = playlist[nextIndex];
                    return playlist[nextIndex];
                }
            }
        }
    }
}
