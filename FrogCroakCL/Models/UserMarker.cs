using SQLite;

namespace FrogCroakCL.Models
{
    [Table("UserMarkers")]
    public class UserMarker
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
