using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models;

namespace Video_Syncer.Views.Room
{
    public class RoomModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<User> userList = new List<User>();
    }
}
