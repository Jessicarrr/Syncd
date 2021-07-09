using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncd.Models.Network.RequestResponses.Enum
{
    public enum RequestType
    {
        Join = 1,
        ChangeVideoState = 2,
        Leave = 3,
        ChangeName = 4,
        Kick = 5,
        Ban = 6,
        MakeAdmin = 7,
        RearrangePlaylist = 8,
        PlayPlaylistVideo = 9,
        PlayVideo = 10,
        RemoveFromPlaylist = 11,
        AddToPlaylist = 12,
        TimeChange = 13
    }
}
