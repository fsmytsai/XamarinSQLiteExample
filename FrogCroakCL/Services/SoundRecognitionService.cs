using System;
using System.Text;
using FrogCroakCL.Models;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

namespace FrogCroakCL.Services
{
    public class SoundRecognitionService
    {
        public async Task<AllRequestResult> SoundRecognition(string FromFilePath)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "audio/x-wav";
                    byte[] result = await client.UploadFileTaskAsync(CPSharedService.BackEndPath + "Api/Frog/SoundRecognition", FromFilePath);
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
    }
}