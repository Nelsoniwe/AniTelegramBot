using System;
using System.Collections.Generic;
using System.Text;

namespace AniTelegramBot
{
    public class AniTelegramBotSettings : IAniTelegramBotSettings
    {

        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

        public interface IAniTelegramBotSettings
        {
            string CollectionName { get; set; }
            string ConnectionString { get; set; }
            string DatabaseName { get; set; }
        }
    
}
