using FrogCroakCL.Models;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FrogCroakCL.Services
{
    public class ChatMessageService
    {
        private SQLiteConnection db;
        public ChatMessageService()
        {
            db = CPSharedService.GetSQLiteConnection();
        }

        public async Task<AllRequestResult> UploadImage(string FromFilePath, string ContentType)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "image/" + ContentType.ToLower();
                    byte[] result = await client.UploadFileTaskAsync(CPSharedService.BackEndPath + "api/ImageApi/UploadImage", FromFilePath);
                    string Frog = JsonConvert.DeserializeObject<String>(Encoding.Default.GetString(result));
                    return new AllRequestResult
                    {
                        IsSuccess = true,
                        Result = Frog
                    };
                }
            }
            catch (WebException ex)
            {
                return new AllRequestResult
                {
                    IsSuccess = false,
                    Result = ex
                };
            }
        }

        public async Task<AllRequestResult> CreateMessage(string Content)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string result = await client.UploadStringTaskAsync(
                        $"{CPSharedService.BackEndPath}api/MessageApi/CreateMessage",
                        $"Content={Content}"
                    );

                    result = JsonConvert.DeserializeObject<String>(result);

                    return new AllRequestResult
                    {
                        IsSuccess = true,
                        Result = result
                    };
                }
            }
            catch (WebException ex)
            {
                return new AllRequestResult
                {
                    IsSuccess = false,
                    Result = ex
                };
            }
        }

        private int newMessageCount = 0;

        public bool CreateChatMessage(ChatMessage chatMessage)
        {
            bool IsSuccess = db.Insert(chatMessage) == 1;
            if (IsSuccess)
                newMessageCount++;
            return IsSuccess;
        }

        public List<ChatMessage> GetChatMessageList(int NowPage)
        {
            var table = db.Table<ChatMessage>().OrderByDescending(m => m.Id).Skip(NowPage * 15 + newMessageCount).Take(15);
            List<ChatMessage> ChatMessageList = new List<ChatMessage>();
            ChatMessageList.AddRange(table);
            return ChatMessageList;
        }
    }
}
