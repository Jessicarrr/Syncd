using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models
{
    public class User
    {
        public double videoTimeSeconds { get; set; }
        public VideoState videoState { get; set; }
        public int id { get; set; }
        public string name { get; set; }

        public User(int Id, string Name)
        {
            this.id = Id;
            this.name = Name;
            videoState = VideoState.None;
            videoTimeSeconds = -1;
        }
        
    }
}
