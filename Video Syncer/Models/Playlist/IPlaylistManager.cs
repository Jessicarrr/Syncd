using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Playlist
{
    public interface IPlaylistManager
    {
        public bool RemoveFromPlaylist(string itemId);
        public void AddToPlaylist(string youtubeId);
        public bool PlaylistContainsId(string id);
        public PlaylistObject PlayPlaylistObject(string id);
        public void ChangeItemPlaying(PlaylistObject obj);
        public PlaylistObject GoToNextVideo();
    }
}
