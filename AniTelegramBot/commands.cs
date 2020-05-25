using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AniTelegramBot
{
    class Commands
    {
        public IMongoCollection<usop1> userInfo;
        public Commands()
        {
            var client = new MongoClient("mongodb+srv://dbUser:rOkHhgCUCVnUK8OQ@cluster0-pnk7e.azure.mongodb.net/test?retryWrites=true&w=majority");
            var database = client.GetDatabase("AniTelegramBotStatuses");
            userInfo = database.GetCollection<usop1>("Statuses");
        }
        public usop1 Get(string id) =>
            userInfo.Find<usop1>(user => user.id == Convert.ToInt64(id)).FirstOrDefault();

        public async Task<usop1> Create(usop1 user)
        {
            await userInfo.InsertOneAsync(user);
            return user;
        }

        public async void UpdateAsync(string id, usop1 userIn) =>
            await userInfo.ReplaceOneAsync(user => user.id == Convert.ToInt64(id), userIn);
    }
}
