using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AniTelegramBot
{
    class Program
    {
        private static ITelegramBotClient botClient;
        static string Nameof;
        static bool IsPressed = false;

        static void Main(string[] args)
        {
            botClient = new TelegramBotClient("TelegramBotApi key") { Timeout = TimeSpan.FromSeconds(10) };

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Bot id:" + me.Id);

            botClient.OnMessage += Bot_OnMessage;
            botClient.OnCallbackQuery += BotClient_OnCallbackQuery;

            botClient.StartReceiving();

            Console.ReadLine();
        }

        private static async void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                string buttonText = e.CallbackQuery.Data;
                string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName} {e.CallbackQuery.From.Id}";

                Console.WriteLine(name + "нажал " + buttonText + " ");

                usoplistdb usoplistdb = JsonConvert.DeserializeObject<usoplistdb>(System.IO.File.ReadAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json"));
                usop1 usop = new usop1();  //проверка юзера в файле
                usop.id = e.CallbackQuery.From.Id;
                usop.status = 0;
                bool cont = false;
                int dbid = 0;
                for (int i = 0; i < usoplistdb.Usersbd.Count; i++)
                {
                    if (usoplistdb.Usersbd[i].id == e.CallbackQuery.From.Id)
                    {
                        dbid = i;
                        cont = true;
                        break;
                    }
                }

                switch (usoplistdb.Usersbd[dbid].DBstatus)
                {
                    case 0:
                        {
                            await botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, "сначала введите команду");
                            break;
                        }
                    case 1:
                        {
                            usoplistdb.Usersbd[dbid].DBstatus = 0;
                            UserInfo userInfo = new UserInfo();
                            userInfo.Name = buttonText;
                            //вот тут начинается поиск, не очень понялк как(магия наверное)
                            var json = JsonConvert.SerializeObject(userInfo);
                            var data = new StringContent(json, Encoding.UTF8, "application/json");
                            using var client = new HttpClient();
                            var content = await client.PostAsync("https://anibotapi20200520082004.azurewebsites.net/api/Ani/name", data);
                            string result = content.Content.ReadAsStringAsync().Result;
                            UserItem useritem = JsonConvert.DeserializeObject<UserItem>(result);

                            UserItem userIteminDB = new UserItem();
                            userIteminDB.Id = Convert.ToString(e.CallbackQuery.From.Id);

                            Userresultitem itemzeroinresult = useritem.results[0];
                            userIteminDB.results.Add(itemzeroinresult);

                            var jsonAPI2 = JsonConvert.SerializeObject(userIteminDB);
                            var dataAPI2 = new StringContent(jsonAPI2, Encoding.UTF8, "application/json");
                            var contentAPI2 = await client.PostAsync("https://anibotapi20200520082004.azurewebsites.net/api/Ani/AddFavorite", dataAPI2);


                            string resultfromadding = contentAPI2.Content.ReadAsStringAsync().Result;

                            if (resultfromadding == "BAD")
                            {
                                await botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, "Это аниме уже есть в вашем списке");
                                break;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, "OK");
                                break;
                            }
                        }
                    case 2:
                        {
                            usoplistdb.Usersbd[dbid].status = 0;
                            usoplistdb.Usersbd[dbid].DBstatus = 0;
                            System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));

                            UserInfo userInfo = new UserInfo();
                            userInfo.Genre = buttonText;

                            //вот тут начинается поиск, не очень понялк как (магия наверное)
                            var json = JsonConvert.SerializeObject(userInfo);
                            var data = new StringContent(json, Encoding.UTF8, "application/json");


                            using var client = new HttpClient();
                            var content = await client.PostAsync("https://anibotapi20200520082004.azurewebsites.net/api/Ani/genre", data);


                            string result = content.Content.ReadAsStringAsync().Result;

                            if (result == "BAD")
                            {
                                await botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, "такого жанра у меня нету");
                                break;
                            }

                            UserItem useritem = JsonConvert.DeserializeObject<UserItem>(result);

                            if (useritem.anime.Count == 0)
                            {
                                await botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, "не нашлось такого");
                                break;
                            }

                            List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();
                            int a = 0;


                            foreach (var anim in useritem.anime)//динамичные кнопочки
                            {
                                if (anim.url.Length <= 60)
                                {
                                    List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                                    ts.Add(InlineKeyboardButton.WithUrl(anim.title, anim.url));
                                    inlineKeyboardList.Add(ts);
                                    a++;
                                    if (a > 80)
                                        break;
                                }
                            }
                            var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                            await botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, "Ваш список аниме", replyMarkup: inline);
                            break;
                        }
                    case 3:
                        {
                            usoplistdb.Usersbd[dbid].DBstatus = 3;
                            System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));

                            usoplistdb.Usersbd[dbid].DBstatus = 0;
                            UserInfo userInfo = new UserInfo();
                            userInfo.Name = buttonText;
                            //вот тут начинается поиск, не очень понял как
                            var json = JsonConvert.SerializeObject(userInfo);
                            var data = new StringContent(json, Encoding.UTF8, "application/json");
                            using var client = new HttpClient();
                            var content = await client.PostAsync("https://anibotapi20200520082004.azurewebsites.net/api/Ani/name", data);
                            string result = content.Content.ReadAsStringAsync().Result;
                            UserItem useritem = JsonConvert.DeserializeObject<UserItem>(result);

                            UserItem userIteminDB = new UserItem();
                            userIteminDB.Id = Convert.ToString(e.CallbackQuery.From.Id);

                            Userresultitem itemzeroinresult = useritem.results[0];
                            userIteminDB.results.Add(itemzeroinresult);

                            var jsonAPI2 = JsonConvert.SerializeObject(userIteminDB);
                            var dataAPI2 = new StringContent(jsonAPI2, Encoding.UTF8, "application/json");
                            var contentAPI2 = await client.PutAsync("https://anibotapi20200520082004.azurewebsites.net/api/Ani/DeleteFavorite", dataAPI2);


                            string resultfromadding = contentAPI2.Content.ReadAsStringAsync().Result;

                            if (resultfromadding == "BAD")
                            {
                                await botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, "ты уже удалил его ¯\\_(ツ)_/¯");
                                break;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, "OK");
                                break;
                            }
                        }
                }
            }
            catch
            {
                await botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, "Не ломай бота");
                Console.WriteLine("бота ломают");
            }
        }

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                var Text = e.Message.Text;

                if (Text == null || e.Message.Type != MessageType.Text)
                    return;

                string Name = $"{e.Message.From.FirstName}";

                string example;

                Console.WriteLine($"'{Name}'с id '{e.Message.Chat.Id}' ввел '{Text}'");

                string[] str = Text.Split(' ');
                usoplistdb usoplistdb = JsonConvert.DeserializeObject<usoplistdb>(System.IO.File.ReadAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json"));


                usop1 usop = new usop1();  //проверка юзера в файле
                usop.id = e.Message.Chat.Id;
                usop.status = 0;
                bool cont = false;
                int dbid = 0;

                for (int i = 0; i < usoplistdb.Usersbd.Count; i++)
                {
                    if (usoplistdb.Usersbd[i].id == e.Message.Chat.Id)
                    {
                        dbid = i;
                        cont = true;
                        break;
                    }
                }

                if (cont != true)
                {
                    usoplistdb.Usersbd.Add(usop);
                    System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));
                }

                try
                {
                    switch (usoplistdb.Usersbd[dbid].status)
                    {
                        case 0:
                            {

                                switch (e.Message.Text)
                                {
                                    case "/start":
                                        {
                                            try
                                            {
                                                string text = "Ищем анимешки, главное ввести команду";
                                                await botClient.SendPhotoAsync(e.Message.Chat.Id, "https://s.tcdn.co/3b6/1c4/3b61c4e8-695a-30c1-9fc9-78530479a8fb/1.png");
                                                await botClient.SendTextMessageAsync(e.Message.Chat.Id, text);
                                                break;
                                            }
                                            catch (Exception exep)
                                            {
                                                Console.WriteLine(exep);
                                                break;
                                            }
                                        }
                                    case "/getbygenre":
                                        {
                                            usoplistdb.Usersbd[dbid].DBstatus = 2;
                                            System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));

                                            string[] genres = { "action", "adventure", "cars", "comedy", "dementia", "demons", "mystery", "drama", "ecchi", "fantasy", "game", "hentai", "historical", "horror", "kids", "magic", "martial arts", "mecha", "music", "parody", "samurai", "romance", "school", "sci fi", "shoujo", "shoujo ai", "shounen", "shounen Ai", "space", "sports", "super power", "vampire", "yaoi", "yuri", "harem", "slice of life", "supernatural", "military", "police", "psychological", "thriller", "seinen", "josei" };

                                            Console.WriteLine(genres.Length);
                                            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Выберите жанр");


                                            List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();
                                            int a = 0;

                                            for (int i = 0; i < genres.Length - 1; i = i + 2)
                                            {
                                                if (genres[i].Length <= 60)
                                                {
                                                    List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                                                    ts.Add(InlineKeyboardButton.WithCallbackData(genres[i], genres[i]));
                                                    ts.Add(InlineKeyboardButton.WithCallbackData(genres[i + 1], genres[i + 1]));
                                                    inlineKeyboardList.Add(ts);
                                                }
                                            }

                                            var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                                            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "выберете жанр", replyMarkup: inline);
                                            break;
                                        }
                                    case "/getbyname":
                                        {
                                            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Введите название аниме");
                                            usoplistdb.Usersbd[dbid].status = 2;
                                            System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));
                                            break;
                                        }
                                    case "/addtofav":
                                        {
                                            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Введите название аниме");
                                            usoplistdb.Usersbd[dbid].status = 3;
                                            System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));
                                            break;
                                        }
                                    case "/getfav":
                                        {
                                            usoplistdb.Usersbd[dbid].status = 0;//выставляем статусы
                                            usoplistdb.Usersbd[dbid].DBstatus = 2;
                                            System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));

                                            UserInfo userInfo = new UserInfo();
                                            userInfo.Id = Convert.ToString(e.Message.From.Id);

                                            var json = JsonConvert.SerializeObject(userInfo);
                                            var data = new StringContent(json, Encoding.UTF8, "application/json");

                                            using var client = new HttpClient();
                                            var content = await client.PostAsync("https://anibotapi20200520082004.azurewebsites.net/api/Ani/GetFavorite", data);//достаем анимки юзера

                                            string result = content.Content.ReadAsStringAsync().Result;

                                            if (result == "BAD")
                                            {
                                                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "а анимок то нету¯\\_(ツ)_/¯");
                                                break;
                                            }

                                            DBuseritem useritem = JsonConvert.DeserializeObject<DBuseritem>(result);

                                            if (useritem.jsresults.results.Count == 0)
                                            {
                                                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "а анимок то нету¯\\_(ツ)_/¯");
                                                break;
                                            }

                                            List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();

                                            int a = 0;
                                            foreach (var anim in useritem.jsresults.results)//динамичные кнопочки
                                            {
                                                if (anim.title.Length <= 60)
                                                {
                                                    List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                                                    ts.Add(InlineKeyboardButton.WithUrl(anim.title, anim.url));
                                                    inlineKeyboardList.Add(ts);
                                                    a++;
                                                    if (a > 80)
                                                        break;
                                                }
                                            }
                                            var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                                            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Ваш список аниме", replyMarkup: inline);
                                            break;

                                        }
                                    case "/delfav":
                                        {
                                            usoplistdb.Usersbd[dbid].status = 0;
                                            usoplistdb.Usersbd[dbid].DBstatus = 3;
                                            System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));

                                            UserInfo userInfo = new UserInfo();
                                            userInfo.Id = Convert.ToString(e.Message.From.Id);

                                            var json = JsonConvert.SerializeObject(userInfo);
                                            var data = new StringContent(json, Encoding.UTF8, "application/json");

                                            using var client = new HttpClient();
                                            var content = await client.PostAsync("https://anibotapi20200520082004.azurewebsites.net/api/Ani/GetFavorite", data);//достаем анимки юзера

                                            string result = content.Content.ReadAsStringAsync().Result;

                                            if (result == "BAD")
                                            {
                                                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "а анимок то нету¯\\_(ツ)_/¯");
                                                break;
                                            }

                                            DBuseritem useritem = JsonConvert.DeserializeObject<DBuseritem>(result);

                                            List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();

                                            int a = 0;
                                            foreach (var anim in useritem.jsresults.results)//динамичные кнопочки
                                            {
                                                if (anim.title.Length <= 60)
                                                {
                                                    List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                                                    ts.Add(InlineKeyboardButton.WithCallbackData(anim.title, anim.title));
                                                    inlineKeyboardList.Add(ts);
                                                    a++;
                                                    if (a > 80)
                                                        break;
                                                }
                                            }
                                            var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                                            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "какое аниме вы хотите удалить?", replyMarkup: inline);
                                            break;
                                        }
                                    case "/send":
                                        {
                                            if (e.Message.Chat.Id == Convert.ToInt64("583599263"))
                                            {
                                                try
                                                {
                                                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "введи ид и сообщение");
                                                    usoplistdb.Usersbd[dbid].status = 4;
                                                    System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));

                                                    break;
                                                }
                                                catch (Exception exep)
                                                {
                                                    Console.WriteLine(exep);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    default:
                                        {
                                            await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Что-то пошло не так");
                                            break;
                                        }
                                }
                                break;
                            }
                        case 1: //getbygenre
                            {
                                usoplistdb.Usersbd[dbid].status = 0;
                                System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));
                                break;
                            }
                        case 2: //getbyname
                            {
                                usoplistdb.Usersbd[dbid].status = 0;
                                System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));

                                UserInfo userInfo = new UserInfo();
                                userInfo.Name = e.Message.Text;

                                //вот тут начинается поиск, не очень понялк как (магия наверное)
                                var json = JsonConvert.SerializeObject(userInfo);
                                var data = new StringContent(json, Encoding.UTF8, "application/json");

                                using var client = new HttpClient();
                                var content = await client.PostAsync("https://anibotapi20200520082004.azurewebsites.net/api/Ani/name", data);


                                string result = content.Content.ReadAsStringAsync().Result;
                                UserItem useritem = JsonConvert.DeserializeObject<UserItem>(result);

                                if (useritem.results.Count == 0)
                                {
                                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "такого аниме нету");
                                    break;
                                }

                                List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();
                                int a = 0;

                                foreach (var anim in useritem.results)//динамичные кнопочки
                                {
                                    if (anim.title.Length <= 60)
                                    {
                                        List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                                        ts.Add(InlineKeyboardButton.WithUrl(anim.title, anim.url));
                                        inlineKeyboardList.Add(ts);
                                        a++;
                                        if (a > 80)
                                            break;
                                    }
                                }
                                var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Ваш список аниме", replyMarkup: inline);
                                break;
                            }
                        case 3: //addtofav
                            {
                                usoplistdb.Usersbd[dbid].status = 0;
                                usoplistdb.Usersbd[dbid].DBstatus = 1;
                                System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));


                                usoplistdb.Usersbd[dbid].status = 0; //поиск по названию
                                System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));


                                UserInfo userInfo = new UserInfo();
                                userInfo.Name = e.Message.Text;


                                //вот тут начинается поиск, не очень понялк как (магия наверное)
                                var json = JsonConvert.SerializeObject(userInfo);
                                var data = new StringContent(json, Encoding.UTF8, "application/json");


                                using var client = new HttpClient();
                                var content = await client.PostAsync("https://anibotapi20200520082004.azurewebsites.net/api/Ani/name", data);


                                string result = content.Content.ReadAsStringAsync().Result;
                                UserItem useritem = JsonConvert.DeserializeObject<UserItem>(result);


                                if (useritem.results.Count == 0)
                                {
                                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "такого аниме нету");
                                    break;
                                }


                                List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();
                                int a = 0;


                                foreach (var anim in useritem.results)//динамичные кнопочки
                                {
                                    if (anim.title.Length <= 60)
                                    {
                                        List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                                        ts.Add(InlineKeyboardButton.WithCallbackData(anim.title, anim.title));
                                        inlineKeyboardList.Add(ts);
                                        a++;
                                        if (a > 10)
                                            break;
                                    }
                                }
                                var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "выберите аниме которое хотите добавить:", replyMarkup: inline);
                                break;
                            }
                        case 4: //send
                            {
                                try
                                {
                                    usoplistdb.Usersbd[dbid].status = 0;
                                    System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));

                                    string[] text = e.Message.Text.Split(" ");
                                    string ex = "";

                                    for (int i = 1; i < text.Length; i++)
                                    {
                                        ex = ex + text[i] + " ";
                                    }

                                    await botClient.SendTextMessageAsync(text[0], ex);

                                    break;
                                }
                                catch
                                {
                                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "ошибочка");
                                    break;
                                }
                            }
                        case 5: //delfav
                            {
                                break;
                            }
                        default:
                            {
                                usoplistdb.Usersbd[dbid].status = 0;
                                System.IO.File.WriteAllText(@"D:\visual studio проекты\AniTelegramBot\AniTelegramBot.useroption.json", JsonConvert.SerializeObject(usoplistdb));
                                break;
                            }
                    }
                }
                catch
                {
                    Console.WriteLine("ХАНАААААААААААААААААААААААААААААААААААААААААА");
                }
            }
            catch
            {

                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Не ломай бота");
                Console.WriteLine("бота ломают");
            }
        }
        }
}
