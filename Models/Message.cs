namespace ApnaRent.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Text { get; set; }
        public DateTime SentOn { get; set; } = DateTime.Now;

        public Item Item { get; set; }
    }
}