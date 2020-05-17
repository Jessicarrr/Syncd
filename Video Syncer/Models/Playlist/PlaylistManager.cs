﻿using Newtonsoft.Json.Linq;
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

        private PlaylistObject currentItemPlaying = null;

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

                        obj.title = titleToken.ToObject<string>();
                        obj.author = authorToken.ToObject<string>();
                    }
                    
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

        public bool DeleteVideo(string id)
        {
            return false;
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