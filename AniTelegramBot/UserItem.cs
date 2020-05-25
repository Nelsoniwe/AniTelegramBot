using System;
using System.Collections.Generic;
using System.Text;

namespace AniTelegramBot
{
    class UserItem
    {
        public string Id { get; set; }
        public List<Userresultitem> results = new List<Userresultitem>();
        public List<Userresultitem> anime = new List<Userresultitem>();
    }
}
