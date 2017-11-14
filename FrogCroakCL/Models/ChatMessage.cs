using SQLite;

namespace FrogCroakCL.Models
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Message { get; set; }
        public bool Isme { get; set; }
        public int Type { get; set; }
    }
}