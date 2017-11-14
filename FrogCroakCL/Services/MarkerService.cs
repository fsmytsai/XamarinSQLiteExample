using System.Text;
using FrogCroakCL.Models;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using SQLite;

namespace FrogCroakCL.Services
{
    public class MarkerService
    {
        private SQLiteConnection db;
        public MarkerService()
        {
            db = CPSharedService.GetSQLiteConnection();
        }

        public AllRequestResult GetMyMarkerList()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string url = $"{CPSharedService.BackEndPath}api/MarkerApi/GetMarkerList";
                    string result = client.DownloadString(
                        url
                    );

                    MyMarkers myMarkers = JsonConvert.DeserializeObject<MyMarkers>(result);

                    return new AllRequestResult
                    {
                        IsSuccess = true,
                        Result = myMarkers
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

        public bool CreateUserMarker(UserMarker userMarker)
        {
            int Result = db.Insert(userMarker);
            return Result == 1;
        }

        public List<UserMarker> GetUserMarkerList()
        {
            var table = db.Table<UserMarker>();
            List<UserMarker> UserMarkerList = new List<UserMarker>();
            UserMarkerList.AddRange(table);
            return UserMarkerList;
        }
    }
}