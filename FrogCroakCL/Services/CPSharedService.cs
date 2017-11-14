using SQLite;
using System;
using System.IO;
using FrogCroakCL.Models;

namespace FrogCroakCL.Services
{
    public class CPSharedService
    {
        public static string BackEndPath = "https://frogcroak.azurewebsites.net/";

        private static SQLiteConnection connection;

        public static SQLiteConnection GetSQLiteConnection()
        {
            if (connection == null)
            {
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "FrogCroak.db");
                connection = new SQLiteConnection(dbPath);
                connection.CreateTable<ChatMessage>();
                connection.CreateTable<UserMarker>();
                if (connection.Table<ChatMessage>().Count() == 0)
                {
                    // only insert the data if it doesn't already exist
                    var chatMessage = new ChatMessage();
                    chatMessage.Message = "目前所有功能已提供以下10種青蛙之辨識：\n1. 艾氏樹蛙\n2. 拉都希氏赤蛙\n3. 虎皮蛙\n4. 豎琴蛙\n5. 小雨蛙\n6. 台北樹蛙\n7. 布氏樹蛙\n8. 面天樹蛙\n9. 貢德氏赤蛙\n10. 澤蛙\n\n智慧問答提供以下5種諮詢類別：\n1.介紹\n2.叫聲\n3.分布\n4.繁殖期\n5.外觀\n\n智慧問答例句：台北樹蛙的分布";
                    chatMessage.Isme = false;
                    chatMessage.Type = 0;
                    bool IsSuccess = connection.Insert(chatMessage) == 1;
                }
            }
            return connection;
        }
    }
}