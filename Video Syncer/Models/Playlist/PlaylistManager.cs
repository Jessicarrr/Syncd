using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Playlist
{
    public class PlaylistManager
    {
        public List<PlaylistObject> playlist = new List<PlaylistObject>();

        private int uniqueIdLength = 15;

        public void AddToPlaylist(string youtubeId)
        {
            PlaylistObject obj = new PlaylistObject();
            Random random = new Random();

            obj.id = GenerateUniqueId();
            obj.title = youtubeId;
            obj.author = "[author]";
            obj.videoId = youtubeId;

            playlist.Add(obj);
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
